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

        [Header("Systèmes")]
        public GridManager  gridManager;
        public DeckManager  playerDeck;
        public DeckManager  enemyDeck;
        public ManaManager  manaManager;
        public TurnManager  turnManager;
        public EnemyAI      enemyAI;

        [Header("UI")]
        public CombatUI        combatUI;
        public HandView        handView;
        public HeroPortraitUI  playerPortrait;
        public HeroPortraitUI  enemyPortrait;
        public RelicBarUI      relicBar;
        public RectTransform   endTurnButtonRT;
        public RectTransform   deckZoneRT;
        public CombatAnimator  combatAnimator;

        [Header("Personnages")]
        public CharacterData playerCharacter;
        public CharacterData enemyCharacter;

        [Header("HP Ennemi (configuré selon node type)")]
        public int enemyMaxHP     = 20;
        public int enemyCurrentHP;

        [Header("Récompenses")]
        public List<CardData>  rewardCardPool;
        public List<RelicData> relicRewardPool;
        private const int RewardCount = 3;

        // ── État joueur ────────────────────────────────────────────────────────
        public int playerHP;
        public int playerMaxHP;

        // ── État de tour ───────────────────────────────────────────────────────
        private bool _gameOver;
        private bool _playerPlayedUnitThisTurn;
        private bool _isFirstPlayerTurn = true;
        private int  _lastGoldEarned;
        private int  _pendingATKBuff = 0; // buff "prochaine unité jouée"

        private GridCellUI[] _allCells;

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
            _allCells = FindObjectsByType<GridCellUI>(FindObjectsInactive.Include);

            if (deckZoneRT == null)
            {
                var go = GameObject.Find("CadreDeckAllié");
                if (go != null) deckZoneRT = go.GetComponent<RectTransform>();
            }

            ConfigureFromNodeType();
            InitializeCombat();
        }

        private void ConfigureFromNodeType()
        {
            var nodeType = RunPersistence.Instance?.CurrentNode?.type;
            enemyMaxHP = nodeType switch
            {
                NodeType.Elite => 35,
                NodeType.Boss  => 50,
                _              => 20,
            };
            enemyCurrentHP = enemyMaxHP;
        }

        private void InitializeCombat()
        {
            SessionLogger.Instance?.StartSession();
            _gameOver                 = false;
            _playerPlayedUnitThisTurn = false;
            _isFirstPlayerTurn        = true;

            var persistence = RunPersistence.Instance;
            if (persistence?.SelectedCharacter != null)
                playerCharacter = persistence.SelectedCharacter;

            // Pool récompenses
            if (persistence?.EffectiveCardPool?.Count > 0)
                rewardCardPool = new List<CardData>(persistence.EffectiveCardPool);
            else if (playerCharacter?.cardPool?.Count > 0)
                rewardCardPool = new List<CardData>(playerCharacter.cardPool);

            // HP joueur (30 HP global persistant)
            playerMaxHP = 30;
            if (persistence != null && persistence.PlayerHP > 0)
            {
                playerMaxHP = persistence.PlayerMaxHP > 0 ? persistence.PlayerMaxHP : 30;
                playerHP    = persistence.PlayerHP;
            }
            else
            {
                playerHP = playerMaxHP;
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

            // IA
            enemyAI.Initialize(enemyDeck, gridManager, manaManager);

            AudioManager.Instance.PlayMusic("music_combat");
            Log("Combat commencé !");

            StartPlayerTurn();
        }

        // ─────────────────────────────────────────────────────────────────────
        // TOUR JOUEUR
        // ─────────────────────────────────────────────────────────────────────

        private void StartPlayerTurn()
        {
            if (_gameOver) return;

            manaManager.OnPlayerTurnStart();

            int bonusMana = RelicManager.Instance?.GetBonusStartMana() ?? 0;
            if (bonusMana > 0) manaManager.AddBonus(bonusMana);

            turnManager.StartPlayerTurn();
            _playerPlayedUnitThisTurn = false;

            // Bonds passifs début de tour (GivreVivant)
            BondSystem.RefreshPassiveBonds(gridManager, Log);

            // Reset flags de dégâts de tour
            gridManager.ResetTurnFlags();

            int extraDraw  = RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0;
            int drawCount  = _isFirstPlayerTurn ? playerDeck.initialDraw : playerDeck.drawPerTurn;
            _isFirstPlayerTurn = false;
            int prevCount  = playerDeck.Hand.Count;
            playerDeck.DrawCards(drawCount + extraDraw);
            int drawn = playerDeck.Hand.Count - prevCount;

            AudioManager.Instance.PlaySFX("sfx_card_draw");
            var drawFrom = deckZoneRT != null ? deckZoneRT : endTurnButtonRT;
            bool playingDrawAnim = drawn > 0 && combatAnimator != null && drawFrom != null && handView != null;
            if (playingDrawAnim)
                StartCoroutine(combatAnimator.PlayDrawCardsAnim(
                    playerDeck.Hand, drawn,
                    playerDeck.Hand.Count,
                    drawFrom, handView));

            Log("--- Votre tour ---");
            if (playingDrawAnim)
                RefreshAllUIExceptHand();
            else
                RefreshAllUI();
        }

        /// <summary>Appelé par le bouton Fin de Tour.</summary>
        public void EndPlayerTurn()
        {
            if (_gameOver) return;
            if (!turnManager.IsPlayerTurn) return;
            if (combatAnimator != null && combatAnimator.IsAnimating) return;

            AudioManager.Instance.PlaySFX("sfx_end_turn");
            Log("--- Fin de votre tour ---");
            turnManager.EndPlayerTurn();
            StartCoroutine(ResolveAttacks());
        }

        // ─────────────────────────────────────────────────────────────────────
        // RÉSOLUTION DES ATTAQUES (cœur du nouveau système)
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ResolveAttacks()
        {
            // 1. Bonds passifs avant attaque
            BondSystem.RefreshPassiveBonds(gridManager, Log);

            // 2. Collecter les attaques ennemies AVANT que les unités ennemies meurent
            //    (on stocke les attaquants qui sont vivants au moment de la résolution)
            var pendingEnemyAttacks = new List<(CardInstance attacker, int col)>();
            for (int c = 0; c < GridManager.COLS; c++)
            {
                var enemy = gridManager.GetUnit(0, c);
                if (enemy != null && enemy.IsAlive && !enemy.isFrozen)
                    pendingEnemyAttacks.Add((enemy, c));
            }

            // 3. Attaques joueur col par col
            for (int c = 0; c < GridManager.COLS; c++)
            {
                var player = gridManager.GetUnit(1, c);
                if (player == null || !player.IsAlive) continue;

                yield return StartCoroutine(ExecutePlayerAttack(player, c));
                if (_gameOver) yield break;
            }

            // 4. Attaques ennemies — seulement celles qui étaient vivantes AVANT
            foreach (var (attacker, col) in pendingEnemyAttacks)
            {
                // L'ennemi a pu être tué par une attaque joueur — on ne réplique pas
                if (!attacker.IsAlive || !attacker.IsOnGrid) continue;

                yield return StartCoroutine(ExecuteEnemyAttack(attacker, col));
                if (_gameOver) yield break;
            }

            // 5. Lane leaks restantes (unités sans opposant)
            yield return StartCoroutine(ProcessLaneLeaks());
            if (_gameOver) yield break;

            // 6. Cleanup des morts résiduels
            CleanupDeadUnits();

            RefreshAllUI();

            // 7. Tour ennemi
            yield return StartCoroutine(EnemyTurnSequence());
        }

        // ─────────────────────────────────────────────────────────────────────
        // ATTAQUE D'UNE UNITÉ JOUEUR
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ExecutePlayerAttack(CardInstance attacker, int col)
        {
            var target = gridManager.GetUnit(0, col);

            // Calcul des bonds avec les voisins
            var neighbors  = gridManager.GetAdjacentSameRow(1, col);
            CardInstance leftNeighbor  = col > 0 ? gridManager.GetUnit(1, col - 1) : null;
            CardInstance rightNeighbor = col < GridManager.COLS - 1 ? gridManager.GetUnit(1, col + 1) : null;

            BondType leftBond  = leftNeighbor  != null ? BondSystem.GetBond(attacker.data.element, leftNeighbor.data.element)  : BondType.None;
            BondType rightBond = rightNeighbor != null ? BondSystem.GetBond(attacker.data.element, rightNeighbor.data.element) : BondType.None;

            // On prend le bond le plus fort (le premier trouvé gauche > droite)
            BondType activeBond = leftBond  != BondType.None ? leftBond :
                                  rightBond != BondType.None ? rightBond : BondType.None;
            CardInstance bondNeighbor = activeBond == leftBond ? leftNeighbor : rightNeighbor;

            if (target != null)
            {
                int dmg = attacker.currentATK;

                // Bonds d'attaque avant impact (FoudreNoire s'applique sur la cible avant les dégâts)
                if (activeBond != BondType.None && BondSystem.GetTiming(activeBond) == BondTiming.OnAttack)
                    BondSystem.ApplyAttackBond(activeBond, attacker, bondNeighbor, target, gridManager, ref dmg, Log);

                var attackerCell = GetCellUI(1, col);
                var defenderCell = GetCellUI(0, col);
                if (combatAnimator != null && attackerCell != null)
                    yield return StartCoroutine(combatAnimator.PlayAttackAnimGrid(attackerCell, defenderCell, dmg));

                bool killed = DamageUnit(target, 0, col, killedByPlayer: true, dmg, attacker);

                // Bond Cendres : si kill → Shadow adjacente +1 ATK
                if (killed && activeBond == BondType.Cendres)
                {
                    var shadowNeighbor = FindShadowNeighbor(attacker, 1, col);
                    if (shadowNeighbor != null)
                    {
                        shadowNeighbor.AddATKBonus(1);
                        Log($"  [Bond Cendres] {shadowNeighbor.data.cardName} gagne +1 ATK permanent");
                    }
                }

                // Bond EclairArdent : ricochet sur c±1
                if (activeBond == BondType.EclairArdent && !killed)
                {
                    yield return StartCoroutine(ApplyEclairArdentRicochet(attacker, 0, col));
                }

                // Bond Abîme : drain après les dégâts
                if (activeBond == BondType.Abime && target != null)
                    BondSystem.ApplyAttackBond(BondType.Abime, attacker, bondNeighbor, target, gridManager, ref dmg, Log);
            }
            else
            {
                // Pas de cible en face → lane leak : 1 dégât direct aux HP ennemis
                Log($"  Lane leak colonne {col} — joueur inflige 1 dégât direct");
                DamageEnemy(1);

                if (_gameOver) yield break;
            }

            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // ATTAQUE D'UNE UNITÉ ENNEMIE
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ExecuteEnemyAttack(CardInstance attacker, int col)
        {
            var target = gridManager.GetUnit(1, col);

            if (target != null)
            {
                int dmg = attacker.currentATK;

                // Bonds ennemis (mêmes règles, mais côté Row 0)
                CardInstance leftNeighbor  = col > 0 ? gridManager.GetUnit(0, col - 1) : null;
                CardInstance rightNeighbor = col < GridManager.COLS - 1 ? gridManager.GetUnit(0, col + 1) : null;

                BondType leftBond  = leftNeighbor  != null ? BondSystem.GetBond(attacker.data.element, leftNeighbor.data.element)  : BondType.None;
                BondType rightBond = rightNeighbor != null ? BondSystem.GetBond(attacker.data.element, rightNeighbor.data.element) : BondType.None;

                BondType activeBond = leftBond  != BondType.None ? leftBond :
                                      rightBond != BondType.None ? rightBond : BondType.None;
                CardInstance bondNeighbor = activeBond == leftBond ? leftNeighbor : rightNeighbor;

                if (activeBond != BondType.None && BondSystem.GetTiming(activeBond) == BondTiming.OnAttack)
                    BondSystem.ApplyAttackBond(activeBond, attacker, bondNeighbor, target, gridManager, ref dmg, Log);

                var attackerCell = GetCellUI(0, col);
                var defenderCell = GetCellUI(1, col);
                if (combatAnimator != null && attackerCell != null)
                    yield return StartCoroutine(combatAnimator.PlayAttackAnimGrid(attackerCell, defenderCell, dmg));

                bool killed = DamageUnit(target, 1, col, killedByPlayer: false, dmg, attacker);

                if (killed && activeBond == BondType.Cendres)
                {
                    var shadowNeighbor = FindShadowNeighbor(attacker, 0, col);
                    if (shadowNeighbor != null)
                    {
                        shadowNeighbor.AddATKBonus(1);
                        Log($"  [Bond Cendres] {shadowNeighbor.data.cardName} gagne +1 ATK permanent");
                    }
                }
            }
            else
            {
                // Lane leak ennemi : 1 dégât direct au joueur
                Log($"  Lane leak colonne {col} (ennemi) — 1 dégât direct au joueur");
                DamagePlayer(1);
                if (_gameOver) yield break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // LANE LEAKS RÉSIDUELS
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ProcessLaneLeaks()
        {
            // Unités joueur face à case ennemie vide (déjà gérées dans ExecutePlayerAttack)
            // Ici on vérifie les cas résiduels après les morts
            // Note : les lane leaks sont déjà traités col par col dans ExecutePlayerAttack/EnemyAttack
            yield return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // TOUR ENNEMI
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator EnemyTurnSequence()
        {
            if (_gameOver) yield break;

            Log("--- Tour ennemi ---");
            enemyAI.PlayTurn();
            RefreshAllUI();
            yield return new WaitForSeconds(0.5f);

            if (_gameOver) yield break;

            // Retour au tour joueur
            StartPlayerTurn();
        }

        // ─────────────────────────────────────────────────────────────────────
        // BOND ÉCLAIR ARDENT — ricochet
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ApplyEclairArdentRicochet(CardInstance attacker, int enemyRow, int hitCol)
        {
            // Frappe les colonnes hitCol-1 et hitCol+1 (1 dégât chacun)
            int[] ricoCols = new[] { hitCol - 1, hitCol + 1 };
            foreach (int rc in ricoCols)
            {
                if (!GridManager.InBounds(enemyRow, rc)) continue;
                var ricoTarget = gridManager.GetUnit(enemyRow, rc);
                if (ricoTarget == null) continue;

                Log($"  [Bond Éclair Ardent] Ricochet sur ({enemyRow},{rc}) — 1 dégât");
                var attackerCell = GetCellUI(attacker.isPlayerCard ? 1 : 0, attacker.col);
                var defenderCell = GetCellUI(enemyRow, rc);
                if (combatAnimator != null && attackerCell != null)
                    yield return StartCoroutine(combatAnimator.PlayAttackAnimGrid(attackerCell, defenderCell, 1));

                DamageUnit(ricoTarget, enemyRow, rc, attacker.isPlayerCard, 1, attacker);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DÉGÂTS UNITÉS
        // ─────────────────────────────────────────────────────────────────────

        private bool DamageUnit(CardInstance defender, int dr, int dc, bool killedByPlayer, int amount, CardInstance attacker = null)
        {
            bool died = defender.TakeDamage(amount);
            Log($"  {defender.data.cardName} reçoit {amount} dégât(s) → HP {defender.currentHP}/{defender.data.hp}");

            if (died)
            {
                Log($"  {defender.data.cardName} est détruit !");
                gridManager.RemoveUnit(dr, dc);
                SendToDiscard(defender);

                var cell = GetCellUI(dr, dc);
                if (combatAnimator != null && cell != null)
                    StartCoroutine(combatAnimator.PlayDeathAnimGrid(cell));

                RefreshAllUI();
            }
            return died;
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLEANUP
        // ─────────────────────────────────────────────────────────────────────

        private void CleanupDeadUnits()
        {
            for (int r = 0; r < GridManager.ROWS; r++)
            for (int c = 0; c < GridManager.COLS; c++)
            {
                var u = gridManager.GetUnit(r, c);
                if (u != null && !u.IsAlive)
                {
                    gridManager.RemoveUnit(r, c);
                    SendToDiscard(u);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POSE DE CARTE — JOUEUR
        // ─────────────────────────────────────────────────────────────────────

        public bool TryPlayUnit(CardInstance card, int row, int col, bool skipRefresh = false)
        {
            if (!CanPlay()) return false;
            if (!card.IsUnit) return false;
            if (_playerPlayedUnitThisTurn) return false;
            if (row != 1) return false; // joueur pose uniquement en Row 1
            if (!gridManager.IsEmpty(row, col)) return false;

            // Les unités ne coûtent pas de mana
            gridManager.PlaceUnit(card, row, col);
            playerDeck.RemoveFromHand(card);
            _playerPlayedUnitThisTurn = true;

            // Consomme le buff ATK en attente
            if (_pendingATKBuff > 0)
            {
                card.AddATKBonus(_pendingATKBuff);
                Log($"  Buff ATK +{_pendingATKBuff} appliqué à {card.data.cardName}");
                _pendingATKBuff = 0;
            }

            Log($"> Vous posez {card.data.cardName} en ({row},{col})");

            // Bonds à la pose (ForetDense, Decomposition)
            BondSystem.ApplyOnPlaceBonds(card, gridManager, Log);

            // Keyword Inspiration : pioche 1 carte
            if (card.data.keyword == UnitKeyword.Inspiration)
            {
                playerDeck.DrawCards(1);
                Log($"  [Inspiration] {card.data.cardName} : pioche 1 carte");
            }

            AudioManager.Instance.PlaySFX("sfx_card_place");
            if (!skipRefresh) RefreshAllUI();
            return true;
        }

        public bool TryPlaySpellOnHero(CardInstance card, bool isPlayerHero)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} sur {(isPlayerHero ? "votre héros" : "le héros ennemi")}");
            ApplyAllEffects(card, -1, -1, isPlayerHero);
            playerDeck.PlayCard(card);
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            RefreshAllUI();
            return true;
        }

        public bool TryPlaySpellOnUnit(CardInstance card, int row, int col)
        {
            if (!CanCastSpell(card)) return false;
            var target = gridManager.GetUnit(row, col);
            if (target == null) return false;

            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} sur {target.data.cardName}");
            ApplyAllEffects(card, row, col, false);
            playerDeck.PlayCard(card);
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            RefreshAllUI();
            return true;
        }

        public bool TryPlayAoESpell(CardInstance card)
        {
            if (!CanCastSpell(card)) return false;

            manaManager.Spend(card.data.manaCost);
            Log($"> Vous lancez {card.data.cardName} — AoE !");
            ApplyAllEffects(card, -1, -1, false);
            playerDeck.PlayCard(card);
            AudioManager.Instance.PlaySFX("sfx_spell_cast");
            RefreshAllUI();
            return true;
        }

        public bool TryPlayRepioche(CardInstance card)
        {
            if (!CanPlay()) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;

            manaManager.Spend(card.data.manaCost);
            playerDeck.PlayCard(card);
            playerDeck.ReshuffleHandAndRedraw();
            Log($"> Carte Repioche jouée — main mélangée et repigée.");
            AudioManager.Instance.PlaySFX("sfx_card_place");
            RefreshAllUI();
            return true;
        }

        /// <summary>
        /// Joue une carte Utilitaire de type Déplacement.
        /// Déplace l'unité alliée en (fromR,fromC) vers (toR,toC).
        /// </summary>
        public bool TryPlayUtility(CardInstance card, int fromR, int fromC, int toR, int toC)
        {
            if (!CanPlay()) return false;
            if (card.data.cardType != CardType.Utility) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;

            var unit = gridManager.GetUnit(fromR, fromC);
            if (unit == null || !unit.isPlayerCard) return false;
            if (!gridManager.IsEmpty(toR, toC)) return false;

            manaManager.Spend(card.data.manaCost);
            bool moved = gridManager.MoveUnit(fromR, fromC, toR, toC);
            if (!moved) { manaManager.AddBonus(card.data.manaCost); return false; }

            Log($"> Carte Déplacement : {unit.data.cardName} ({fromR},{fromC}) → ({toR},{toC})");

            // Bonds passifs réévalués à la nouvelle position
            BondSystem.ApplyOnPlaceBonds(unit, gridManager, Log);

            playerDeck.PlayCard(card);
            AudioManager.Instance.PlaySFX("sfx_card_place");
            RefreshAllUI();
            return true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // POSE DE CARTE — ENNEMI (appelé par EnemyAI)
        // ─────────────────────────────────────────────────────────────────────

        public void EnemyPlaceUnit(CardInstance card, int row, int col)
        {
            if (row != 0) return; // ennemi pose uniquement en Row 0
            if (!gridManager.IsEmpty(row, col)) return;
            gridManager.PlaceUnit(card, row, col);
            enemyDeck.RemoveFromHand(card);

            Log($"  Ennemi pose {card.data.cardName} en ({row},{col})");

            // Bonds à la pose côté ennemi
            BondSystem.ApplyOnPlaceBonds(card, gridManager, Log);

            RefreshAllUI();
        }

        public void EnemyCastSpellOnUnit(CardInstance card, int row, int col)
        {
            var target = gridManager.GetUnit(row, col);
            if (target == null) return;
            Log($"  Ennemi lance {card.data.cardName} sur {target.data.cardName}");
            ApplyAllEffectsEnemy(card, row, col);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        public void EnemyCastSpell(CardInstance card)
        {
            Log($"  Ennemi lance {card.data.cardName}");
            ApplyAllEffectsEnemy(card, -1, -1);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // DÉGÂTS HÉROS
        // ─────────────────────────────────────────────────────────────────────

        public void DamagePlayer(int amount)
        {
            if (amount <= 0) return;
            playerHP = Mathf.Max(0, playerHP - amount);
            Log($"  Votre héros : {playerHP}/{playerMaxHP} HP");
            if (playerHP <= 0) OnDefeat();
        }

        public void DamageEnemy(int amount)
        {
            if (amount <= 0) return;
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - amount);
            Log($"  Héros ennemi : {enemyCurrentHP}/{enemyMaxHP} HP");
            if (enemyCurrentHP <= 0) OnVictory();
        }

        // ─────────────────────────────────────────────────────────────────────
        // DÉFAUSSE
        // ─────────────────────────────────────────────────────────────────────

        private void SendToDiscard(CardInstance unit)
        {
            if (unit.isPlayerCard) playerDeck.AddToDiscard(unit);
            else                   enemyDeck.AddToDiscard(unit);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EFFETS DE SORTS
        // ─────────────────────────────────────────────────────────────────────

        private void ApplyAllEffects(CardInstance card, int targetRow, int targetCol, bool targetIsPlayerHero)
        {
            foreach (var eff in card.data.effects)
                ApplySingleEffect(eff, targetRow, targetCol, targetIsPlayerHero, playerCasting: true);
        }

        private void ApplyAllEffectsEnemy(CardInstance card, int targetRow, int targetCol)
        {
            bool targetIsEnemyHero = (card.data.spellTarget == SpellTarget.PlayerHero);
            foreach (var eff in card.data.effects)
                ApplySingleEffect(eff, targetRow, targetCol, targetIsEnemyHero, playerCasting: false);
        }

        private void ApplySingleEffect(CardEffect eff, int row, int col, bool heroIsPlayer, bool playerCasting)
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
                {
                    var target = row >= 0 ? gridManager.GetUnit(row, col) : null;
                    if (target != null)
                        ApplyEffectToUnit(eff, target, row, col, playerCasting);
                    break;
                }

                case SpellTarget.AllEnemyUnits:
                {
                    var targets = playerCasting
                        ? gridManager.GetAllUnits(false)
                        : gridManager.GetAllUnits(true);
                    foreach (var t in new List<CardInstance>(targets))
                        ApplyEffectToUnit(eff, t, t.row, t.col, playerCasting);
                    break;
                }

                case SpellTarget.AllAllyUnits:
                {
                    var targets = playerCasting
                        ? gridManager.GetAllUnits(true)
                        : gridManager.GetAllUnits(false);
                    foreach (var t in new List<CardInstance>(targets))
                        ApplyEffectToUnit(eff, t, t.row, t.col, playerCasting);
                    break;
                }
            }
        }

        private void ApplyEffectToPlayerHero(CardEffect eff, bool playerCasting)
        {
            switch (eff.effectType)
            {
                case EffectType.Heal:
                    if (playerCasting) { playerHP = Mathf.Min(playerMaxHP, playerHP + eff.value); Log($"  Soins : +{eff.value} HP ({playerHP}/{playerMaxHP})"); }
                    break;
                case EffectType.Damage:
                    if (playerCasting) DamagePlayer(eff.value);
                    else               DamageEnemy(eff.value);
                    break;
                case EffectType.DrawCard:
                    if (playerCasting) { playerDeck.DrawCards(eff.value); Log($"  Pioche {eff.value} carte(s)"); }
                    break;
                case EffectType.BuffATK:
                    if (playerCasting) { _pendingATKBuff += eff.value; Log($"  Buff ATK +{eff.value} en attente"); }
                    break;
                // Compatibilité anciens EffectType
                case EffectType.BuffNextUnitATK:
                    if (playerCasting) { _pendingATKBuff += eff.value; Log($"  Buff ATK +{eff.value} en attente"); }
                    break;
            }
        }

        private void ApplyEffectToEnemyHero(CardEffect eff, bool playerCasting)
        {
            switch (eff.effectType)
            {
                case EffectType.Damage:
                    DamageEnemy(eff.value);
                    Log($"  {eff.value} dmg au héros ennemi ({enemyCurrentHP}/{enemyMaxHP})");
                    break;
            }
        }

        private void ApplyEffectToUnit(CardEffect eff, CardInstance target, int tr, int tc, bool playerCasting)
        {
            if (target == null || !target.IsAlive) return;

            switch (eff.effectType)
            {
                case EffectType.DestroyUnit:
                    Log($"  Sort : {target.data.cardName} détruit !");
                    DamageUnit(target, tr, tc, playerCasting, target.currentHP);
                    break;
                case EffectType.Damage:
                    DamageUnit(target, tr, tc, playerCasting, eff.value);
                    break;
                case EffectType.BuffATK:
                    target.AddATKBonus(eff.value);
                    Log($"  {target.data.cardName} +{eff.value} ATK → {target.currentATK}");
                    break;
                case EffectType.BuffHP:
                {
                    target.currentHP = System.Math.Min(target.data.hp + eff.value, target.currentHP + eff.value);
                    Log($"  {target.data.cardName} +{eff.value} HP → {target.currentHP}");
                    break;
                }
                case EffectType.Freeze:
                    target.isFrozen = true;
                    Log($"  {target.data.cardName} gelé — passera son prochain tour d'attaque");
                    break;
                case EffectType.BuffNextUnitATK:
                    _pendingATKBuff += eff.value;
                    Log($"  Buff ATK +{eff.value} en attente (prochaine unité posée)");
                    break;
                case EffectType.BuffAllAllyHP:
                {
                    bool isPlayerCaster = playerCasting;
                    foreach (var ally in gridManager.GetAllUnits(isPlayerCaster))
                    {
                        ally.currentHP = System.Math.Min(ally.data.hp + 1, ally.currentHP + eff.value);
                        Log($"  {ally.data.cardName} +{eff.value} HP → {ally.currentHP}");
                    }
                    break;
                }
                case EffectType.TriggerAllAllyAttack:
                {
                    // Dans le nouveau système, les attaques se font en fin de tour — on peut ajouter un trigger immédiat ici
                    Log($"  [TriggerAllAllyAttack] sort appliqué");
                    break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS BOND
        // ─────────────────────────────────────────────────────────────────────

        private CardInstance FindShadowNeighbor(CardInstance unit, int row, int col)
        {
            if (col > 0)
            {
                var left = gridManager.GetUnit(row, col - 1);
                if (left != null && left.data.element == Element.Shadow && left.isPlayerCard == unit.isPlayerCard)
                    return left;
            }
            if (col < GridManager.COLS - 1)
            {
                var right = gridManager.GetUnit(row, col + 1);
                if (right != null && right.data.element == Element.Shadow && right.isPlayerCard == unit.isPlayerCard)
                    return right;
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ÉTATS FINAUX
        // ─────────────────────────────────────────────────────────────────────

        private void OnVictory()
        {
            if (_gameOver) return;
            _gameOver = true;
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
            if (_gameOver) return;
            _gameOver = true;
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

        // ─────────────────────────────────────────────────────────────────────
        // UI REFRESH
        // ─────────────────────────────────────────────────────────────────────

        private void RefreshAllUIExceptHand()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.MaxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count);
                combatUI.RefreshEnemyHP(enemyCurrentHP, enemyMaxHP);
                combatUI.RefreshEnemyInfo(enemyDeck.DeckCount, enemyDeck.DiscardCount,
                    manaManager.CurrentMana, manaManager.MaxMana);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);
                combatUI.RefreshCemetery(0, playerDeck.DiscardCount);
                combatUI.RefreshScores(0, 0);
                combatUI.RefreshRoundInfo(1, 0);
            }

            if (_allCells != null)
                foreach (var cell in _allCells)
                    if (cell != null)
                        cell.Refresh(gridManager.GetUnit(cell.row, cell.col));
        }

        public void RefreshAllUI()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.MaxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count);
                combatUI.RefreshEnemyHP(enemyCurrentHP, enemyMaxHP);
                combatUI.RefreshEnemyInfo(enemyDeck.DeckCount, enemyDeck.DiscardCount,
                    manaManager.CurrentMana, manaManager.MaxMana);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);
                combatUI.RefreshCemetery(0, playerDeck.DiscardCount);
                combatUI.RefreshScores(0, 0);
                combatUI.RefreshRoundInfo(1, 0);
            }

            if (handView != null)
                handView.RefreshHand(playerDeck.Hand);

            if (_allCells != null)
                foreach (var cell in _allCells)
                    if (cell != null)
                        cell.Refresh(gridManager.GetUnit(cell.row, cell.col));
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private bool CanPlay()
        {
            if (_gameOver) return false;
            if (combatAnimator != null && combatAnimator.IsAnimating) return false;
            if (!turnManager.IsPlayerTurn) return false;
            return true;
        }

        private bool CanCastSpell(CardInstance card)
        {
            if (!CanPlay()) return false;
            if (card.IsUnit) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;
            return true;
        }

        private GridCellUI GetCellUI(int row, int col)
        {
            if (_allCells == null) return null;
            foreach (var c in _allCells)
                if (c != null && c.row == row && c.col == col) return c;
            return null;
        }

        public void SaveBugReport() => SessionLogger.Instance?.SaveAsBugReport();

        public void SkipCombat() { if (!_gameOver) OnVictory(); }

        private void Log(string msg)
        {
            SessionLogger.Instance?.Write(msg);
            Debug.Log(msg);
        }

        // ─────────────────────────────────────────────────────────────────────
        // OVERLAYS RÉCOMPENSES (conservés de l'ancien système)
        // ─────────────────────────────────────────────────────────────────────

        private void ShowRelicReward()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null || relicRewardPool == null || relicRewardPool.Count == 0)
            { ShowResultOverlay(won: true); return; }

            var owned     = RunPersistence.Instance?.PlayerRelics;
            var available = relicRewardPool.FindAll(r => owned == null || !owned.Contains(r));
            if (available.Count == 0) available = relicRewardPool;

            var relic      = available[Random.Range(0, available.Count)];
            var overlayGO  = MakeFullOverlay(canvas, "RelicRewardOverlay", new Color(0.04f, 0.04f, 0.08f, 0.96f));

            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.84f, 0.90f, 0.96f,
                "Récompense de relique !", 42f, new Color(0.95f, 0.82f, 0.35f), FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Gold", 0.10f, 0.76f, 0.90f, 0.84f,
                $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or",
                18f, new Color(0.95f, 0.82f, 0.35f));

            var panel = new GameObject("RelicPanel", typeof(RectTransform));
            panel.transform.SetParent(overlayGO.transform, false);
            SetAnchors(panel, 0.30f, 0.35f, 0.70f, 0.74f);
            panel.AddComponent<Image>().color = new Color(0.40f, 0.24f, 0.06f);
            MakeOverlayTMP(panel, "Name",   0.05f, 0.65f, 0.95f, 0.90f, relic.relicName,           22f, Color.white, FontStyles.Bold);
            MakeOverlayTMP(panel, "Effect", 0.05f, 0.35f, 0.95f, 0.65f, GetRelicEffectText(relic), 14f, new Color(0.95f, 0.82f, 0.35f));
            MakeOverlayTMP(panel, "Desc",   0.05f, 0.05f, 0.95f, 0.35f, relic.description,         12f, new Color(0.82f, 0.80f, 0.74f));

            var btn     = MakeButton(overlayGO, "TakeBtn", 0.32f, 0.20f, 0.68f, 0.32f,
                $"Prendre {relic.relicName}", new Color(0.40f, 0.24f, 0.06f));
            var capR    = relic;
            var capOver = overlayGO;
            btn.onClick.AddListener(() =>
            {
                RunPersistence.Instance?.AddRelic(capR);
                Log($"Relique obtenue : {capR.relicName}");
                Destroy(capOver);
                ShowResultOverlay(won: true);
            });
        }

        private void ShowRewardScreen()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null || rewardCardPool == null || rewardCardPool.Count == 0)
            { ShowResultOverlay(won: true); return; }

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
            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.84f, 0.90f, 0.96f,
                "Choisissez une carte de récompense", 38f, new Color(0.95f, 0.82f, 0.35f), FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Gold", 0.10f, 0.76f, 0.90f, 0.84f,
                $"+ {_lastGoldEarned} or  ·  Total : {RunPersistence.Instance?.PlayerGold ?? 0} or",
                18f, new Color(0.95f, 0.82f, 0.35f));

            float cardW = 0.22f, gap = 0.05f, total = options.Count * cardW + (options.Count - 1) * gap;
            float startX = 0.5f - total / 2f;

            for (int i = 0; i < options.Count; i++)
            {
                var card  = options[i];
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
                    Log($"{capCard.cardName} ajoutée au deck !");
                    Destroy(capOver);
                    ShowResultOverlay(won: true);
                });

                int sellPrice = GetSellPrice(card.rarity);
                var sellBtn   = MakeButton(overlayGO, $"SellBtn_{i}", xMin, 0.13f, xMax, 0.21f,
                    $"Vendre : {sellPrice} or", new Color(0.28f, 0.18f, 0.10f));
                int capPrice = sellPrice;
                sellBtn.onClick.AddListener(() =>
                {
                    RunPersistence.Instance?.AddGold(capPrice);
                    Log($"{capCard.cardName} vendue pour {capPrice} or.");
                    Destroy(capOver);
                    ShowResultOverlay(won: true);
                });
            }

            var skipBtn = MakeButton(overlayGO, "SkipBtn", 0.37f, 0.03f, 0.63f, 0.11f,
                "Passer (aucune carte)", new Color(0.22f, 0.22f, 0.26f));
            var capSkip = overlayGO;
            skipBtn.onClick.AddListener(() => { Destroy(capSkip); ShowResultOverlay(won: true); });
        }

        private void ShowResultOverlay(bool won)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            bool isBossVict = won && (RunPersistence.Instance?.CurrentNode?.type == NodeType.Boss);
            var overlayGO   = MakeFullOverlay(canvas, "ResultOverlay", new Color(0f, 0f, 0f, 0.88f));

            MakeOverlayTMP(overlayGO, "Title", 0.10f, 0.58f, 0.90f, 0.80f,
                won ? "VICTOIRE !" : "DÉFAITE", 64f,
                won ? new Color(0.30f, 1.00f, 0.40f) : new Color(1.00f, 0.28f, 0.28f),
                FontStyles.Bold);
            MakeOverlayTMP(overlayGO, "Sub", 0.15f, 0.44f, 0.85f, 0.57f,
                isBossVict ? "Vous avez triomphé du boss — la run est terminée !"
                : won ? "L'ennemi a été vaincu !"
                      : "Vos points de vie sont tombés à zéro.",
                20f, new Color(0.82f, 0.80f, 0.75f));

            Color  btnColor = won ? new Color(0.12f, 0.40f, 0.18f) : new Color(0.35f, 0.10f, 0.10f);
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
        // OVERLAY BUILDERS (statiques)
        // ─────────────────────────────────────────────────────────────────────

        private static int GetSellPrice(CardRarity r) => r switch
        {
            CardRarity.Common    => 25,
            CardRarity.Uncommon  => 37,
            CardRarity.Rare      => 50,
            CardRarity.Epic      => 75,
            CardRarity.Legendary => 100,
            _                    => 25,
        };

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
            FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.fontStyle = style; tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
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
