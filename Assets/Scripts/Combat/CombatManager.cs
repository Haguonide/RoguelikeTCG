using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;
using RoguelikeTCG.AI;
using RoguelikeTCG.UI;
using RoguelikeTCG.Core;
using RoguelikeTCG.RunMap;

namespace RoguelikeTCG.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Systems")]
        public BoardManager boardManager;
        public DeckManager playerDeck;
        public DeckManager enemyDeck;
        public ManaManager manaManager;
        public TurnManager turnManager;
        public EnemyAI enemyAI;
        public CombatLog combatLog;

        [Header("UI")]
        public CombatUI combatUI;
        public HandView handView;

        [Header("Animations")]
        public CombatAnimator combatAnimator;

        [Header("Characters")]
        public CharacterData playerCharacter;
        public CharacterData enemyCharacter;

        [Header("Hero Portraits (un par board)")]
        public HeroPortraitUI[] playerHeroBtns;
        public HeroPortraitUI[] enemyHeroBtns;

        [Header("Rewards")]
        [Tooltip("Pool de cartes proposées en récompense après victoire (combats normaux)")]
        public List<CardData> rewardCardPool;
        private const int RewardCount = 3;

        [Tooltip("Pool de reliques proposées en récompense (Élite / Boss)")]
        public List<RelicData> relicRewardPool;

        [Header("Bricolage (De Vinci only)")]
        public CardData bricolageCardData;

        [Header("State")]
        public int playerHP;
        public int playerMaxHP;
        public int playerShieldHP;
        public int gearCount;
        public int gearCumulatedATK;
        public int gearCumulatedHP;

        private bool gameOver;
        private LaneSlotUI[] allLaneSlots;
        private readonly HashSet<int> _clearedBoards = new HashSet<int>();
        private int _lastGoldEarned;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void ConfigureBoardCount()
        {
            var nodeType = RunPersistence.Instance?.CurrentNode?.type;
            int count = (nodeType == NodeType.Combat) ? Random.Range(1, 3) : 3;
            boardManager?.SetActiveBoardCount(count);

            int hpPerBoard = nodeType switch
            {
                NodeType.Elite => 35,
                NodeType.Boss  => 55,
                _              => 25,
            };
            foreach (var board in boardManager.boards)
            {
                board.enemyMaxHP     = hpPerBoard;
                board.enemyCurrentHP = hpPerBoard;
            }
        }

        private void Start()
        {
            allLaneSlots = FindObjectsOfType<LaneSlotUI>(true);
            ConfigureBoardCount();
            InitializeCombat();
        }

        private void InitializeCombat()
        {
            gameOver = false;
            _clearedBoards.Clear();
            playerShieldHP   = 0;
            gearCount        = 0;
            gearCumulatedATK = 0;
            gearCumulatedHP  = 0;

            // Personnage : RunPersistence en priorité sur le champ sérialisé en scène
            var persistence = RunPersistence.Instance;
            if (persistence?.SelectedCharacter != null)
                playerCharacter = persistence.SelectedCharacter;

            // Pool de récompenses : EffectiveCardPool de la run (cardPool de base + épiques débloquées)
            if (persistence?.EffectiveCardPool != null && persistence.EffectiveCardPool.Count > 0)
                rewardCardPool = new List<CardData>(persistence.EffectiveCardPool);
            else if (playerCharacter?.cardPool != null && playerCharacter.cardPool.Count > 0)
                rewardCardPool = new List<CardData>(playerCharacter.cardPool);

            // HP joueur : reprendre depuis RunPersistence si disponible
            if (persistence != null && persistence.PlayerHP > 0)
            {
                playerMaxHP = persistence.PlayerMaxHP;
                playerHP    = persistence.PlayerHP;
            }
            else
            {
                playerMaxHP = playerCharacter != null ? playerCharacter.maxHP : 80;
                playerHP    = playerMaxHP;
            }

            // Deck joueur : reprendre depuis RunPersistence si disponible, sinon deck de départ
            var persistedDeck = persistence?.PlayerDeck;
            if (persistedDeck != null && persistedDeck.Count > 0)
            {
                playerDeck.InitializeDeck(persistedDeck, true);
            }
            else
            {
                var startingDeck = playerCharacter != null ? playerCharacter.startingDeck : new List<CardData>();
                playerDeck.InitializeDeck(startingDeck, true);
                if (persistence != null)
                    persistence.PlayerDeck = new List<CardData>(startingDeck);
            }

            if (enemyCharacter != null)
                enemyDeck.InitializeDeck(enemyCharacter.startingDeck, false);
            else
                enemyDeck.InitializeDeck(new List<CardData>(), false);

            // Portraits héros
            if (playerCharacter?.portrait != null)
                foreach (var btn in playerHeroBtns) btn?.SetPortrait(playerCharacter.portrait);
            if (enemyCharacter?.portrait != null)
                foreach (var btn in enemyHeroBtns) btn?.SetPortrait(enemyCharacter.portrait);

            manaManager.Initialize();
            // Bonus mana des reliques
            int bonusMana = RelicManager.Instance?.GetBonusStartMana() ?? 0;
            if (bonusMana > 0) manaManager.AddBonus(bonusMana);

            enemyAI.Initialize(enemyDeck, boardManager, manaManager);

            AudioManager.Instance.PlayMusic("music_combat");
            Log("⚔ Combat commencé !");
            turnManager.StartPlayerTurn();
            manaManager.RegenTurn();
            int extraDraw = RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0;
            playerDeck.DrawCards(playerDeck.drawPerTurn + extraDraw);
            AudioManager.Instance.PlaySFX("sfx_card_draw");

            RefreshAllUI();
        }

        // ── Card Play ─────────────────────────────────────────────────────────

        public bool TryPlayUnit(CardInstance card, Lane lane)
        {
            if (gameOver) return false;
            if (CombatAnimator.Instance != null && CombatAnimator.Instance.IsAnimating) return false;
            if (!turnManager.IsPlayerTurn || !turnManager.CanPlayCard) return false;
            if (boardManager.ActiveBoard.IsDefeated) return false;
            if (!card.IsUnit || lane.IsOccupied || !lane.isPlayerLane) return false;

            card.placedThisTurn = true;
            lane.PlaceCard(card);
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            Log($"> Vous posez {card.data.cardName} ({card.CurrentAttack}/{card.currentHP})");
            TriggerOnEntryPassives(card, lane, boardManager.ActiveBoard);
            RefreshAllUI();
            if (HasDamageOnEntry(card))
                StartCoroutine(PlayChargeEntryAnim(card, lane, boardManager.ActiveBoard));
            return true;
        }

        public void NotifyUnitPlaced(CardInstance unit, Lane lane, Board board)
        {
            TriggerOnEntryPassives(unit, lane, board);
            RefreshAllUI();
            if (HasDamageOnEntry(unit))
                StartCoroutine(PlayChargeEntryAnim(unit, lane, board));
        }

        public bool TryPlaySpellOnHero(CardInstance card, bool isPlayerHero)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            ApplyEffectsToHero(card, isPlayerHero);
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            Log($"> Vous lancez {card.data.cardName} sur {(isPlayerHero ? "votre héros" : "le héros ennemi")}");
            RefreshAllUI();
            return true;
        }

        public bool TryPlaySpellOnUnit(CardInstance card, Lane targetLane)
        {
            if (!CanCastSpell(card)) return false;
            if (targetLane == null || !targetLane.IsOccupied) return false;

            string targetName = targetLane.Occupant.data.cardName;
            manaManager.Spend(card.data.manaCost);
            ApplyEffectsToUnit(card, targetLane);
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            Log($"> Vous lancez {card.data.cardName} sur {targetName}");
            RefreshAllUI();
            return true;
        }

        private bool CanCastSpell(CardInstance card)
        {
            if (gameOver) return false;
            if (CombatAnimator.Instance != null && CombatAnimator.Instance.IsAnimating) return false;
            if (boardManager.ActiveBoard.IsDefeated) return false;
            if (!turnManager.IsPlayerTurn || !turnManager.CanPlayCard) return false;
            if (card.IsUnit) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;
            return true;
        }

        private void ApplyEffectsToHero(CardInstance card, bool isPlayer)
        {
            foreach (var effect in card.data.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.Damage:
                        if (isPlayer)
                        {
                            DamagePlayer(effect.value);
                            Log($"  → {effect.value} dégâts à votre héros (HP: {playerHP}/{playerMaxHP})");
                        }
                        else
                        {
                            foreach (var board in boardManager.boards)
                                if (!board.IsDefeated) board.TakeDamage(effect.value);
                            Log($"  → {effect.value} dégâts au héros ennemi");
                        }
                        break;
                    case EffectType.Heal:
                        if (isPlayer)
                        {
                            playerHP = Mathf.Min(playerMaxHP, playerHP + effect.value);
                            Log($"  → Vous récupérez {effect.value} HP (HP: {playerHP}/{playerMaxHP})");
                        }
                        break;
                    case EffectType.BuffAttack:
                        foreach (var board in boardManager.boards)
                            foreach (var lane in board.playerLanes)
                                if (lane != null && lane.IsOccupied)
                                    lane.Occupant.bonusAttack += effect.value;
                        Log($"  → {(effect.value >= 0 ? $"+{effect.value}" : $"{effect.value}")} ATK à toutes vos unités");
                        break;
                    case EffectType.DrawCard:
                        playerDeck.DrawCards(effect.value);
                        Log($"  → Vous piochez {effect.value} carte(s)");
                        break;
                    case EffectType.Shield:
                        if (isPlayer)
                        {
                            playerShieldHP += effect.value;
                            Log($"  → Votre héros gagne {effect.value} de bouclier ({playerShieldHP} total)");
                        }
                        break;
                }
            }
        }

        private void ApplyEffectsToUnit(CardInstance card, Lane targetLane)
        {
            var target     = targetLane.Occupant;
            var targetSlot = FindSlotForLane(targetLane);
            var targetRT   = targetSlot != null ? (RectTransform)targetSlot.transform : null;

            foreach (var effect in card.data.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.Damage:
                        int spellDmg = effect.value;
                        if (target.shieldHP > 0)
                        {
                            int absorbed = Mathf.Min(target.shieldHP, spellDmg);
                            target.shieldHP -= absorbed;
                            spellDmg -= absorbed;
                            Log($"  → Le bouclier absorbe {absorbed} dégât(s) ({target.shieldHP} restants)");
                        }
                        target.currentHP -= spellDmg;
                        if (targetRT != null && spellDmg > 0)
                            RoguelikeTCG.UI.DamagePopup.ShowDamage(targetRT, spellDmg);
                        Log($"  → {effect.value} dégâts à {target.data.cardName} ({target.currentHP} HP restants)");
                        if (!target.IsAlive)
                        {
                            var killedBySpell = target;
                            targetLane.ClearCard();
                            Log($"  → {killedBySpell.data.cardName} est détruit !");
                            TriggerOnDeathPassives(killedBySpell, null, boardManager.ActiveBoard, true);
                        }
                        break;
                    case EffectType.Heal:
                        int healed = Mathf.Min(target.data.maxHP, target.currentHP + effect.value) - target.currentHP;
                        target.currentHP += healed;
                        if (targetRT != null && healed > 0)
                            RoguelikeTCG.UI.DamagePopup.ShowHeal(targetRT, healed);
                        Log($"  → {target.data.cardName} récupère {healed} HP ({target.currentHP}/{target.data.maxHP})");
                        break;
                    case EffectType.BuffAttack:
                        target.bonusAttack += effect.value;
                        Log($"  → {target.data.cardName} {(effect.value >= 0 ? $"gagne +{effect.value}" : $"perd {-effect.value}")} ATK ({target.CurrentAttack} ATK)");
                        break;
                    case EffectType.BuffHP:
                        target.data.maxHP   += effect.value;
                        target.currentHP    += effect.value;
                        Log($"  → {target.data.cardName} gagne +{effect.value} HP max ({target.currentHP}/{target.data.maxHP})");
                        break;
                    case EffectType.Shield:
                        target.shieldHP += effect.value;
                        if (targetRT != null)
                            RoguelikeTCG.UI.DamagePopup.ShowShield(targetRT, effect.value);
                        Log($"  → {target.data.cardName} gagne un bouclier de {effect.value} ({target.shieldHP} total)");
                        break;
                    case EffectType.ApplyPoison:
                        target.poisonStacks += effect.value;
                        Log($"  → {target.data.cardName} est empoisonné ({target.poisonStacks} charge{(target.poisonStacks > 1 ? "s" : "")})");
                        break;
                }
            }
        }

        // ── AoE Spell ─────────────────────────────────────────────────────────

        public bool TryPlayAoESpell(CardInstance card)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            AudioManager.Instance.PlaySFX("sfx_spell_aoe");
            Log($"> Vous lancez {card.data.cardName} sur toutes les unités ennemies !");
            ApplyAoEEffects(card);
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            RefreshAllUI();
            return true;
        }

        private void ApplyAoEEffects(CardInstance card)
        {
            var activeBoard = boardManager.ActiveBoard;
            if (activeBoard == null) return;

            foreach (var effect in card.data.effects)
            {
                if (effect.effectType != EffectType.Damage) continue;

                foreach (var lane in activeBoard.enemyLanes)
                {
                    if (lane == null || !lane.IsOccupied) continue;
                    var target = lane.Occupant;
                    string targetName = target.data.cardName;

                    int dmg = effect.value;
                    if (target.shieldHP > 0)
                    {
                        int absorbed = Mathf.Min(target.shieldHP, dmg);
                        target.shieldHP -= absorbed;
                        dmg -= absorbed;
                    }
                    target.currentHP -= dmg;
                    Log($"  → {effect.value} dégâts à {targetName} ({target.currentHP} HP restants)");
                    if (!target.IsAlive)
                    {
                        lane.ClearCard();
                        Log($"  → {targetName} est détruit !");
                        TriggerOnDeathPassives(target, null, activeBoard, true);
                    }
                }
            }
        }

        // ── Turn Flow ─────────────────────────────────────────────────────────

        public void EndPlayerTurn()
        {
            if (gameOver || !turnManager.IsPlayerTurn) return;
            if (CombatAnimator.Instance != null && CombatAnimator.Instance.IsAnimating) return;
            AudioManager.Instance.PlaySFX("sfx_end_turn");
            Log("--- Fin de votre tour ---");
            turnManager.EndPlayerTurn();
            StartCoroutine(ResolveAndEnemyTurn());
        }

        private IEnumerator ResolveAndEnemyTurn()
        {
            ResetSurvivedAttackFlags();
            TriggerStartOfTurnPassives();
            RefreshAllUI();

            Log("[ Résolution des attaques joueur ]");
            for (int i = 0; i < boardManager.boards.Count; i++)
            {
                var board = boardManager.boards[i];
                if (!board.isActive || board.IsDefeated) continue;

                if (combatAnimator != null && i != boardManager.ActiveBoardIndex)
                    yield return StartCoroutine(combatAnimator.PlayBoardSlide(boardManager.ActiveBoardIndex, i));
                boardManager.SetActiveBoard(i);

                yield return StartCoroutine(ResolveBoard(board, playerAttacks: true));

                // Board nouvellement vaincu → récupérer unités + bonus action
                if (board.IsDefeated && !_clearedBoards.Contains(i))
                {
                    _clearedBoards.Add(i);
                    yield return StartCoroutine(HandleBoardCleared(board, i));
                }

                RefreshAllUI();
            }

            if (boardManager.AllBoardsDefeated()) { OnVictory(); yield break; }
            if (playerHP <= 0)                    { OnDefeat();  yield break; }

            // L'IA pose ses cartes maintenant — elles ont placedThisTurn=true
            // et ne peuvent donc pas attaquer ce tour (summoning sickness symétrique)
            Log("--- Tour ennemi ---");
            turnManager.StartEnemyTurn();
            manaManager.RegenTurn();
            enemyAI.PlayTurn();
            RefreshAllUI();
            yield return new WaitForSeconds(0.3f);

            Log("[ Résolution des attaques ennemies ]");
            for (int i = 0; i < boardManager.boards.Count; i++)
            {
                var board = boardManager.boards[i];
                if (!board.isActive || board.IsDefeated) continue;

                if (combatAnimator != null && i != boardManager.ActiveBoardIndex)
                    yield return StartCoroutine(combatAnimator.PlayBoardSlide(boardManager.ActiveBoardIndex, i));
                boardManager.SetActiveBoard(i);

                yield return StartCoroutine(ResolveBoard(board, playerAttacks: false));
                RefreshAllUI();
            }

            if (playerHP <= 0) { OnDefeat(); yield break; }

            TriggerEndOfRoundPassives();
            RefreshAllUI();

            ResetPlacedThisTurnFlags();
            Log("--- Votre tour ---");
            turnManager.StartPlayerTurn();
            manaManager.RegenTurn();
            playerDeck.DrawCards(playerDeck.drawPerTurn + (RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0));
            AudioManager.Instance.PlaySFX("sfx_card_draw");
            RefreshAllUI();
        }

        // ── Board Cleared ─────────────────────────────────────────────────────

        private IEnumerator HandleBoardCleared(Board board, int boardIndex)
        {
            Log($"=== Board {boardIndex + 1} vaincu ! ===");
            yield return new WaitForSeconds(0.4f);

            int returned = 0;
            foreach (var lane in board.playerLanes)
            {
                if (lane != null && lane.IsOccupied)
                {
                    playerDeck.ReturnToHand(lane.Occupant);
                    lane.ClearCard();
                    returned++;
                }
            }

            foreach (var lane in board.enemyLanes)
                lane?.ClearCard();

            turnManager.GrantBonusNextTurn();

            if (returned > 0)
                Log($"  {returned} unité(s) alliée(s) récupérées en main.");
            Log($"  +1 action accordée pour le prochain tour !");

            RefreshAllUI();
        }

        // ── Combat Resolution ─────────────────────────────────────────────────

        private IEnumerator ResolveBoard(Board board, bool playerAttacks)
        {
            for (int i = 0; i < 3; i++)
            {
                var pLane = board.playerLanes[i];
                var eLane = board.enemyLanes[i];
                if (pLane == null || eLane == null) continue;

                if (playerAttacks)
                {
                    // Skip units placed this turn (summoning sickness)
                    if (pLane.IsOccupied && pLane.Occupant.placedThisTurn) continue;
                    yield return StartCoroutine(AttackWithUnit(pLane, eLane, board, isPlayerAttacking: true));
                    if (board.IsDefeated) yield break;
                }
                else
                {
                    // Skip units placed this turn (summoning sickness)
                    if (eLane.IsOccupied && eLane.Occupant.placedThisTurn) continue;
                    yield return StartCoroutine(AttackWithUnit(eLane, pLane, board, isPlayerAttacking: false));
                    if (playerHP <= 0) yield break;
                }
            }
        }

        private IEnumerator AttackWithUnit(Lane attackerLane, Lane defenderLane,
                                           Board board, bool isPlayerAttacking)
        {
            if (!attackerLane.IsOccupied) yield break;

            var attacker     = attackerLane.Occupant;
            var attackerSlot = FindSlotForLane(attackerLane);

            if (combatAnimator != null && attackerSlot != null)
                yield return StartCoroutine(combatAnimator.PlayAttackAnim(attackerSlot, isPlayerAttacking));

            if (defenderLane.IsOccupied)
            {
                var defender     = defenderLane.Occupant;
                var defenderSlot = FindSlotForLane(defenderLane);

                // Damage calculation with passive reductions
                int dmg = attacker.CurrentAttack;

                // Positional bonuses: centre attaquant +1 ATK, défenseur sur flanc -1 dmg
                dmg += LaneAttackBonus(attackerLane);
                dmg  = Mathf.Max(0, dmg - LaneDefenseReduction(defenderLane));

                // DmgReduction on the defender itself (individual passive, not shown on card)
                foreach (var p in GetPassives(defender))
                    if (p.passiveType == UnitPassiveType.DmgReduction)
                        dmg = Mathf.Max(0, dmg - p.value);

                // Shield absorption
                if (defender.shieldHP > 0)
                {
                    int absorbed = Mathf.Min(defender.shieldHP, dmg);
                    defender.shieldHP -= absorbed;
                    dmg -= absorbed;
                    Log($"  → Le bouclier absorbe {absorbed} dégât(s) ({defender.shieldHP} restants)");
                }

                defender.currentHP -= dmg;
                if (defenderSlot != null && dmg > 0)
                    RoguelikeTCG.UI.DamagePopup.ShowDamage((RectTransform)defenderSlot.transform, dmg);
                string posLog = BuildPositionalLog(attackerLane, defenderLane);
                Log($"> {attacker.data.cardName} attaque {defender.data.cardName} " +
                    $"({attacker.CurrentAttack}{posLog} dmg → {defender.currentHP} HP restants)");

                // BonusDmgIfEnemyWeakened: if the defender has ≤0 ATK, deal bonus direct dmg
                foreach (var p in GetPassives(attacker))
                {
                    if (p.passiveType == UnitPassiveType.BonusDmgIfEnemyWeakened && defender.CurrentAttack <= 0)
                    {
                        if (isPlayerAttacking)
                        {
                            board.TakeDamage(p.value);
                            Log($"  → [{p.keyword}] +{p.value} dégâts directs au héros ennemi !");
                        }
                        else
                        {
                            DamagePlayer(p.value);
                            Log($"  → [{p.keyword}] +{p.value} dégâts directs à votre héros !");
                        }
                    }
                }

                RefreshAllUI();

                if (!defender.IsAlive)
                {
                    // ExcessDamageBreakthrough: excess dmg bleeds through to hero
                    int excess = -defender.currentHP;
                    foreach (var p in GetPassives(attacker))
                    {
                        if (p.passiveType == UnitPassiveType.ExcessDamageBreakthrough && excess > 0)
                        {
                            if (isPlayerAttacking)
                            {
                                board.TakeDamage(excess);
                                Log($"  → [{p.keyword}] {excess} dégâts excédentaires au héros ennemi !");
                            }
                            else
                            {
                                DamagePlayer(excess);
                                Log($"  → [{p.keyword}] {excess} dégâts excédentaires à votre héros !");
                            }
                        }
                    }

                    // LifestealOnKill
                    foreach (var p in GetPassives(attacker))
                    {
                        if (p.passiveType == UnitPassiveType.LifestealOnKill)
                        {
                            if (isPlayerAttacking)
                            {
                                playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                                Log($"  → [{p.keyword}] +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                            }
                            else
                            {
                                board.enemyCurrentHP = Mathf.Min(board.enemyMaxHP, board.enemyCurrentHP + p.value);
                                Log($"  → [{p.keyword}] héros ennemi récupère {p.value} HP");
                            }
                        }
                    }

                    RefreshAllUI();

                    if (combatAnimator != null && defenderSlot != null)
                        yield return StartCoroutine(combatAnimator.PlayDeathAnim(defenderSlot));

                    var deadUnit = defender;
                    defenderLane.ClearCard();
                    Log($"> {deadUnit.data.cardName} est détruit !");

                    TriggerOnDeathPassives(deadUnit, attackerLane, board, isPlayerAttacking);
                    RefreshAllUI();
                }
                else
                {
                    // Track survival for end-of-round passives (player units only)
                    if (!isPlayerAttacking)
                        defender.survivedAttackThisTurn = true;
                    else
                    {
                        // Apply poison on hit (player attacking, defender survived)
                        foreach (var p in GetPassives(attacker))
                        {
                            if (p.passiveType == UnitPassiveType.ApplyPoisonOnHit)
                            {
                                defender.poisonStacks += p.value;
                                Log($"  → [{p.keyword}] {defender.data.cardName} est empoisonné ({defender.poisonStacks}🧪)");
                            }
                        }
                    }

                    if (combatAnimator != null && defenderSlot != null)
                        yield return StartCoroutine(combatAnimator.PlayHitFlash(defenderSlot));
                }
            }
            else
            {
                // Empty lane: direct damage
                int directDmg = attacker.CurrentAttack + LaneAttackBonus(attackerLane);

                // DoubleATKIfLaneEmpty: attack power doubles when lane is uncontested
                foreach (var p in GetPassives(attacker))
                    if (p.passiveType == UnitPassiveType.DoubleATKIfLaneEmpty)
                        directDmg *= 2;

                if (isPlayerAttacking)
                {
                    board.TakeDamage(directDmg);
                    Log($"> {attacker.data.cardName} inflige {directDmg} dégâts au board ennemi " +
                        $"({board.enemyCurrentHP}/{board.enemyMaxHP} HP)");
                }
                else
                {
                    DamagePlayer(directDmg);
                    Log($"> {attacker.data.cardName} vous inflige {directDmg} dégâts ! " +
                        $"(HP: {playerHP}/{playerMaxHP})");
                }

                RefreshAllUI();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        public void DamagePlayer(int amount)
        {
            if (playerShieldHP > 0)
            {
                int absorbed = Mathf.Min(playerShieldHP, amount);
                playerShieldHP -= absorbed;
                amount -= absorbed;
                if (absorbed > 0) Log($"  → Bouclier héros absorbe {absorbed} ({playerShieldHP} restant)");
            }
            playerHP = Mathf.Max(0, playerHP - amount);
        }

        public bool IsDeVinciRun() =>
            playerCharacter?.characterName == "Léonard de Vinci";

        private void AddGear(CardInstance dead)
        {
            if (!IsDeVinciRun() || gearCount >= 3) return;
            gearCumulatedATK += dead.CurrentAttack;
            gearCumulatedHP  += dead.data.maxHP;
            gearCount++;
            int cappedATK = Mathf.Min(gearCumulatedATK, 5);
            int cappedHP  = Mathf.Min(gearCumulatedHP,  8);
            Log($"  ⚙ Rouage [{gearCount}/3] — Automate prévu : {cappedATK}/{cappedHP}");
            RefreshAllUI();
        }

        public void TryPlayBricolage(Lane lane)
        {
            if (gameOver) return;
            if (CombatAnimator.Instance != null && CombatAnimator.Instance.IsAnimating) return;
            if (!turnManager.IsPlayerTurn || !turnManager.CanPlayCard) return;
            if (boardManager.ActiveBoard.IsDefeated) return;
            if (lane == null || lane.IsOccupied || !lane.isPlayerLane) return;
            if (gearCount < 3 || !IsDeVinciRun()) return;

            int cappedATK = Mathf.Min(gearCumulatedATK, 5);
            int cappedHP  = Mathf.Min(gearCumulatedHP,  8);

            var autoInstance = new CardInstance(bricolageCardData, isPlayerCard: true);
            autoInstance.bonusAttack   = cappedATK - bricolageCardData.attackPower;
            autoInstance.currentHP     = cappedHP;
            autoInstance.placedThisTurn = true;
            lane.PlaceCard(autoInstance);
            turnManager.RegisterCardPlayed();
            Log($"> Bricolage ! Automate Réparé ({cappedATK}/{cappedHP}) invoqué !");
            AudioManager.Instance.PlaySFX("sfx_card_place");

            gearCount        = 0;
            gearCumulatedATK = 0;
            gearCumulatedHP  = 0;

            Board targetBoard = null;
            foreach (var board in boardManager.boards)
            {
                foreach (var pl in board.playerLanes)
                    if (pl == lane) { targetBoard = board; break; }
                if (targetBoard != null) break;
            }
            TriggerOnEntryPassives(autoInstance, lane, targetBoard ?? boardManager.ActiveBoard);
            RefreshAllUI();
        }

        private LaneSlotUI FindSlotForLane(Lane lane)
        {
            if (allLaneSlots == null || lane == null) return null;
            foreach (var slot in allLaneSlots)
                if (slot != null && slot.Lane == lane) return slot;
            return null;
        }

        // ── End States ────────────────────────────────────────────────────────

        private void OnVictory()
        {
            gameOver = true;
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX("sfx_victory");
            Log("=== VICTOIRE ! ===");

            // Soin post-combat (reliques)
            int heal = RelicManager.Instance?.GetHealAfterCombat() ?? 0;
            if (heal > 0)
            {
                playerHP = Mathf.Min(playerMaxHP, playerHP + heal);
                Log($"  → Relique : +{heal} HP ({playerHP}/{playerMaxHP})");
            }

            RunPersistence.Instance?.SavePlayerHP(playerHP, playerMaxHP);
            _lastGoldEarned = CalculateGoldReward();
            RunPersistence.Instance?.AddGold(_lastGoldEarned);
            Log($"+ {_lastGoldEarned} or");

            var nodeType = RunPersistence.Instance?.CurrentNode?.type ?? NodeType.Combat;
            RunPersistence.Instance?.RecordCombatWin(nodeType);

            if (nodeType == NodeType.Elite || nodeType == NodeType.Boss)
                ShowRelicReward();
            else
                ShowRewardScreen();
        }

        private static int GetSellPrice(CardRarity rarity) => rarity switch
        {
            CardRarity.Common    => 25,
            CardRarity.Uncommon  => 37,
            CardRarity.Rare      => 50,
            CardRarity.Epic      => 75,
            CardRarity.Legendary => 100,
            _                    => 25,
        };

        private int CalculateGoldReward()
        {
            var nodeType = RunPersistence.Instance?.CurrentNode?.type ?? NodeType.Combat;
            return nodeType switch
            {
                NodeType.Elite => Random.Range(35, 51),
                NodeType.Boss  => Random.Range(60, 81),
                _              => Random.Range(20, 31),
            };
        }

        private void OnDefeat()
        {
            gameOver = true;
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX("sfx_defeat");
            Log("=== DÉFAITE ===");
            ShowResultOverlay(won: false);
        }

        private void ShowRelicReward()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null || relicRewardPool == null || relicRewardPool.Count == 0)
            {
                ShowResultOverlay(won: true);
                return;
            }

            // Tirer une relique aléatoire non encore possédée
            var owned = RunPersistence.Instance?.PlayerRelics;
            var available = relicRewardPool.FindAll(r => owned == null || !owned.Contains(r));
            if (available.Count == 0) available = relicRewardPool; // fallback

            var relic = available[Random.Range(0, available.Count)];

            var overlayGO = new GameObject("RelicRewardOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvas.transform, false);
            var rt = overlayGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            overlayGO.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.96f);
            overlayGO.transform.SetAsLastSibling();

            // Or gagné
            MakeOverlayTMP(overlayGO, "Gold", 0.10f, 0.76f, 0.90f, 0.84f,
                $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or",
                18f, new Color(0.95f, 0.82f, 0.35f));

            // Titre
            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.84f, 0.90f, 0.96f,
                "Récompense de relique !", 42f, new Color(0.95f, 0.82f, 0.35f), FontStyles.Bold);

            // Panneau relique
            var panel = new GameObject("RelicPanel", typeof(RectTransform));
            panel.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(panel, 0.30f, 0.35f, 0.70f, 0.74f);
            panel.AddComponent<Image>().color = new Color(0.40f, 0.24f, 0.06f);

            MakeOverlayTMP(panel, "Name", 0.05f, 0.65f, 0.95f, 0.90f,
                relic.relicName, 22f, Color.white, FontStyles.Bold);
            MakeOverlayTMP(panel, "Effect", 0.05f, 0.35f, 0.95f, 0.65f,
                GetRelicEffectText(relic), 14f, new Color(0.95f, 0.82f, 0.35f));
            MakeOverlayTMP(panel, "Desc", 0.05f, 0.05f, 0.95f, 0.35f,
                relic.description, 12f, new Color(0.82f, 0.80f, 0.74f));

            // Bouton prendre
            var btnGO = new GameObject("TakeBtn", typeof(RectTransform));
            btnGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(btnGO, 0.32f, 0.20f, 0.68f, 0.32f);
            btnGO.AddComponent<Image>().color = new Color(0.40f, 0.24f, 0.06f);
            var btn = btnGO.AddComponent<Button>();
            MakeOverlayTMP(btnGO, "Label", 0f, 0f, 1f, 1f,
                $"Prendre {relic.relicName}", 20f, Color.white, FontStyles.Bold, raycast: false);

            var capturedRelic   = relic;
            var capturedOverlay = overlayGO;
            btn.onClick.AddListener(() =>
            {
                RunPersistence.Instance?.AddRelic(capturedRelic);
                Log($"✦ Relique obtenue : {capturedRelic.relicName}");
                Destroy(capturedOverlay);
                ShowResultOverlay(won: true);
            });
        }

        private string GetRelicEffectText(RelicData relic) => relic.effect switch
        {
            RelicEffect.DrawExtraCardPerTurn => $"Pioche +{relic.effectValue} carte(s) par tour",
            RelicEffect.StartWithBonusMana   => $"Commence chaque combat avec +{relic.effectValue} mana",
            RelicEffect.MaxHPBonus           => $"+{relic.effectValue} HP max",
            RelicEffect.HealAfterCombat      => $"Récupère {relic.effectValue} HP après chaque victoire",
            _                                => "",
        };

        private static void MakeOverlayTMP(GameObject parent, string name,
            float xMin, float yMin, float xMax, float yMax,
            string text, float size, Color color,
            FontStyles style = FontStyles.Normal, bool raycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin); rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.fontStyle = style; tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = raycast;
        }

        private void ShowRewardScreen()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null || rewardCardPool == null || rewardCardPool.Count == 0)
            {
                ShowResultOverlay(won: true);
                return;
            }

            // Tirer aléatoirement jusqu'à RewardCount cartes sans doublon
            // Exclure les cartes dont le joueur possède déjà ≥ 3 copies dans le deck
            var deck = RunPersistence.Instance?.PlayerDeck;
            var pool = rewardCardPool.FindAll(c =>
            {
                if (c == null) return false;
                if (deck == null) return true;
                int copies = 0;
                foreach (var d in deck) if (d == c) copies++;
                return copies < 3;
            });
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            var options = pool.GetRange(0, Mathf.Min(RewardCount, pool.Count));

            // Fond semi-transparent
            var overlayGO = new GameObject("RewardOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvas.transform, false);
            var overlayRT = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
            overlayGO.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.96f);
            overlayGO.transform.SetAsLastSibling();

            // Titre
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(titleGO, 0.10f, 0.84f, 0.90f, 0.96f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text      = "Choisissez une carte de récompense";
            titleTMP.fontSize  = 38f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color     = new Color(0.95f, 0.82f, 0.35f);

            // Or gagné
            var goldGO = new GameObject("Gold", typeof(RectTransform));
            goldGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(goldGO, 0.10f, 0.76f, 0.90f, 0.84f);
            var goldTMP = goldGO.AddComponent<TextMeshProUGUI>();
            goldTMP.text      = $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or";
            goldTMP.fontSize  = 18f;
            goldTMP.alignment = TextAlignmentOptions.Center;
            goldTMP.color     = new Color(0.95f, 0.82f, 0.35f);

            // Cartes
            float cardW   = 0.22f;
            float gap     = 0.05f;
            float total   = options.Count * cardW + (options.Count - 1) * gap;
            float startX  = 0.5f - total / 2f;

            for (int i = 0; i < options.Count; i++)
            {
                var card = options[i];
                float xMin = startX + i * (cardW + gap);
                float xMax = xMin + cardW;

                // Panneau carte — système 4 couches (template)
                var cardGO = new GameObject($"RewardCard_{i}", typeof(RectTransform));
                cardGO.transform.SetParent(overlayGO.transform, false);
                SetOverlayAnchors(cardGO, xMin, 0.22f, xMax, 0.80f);
                cardGO.AddComponent<Image>();
                var cardBtn = cardGO.AddComponent<Button>();
                CardUIBuilder.ApplyTemplate(card, cardGO);

                var capturedCard    = card;
                var capturedOverlay = overlayGO;
                cardBtn.onClick.AddListener(() =>
                {
                    RunPersistence.Instance?.AddCardToDeck(capturedCard);
                    Log($"✦ {capturedCard.cardName} ajoutée à votre deck !");
                    Destroy(capturedOverlay);
                    ShowResultOverlay(won: true);
                });

                // Bouton "Vendre pour X or"
                int sellPrice = GetSellPrice(card.rarity);
                var sellGO = new GameObject($"SellBtn_{i}", typeof(RectTransform));
                sellGO.transform.SetParent(overlayGO.transform, false);
                SetOverlayAnchors(sellGO, xMin, 0.13f, xMax, 0.21f);
                sellGO.AddComponent<Image>().color = new Color(0.28f, 0.18f, 0.10f);
                var sellBtn = sellGO.AddComponent<Button>();
                var sellLblGO = new GameObject("Label", typeof(RectTransform));
                sellLblGO.transform.SetParent(sellGO.transform, false);
                SetOverlayAnchors(sellLblGO, 0f, 0f, 1f, 1f);
                var sellTMP = sellLblGO.AddComponent<TextMeshProUGUI>();
                sellTMP.text          = $"Vendre : {sellPrice} or";
                sellTMP.fontSize      = 14f;
                sellTMP.fontStyle     = FontStyles.Bold;
                sellTMP.alignment     = TextAlignmentOptions.Center;
                sellTMP.color         = new Color(0.95f, 0.82f, 0.35f);
                sellTMP.raycastTarget = false;
                int capturedPrice = sellPrice;
                sellBtn.onClick.AddListener(() =>
                {
                    RunPersistence.Instance?.AddGold(capturedPrice);
                    Log($"✦ {capturedCard.cardName} vendue pour {capturedPrice} or.");
                    Destroy(capturedOverlay);
                    ShowResultOverlay(won: true);
                });
            }

            // Bouton "Passer"
            var skipGO = new GameObject("SkipBtn", typeof(RectTransform));
            skipGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(skipGO, 0.37f, 0.03f, 0.63f, 0.11f);
            skipGO.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.26f);
            var skipBtn = skipGO.AddComponent<Button>();
            var skipLabelGO = new GameObject("Label", typeof(RectTransform));
            skipLabelGO.transform.SetParent(skipGO.transform, false);
            SetOverlayAnchors(skipLabelGO, 0f, 0f, 1f, 1f);
            var skipTMP = skipLabelGO.AddComponent<TextMeshProUGUI>();
            skipTMP.text          = "Passer (aucune carte)";
            skipTMP.fontSize      = 15f;
            skipTMP.alignment     = TextAlignmentOptions.Center;
            skipTMP.color         = new Color(0.70f, 0.70f, 0.70f);
            skipTMP.raycastTarget = false;

            var capturedOverlaySkip = overlayGO;
            skipBtn.onClick.AddListener(() =>
            {
                Destroy(capturedOverlaySkip);
                ShowResultOverlay(won: true);
            });
        }

        private void ShowResultOverlay(bool won)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            var overlayGO = new GameObject("ResultOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(canvas.transform, false);
            var overlayRT = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
            overlayGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);
            overlayGO.transform.SetAsLastSibling();

            // Titre
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(titleGO, 0.10f, 0.58f, 0.90f, 0.80f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text      = won ? "VICTOIRE !" : "DÉFAITE";
            titleTMP.fontSize  = 64f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color     = won ? new Color(0.30f, 1.00f, 0.40f) : new Color(1.00f, 0.28f, 0.28f);

            // Victoire de boss = fin de run ; victoire normale = retour à la carte
            bool isBossVictory = won &&
                (RunPersistence.Instance?.CurrentNode?.type == NodeType.Boss);

            // Sous-titre
            var subGO = new GameObject("Sub", typeof(RectTransform));
            subGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(subGO, 0.15f, 0.44f, 0.85f, 0.57f);
            var subTMP = subGO.AddComponent<TextMeshProUGUI>();
            subTMP.text = isBossVictory ? "Vous avez triomphé du boss — la run est terminée !"
                        : won           ? "Tous les boards ennemis ont été vaincus !"
                                        : "Vos points de vie sont tombés à zéro.";
            subTMP.fontSize  = 20f;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.color     = new Color(0.82f, 0.80f, 0.75f);

            // Bouton retour
            var btnGO = new GameObject("ReturnBtn", typeof(RectTransform));
            btnGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(btnGO, 0.30f, 0.28f, 0.70f, 0.42f);
            btnGO.AddComponent<Image>().color = won ? new Color(0.12f, 0.40f, 0.18f)
                                                    : new Color(0.35f, 0.10f, 0.10f);
            var btn = btnGO.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(btnGO.transform, false);
            SetOverlayAnchors(labelGO, 0f, 0f, 1f, 1f);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text          = isBossVictory ? "Terminer la run"
                                   : won           ? "Retour à la carte"
                                                   : "Menu principal";
            labelTMP.fontSize      = 22f;
            labelTMP.fontStyle     = FontStyles.Bold;
            labelTMP.alignment     = TextAlignmentOptions.Center;
            labelTMP.color         = Color.white;
            labelTMP.raycastTarget = false;

            bool capturedWon       = won;
            bool capturedBossVict  = isBossVictory;
            btn.onClick.AddListener(() =>
            {
                if (capturedWon && !capturedBossVict)
                    SceneManager.LoadScene("RunMap");
                else
                {
                    // Défaite OU victoire boss : fin de run → XP puis menu
                    RunPersistence.Instance?.AwardRunXPAndReset();
                    SceneManager.LoadScene("MainMenu");
                }
            });
        }

        private static void SetOverlayAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // ── Unit Passives ─────────────────────────────────────────────────────

        private static readonly List<UnitPassive> _emptyPassives = new List<UnitPassive>();
        private static List<UnitPassive> GetPassives(CardInstance unit) =>
            unit?.data?.unitPassives ?? _emptyPassives;

        private int GetAuraDmgReduction(Board board, bool forPlayerSide)
        {
            int reduction = 0;
            var lanes = forPlayerSide ? board.playerLanes : board.enemyLanes;
            foreach (var lane in lanes)
            {
                if (lane == null || !lane.IsOccupied) continue;
                foreach (var p in GetPassives(lane.Occupant))
                    if (p.passiveType == UnitPassiveType.AuraAlliesReduceDmg)
                        reduction = Mathf.Max(reduction, p.value);
            }
            return reduction;
        }

        private void TriggerOnEntryPassives(CardInstance unit, Lane placedLane, Board board)
        {
            bool isPlayer     = unit.isPlayerCard;
            var  alliesLanes  = isPlayer ? board.playerLanes : board.enemyLanes;
            var  enemiesLanes = isPlayer ? board.enemyLanes  : board.playerLanes;

            // Apply existing opponent auras to this newly placed unit
            foreach (var opLane in enemiesLanes)
            {
                if (opLane == null || !opLane.IsOccupied) continue;
                foreach (var ap in GetPassives(opLane.Occupant))
                {
                    if (ap.passiveType == UnitPassiveType.AuraAlliesReduceDmg)
                    {
                        unit.bonusAttack -= ap.value;
                        Log($"  → [{ap.keyword}] {unit.data.cardName} subit -{ap.value} ATK (aura de {opLane.Occupant.data.cardName})");
                    }
                }
            }

            if (unit.data.unitPassives == null || unit.data.unitPassives.Count == 0) return;

            foreach (var p in unit.data.unitPassives)
            {
                switch (p.passiveType)
                {
                    case UnitPassiveType.DrawOnEntry:
                        if (isPlayer)
                        {
                            playerDeck.DrawCards(p.value);
                            Log($"  → [{p.keyword}] {unit.data.cardName} : vous piochez {p.value} carte(s)");
                        }
                        break;

                    case UnitPassiveType.AoEOnEntry:
                        foreach (var lane in enemiesLanes)
                        {
                            if (lane == null || !lane.IsOccupied) continue;
                            string eName = lane.Occupant.data.cardName;
                            lane.Occupant.currentHP -= p.value;
                            if (!lane.Occupant.IsAlive) lane.ClearCard();
                        }
                        Log($"  → [{p.keyword}] {unit.data.cardName} inflige {p.value} à toutes les unités adverses !");
                        break;

                    case UnitPassiveType.DamageOnEntry:
                        // Handled by PlayChargeEntryAnim coroutine (animation + damage sequenced)
                        break;

                    case UnitPassiveType.ATKDebuffRandomOnEntry:
                    {
                        var alive = new List<CardInstance>();
                        foreach (var lane in enemiesLanes)
                            if (lane != null && lane.IsOccupied) alive.Add(lane.Occupant);
                        if (alive.Count > 0)
                        {
                            var t = alive[Random.Range(0, alive.Count)];
                            t.bonusAttack -= p.value;
                            Log($"  → [{p.keyword}] {unit.data.cardName} réduit l'ATK de {t.data.cardName} de {p.value} ({t.CurrentAttack} ATK)");
                        }
                        break;
                    }

                    case UnitPassiveType.BuffAlliesOnEntry:
                        foreach (var lane in alliesLanes)
                        {
                            if (lane == null || !lane.IsOccupied || lane.Occupant == unit) continue;
                            lane.Occupant.bonusAttack += p.value;
                            Log($"  → [{p.keyword}] {lane.Occupant.data.cardName} gagne +{p.value} ATK");
                        }
                        break;

                    case UnitPassiveType.LegionBonusATK:
                    {
                        bool hasAlly = false;
                        foreach (var lane in alliesLanes)
                            if (lane != null && lane.IsOccupied && lane.Occupant != unit) { hasAlly = true; break; }
                        if (hasAlly)
                        {
                            unit.bonusAttack += p.value;
                            Log($"  → [{p.keyword}] {unit.data.cardName} gagne +{p.value} ATK ({unit.CurrentAttack} ATK)");
                        }
                        break;
                    }

                    case UnitPassiveType.AuraAlliesReduceDmg:
                        foreach (var lane in enemiesLanes)
                        {
                            if (lane == null || !lane.IsOccupied) continue;
                            lane.Occupant.bonusAttack -= p.value;
                            Log($"  → [{p.keyword}] {lane.Occupant.data.cardName} perd {p.value} ATK effectif");
                        }
                        break;
                }
            }
        }

        public void TriggerOnDeathPassives(CardInstance dead, Lane attackerLane, Board board, bool isPlayerAttacking)
        {
            if (dead.isPlayerCard)
                AddGear(dead);

            if (dead.data.unitPassives == null || dead.data.unitPassives.Count == 0) return;

            foreach (var p in dead.data.unitPassives)
            {
                switch (p.passiveType)
                {
                    case UnitPassiveType.ThornsOnDeath:
                        if (attackerLane != null && attackerLane.IsOccupied)
                        {
                            var t = attackerLane.Occupant;
                            t.currentHP -= p.value;
                            Log($"  → [{p.keyword}] {dead.data.cardName} inflige {p.value} à {t.data.cardName} en mourant !");
                            if (!t.IsAlive) attackerLane.ClearCard();
                        }
                        break;

                    case UnitPassiveType.ATKDebuffOnDeath:
                        if (attackerLane != null && attackerLane.IsOccupied)
                        {
                            attackerLane.Occupant.bonusAttack -= p.value;
                            Log($"  → [{p.keyword}] {dead.data.cardName} réduit l'ATK de {attackerLane.Occupant.data.cardName} de {p.value}");
                        }
                        break;

                    case UnitPassiveType.DamageHeroOnDeath:
                        // isPlayerAttacking=true → enemy died → punish player hero
                        // isPlayerAttacking=false → player died → punish enemy hero
                        if (isPlayerAttacking)
                        {
                            DamagePlayer(p.value);
                            Log($"  → [{p.keyword}] {dead.data.cardName} vous inflige {p.value} en mourant ! (HP: {playerHP}/{playerMaxHP})");
                        }
                        else
                        {
                            board.TakeDamage(p.value);
                            Log($"  → [{p.keyword}] {dead.data.cardName} inflige {p.value} au héros ennemi en mourant !");
                        }
                        break;

                    case UnitPassiveType.AoEOnDeath:
                        // Target the side opposite to the dead unit
                        // enemy died (isPlayerAttacking=true) → revenge on player lanes
                        // player died (isPlayerAttacking=false) → nuke enemy lanes
                        var targetLanes = isPlayerAttacking ? board.playerLanes : board.enemyLanes;
                        int aoeKills = 0;
                        foreach (var lane in targetLanes)
                        {
                            if (lane == null || !lane.IsOccupied) continue;
                            lane.Occupant.currentHP -= p.value;
                            if (!lane.Occupant.IsAlive) { lane.ClearCard(); aoeKills++; }
                        }
                        Log($"  → [{p.keyword}] {dead.data.cardName} inflige {p.value} à toutes les unités adverses en mourant !" +
                            (aoeKills > 0 ? $" ({aoeKills} détruites)" : ""));
                        break;

                    case UnitPassiveType.AuraAlliesReduceDmg:
                    {
                        // Aura-giver died → reverse the ATK penalty on opponents
                        // enemy aura died (isPlayerAttacking=true) → player units get ATK back
                        // player aura died (isPlayerAttacking=false) → enemy units get ATK back
                        var affectedLanes = isPlayerAttacking ? board.playerLanes : board.enemyLanes;
                        foreach (var lane in affectedLanes)
                        {
                            if (lane == null || !lane.IsOccupied) continue;
                            lane.Occupant.bonusAttack += p.value;
                        }
                        Log($"  → [{p.keyword}] L'aura de {dead.data.cardName} disparaît (+{p.value} ATK aux unités adverses)");
                        break;
                    }
                }
            }
        }

        private void TriggerStartOfTurnPassives()
        {
            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                foreach (var lane in board.playerLanes)
                {
                    if (lane == null || !lane.IsOccupied) continue;
                    foreach (var p in GetPassives(lane.Occupant))
                    {
                        if (p.passiveType != UnitPassiveType.AoEStartOfTurn) continue;
                        int hits = 0;
                        foreach (var eLane in board.enemyLanes)
                        {
                            if (eLane == null || !eLane.IsOccupied) continue;
                            eLane.Occupant.currentHP -= p.value;
                            hits++;
                            if (!eLane.Occupant.IsAlive) eLane.ClearCard();
                        }
                        if (hits > 0)
                            Log($"  → [{p.keyword}] {lane.Occupant.data.cardName} inflige {p.value} à {hits} unité(s) ennemie(s)");
                    }
                }
            }
        }

        private void TriggerEndOfRoundPassives()
        {
            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                foreach (var lane in board.playerLanes)
                {
                    if (lane == null || !lane.IsOccupied) continue;
                    var unit = lane.Occupant;
                    foreach (var p in GetPassives(unit))
                    {
                        switch (p.passiveType)
                        {
                            case UnitPassiveType.HealHeroIfAlive:
                                if (unit.survivedAttackThisTurn)
                                {
                                    playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                                    Log($"  → [{p.keyword}] {unit.data.cardName} : +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                                }
                                break;

                            case UnitPassiveType.HealHeroIfEnemyWeak:
                            {
                                bool hasWeak = false;
                                foreach (var eLane in board.enemyLanes)
                                    if (eLane != null && eLane.IsOccupied && eLane.Occupant.CurrentAttack <= 0) { hasWeak = true; break; }
                                if (hasWeak)
                                {
                                    playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                                    Log($"  → [{p.keyword}] {unit.data.cardName} : +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                                }
                                break;
                            }
                        }
                    }
                    unit.survivedAttackThisTurn = false;
                }
            }

            // Poison tick — tous les boards, toutes les unités ennemies
            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                for (int li = 0; li < board.enemyLanes.Length; li++)
                {
                    var lane = board.enemyLanes[li];
                    if (lane == null || !lane.IsOccupied) continue;
                    var unit = lane.Occupant;
                    if (unit.poisonStacks <= 0) continue;

                    unit.currentHP -= unit.poisonStacks;
                    Log($"  🧪 Poison : {unit.data.cardName} subit {unit.poisonStacks} dégât(s) ({unit.currentHP} HP restants)");

                    if (!unit.IsAlive)
                    {
                        var dead = unit;
                        lane.ClearCard();
                        Log($"  → {dead.data.cardName} est détruit par le poison !");
                        TriggerOnDeathPassives(dead, null, board, isPlayerAttacking: true);
                    }
                }
            }
        }

        private void ResetSurvivedAttackFlags()
        {
            foreach (var board in boardManager.boards)
                foreach (var lane in board.playerLanes)
                    if (lane != null && lane.IsOccupied) lane.Occupant.survivedAttackThisTurn = false;
        }

        private static int LaneAttackBonus(Lane lane)     => lane?.laneIndex == 1 ? 1 : 0;
        private static int LaneDefenseReduction(Lane lane) => (lane != null && lane.laneIndex != 1) ? 1 : 0;

        private static string BuildPositionalLog(Lane atk, Lane def)
        {
            int bonus = LaneAttackBonus(atk) - LaneDefenseReduction(def);
            return bonus > 0 ? $"+{bonus}(pos)" : bonus < 0 ? $"{bonus}(pos)" : "";
        }

        private static bool HasDamageOnEntry(CardInstance unit)
        {
            if (unit.data.unitPassives == null) return false;
            foreach (var p in unit.data.unitPassives)
                if (p.passiveType == UnitPassiveType.DamageOnEntry) return true;
            return false;
        }

        private IEnumerator PlayChargeEntryAnim(CardInstance unit, Lane placedLane, Board board)
        {
            yield return null; // attendre que RefreshAllUI ait créé le PlayedCardUI

            var slot = FindSlotForLane(placedLane);
            if (combatAnimator != null && slot != null)
                yield return StartCoroutine(combatAnimator.PlayAttackAnim(slot, unit.isPlayerCard));

            bool isPlayer     = unit.isPlayerCard;
            var  enemiesLanes = isPlayer ? board.enemyLanes : board.playerLanes;

            foreach (var p in GetPassives(unit))
            {
                if (p.passiveType != UnitPassiveType.DamageOnEntry) continue;

                int idx = placedLane.laneIndex;
                if (idx >= enemiesLanes.Length || enemiesLanes[idx] == null) continue;

                var oppLane = enemiesLanes[idx];
                if (oppLane.IsOccupied)
                {
                    var    target  = oppLane.Occupant;
                    string tName   = target.data.cardName;
                    var    defSlot = FindSlotForLane(oppLane);

                    int chargeDmg = Mathf.Max(0, p.value + LaneAttackBonus(placedLane) - LaneDefenseReduction(oppLane));
                    target.currentHP -= chargeDmg;
                    if (defSlot != null && chargeDmg > 0)
                        RoguelikeTCG.UI.DamagePopup.ShowDamage((RectTransform)defSlot.transform, chargeDmg);
                    Log($"  → [{p.keyword}] {unit.data.cardName} inflige {chargeDmg} à {tName} ({target.currentHP} HP restants)");
                    RefreshAllUI();

                    if (!target.IsAlive)
                    {
                        if (combatAnimator != null && defSlot != null)
                            yield return StartCoroutine(combatAnimator.PlayDeathAnim(defSlot));
                        oppLane.ClearCard();
                        Log($"  → {tName} est détruit !");
                        TriggerOnDeathPassives(target, placedLane, board, isPlayer);
                    }
                    else
                    {
                        if (combatAnimator != null && defSlot != null)
                            yield return StartCoroutine(combatAnimator.PlayHitFlash(defSlot));
                    }
                }
                else
                {
                    if (isPlayer)
                    {
                        board.TakeDamage(p.value);
                        Log($"  → [{p.keyword}] {unit.data.cardName} inflige {p.value} au héros ennemi !");
                    }
                    else
                    {
                        DamagePlayer(p.value);
                        Log($"  → [{p.keyword}] {unit.data.cardName} inflige {p.value} à votre héros !");
                    }
                }
                RefreshAllUI();
            }
        }

        private void ResetPlacedThisTurnFlags()
        {
            foreach (var board in boardManager.boards)
            {
                foreach (var lane in board.playerLanes)
                    if (lane != null && lane.IsOccupied) lane.Occupant.placedThisTurn = false;
                foreach (var lane in board.enemyLanes)
                    if (lane != null && lane.IsOccupied) lane.Occupant.placedThisTurn = false;
            }
        }

        // ── UI Refresh ────────────────────────────────────────────────────────

        public void RefreshAllUI()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.maxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count, playerShieldHP);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);

                for (int i = 0; i < boardManager.boards.Count; i++)
                {
                    var b = boardManager.boards[i];
                    if (!b.isActive) { combatUI.ClearEnemyBoard(i); continue; }
                    combatUI.RefreshEnemyBoard(i, b.enemyCurrentHP, b.enemyMaxHP, b.HasDangerousEnemyUnit());
                }
            }

            if (handView != null)
                handView.RefreshHand(
                    playerDeck.Hand,
                    IsDeVinciRun() ? bricolageCardData : null,
                    gearCount, gearCumulatedATK, gearCumulatedHP);

            if (allLaneSlots != null)
                foreach (var slot in allLaneSlots) slot?.Refresh();
        }

        // ── Log ───────────────────────────────────────────────────────────────

        private void Log(string message)
        {
            if (combatLog != null) combatLog.AddEntry(message);
            Debug.Log(message);
        }
    }
}
