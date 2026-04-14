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

        [Header("State")]
        public int playerHP;
        public int playerMaxHP;

        private bool gameOver;
        private LaneSlotUI[] allLaneSlots;
        private readonly HashSet<int> _clearedBoards = new HashSet<int>();
        private int _lastGoldEarned;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            allLaneSlots = FindObjectsOfType<LaneSlotUI>(true);
            InitializeCombat();
        }

        private void InitializeCombat()
        {
            gameOver = false;
            _clearedBoards.Clear();

            // Personnage : RunPersistence en priorité sur le champ sérialisé en scène
            var persistence = RunPersistence.Instance;
            if (persistence?.SelectedCharacter != null)
                playerCharacter = persistence.SelectedCharacter;

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

            // Relique de départ (premier combat de la run uniquement)
            if (persistence != null &&
                (persistence.PlayerRelics == null || persistence.PlayerRelics.Count == 0) &&
                playerCharacter?.startingRelic != null)
            {
                persistence.AddRelic(playerCharacter.startingRelic);
                // Récupérer l'HP mis à jour si MaxHPBonus
                playerMaxHP = persistence.PlayerMaxHP;
                playerHP    = persistence.PlayerHP > 0 ? persistence.PlayerHP : playerMaxHP;
            }

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
            if (!turnManager.IsPlayerTurn || !turnManager.CanPlayCard) return false;
            if (!card.IsUnit || lane.IsOccupied || !lane.isPlayerLane) return false;

            lane.PlaceCard(card);
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            Log($"> Vous posez {card.data.cardName} ({card.CurrentAttack}/{card.currentHP})");
            RefreshAllUI();
            return true;
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
                            playerHP = Mathf.Max(0, playerHP - effect.value);
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
                        Log($"  → +{effect.value} ATK à toutes vos unités");
                        break;
                    case EffectType.DrawCard:
                        playerDeck.DrawCards(effect.value);
                        Log($"  → Vous piochez {effect.value} carte(s)");
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
                            targetLane.ClearCard();
                            Log($"  → {target.data.cardName} est détruit !");
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
                        Log($"  → {target.data.cardName} gagne +{effect.value} ATK ({target.CurrentAttack} ATK)");
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
                    }
                }
            }
        }

        // ── Turn Flow ─────────────────────────────────────────────────────────

        public void EndPlayerTurn()
        {
            if (gameOver || !turnManager.IsPlayerTurn) return;
            AudioManager.Instance.PlaySFX("sfx_end_turn");
            Log("--- Fin de votre tour ---");
            turnManager.EndPlayerTurn();
            StartCoroutine(ResolveAndEnemyTurn());
        }

        private IEnumerator ResolveAndEnemyTurn()
        {
            Log("[ Résolution des attaques joueur ]");
            for (int i = 0; i < boardManager.boards.Count; i++)
            {
                var board = boardManager.boards[i];
                if (board.IsDefeated) continue;

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

            Log("--- Tour ennemi ---");
            turnManager.StartEnemyTurn();
            manaManager.RegenTurn();
            enemyAI.PlayTurn();
            RefreshAllUI();
            yield return new WaitForSeconds(0.5f);

            Log("[ Résolution des attaques ennemies ]");
            for (int i = 0; i < boardManager.boards.Count; i++)
            {
                var board = boardManager.boards[i];
                if (board.IsDefeated) continue;

                if (combatAnimator != null && i != boardManager.ActiveBoardIndex)
                    yield return StartCoroutine(combatAnimator.PlayBoardSlide(boardManager.ActiveBoardIndex, i));
                boardManager.SetActiveBoard(i);

                yield return StartCoroutine(ResolveBoard(board, playerAttacks: false));
                RefreshAllUI();
            }

            if (playerHP <= 0) { OnDefeat(); yield break; }

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
                    yield return StartCoroutine(AttackWithUnit(pLane, eLane, board, isPlayerAttacking: true));
                else
                    yield return StartCoroutine(AttackWithUnit(eLane, pLane, board, isPlayerAttacking: false));
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

                int dmg = attacker.CurrentAttack;
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
                Log($"> {attacker.data.cardName} attaque {defender.data.cardName} " +
                    $"({attacker.CurrentAttack} dmg → {defender.currentHP} HP restants)");

                RefreshAllUI();

                if (!defender.IsAlive)
                {
                    if (combatAnimator != null && defenderSlot != null)
                        yield return StartCoroutine(combatAnimator.PlayDeathAnim(defenderSlot));
                    defenderLane.ClearCard();
                    Log($"> {defender.data.cardName} est détruit !");
                }
                else if (combatAnimator != null && defenderSlot != null)
                {
                    yield return StartCoroutine(combatAnimator.PlayHitFlash(defenderSlot));
                }
            }
            else
            {
                if (isPlayerAttacking)
                {
                    board.TakeDamage(attacker.CurrentAttack);
                    Log($"> {attacker.data.cardName} inflige {attacker.CurrentAttack} dégâts au board ennemi " +
                        $"({board.enemyCurrentHP}/{board.enemyMaxHP} HP)");
                }
                else
                {
                    playerHP = Mathf.Max(0, playerHP - attacker.CurrentAttack);
                    Log($"> {attacker.data.cardName} vous inflige {attacker.CurrentAttack} dégâts ! " +
                        $"(HP: {playerHP}/{playerMaxHP})");
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
            if (nodeType == NodeType.Elite || nodeType == NodeType.Boss)
                ShowRelicReward();
            else
                ShowRewardScreen();
        }

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
            var pool = new List<CardData>(rewardCardPool);
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
            }

            // Bouton "Passer"
            var skipGO = new GameObject("SkipBtn", typeof(RectTransform));
            skipGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(skipGO, 0.37f, 0.07f, 0.63f, 0.17f);
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

            // Sous-titre
            var subGO = new GameObject("Sub", typeof(RectTransform));
            subGO.transform.SetParent(overlayGO.transform, false);
            SetOverlayAnchors(subGO, 0.15f, 0.44f, 0.85f, 0.57f);
            var subTMP = subGO.AddComponent<TextMeshProUGUI>();
            subTMP.text      = won ? "Tous les boards ennemis ont été vaincus !"
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
            labelTMP.text          = won ? "Retour à la carte" : "Menu principal";
            labelTMP.fontSize      = 22f;
            labelTMP.fontStyle     = FontStyles.Bold;
            labelTMP.alignment     = TextAlignmentOptions.Center;
            labelTMP.color         = Color.white;
            labelTMP.raycastTarget = false;

            bool capturedWon = won;
            btn.onClick.AddListener(() =>
            {
                if (capturedWon)
                    SceneManager.LoadScene("RunMap");
                else
                {
                    RunPersistence.Instance?.ResetRun();
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

        // ── UI Refresh ────────────────────────────────────────────────────────

        public void RefreshAllUI()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.maxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);

                for (int i = 0; i < boardManager.boards.Count; i++)
                {
                    var b = boardManager.boards[i];
                    combatUI.RefreshEnemyBoard(i, b.enemyCurrentHP, b.enemyMaxHP, b.HasDangerousEnemyUnit());
                }
            }

            if (handView != null)
                handView.RefreshHand(playerDeck.Hand);

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
