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

        [Header("Lanes")]
        public CombatLane[]  lanes;
        public GameObject[]  laneRowUIs;

        [Header("Systems")]
        public DeckManager  playerDeck;
        public DeckManager  enemyDeck;
        public ManaManager  manaManager;
        public TurnManager  turnManager;
        public EnemyAI      enemyAI;
        public CombatLog    combatLog;

        [Header("UI")]
        public CombatUI     combatUI;
        public HandView     handView;
        public HeroPortraitUI playerPortrait;
        public HeroPortraitUI enemyPortrait;

        [Header("Animations")]
        public CombatAnimator combatAnimator;
        public RectTransform  endTurnButtonRT;

        [Header("Characters")]
        public CharacterData playerCharacter;
        public CharacterData enemyCharacter;

        [Header("Enemy HP")]
        public int enemyMaxHP     = 30;
        public int enemyCurrentHP;

        [Header("Rewards")]
        public List<CardData>  rewardCardPool;
        public List<RelicData> relicRewardPool;
        private const int RewardCount = 3;

        [Header("Bricolage (De Vinci)")]
        public CardData bricolageCardData;

        // ── Player state ──────────────────────────────────────────────────────
        public int playerHP;
        public int playerMaxHP;
        public int playerShieldHP;

        // ── Bricolage state ───────────────────────────────────────────────────
        private readonly List<CardInstance> bricolageQueue = new();
        public int BricolageDeadCount => bricolageQueue.Count;
        public bool CanUseBricolage   => BricolageDeadCount >= 2 && IsDeVinciRun();

        // ── Internal ──────────────────────────────────────────────────────────
        private bool gameOver;
        private LaneSlotUI[] allSlots;
        private int _lastGoldEarned;

        // ─────────────────────────────────────────────────────────────────────
        // LIFECYCLE
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            allSlots = FindObjectsOfType<LaneSlotUI>(true);
            ConfigureFromNodeType();
            InitializeCombat();
        }

        private void ConfigureFromNodeType()
        {
            var nodeType = RunPersistence.Instance?.CurrentNode?.type;

            // Lane count: Normal/Elite=2, Mini-boss/Boss=3
            // null = pas de RunPersistence (CombatSandbox) → 3 lanes pour les tests
            int laneCount = nodeType switch
            {
                null          => 3,
                NodeType.Boss => 3,
                _             => 2,
            };

            // Activate only the required lanes (logic + UI rows)
            for (int i = 0; i < lanes.Length; i++)
            {
                bool active = i < laneCount;
                if (lanes[i] != null) lanes[i].gameObject.SetActive(active);
                if (laneRowUIs != null && i < laneRowUIs.Length && laneRowUIs[i] != null)
                    laneRowUIs[i].SetActive(active);
            }

            // Center active lanes vertically inside LanesArea
            // Max 3 lanes — each lane takes 1/3 of LanesArea height
            const float laneH = 1f / 3f;
            float padY = (1f - laneCount * laneH) / 2f;
            for (int i = 0; i < laneCount; i++)
            {
                if (laneRowUIs == null || i >= laneRowUIs.Length || laneRowUIs[i] == null) continue;
                var rt = laneRowUIs[i].GetComponent<RectTransform>();
                if (rt == null) continue;
                float maxY = 1f - padY - i * laneH;
                float minY = maxY - laneH;
                rt.anchorMin        = new Vector2(rt.anchorMin.x, minY);
                rt.anchorMax        = new Vector2(rt.anchorMax.x, maxY);
                rt.sizeDelta        = new Vector2(rt.sizeDelta.x, 0f);
                rt.anchoredPosition = Vector2.zero;
            }

            enemyMaxHP = nodeType switch
            {
                NodeType.Elite => 45,
                NodeType.Boss  => 70,
                _              => 30,
            };
            enemyCurrentHP = enemyMaxHP;
        }

        private void InitializeCombat()
        {
            SessionLogger.Instance?.StartSession();
            gameOver      = false;
            playerShieldHP = 0;
            bricolageQueue.Clear();

            var persistence = RunPersistence.Instance;
            if (persistence?.SelectedCharacter != null)
                playerCharacter = persistence.SelectedCharacter;

            // Reward pool
            if (persistence?.EffectiveCardPool?.Count > 0)
                rewardCardPool = new List<CardData>(persistence.EffectiveCardPool);
            else if (playerCharacter?.cardPool?.Count > 0)
                rewardCardPool = new List<CardData>(playerCharacter.cardPool);

            // Player HP
            if (persistence != null && persistence.PlayerHP > 0)
            {
                playerMaxHP = persistence.PlayerMaxHP;
                playerHP    = persistence.PlayerHP;
            }
            else
            {
                playerMaxHP = playerCharacter?.maxHP ?? 80;
                playerHP    = playerMaxHP;
            }

            // Decks
            var savedDeck = persistence?.PlayerDeck;
            if (savedDeck?.Count > 0)
                playerDeck.InitializeDeck(savedDeck, true);
            else
            {
                var start = playerCharacter?.startingDeck ?? new List<CardData>();
                playerDeck.InitializeDeck(start, true);
                if (persistence != null) persistence.PlayerDeck = new List<CardData>(start);
            }

            if (enemyCharacter != null)
                enemyDeck.InitializeDeck(enemyCharacter.startingDeck, false);
            else
                enemyDeck.InitializeDeck(new List<CardData>(), false);

            // Portraits
            if (playerCharacter?.portrait != null) playerPortrait?.SetPortrait(playerCharacter.portrait);
            if (enemyCharacter?.portrait  != null) enemyPortrait?.SetPortrait(enemyCharacter.portrait);

            // Mana
            manaManager.Initialize();
            int bonusMana = RelicManager.Instance?.GetBonusStartMana() ?? 0;

            // Enemy AI init
            enemyAI.Initialize(enemyDeck, lanes, manaManager);

            // First turn
            AudioManager.Instance.PlayMusic("music_combat");
            Log("⚔ Combat commencé !");

            // Coin flip: player or enemy goes first
            bool playerFirst = Random.value >= 0.5f;
            Log(playerFirst ? "Pile — vous commencez !" : "Face — l'ennemi commence !");

            if (playerFirst)
            {
                turnManager.StartPlayerTurn();
                manaManager.PlayerTurnRegen();
                if (bonusMana > 0) manaManager.AddBonus(bonusMana);
                int prevCount = playerDeck.Hand.Count;
                playerDeck.DrawCards(playerDeck.initialDraw + (RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0));
                int drawn = playerDeck.Hand.Count - prevCount;
                AudioManager.Instance.PlaySFX("sfx_card_draw");
                StartCoroutine(DrawAndRefresh(playerDeck.Hand, drawn));
            }
            else
            {
                // Enemy goes first: draw and play, then hand off to player
                StartCoroutine(EnemyFirstTurn(bonusMana));
                RefreshAllUI();
            }
        }

        private IEnumerator EnemyFirstTurn(int bonusMana)
        {
            turnManager.StartEnemyTurn();
            manaManager.EnemyTurnRegen();
            enemyAI.PlayTurn();
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(AdvanceEnemyUnits());

            ResetPlacedFlags(isPlayer: false);
            TriggerEndOfRoundPassives();

            turnManager.StartPlayerTurn();
            manaManager.PlayerTurnRegen();
            if (bonusMana > 0) manaManager.AddBonus(bonusMana);
            int prevCountEFT = playerDeck.Hand.Count;
            playerDeck.DrawCards(playerDeck.initialDraw + (RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0));
            int drawnEFT = playerDeck.Hand.Count - prevCountEFT;
            AudioManager.Instance.PlaySFX("sfx_card_draw");
            yield return StartCoroutine(DrawAndRefresh(playerDeck.Hand, drawnEFT));
        }

        // ─────────────────────────────────────────────────────────────────────
        // CARD PLAY — PLAYER
        // ─────────────────────────────────────────────────────────────────────

        public bool TryPlayUnit(CardInstance card, CombatLane lane, int cellIndex)
        {
            if (!CanPlay()) return false;
            if (!card.IsUnit) return false;
            if (lane == null || cellIndex != CombatLane.PLAYER_DEPLOY_CELL) return false;
            if (lane.IsOccupied(cellIndex)) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;

            manaManager.Spend(card.data.manaCost);
            card.placedThisTurn = !HasPassive(card, UnitPassiveType.ChargeNoSickness);
            lane.PlaceUnit(card, cellIndex);
            playerDeck.RemoveFromHand(card);  // stays on board until death or traverse

            turnManager.RegisterCardPlayed();
            Log($"> Vous posez {card.data.cardName} en lane {lane.laneIndex} case {cellIndex} ({card.CurrentAttack}/{card.currentHP})");
            TriggerOnEntryPassives(card, lane, cellIndex);
            RefreshAllUI();
            return true;
        }

        public bool TryPlaySpellOnHero(CardInstance card, bool isPlayerHero)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} sur {(isPlayerHero ? "votre héros" : "le héros ennemi")}");
            ApplyAllEffects(card, null, -1, isPlayerHero);
            if (IsCurieRun()) ApplyContamination();
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            RefreshAllUI();
            return true;
        }

        public bool TryPlaySpellOnUnit(CardInstance card, CombatLane lane, int cellIndex)
        {
            if (!CanCastSpell(card)) return false;
            if (lane == null || !lane.IsOccupied(cellIndex)) return false;

            string tName = lane.GetUnit(cellIndex).data.cardName;
            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} sur {tName}");
            ApplyAllEffects(card, lane, cellIndex, false);
            if (IsCurieRun()) ApplyContamination();
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            RefreshAllUI();
            return true;
        }

        public bool TryPlayAoESpell(CardInstance card)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} — AoE !");
            ApplyAllEffects(card, null, -1, false);
            if (IsCurieRun()) ApplyContamination();
            playerDeck.PlayCard(card);
            turnManager.RegisterCardPlayed();
            AudioManager.Instance.PlaySFX("sfx_spell_aoe");
            RefreshAllUI();
            return true;
        }

        public void TryPlayBricolage(CombatLane lane, int cellIndex)
        {
            if (!CanPlay()) return;
            if (!CanUseBricolage) return;
            if (lane == null || cellIndex != CombatLane.PLAYER_DEPLOY_CELL) return;
            if (lane.IsOccupied(cellIndex)) return;

            int totalATK = 0;
            foreach (var u in bricolageQueue) totalATK += u.data.attackPower;
            int atk = Mathf.CeilToInt(totalATK / 2f);

            var auto = new CardInstance(bricolageCardData, isPlayerCard: true);
            auto.bonusAttack    = atk - bricolageCardData.attackPower;
            auto.currentHP      = 2;
            auto.placedThisTurn = true;

            lane.PlaceUnit(auto, cellIndex);
            turnManager.RegisterCardPlayed();
            Log($"> Bricolage ! Automate Réparé ({auto.CurrentAttack}/2) invoqué !");
            AudioManager.Instance.PlaySFX("sfx_card_place");

            bricolageQueue.Clear();
            TriggerOnEntryPassives(auto, lane, cellIndex);
            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // CARD PLAY — ENEMY (called by EnemyAI)
        // ─────────────────────────────────────────────────────────────────────

        public void EnemyPlaceUnit(CardInstance card, CombatLane lane, int cellIndex)
        {
            if (lane.IsOccupied(cellIndex)) return;

            card.placedThisTurn = !HasPassive(card, UnitPassiveType.ChargeNoSickness);
            lane.PlaceUnit(card, cellIndex);
            enemyDeck.RemoveFromHand(card);
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi pose {card.data.cardName} en lane {lane.laneIndex} case {cellIndex}");
            TriggerOnEntryPassivesEnemy(card, lane, cellIndex);
            RefreshAllUI();
        }

        public void EnemyCastSpellOnUnit(CardInstance card, CombatLane lane, int cellIndex)
        {
            if (!lane.IsOccupied(cellIndex)) return;
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi lance {card.data.cardName}");
            ApplyAllEffectsEnemy(card, lane, cellIndex);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        public void EnemyCastSpell(CardInstance card)
        {
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi lance {card.data.cardName}");
            ApplyAllEffectsEnemy(card, null, -1);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // TURN FLOW
        // ─────────────────────────────────────────────────────────────────────

        public void EndPlayerTurn()
        {
            if (gameOver || !turnManager.IsPlayerTurn) return;
            if (combatAnimator != null && combatAnimator.IsAnimating) return;
            AudioManager.Instance.PlaySFX("sfx_end_turn");
            Log("--- Fin de votre tour ---");
            turnManager.EndPlayerTurn();
            StartCoroutine(ResolveTurn());
        }

        private IEnumerator ResolveTurn()
        {
            // Start-of-resolution passives (Irradiation)
            TriggerStartOfTurnPassives(isPlayerPassives: true);
            RefreshAllUI();

            // Advance player units
            Log("[ Avancée des unités joueur ]");
            yield return StartCoroutine(AdvancePlayerUnits());

            if (enemyCurrentHP <= 0) { OnVictory(); yield break; }
            if (playerHP <= 0)       { OnDefeat();  yield break; }

            // Enemy turn
            Log("--- Tour ennemi ---");
            turnManager.StartEnemyTurn();
            manaManager.EnemyTurnRegen();
            enemyAI.PlayTurn();
            RefreshAllUI();
            yield return new WaitForSeconds(0.35f);

            TriggerStartOfTurnPassives(isPlayerPassives: false);
            RefreshAllUI();

            Log("[ Avancée des unités ennemies ]");
            yield return StartCoroutine(AdvanceEnemyUnits());

            if (playerHP <= 0) { OnDefeat(); yield break; }

            // End-of-round passives (Résilience, poison, Synthèse)
            TriggerEndOfRoundPassives();

            // Reset flags
            ResetPlacedFlags(isPlayer: true);
            ResetPlacedFlags(isPlayer: false);
            ResetSurvivedClashFlags();

            Log("--- Votre tour ---");
            turnManager.StartPlayerTurn();
            manaManager.PlayerTurnRegen();
            int extraDraw  = RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0;
            int prevCountRT = playerDeck.Hand.Count;
            playerDeck.DrawCards(playerDeck.drawPerTurn + extraDraw);
            int drawnRT = playerDeck.Hand.Count - prevCountRT;
            AudioManager.Instance.PlaySFX("sfx_card_draw");
            yield return StartCoroutine(DrawAndRefresh(playerDeck.Hand, drawnRT));
        }

        // ─────────────────────────────────────────────────────────────────────
        // ADVANCEMENT
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator AdvancePlayerUnits()
        {
            foreach (var lane in lanes)
            {
                if (lane == null || !lane.gameObject.activeSelf) continue;

                // Process from right to left so we don't double-advance a unit we just moved
                for (int c = CombatLane.LANE_LENGTH - 1; c >= 0; c--)
                {
                    var unit = lane.GetUnit(c);
                    if (unit == null || !unit.isPlayerCard) continue;
                    if (unit.placedThisTurn && !HasPassive(unit, UnitPassiveType.ChargeNoSickness)) continue;
                    if (unit.slowed) { unit.slowed = false; Log($"  {unit.data.cardName} est ralenti — ne peut pas avancer."); continue; }

                    int steps = HasPassive(unit, UnitPassiveType.FastAdvance) ? 2 : 1;
                    int currentCell = c;

                    for (int step = 0; step < steps; step++)
                    {
                        int nextCell = currentCell + 1;

                        if (nextCell >= CombatLane.LANE_LENGTH)
                        {
                            // TRAVERSE
                            yield return StartCoroutine(HandleTraverse(lane, currentCell, unit, isPlayer: true));
                            goto nextUnit;
                        }

                        var opponent = lane.GetUnit(nextCell);
                        if (opponent != null && !opponent.isPlayerCard)
                        {
                            // CLASH
                            yield return StartCoroutine(HandleClash(lane, currentCell, nextCell, unit, opponent, playerAttacking: true));
                            goto nextUnit;
                        }

                        // Advance one cell
                        var fromSlot = FindSlot(lane, currentCell);
                        var toSlot   = FindSlot(lane, nextCell);
                        if (combatAnimator != null && fromSlot != null && toSlot != null)
                            yield return StartCoroutine(combatAnimator.PlayAdvanceAnim(fromSlot, toSlot, isPlayer: true));

                        lane.ClearCell(currentCell);
                        lane.PlaceUnit(unit, nextCell);
                        currentCell = nextCell;
                        RefreshAllUI();
                    }

                    nextUnit:;
                }
            }
        }

        private IEnumerator AdvanceEnemyUnits()
        {
            foreach (var lane in lanes)
            {
                if (lane == null || !lane.gameObject.activeSelf) continue;

                // Process from left to right so we don't double-advance
                for (int c = 0; c < CombatLane.LANE_LENGTH; c++)
                {
                    var unit = lane.GetUnit(c);
                    if (unit == null || unit.isPlayerCard) continue;
                    if (unit.placedThisTurn && !HasPassive(unit, UnitPassiveType.ChargeNoSickness)) continue;
                    if (unit.slowed) { unit.slowed = false; Log($"  {unit.data.cardName} est ralenti — ne peut pas avancer."); continue; }

                    int steps = HasPassive(unit, UnitPassiveType.FastAdvance) ? 2 : 1;
                    int currentCell = c;

                    for (int step = 0; step < steps; step++)
                    {
                        int nextCell = currentCell - 1;

                        if (nextCell < 0)
                        {
                            yield return StartCoroutine(HandleTraverse(lane, currentCell, unit, isPlayer: false));
                            goto nextUnit;
                        }

                        var opponent = lane.GetUnit(nextCell);
                        if (opponent != null && opponent.isPlayerCard)
                        {
                            yield return StartCoroutine(HandleClash(lane, nextCell, currentCell, opponent, unit, playerAttacking: false));
                            goto nextUnit;
                        }

                        var fromSlot = FindSlot(lane, currentCell);
                        var toSlot   = FindSlot(lane, nextCell);
                        if (combatAnimator != null && fromSlot != null && toSlot != null)
                            yield return StartCoroutine(combatAnimator.PlayAdvanceAnim(fromSlot, toSlot, isPlayer: false));

                        lane.ClearCell(currentCell);
                        lane.PlaceUnit(unit, nextCell);
                        currentCell = nextCell;
                        RefreshAllUI();
                    }

                    nextUnit:;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLASH
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator HandleClash(CombatLane lane, int playerCell, int enemyCell,
                                        CardInstance player, CardInstance enemy, bool playerAttacking)
        {
            var pSlot = FindSlot(lane, playerCell);
            var eSlot = FindSlot(lane, enemyCell);

            // ── Calculate mutual damage BEFORE the animation so we can show it ─
            int pDmg = player.CurrentAttack;
            int eDmg = enemy.CurrentAttack;

            // Blindage (DmgReduction)
            foreach (var p in GetPassives(enemy))
                if (p.passiveType == UnitPassiveType.DmgReduction)
                    pDmg = Mathf.Max(0, pDmg - p.value);
            foreach (var p in GetPassives(player))
                if (p.passiveType == UnitPassiveType.DmgReduction)
                    eDmg = Mathf.Max(0, eDmg - p.value);

            // Shield absorption (preview — shields are consumed after anim)
            int pDmgPreview = pDmg;
            int eDmgPreview = eDmg;
            if (enemy.shieldHP  > 0) pDmgPreview = Mathf.Max(0, pDmg - enemy.shieldHP);
            if (player.shieldHP > 0) eDmgPreview = Mathf.Max(0, eDmg - player.shieldHP);

            bool playerWillDie = player.currentHP - eDmgPreview <= 0;
            bool enemyWillDie  = enemy.currentHP  - pDmgPreview <= 0;

            // Clash animation — passes damage values so popups appear at impact
            if (combatAnimator != null && pSlot != null && eSlot != null)
                yield return StartCoroutine(combatAnimator.PlayClashAnim(
                    pSlot, eSlot,
                    dmgToPlayer: eDmgPreview, dmgToEnemy: pDmgPreview,
                    playerDies: playerWillDie, enemyDies: enemyWillDie));

            // ── Apply shields (actual consumption post-anim) ───────────────────
            if (enemy.shieldHP > 0)
            {
                int abs = Mathf.Min(enemy.shieldHP, pDmg);
                enemy.shieldHP -= abs; pDmg -= abs;
                if (abs > 0) Log($"  Bouclier de {enemy.data.cardName} absorbe {abs}");
            }
            if (player.shieldHP > 0)
            {
                int abs = Mathf.Min(player.shieldHP, eDmg);
                player.shieldHP -= abs; eDmg -= abs;
                if (abs > 0) Log($"  Bouclier de {player.data.cardName} absorbe {abs}");
            }

            // BonusDmgIfEnemyWeakened (Exploiter) — applied before damage dealt
            foreach (var p in GetPassives(player))
                if (p.passiveType == UnitPassiveType.BonusDmgIfEnemyWeakened && enemy.CurrentAttack <= 0)
                {
                    DamageEnemy(p.value);
                    Log($"  [{p.keyword}] +{p.value} dégâts directs (ennemi affaibli)");
                }
            foreach (var p in GetPassives(enemy))
                if (p.passiveType == UnitPassiveType.BonusDmgIfEnemyWeakened && player.CurrentAttack <= 0)
                {
                    DamagePlayer(p.value);
                    Log($"  [{p.keyword}] +{p.value} dégâts directs (joueur affaibli)");
                }

            // Apply simultaneous damage
            player.currentHP -= eDmg;
            enemy.currentHP  -= pDmg;

            Log($"> Clash : {player.data.cardName} ({player.CurrentAttack} ATK) vs {enemy.data.cardName} ({enemy.CurrentAttack} ATK)");
            Log($"  {enemy.data.cardName} prend {pDmg} dmg → {enemy.currentHP} HP | {player.data.cardName} prend {eDmg} dmg → {player.currentHP} HP");

            // Damage popups are now shown inside PlayClashAnim at the impact frame
            // (no duplicate ShowDamage calls here)

            RefreshAllUI();

            bool playerDied = !player.IsAlive;
            bool enemyDied  = !enemy.IsAlive;

            // Percée (ExcessDamageBreakthrough)
            if (enemyDied)
            {
                int excess = -enemy.currentHP;
                foreach (var p in GetPassives(player))
                    if (p.passiveType == UnitPassiveType.ExcessDamageBreakthrough && excess > 0)
                    {
                        DamageEnemy(excess);
                        Log($"  [{p.keyword}] {excess} dégâts excédentaires au héros ennemi !");
                    }
            }
            if (playerDied)
            {
                int excess = -player.currentHP;
                foreach (var p in GetPassives(enemy))
                    if (p.passiveType == UnitPassiveType.ExcessDamageBreakthrough && excess > 0)
                    {
                        DamagePlayer(excess);
                        Log($"  [{p.keyword}] {excess} dégâts excédentaires à votre héros !");
                    }
            }

            // Conquête (LifestealOnKill)
            if (enemyDied)
                foreach (var p in GetPassives(player))
                    if (p.passiveType == UnitPassiveType.LifestealOnKill)
                    {
                        playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                        Log($"  [{p.keyword}] +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                    }
            if (playerDied)
                foreach (var p in GetPassives(enemy))
                    if (p.passiveType == UnitPassiveType.LifestealOnKill)
                    {
                        // Enemy heals its hero (tracked as enemy HP gain)
                        // Not in current design — skip for now
                    }

            // Death animations & cleanup
            if (enemyDied)
            {
                if (combatAnimator != null && eSlot != null)
                    yield return StartCoroutine(combatAnimator.PlayDeathAnim(eSlot));
                lane.ClearCell(enemyCell);
                SendToCemetery(enemy, isPlayerUnit: false);
                Log($"> {enemy.data.cardName} est détruit !");
                TriggerOnDeathPassives(enemy, player, killerIsPlayer: true, lane: lane);
            }
            if (playerDied)
            {
                if (combatAnimator != null && pSlot != null)
                    yield return StartCoroutine(combatAnimator.PlayDeathAnim(pSlot));
                lane.ClearCell(playerCell);
                SendToCemetery(player, isPlayerUnit: true);
                Log($"> {player.data.cardName} est détruit !");
                TriggerOnDeathPassives(player, enemy, killerIsPlayer: false, lane: lane);
            }

            // VenomOnClash — apply poison if the unit dealt damage and the target survived
            if (!enemyDied && pDmg > 0)
                foreach (var p in GetPassives(player))
                    if (p.passiveType == UnitPassiveType.VenomOnClash && p.value > 0)
                    {
                        enemy.poisonStacks += p.value;
                        Log($"  [{p.keyword}] {player.data.cardName} : +{p.value} poison sur {enemy.data.cardName} ({enemy.poisonStacks} 🧪)");
                    }
            if (!playerDied && eDmg > 0)
                foreach (var p in GetPassives(enemy))
                    if (p.passiveType == UnitPassiveType.VenomOnClash && p.value > 0)
                    {
                        player.poisonStacks += p.value;
                        Log($"  [{p.keyword}] {enemy.data.cardName} : +{p.value} poison sur {player.data.cardName} ({player.poisonStacks} 🧪)");
                    }

            // Mark survival for Résilience
            if (!playerDied) { player.survivedClashThisTurn = true; player.hadClashThisTurn = true; }
            if (!enemyDied)  { enemy.survivedClashThisTurn  = true; enemy.hadClashThisTurn  = true; }

            if (combatAnimator != null)
            {
                if (!playerDied && pSlot != null)
                    yield return StartCoroutine(combatAnimator.PlayHitFlash(pSlot));
                if (!enemyDied && eSlot != null)
                    yield return StartCoroutine(combatAnimator.PlayHitFlash(eSlot));
            }

            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // TRAVERSE
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator HandleTraverse(CombatLane lane, int cell, CardInstance unit, bool isPlayer)
        {
            int dmg = unit.CurrentAttack;

            // Vigilance — double damage if unit never clashed during its advance this turn
            if (!unit.hadClashThisTurn && HasPassive(unit, UnitPassiveType.Vigilance))
            {
                dmg *= 2;
                Log($"  [Vigilance] {unit.data.cardName} traverse sans clash — dégâts ×2 !");
            }

            var slot = FindSlot(lane, cell);
            if (combatAnimator != null && slot != null)
                yield return StartCoroutine(combatAnimator.PlayAttackAnim(slot, isPlayer));

            lane.ClearCell(cell);

            if (isPlayer)
            {
                DamageEnemy(dmg);
                Log($"> {unit.data.cardName} traverse ! {dmg} dégâts directs au héros ennemi. ({enemyCurrentHP}/{enemyMaxHP})");
                SendToDiscard(unit, isPlayerUnit: true);
            }
            else
            {
                DamagePlayer(dmg);
                Log($"> {unit.data.cardName} traverse ! {dmg} dégâts directs à votre héros. ({playerHP}/{playerMaxHP})");
                SendToDiscard(unit, isPlayerUnit: false);
            }

            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSIVES — ENTRY
        // ─────────────────────────────────────────────────────────────────────

        private void TriggerOnEntryPassives(CardInstance unit, CombatLane lane, int cell)
        {
            if (unit.data.unitPassives == null) return;
            foreach (var p in unit.data.unitPassives)
                ApplyEntryPassive(p, unit, lane, cell, isPlayerUnit: true);
        }

        private void TriggerOnEntryPassivesEnemy(CardInstance unit, CombatLane lane, int cell)
        {
            if (unit.data.unitPassives == null) return;
            foreach (var p in unit.data.unitPassives)
                ApplyEntryPassive(p, unit, lane, cell, isPlayerUnit: false);
        }

        private void ApplyEntryPassive(UnitPassive p, CardInstance unit, CombatLane lane, int cell, bool isPlayerUnit)
        {
            switch (p.passiveType)
            {
                case UnitPassiveType.DrawOnEntry:
                    if (isPlayerUnit)
                    {
                        playerDeck.DrawCards(p.value);
                        Log($"  [{p.keyword}] {unit.data.cardName} : vous piochez {p.value} carte(s)");
                    }
                    break;

                case UnitPassiveType.ATKDebuffRandomOnEntry:
                {
                    var targets = isPlayerUnit ? GetAllEnemyUnits() : GetAllPlayerUnits();
                    if (targets.Count > 0)
                    {
                        var t = targets[Random.Range(0, targets.Count)];
                        t.bonusAttack -= p.value;
                        Log($"  [{p.keyword}] {unit.data.cardName} : -{p.value} ATK à {t.data.cardName} ({t.CurrentAttack} ATK)");
                    }
                    break;
                }

                case UnitPassiveType.BuffAlliesOnEntry:
                {
                    var allies = isPlayerUnit ? GetAllPlayerUnits() : GetAllEnemyUnits();
                    foreach (var a in allies)
                        if (a != unit) { a.bonusAttack += p.value; Log($"  [{p.keyword}] {a.data.cardName} +{p.value} ATK"); }
                    break;
                }

                case UnitPassiveType.LegionBonusATK:
                {
                    var allies = isPlayerUnit ? GetAllPlayerUnits() : GetAllEnemyUnits();
                    bool hasOther = false;
                    foreach (var a in allies) if (a != unit) { hasOther = true; break; }
                    if (hasOther) { unit.bonusAttack += p.value; Log($"  [{p.keyword}] {unit.data.cardName} +{p.value} ATK ({unit.CurrentAttack} ATK)"); }
                    break;
                }

                case UnitPassiveType.AoEAllLanesOnEntry:
                {
                    var enemies = isPlayerUnit ? GetAllEnemyUnits() : GetAllPlayerUnits();
                    int killed = 0;
                    foreach (var e in new List<CardInstance>(enemies))
                    {
                        e.currentHP -= p.value;
                        if (!e.IsAlive)
                        {
                            ClearUnitFromLanes(e);
                            SendToCemetery(e, e.isPlayerCard);
                            killed++;
                            TriggerOnDeathPassives(e, unit, isPlayerUnit, null);
                        }
                    }
                    Log($"  [{p.keyword}] {unit.data.cardName} inflige {p.value} à toutes les unités adverses ! ({killed} tuées)");
                    RefreshAllUI();
                    break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSIVES — DEATH
        // ─────────────────────────────────────────────────────────────────────

        private void TriggerOnDeathPassives(CardInstance dead, CardInstance killer, bool killerIsPlayer, CombatLane lane)
        {
            // Bricolage tracking
            if (dead.isPlayerCard && IsDeVinciRun())
                bricolageQueue.Add(dead);

            if (dead.data.unitPassives == null) return;

            foreach (var p in dead.data.unitPassives)
            {
                switch (p.passiveType)
                {
                    case UnitPassiveType.ThornsOnDeath:
                        if (killer != null && killer.IsAlive)
                        {
                            killer.currentHP -= p.value;
                            Log($"  [{p.keyword}] {dead.data.cardName} : {p.value} dmg à {killer.data.cardName}");
                            if (!killer.IsAlive)
                            {
                                ClearUnitFromLanes(killer);
                                SendToCemetery(killer, killerIsPlayer);
                            }
                        }
                        break;

                    case UnitPassiveType.ATKDebuffOnDeath:
                        // Debuff the unit in the adjacent enemy cell in the same lane
                        // "adjacent" = the closest opponent in this lane
                        if (lane != null)
                        {
                            var adj = FindClosestOpponent(lane, dead.cellPosition, dead.isPlayerCard);
                            if (adj != null)
                            {
                                adj.bonusAttack -= p.value;
                                Log($"  [{p.keyword}] {dead.data.cardName} : -{p.value} ATK à {adj.data.cardName}");
                            }
                        }
                        break;

                    case UnitPassiveType.DamageHeroOnDeath:
                        if (dead.isPlayerCard)
                        {
                            DamageEnemy(p.value);
                            Log($"  [{p.keyword}] {dead.data.cardName} : {p.value} dmg au héros ennemi en mourant");
                        }
                        else
                        {
                            DamagePlayer(p.value);
                            Log($"  [{p.keyword}] {dead.data.cardName} : {p.value} dmg à votre héros en mourant");
                        }
                        break;

                    case UnitPassiveType.AoEOnDeath:
                    {
                        var targets = dead.isPlayerCard ? GetAllEnemyUnits() : GetAllPlayerUnits();
                        int killed = 0;
                        foreach (var t in new List<CardInstance>(targets))
                        {
                            t.currentHP -= p.value;
                            if (!t.IsAlive)
                            {
                                ClearUnitFromLanes(t);
                                SendToCemetery(t, t.isPlayerCard);
                                killed++;
                                TriggerOnDeathPassives(t, dead, dead.isPlayerCard, lane);
                            }
                        }
                        Log($"  [{p.keyword}] {dead.data.cardName} inflige {p.value} à toutes les unités adverses ({killed} tuées)");
                        break;
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSIVES — START / END OF TURN
        // ─────────────────────────────────────────────────────────────────────

        private void TriggerStartOfTurnPassives(bool isPlayerPassives)
        {
            var units = isPlayerPassives ? GetAllPlayerUnits() : GetAllEnemyUnits();
            foreach (var unit in units)
            {
                foreach (var p in GetPassives(unit))
                {
                    if (p.passiveType != UnitPassiveType.AoEStartOfTurn) continue;
                    var targets = isPlayerPassives ? GetAllEnemyUnits() : GetAllPlayerUnits();
                    int killed = 0;
                    foreach (var t in new List<CardInstance>(targets))
                    {
                        t.currentHP -= p.value;
                        if (!t.IsAlive)
                        {
                            ClearUnitFromLanes(t);
                            SendToCemetery(t, t.isPlayerCard);
                            killed++;
                            TriggerOnDeathPassives(t, unit, isPlayerPassives, null);
                        }
                    }
                    if (targets.Count > 0)
                        Log($"  [{p.keyword}] {unit.data.cardName} : {p.value} dmg AoE ({killed} tuées)");
                }
            }
        }

        private void TriggerEndOfRoundPassives()
        {
            // Résilience (HealHeroIfAlive)
            foreach (var unit in GetAllPlayerUnits())
            {
                foreach (var p in GetPassives(unit))
                {
                    if (p.passiveType == UnitPassiveType.HealHeroIfAlive && unit.survivedClashThisTurn)
                    {
                        playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                        Log($"  [{p.keyword}] {unit.data.cardName} : +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                    }
                    if (p.passiveType == UnitPassiveType.HealHeroIfEnemyWeak)
                    {
                        bool hasWeak = false;
                        foreach (var e in GetAllEnemyUnits())
                            if (e.CurrentAttack <= 0) { hasWeak = true; break; }
                        if (hasWeak)
                        {
                            playerHP = Mathf.Min(playerMaxHP, playerHP + p.value);
                            Log($"  [{p.keyword}] {unit.data.cardName} : +{p.value} HP héros ({playerHP}/{playerMaxHP})");
                        }
                    }
                }
            }

            // Poison tick on ALL units (player and enemy)
            PoisonTick(GetAllEnemyUnits(), isPlayer: false);
            PoisonTick(GetAllPlayerUnits(), isPlayer: true);
        }

        private void PoisonTick(List<CardInstance> units, bool isPlayer)
        {
            foreach (var unit in new List<CardInstance>(units))
            {
                if (unit.poisonStacks <= 0) continue;
                unit.currentHP -= unit.poisonStacks;
                Log($"  🧪 Poison : {unit.data.cardName} subit {unit.poisonStacks} dmg ({unit.currentHP} HP)");
                if (!unit.IsAlive)
                {
                    ClearUnitFromLanes(unit);
                    SendToCemetery(unit, isPlayer);
                    Log($"  → {unit.data.cardName} est détruit par le poison !");
                    TriggerOnDeathPassives(unit, null, !isPlayer, null);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CONTAMINATION (Curie)
        // ─────────────────────────────────────────────────────────────────────

        private void ApplyContamination()
        {
            var enemies = GetAllEnemyUnits();
            if (enemies.Count > 0)
            {
                var target = enemies[Random.Range(0, enemies.Count)];
                target.poisonStacks++;
                Log($"  [Contamination] 1 poison sur {target.data.cardName} ({target.poisonStacks} 🧪)");
            }
            // If no enemy units: no effect (the card description says "ou héros ennemi si aucune unité"
            // but we don't track hero poison stacks yet — simplification for MVP)
        }

        // ─────────────────────────────────────────────────────────────────────
        // SPELL EFFECT APPLICATION
        // ─────────────────────────────────────────────────────────────────────

        private void ApplyAllEffects(CardInstance card, CombatLane targetLane, int targetCell, bool targetIsPlayerHero)
        {
            foreach (var eff in card.data.effects)
                ApplySingleEffect(eff, targetLane, targetCell, targetIsPlayerHero, playerCasting: true);
        }

        private void ApplyAllEffectsEnemy(CardInstance card, CombatLane targetLane, int targetCell)
        {
            bool targetIsEnemyHero = (card.data.spellTarget == SpellTarget.PlayerHero);
            foreach (var eff in card.data.effects)
                ApplySingleEffect(eff, targetLane, targetCell, targetIsEnemyHero, playerCasting: false);
        }

        private void ApplySingleEffect(CardEffect eff, CombatLane lane, int cell, bool heroIsPlayer, bool playerCasting)
        {
            switch (eff.target)
            {
                case SpellTarget.PlayerHero:
                    ApplyEffectToPlayerHero(eff, playerCasting);
                    break;

                case SpellTarget.EnemyHero:
                    ApplyEffectToEnemyHero(eff, playerCasting);
                    break;

                case SpellTarget.AllyUnit:
                case SpellTarget.EnemyUnit:
                    if (lane != null && cell >= 0 && lane.IsOccupied(cell))
                        ApplyEffectToUnit(eff, lane.GetUnit(cell), lane, cell, playerCasting);
                    break;

                case SpellTarget.AllEnemyUnits:
                {
                    var targets = playerCasting ? GetAllEnemyUnits() : GetAllPlayerUnits();
                    foreach (var t in new List<CardInstance>(targets))
                        ApplyEffectToUnit(eff, t, null, -1, playerCasting);
                    break;
                }

                case SpellTarget.AllAllyUnits:
                {
                    var targets = playerCasting ? GetAllPlayerUnits() : GetAllEnemyUnits();
                    foreach (var t in new List<CardInstance>(targets))
                        ApplyEffectToUnit(eff, t, null, -1, playerCasting);
                    break;
                }
            }
        }

        private void ApplyEffectToPlayerHero(CardEffect eff, bool playerCasting)
        {
            switch (eff.effectType)
            {
                case EffectType.Heal:
                    if (playerCasting) { playerHP = Mathf.Min(playerMaxHP, playerHP + eff.value); Log($"  → Soins : +{eff.value} HP ({playerHP}/{playerMaxHP})"); }
                    break;
                case EffectType.Damage:
                    if (playerCasting) DamagePlayer(eff.value);
                    else { DamageEnemy(eff.value); Log($"  → Ennemi inflige {eff.value} aux HP ennemis"); } // reversed
                    break;
                case EffectType.Shield:
                    if (playerCasting) { playerShieldHP += eff.value; Log($"  → Bouclier : +{eff.value} ({playerShieldHP} total)"); }
                    break;
                case EffectType.DrawCard:
                    if (playerCasting) { playerDeck.DrawCards(eff.value); Log($"  → Pioche {eff.value} carte(s)"); }
                    break;
                case EffectType.BuffAttack:
                    foreach (var u in GetAllPlayerUnits()) u.bonusAttack += eff.value;
                    Log($"  → {(eff.value >= 0 ? $"+{eff.value}" : $"{eff.value}")} ATK à toutes vos unités");
                    break;
            }
        }

        private void ApplyEffectToEnemyHero(CardEffect eff, bool playerCasting)
        {
            switch (eff.effectType)
            {
                case EffectType.Damage:
                    DamageEnemy(eff.value);
                    Log($"  → {eff.value} dmg au héros ennemi ({enemyCurrentHP}/{enemyMaxHP})");
                    break;
                case EffectType.BuffAttack:
                    foreach (var u in GetAllEnemyUnits()) u.bonusAttack += eff.value;
                    Log($"  → {(eff.value >= 0 ? $"+{eff.value}" : $"{eff.value}")} ATK à toutes les unités ennemies");
                    break;
            }
        }

        private void ApplyEffectToUnit(CardEffect eff, CardInstance target, CombatLane lane, int cell, bool playerCasting)
        {
            if (target == null || !target.IsAlive) return;

            var slotRT = (lane != null && cell >= 0) ? GetSlotRT(lane, cell) : null;

            switch (eff.effectType)
            {
                case EffectType.Damage:
                {
                    int dmg = eff.value;
                    if (target.shieldHP > 0) { int abs = Mathf.Min(target.shieldHP, dmg); target.shieldHP -= abs; dmg -= abs; }
                    target.currentHP -= dmg;
                    if (slotRT != null && dmg > 0) DamagePopup.ShowDamage(slotRT, dmg);
                    Log($"  → {eff.value} dmg à {target.data.cardName} ({target.currentHP} HP)");
                    if (!target.IsAlive)
                    {
                        ClearUnitFromLanes(target);
                        SendToCemetery(target, target.isPlayerCard);
                        Log($"  → {target.data.cardName} est détruit !");
                        TriggerOnDeathPassives(target, null, playerCasting, lane);
                    }
                    break;
                }
                case EffectType.Heal:
                {
                    int healed = Mathf.Min(target.data.maxHP - target.currentHP, eff.value);
                    target.currentHP += healed;
                    if (slotRT != null && healed > 0) DamagePopup.ShowHeal(slotRT, healed);
                    Log($"  → {target.data.cardName} récupère {healed} HP ({target.currentHP}/{target.data.maxHP})");
                    break;
                }
                case EffectType.BuffAttack:
                    target.bonusAttack += eff.value;
                    Log($"  → {target.data.cardName} {(eff.value >= 0 ? $"+{eff.value}" : $"{eff.value}")} ATK ({target.CurrentAttack} ATK)");
                    break;
                case EffectType.BuffHP:
                    target.data.maxHP   += eff.value;
                    target.currentHP    += eff.value;
                    Log($"  → {target.data.cardName} +{eff.value} HP max ({target.currentHP}/{target.data.maxHP})");
                    break;
                case EffectType.Shield:
                    target.shieldHP += eff.value;
                    if (slotRT != null) DamagePopup.ShowShield(slotRT, eff.value);
                    Log($"  → {target.data.cardName} +{eff.value} bouclier ({target.shieldHP} total)");
                    break;
                case EffectType.ApplyPoison:
                    target.poisonStacks += eff.value;
                    Log($"  → {target.data.cardName} empoisonné ({target.poisonStacks} 🧪)");
                    break;
                case EffectType.SlowUnit:
                    target.slowed = true;
                    Log($"  → {target.data.cardName} est ralenti (ne peut pas avancer ce tour)");
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DAMAGE
        // ─────────────────────────────────────────────────────────────────────

        public void DamagePlayer(int amount)
        {
            if (amount <= 0) return;
            if (playerShieldHP > 0)
            {
                int abs = Mathf.Min(playerShieldHP, amount);
                playerShieldHP -= abs; amount -= abs;
                if (abs > 0) Log($"  Bouclier héros absorbe {abs} ({playerShieldHP} restant)");
            }
            playerHP = Mathf.Max(0, playerHP - amount);
            if (amount > 0) Log($"  Votre héros : {playerHP}/{playerMaxHP} HP");
        }

        public void DamageEnemy(int amount)
        {
            if (amount <= 0) return;
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - amount);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CEMETERY / DISCARD
        // ─────────────────────────────────────────────────────────────────────

        private void SendToCemetery(CardInstance unit, bool isPlayerUnit)
        {
            if (isPlayerUnit) playerDeck.AddToCemetery(unit);
            else              enemyDeck.AddToCemetery(unit);
        }

        private void SendToDiscard(CardInstance unit, bool isPlayerUnit)
        {
            if (isPlayerUnit) playerDeck.AddToDiscard(unit);
            else              enemyDeck.AddToDiscard(unit);
        }

        // ─────────────────────────────────────────────────────────────────────
        // END STATES
        // ─────────────────────────────────────────────────────────────────────

        private void OnVictory()
        {
            gameOver = true;
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX("sfx_victory");
            Log("=== VICTOIRE ! ===");
            SessionLogger.Instance?.EndSession("VICTOIRE");

            int heal = RelicManager.Instance?.GetHealAfterCombat() ?? 0;
            if (heal > 0) { playerHP = Mathf.Min(playerMaxHP, playerHP + heal); Log($"  Relique : +{heal} HP"); }

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

        private void OnDefeat()
        {
            gameOver = true;
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX("sfx_defeat");
            Log("=== DÉFAITE ===");
            SessionLogger.Instance?.EndSession("DÉFAITE");
            ShowResultOverlay(won: false);
        }

        private int CalculateGoldReward()
        {
            var nt = RunPersistence.Instance?.CurrentNode?.type ?? NodeType.Combat;
            return nt switch
            {
                NodeType.Elite => Random.Range(35, 51),
                NodeType.Boss  => Random.Range(60, 81),
                _              => Random.Range(20, 31),
            };
        }

        private static int GetSellPrice(CardRarity r) => r switch
        {
            CardRarity.Common    => 25,
            CardRarity.Uncommon  => 37,
            CardRarity.Rare      => 50,
            CardRarity.Epic      => 75,
            CardRarity.Legendary => 100,
            _                    => 25,
        };

        // ─────────────────────────────────────────────────────────────────────
        // REWARD UI (kept from original — code creates overlay at runtime)
        // ─────────────────────────────────────────────────────────────────────

        private void ShowRelicReward()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null || relicRewardPool == null || relicRewardPool.Count == 0) { ShowResultOverlay(won: true); return; }

            var owned     = RunPersistence.Instance?.PlayerRelics;
            var available = relicRewardPool.FindAll(r => owned == null || !owned.Contains(r));
            if (available.Count == 0) available = relicRewardPool;

            var relic      = available[Random.Range(0, available.Count)];
            var overlayGO  = MakeFullOverlay(canvas, "RelicRewardOverlay", new Color(0.04f, 0.04f, 0.08f, 0.96f));

            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.84f, 0.90f, 0.96f, "Récompense de relique !", 42f, new Color(0.95f, 0.82f, 0.35f), FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Gold", 0.10f, 0.76f, 0.90f, 0.84f, $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or", 18f, new Color(0.95f, 0.82f, 0.35f));

            var panel = new GameObject("RelicPanel", typeof(RectTransform));
            panel.transform.SetParent(overlayGO.transform, false);
            SetAnchors(panel, 0.30f, 0.35f, 0.70f, 0.74f);
            panel.AddComponent<Image>().color = new Color(0.40f, 0.24f, 0.06f);
            MakeOverlayTMP(panel, "Name",   0.05f, 0.65f, 0.95f, 0.90f, relic.relicName,             22f, Color.white,                   FontStyles.Bold);
            MakeOverlayTMP(panel, "Effect", 0.05f, 0.35f, 0.95f, 0.65f, GetRelicEffectText(relic),   14f, new Color(0.95f, 0.82f, 0.35f));
            MakeOverlayTMP(panel, "Desc",   0.05f, 0.05f, 0.95f, 0.35f, relic.description,           12f, new Color(0.82f, 0.80f, 0.74f));

            var btn     = MakeButton(overlayGO, "TakeBtn", 0.32f, 0.20f, 0.68f, 0.32f, $"Prendre {relic.relicName}", new Color(0.40f, 0.24f, 0.06f));
            var capR    = relic;
            var capOver = overlayGO;
            btn.onClick.AddListener(() =>
            {
                RunPersistence.Instance?.AddRelic(capR);
                Log($"✦ Relique obtenue : {capR.relicName}");
                Destroy(capOver);
                ShowResultOverlay(won: true);
            });
        }

        private void ShowRewardScreen()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null || rewardCardPool == null || rewardCardPool.Count == 0) { ShowResultOverlay(won: true); return; }

            var deck = RunPersistence.Instance?.PlayerDeck;
            var pool = rewardCardPool.FindAll(c =>
            {
                if (c == null) return false;
                if (deck == null) return true;
                int n = 0; foreach (var d in deck) if (d == c) n++;
                return n < 3;
            });
            for (int i = pool.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            var options = pool.GetRange(0, Mathf.Min(RewardCount, pool.Count));

            var overlayGO = MakeFullOverlay(canvas, "RewardOverlay", new Color(0.04f, 0.04f, 0.08f, 0.96f));
            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.84f, 0.90f, 0.96f, "Choisissez une carte de récompense", 38f, new Color(0.95f, 0.82f, 0.35f), FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Gold",  0.10f, 0.76f, 0.90f, 0.84f, $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or", 18f, new Color(0.95f, 0.82f, 0.35f));

            float cardW  = 0.22f, gap = 0.05f, total = options.Count * cardW + (options.Count - 1) * gap;
            float startX = 0.5f - total / 2f;

            for (int i = 0; i < options.Count; i++)
            {
                var card = options[i];
                float xMin = startX + i * (cardW + gap), xMax = xMin + cardW;

                var cardGO = new GameObject($"RewardCard_{i}", typeof(RectTransform));
                cardGO.transform.SetParent(overlayGO.transform, false);
                SetAnchors(cardGO, xMin, 0.22f, xMax, 0.80f);
                cardGO.AddComponent<Image>();
                var cardBtn = cardGO.AddComponent<Button>();
                CardUIBuilder.ApplyTemplate(card, cardGO);

                var capCard = card; var capOver = overlayGO;
                cardBtn.onClick.AddListener(() =>
                {
                    RunPersistence.Instance?.AddCardToDeck(capCard);
                    Log($"✦ {capCard.cardName} ajoutée au deck !");
                    Destroy(capOver);
                    ShowResultOverlay(won: true);
                });

                int sellPrice = GetSellPrice(card.rarity);
                var sellBtn = MakeButton(overlayGO, $"SellBtn_{i}", xMin, 0.13f, xMax, 0.21f, $"Vendre : {sellPrice} or", new Color(0.28f, 0.18f, 0.10f));
                int capPrice = sellPrice;
                sellBtn.onClick.AddListener(() =>
                {
                    RunPersistence.Instance?.AddGold(capPrice);
                    Log($"✦ {capCard.cardName} vendue pour {capPrice} or.");
                    Destroy(capOver);
                    ShowResultOverlay(won: true);
                });
            }

            var skipBtn = MakeButton(overlayGO, "SkipBtn", 0.37f, 0.03f, 0.63f, 0.11f, "Passer (aucune carte)", new Color(0.22f, 0.22f, 0.26f));
            var capSkip = overlayGO;
            skipBtn.onClick.AddListener(() => { Destroy(capSkip); ShowResultOverlay(won: true); });
        }

        private void ShowResultOverlay(bool won)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            bool isBossVict = won && (RunPersistence.Instance?.CurrentNode?.type == NodeType.Boss);
            var overlayGO = MakeFullOverlay(canvas, "ResultOverlay", new Color(0f, 0f, 0f, 0.88f));

            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.58f, 0.90f, 0.80f, won ? "VICTOIRE !" : "DÉFAITE", 64f,
                won ? new Color(0.30f, 1.00f, 0.40f) : new Color(1.00f, 0.28f, 0.28f), FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Sub",   0.15f, 0.44f, 0.85f, 0.57f,
                isBossVict ? "Vous avez triomphé du boss — la run est terminée !"
                : won      ? "L'ennemi a été vaincu !"
                           : "Vos points de vie sont tombés à zéro.",
                20f, new Color(0.82f, 0.80f, 0.75f));

            Color btnColor = won ? new Color(0.12f, 0.40f, 0.18f) : new Color(0.35f, 0.10f, 0.10f);
            string btnLabel = isBossVict ? "Terminer la run" : won ? "Retour à la carte" : "Menu principal";
            var btn = MakeButton(overlayGO, "ReturnBtn", 0.30f, 0.28f, 0.70f, 0.42f, btnLabel, btnColor);
            bool capWon = won, capBoss = isBossVict;
            btn.onClick.AddListener(() =>
            {
                if (capWon && !capBoss) SceneManager.LoadScene("RunMap");
                else { RunPersistence.Instance?.AwardRunXPAndReset(); SceneManager.LoadScene("MainMenu"); }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // DRAW + REFRESH
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator DrawAndRefresh(List<CardInstance> hand, int drawnCount)
        {
            bool hasBricolage = IsDeVinciRun() && bricolageCardData != null;
            int  totalLayout  = hand.Count + (hasBricolage ? 1 : 0);

            if (drawnCount > 0 && combatAnimator != null
                && endTurnButtonRT != null && handView != null)
            {
                yield return StartCoroutine(combatAnimator.PlayDrawCardsAnim(
                    hand, drawnCount, totalLayout, endTurnButtonRT, handView));
            }

            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI REFRESH
        // ─────────────────────────────────────────────────────────────────────

        public void RefreshAllUI()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.MaxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count, playerShieldHP);
                combatUI.RefreshEnemyHP(enemyCurrentHP, enemyMaxHP);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);
                combatUI.RefreshCemetery(playerDeck.CemeteryCount, playerDeck.DiscardCount);
            }

            if (handView != null)
                handView.RefreshHand(playerDeck.Hand, IsDeVinciRun() ? bricolageCardData : null,
                    BricolageDeadCount, 0, 0);

            if (allSlots != null)
                foreach (var s in allSlots) s?.Refresh();
        }

        public void SaveBugReport() => SessionLogger.Instance?.SaveAsBugReport();

        public void SkipCombat() { if (!gameOver) OnVictory(); }

        private void Log(string msg)
        {
            combatLog?.AddEntry(msg);
            SessionLogger.Instance?.Write(msg);
            Debug.Log(msg);
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private bool CanPlay()
        {
            if (gameOver) return false;
            if (combatAnimator != null && combatAnimator.IsAnimating) return false;
            if (!turnManager.IsPlayerTurn || !turnManager.CanPlayCard) return false;
            return true;
        }

        private bool CanCastSpell(CardInstance card)
        {
            if (!CanPlay()) return false;
            if (card.IsUnit) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;
            return true;
        }

        public bool IsDeVinciRun() => playerCharacter?.characterName == "Léonard de Vinci";
        public bool IsCurieRun()   => playerCharacter?.characterName == "Marie Curie";

        private List<CardInstance> GetAllPlayerUnits()
        {
            var result = new List<CardInstance>();
            foreach (var lane in lanes)
            {
                if (lane == null || !lane.gameObject.activeSelf) continue;
                foreach (var (_, u) in lane.GetPlayerUnits()) result.Add(u);
            }
            return result;
        }

        private List<CardInstance> GetAllEnemyUnits()
        {
            var result = new List<CardInstance>();
            foreach (var lane in lanes)
            {
                if (lane == null || !lane.gameObject.activeSelf) continue;
                foreach (var (_, u) in lane.GetEnemyUnits()) result.Add(u);
            }
            return result;
        }

        private void ClearUnitFromLanes(CardInstance unit)
        {
            if (unit.laneIndex < 0 || unit.cellPosition < 0) return;
            if (unit.laneIndex < lanes.Length && lanes[unit.laneIndex] != null)
                lanes[unit.laneIndex].ClearCell(unit.cellPosition);
        }

        private CardInstance FindClosestOpponent(CombatLane lane, int fromCell, bool isPlayerUnit)
        {
            // For player units (moving right), closest opponent is the enemy unit nearest to them
            // For enemy units (moving left), same logic reversed
            int bestDist = int.MaxValue;
            CardInstance best = null;
            for (int c = 0; c < CombatLane.LANE_LENGTH; c++)
            {
                var u = lane.GetUnit(c);
                if (u == null || u.isPlayerCard == isPlayerUnit) continue;
                int dist = Mathf.Abs(c - fromCell);
                if (dist < bestDist) { bestDist = dist; best = u; }
            }
            return best;
        }

        private LaneSlotUI FindSlot(CombatLane lane, int cell)
        {
            if (allSlots == null) return null;
            foreach (var s in allSlots)
                if (s != null && s.lane == lane && s.cellIndex == cell) return s;
            return null;
        }

        private RectTransform GetSlotRT(CombatLane lane, int cell)
        {
            var s = FindSlot(lane, cell);
            return s != null ? (RectTransform)s.transform : null;
        }

        private void ResetPlacedFlags(bool isPlayer)
        {
            var units = isPlayer ? GetAllPlayerUnits() : GetAllEnemyUnits();
            foreach (var u in units) u.placedThisTurn = false;
        }

        private void ResetSurvivedClashFlags()
        {
            foreach (var u in GetAllPlayerUnits()) { u.survivedClashThisTurn = false; u.hadClashThisTurn = false; }
            foreach (var u in GetAllEnemyUnits())  { u.survivedClashThisTurn = false; u.hadClashThisTurn = false; }
        }

        private static readonly List<UnitPassive> _empty = new();
        private static List<UnitPassive> GetPassives(CardInstance u) => u?.data?.unitPassives ?? _empty;

        private static bool HasPassive(CardInstance u, UnitPassiveType type)
        {
            foreach (var p in GetPassives(u)) if (p.passiveType == type) return true;
            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // OVERLAY BUILDERS
        // ─────────────────────────────────────────────────────────────────────

        private static GameObject MakeFullOverlay(Canvas canvas, string name, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = bgColor;
            go.transform.SetAsLastSibling();
            return go;
        }

        private static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin); rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void MakeOverlayTMP(GameObject parent, string name,
            float xMin, float yMin, float xMax, float yMax,
            string text, float size, Color color,
            FontStyles style = FontStyles.Normal, bool raycast = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.fontStyle = style; tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = raycast;
        }

        private static Button MakeButton(GameObject parent, string name,
            float xMin, float yMin, float xMax, float yMax, string label, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);
            go.AddComponent<Image>().color = bg;
            var btn = go.AddComponent<Button>();
            MakeOverlayTMP(go, "Label", 0f, 0f, 1f, 1f, label, 20f, Color.white, FontStyles.Bold);
            return btn;
        }

        private static string GetRelicEffectText(RelicData r) => r.effect switch
        {
            RelicEffect.DrawExtraCardPerTurn => $"Pioche +{r.effectValue} carte(s) par tour",
            RelicEffect.StartWithBonusMana   => $"Commence chaque combat avec +{r.effectValue} mana",
            RelicEffect.MaxHPBonus           => $"+{r.effectValue} HP max",
            RelicEffect.HealAfterCombat      => $"Récupère {r.effectValue} HP après chaque victoire",
            _                                => "",
        };
    }
}
