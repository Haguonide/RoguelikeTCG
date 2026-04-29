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
        public GridManager     gridManager;
        public DeckManager     playerDeck;
        public DeckManager     enemyDeck;
        public ManaManager     manaManager;
        public TurnManager     turnManager;
        public EnemyAI         enemyAI;

        [Header("UI")]
        public CombatUI        combatUI;
        public HandView        handView;
        public HeroPortraitUI  playerPortrait;
        public HeroPortraitUI  enemyPortrait;
        public RelicBarUI      relicBar;
        public RectTransform   endTurnButtonRT;
        public CombatAnimator  combatAnimator;

        [Header("Personnages")]
        public CharacterData   playerCharacter;
        public CharacterData   enemyCharacter;

        [Header("HP Ennemi")]
        public int enemyMaxHP     = 30;
        public int enemyCurrentHP;

        [Header("Récompenses")]
        public List<CardData>  rewardCardPool;
        public List<RelicData> relicRewardPool;
        private const int      RewardCount = 3;

        // ── État joueur ───────────────────────────────────────────────────────
        public int playerHP;
        public int playerMaxHP;

        // ── Interne ───────────────────────────────────────────────────────────
        private bool _gameOver;
        private int  _lastGoldEarned;
        private GridCellUI[] _allCells;
        private bool _playerPlayedUnitThisTurn;

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
            _allCells = FindObjectsOfType<GridCellUI>(true);
            ConfigureFromNodeType();
            InitializeCombat();
        }

        private void ConfigureFromNodeType()
        {
            var nodeType = RunPersistence.Instance?.CurrentNode?.type;
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
            _gameOver = false;
            _playerPlayedUnitThisTurn = false;

            var persistence = RunPersistence.Instance;
            if (persistence?.SelectedCharacter != null)
                playerCharacter = persistence.SelectedCharacter;

            // Pool récompenses
            if (persistence?.EffectiveCardPool?.Count > 0)
                rewardCardPool = new List<CardData>(persistence.EffectiveCardPool);
            else if (playerCharacter?.cardPool?.Count > 0)
                rewardCardPool = new List<CardData>(playerCharacter.cardPool);

            // HP joueur
            if (persistence != null && persistence.PlayerHP > 0)
            {
                playerMaxHP = persistence.PlayerMaxHP;
                playerHP    = persistence.PlayerHP;
            }
            else
            {
                playerMaxHP = persistence?.PlayerMaxHP ?? 80;
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

            // Motifs de scoring — tirage unique pour tout le combat
            gridManager.patternManager?.DrawPatterns();

            // IA
            enemyAI.Initialize(enemyDeck, gridManager, manaManager);

            AudioManager.Instance.PlayMusic("music_combat");
            Log("Combat commencé !");

            // Coin flip
            bool playerFirst = Random.value >= 0.5f;
            Log(playerFirst ? "Pile — vous commencez !" : "Face — l'ennemi commence !");

            StartNewRound(playerFirst);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROUND FLOW
        // ─────────────────────────────────────────────────────────────────────

        private void StartNewRound(bool playerFirst)
        {
            Log($"=== Manche {turnManager.CurrentRound} ===");
            gridManager.ResetRoundTracking();
            manaManager.ResetForNewRound();
            turnManager.ResetRound();
            _playerPlayedUnitThisTurn = false;

            if (playerFirst)
                StartPlayerTurn();
            else
                StartCoroutine(EnemyTurnSequence());
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

            int extraDraw = RelicManager.Instance?.GetExtraDrawPerTurn() ?? 0;
            int prevCount = playerDeck.Hand.Count;
            playerDeck.DrawCards(playerDeck.drawPerTurn + extraDraw);
            int drawn = playerDeck.Hand.Count - prevCount;

            AudioManager.Instance.PlaySFX("sfx_card_draw");
            if (drawn > 0 && combatAnimator != null && endTurnButtonRT != null && handView != null)
                StartCoroutine(combatAnimator.PlayDrawCardsAnim(
                    playerDeck.Hand, drawn,
                    playerDeck.Hand.Count,
                    endTurnButtonRT, handView));

            Log($"--- Votre tour ({TurnManager.TURNS_PER_ROUND - turnManager.PlayerTurnsLeft + 1}/{TurnManager.TURNS_PER_ROUND}) ---");
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
            StartCoroutine(PlayerTurnResolution());
        }

        private IEnumerator PlayerTurnResolution()
        {
            // Tick countdowns + attaques
            yield return StartCoroutine(ProcessAttacks());
            if (_gameOver) yield break;

            RefreshAllUI();

            if (turnManager.PlayerTurnsLeft <= 0 && turnManager.EnemyTurnsLeft <= 0)
            {
                yield return StartCoroutine(ResolveRound());
                yield break;
            }

            // Tour ennemi
            yield return StartCoroutine(EnemyTurnSequence());
        }

        // ─────────────────────────────────────────────────────────────────────
        // TOUR ENNEMI
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator EnemyTurnSequence()
        {
            if (_gameOver) yield break;

            manaManager.EnemyTurnRegen();
            turnManager.StartEnemyTurn();
            Log("--- Tour ennemi ---");

            enemyAI.PlayTurn();
            RefreshAllUI();
            yield return new WaitForSeconds(0.5f);

            // Tick countdowns + attaques
            yield return StartCoroutine(ProcessAttacks());
            if (_gameOver) yield break;

            turnManager.EndEnemyTurn();
            RefreshAllUI();

            if (turnManager.PlayerTurnsLeft <= 0 && turnManager.EnemyTurnsLeft <= 0)
            {
                yield return StartCoroutine(ResolveRound());
                yield break;
            }

            // Retour au tour joueur
            StartPlayerTurn();
        }

        // ─────────────────────────────────────────────────────────────────────
        // ATTAQUES (Tick Countdown)
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ProcessAttacks()
        {
            var readyUnits = gridManager.TickCountdowns();
            if (readyUnits.Count == 0) yield break;

            // Tri : d'abord les joueurs, puis les ennemis
            readyUnits.Sort((a, b) =>
            {
                if (a.isPlayerCard != b.isPlayerCard) return a.isPlayerCard ? -1 : 1;
                return 0;
            });

            foreach (var attacker in new List<CardInstance>(readyUnits))
            {
                if (!attacker.IsAlive || !attacker.IsOnGrid) continue;
                yield return StartCoroutine(ExecuteAttack(attacker));
                if (_gameOver) yield break;
            }

            // Reset CD des unités qui ont attaqué et sont encore en vie
            foreach (var attacker in readyUnits)
                if (attacker.IsAlive && attacker.IsOnGrid)
                    gridManager.ResetCountdown(attacker);
        }

        private IEnumerator ExecuteAttack(CardInstance attacker)
        {
            int row = attacker.gridRow;
            int col = attacker.gridCol;
            var dirs = attacker.data.attackDirections;
            var targets = gridManager.GetAttackTargets(row, col, dirs);

            foreach (var (tr, tc) in new List<(int, int)>(targets))
            {
                var defender = gridManager.GetUnit(tr, tc);
                if (defender == null) continue;
                if (defender.isPlayerCard == attacker.isPlayerCard) continue;

                // Bouclier : absorbe la première attaque
                if (defender.data.keyword == UnitKeyword.Bouclier && defender.hasShield)
                {
                    defender.hasShield = false;
                    Log($"  [Bouclier] {defender.data.cardName} absorbe l'attaque !");
                    continue;
                }

                var attackerCell = GetCellUI(row, col);
                var defenderCell = GetCellUI(tr, tc);
                if (combatAnimator != null && attackerCell != null)
                    yield return StartCoroutine(
                        combatAnimator.PlayAttackAnimGrid(attackerCell, defenderCell, 1));

                bool killed = InstantKillUnit(defender, tr, tc, attacker.isPlayerCard);

                if (killed && attacker.data.keyword == UnitKeyword.Percée && attacker.IsAlive)
                {
                    foreach (AttackDirection dir in new[] {
                        AttackDirection.Up, AttackDirection.Down,
                        AttackDirection.Left, AttackDirection.Right })
                    {
                        if ((dirs & dir) == 0) continue;
                        var (nr, nc) = ScoringSystem.GetCellInDirection(tr, tc, dir);
                        if (nr < 0) continue;
                        var behind = gridManager.GetUnit(nr, nc);
                        if (behind == null || behind.isPlayerCard == attacker.isPlayerCard) continue;
                        Log($"  [Percée] {attacker.data.cardName} enchaîne sur {behind.data.cardName}");
                        InstantKillUnit(behind, nr, nc, attacker.isPlayerCard);
                    }
                    yield return null;
                }
            }

            RefreshAllUI();
            if (_gameOver) yield break;
            if (playerHP <= 0)        { OnDefeat();  yield break; }
            if (enemyCurrentHP <= 0)  { OnVictory(); yield break; }
        }

        private bool InstantKillUnit(CardInstance unit, int row, int col, bool killedByPlayer)
        {
            Log($"  {unit.data.cardName} est détruit !");
            TriggerOnDeathKeyword(unit, row, col);
            gridManager.RemoveUnit(row, col);
            SendToDiscard(unit);

            if (killedByPlayer)
            {
                gridManager.PlayerRoundScore++;
                Log($"  Kill ! Score joueur +1 ({gridManager.PlayerRoundScore} pts)");
            }
            else
            {
                gridManager.EnemyRoundScore++;
                Log($"  Kill ! Score ennemi +1 ({gridManager.EnemyRoundScore} pts)");
            }

            var cell = GetCellUI(row, col);
            if (combatAnimator != null && cell != null)
                StartCoroutine(combatAnimator.PlayDeathAnimGrid(cell));

            RefreshAllUI();
            return true;
        }

        private void TriggerOnDeathKeyword(CardInstance dead, int dr, int dc)
        {
            switch (dead.data.keyword)
            {
                case UnitKeyword.Épine:
                {
                    // Détruit une unité ennemie adjacente (la première trouvée)
                    foreach (var (nr, nc) in ScoringSystem.GetOrthogonalNeighbors(dr, dc))
                    {
                        var adj = gridManager.GetUnit(nr, nc);
                        if (adj == null || adj.isPlayerCard == dead.isPlayerCard) continue;
                        Log($"  [Épine] {dead.data.cardName} détruit {adj.data.cardName}");
                        TriggerOnDeathKeyword(adj, nr, nc);
                        gridManager.RemoveUnit(nr, nc);
                        SendToDiscard(adj);
                        if (dead.isPlayerCard) gridManager.PlayerRoundScore++;
                        else                   gridManager.EnemyRoundScore++;
                        break;
                    }
                    break;
                }

                case UnitKeyword.Explosion:
                {
                    // Détruit toutes les unités adjacentes (alliées + ennemies)
                    foreach (var (nr, nc) in ScoringSystem.GetAllNeighbors(dr, dc))
                    {
                        var adj = gridManager.GetUnit(nr, nc);
                        if (adj == null) continue;
                        Log($"  [Explosion] {dead.data.cardName} détruit {adj.data.cardName}");
                        TriggerOnDeathKeyword(adj, nr, nc);
                        gridManager.RemoveUnit(nr, nc);
                        SendToDiscard(adj);
                        if (dead.isPlayerCard) gridManager.PlayerRoundScore++;
                        else                   gridManager.EnemyRoundScore++;
                    }
                    break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POSE DE CARTE — JOUEUR
        // ─────────────────────────────────────────────────────────────────────

        public bool TryPlayUnit(CardInstance card, int row, int col)
        {
            if (!CanPlay()) return false;
            if (!card.IsUnit) return false;
            if (_playerPlayedUnitThisTurn) return false;
            if (!gridManager.IsEmpty(row, col)) return false;
            if (!manaManager.CanAfford(card.data.manaCost)) return false;

            manaManager.Spend(card.data.manaCost);
            gridManager.PlaceUnit(card, row, col);
            playerDeck.RemoveFromHand(card);
            _playerPlayedUnitThisTurn = true;

            Log($"> Vous posez {card.data.cardName} en ({row},{col}) CD:{card.currentCountdown}");

            TriggerOnPlaceKeyword(card, row, col, isPlayer: true);

            // Score immédiat si motif complété (scoring à la pose)
            int pts = gridManager.CheckAndScorePlayer(row, col);
            if (pts > 0) Log($"  Motif complété ! +{pts} pts (total manche : {gridManager.PlayerRoundScore})");

            // Keyword Combo : +1 pt bonus si la pose complète une ligne/diag/carré
            if (card.data.keyword == UnitKeyword.Combo && pts > 0)
            {
                gridManager.PlayerRoundScore++; // +1 bonus direct
                Log($"  [Combo] Bonus +1 pt !");
            }

            AudioManager.Instance.PlaySFX("sfx_card_place");
            RefreshAllUI();
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

        // ─────────────────────────────────────────────────────────────────────
        // POSE DE CARTE — ENNEMI (appelé par EnemyAI)
        // ─────────────────────────────────────────────────────────────────────

        public void EnemyPlaceUnit(CardInstance card, int row, int col)
        {
            if (!gridManager.IsEmpty(row, col)) return;
            gridManager.PlaceUnit(card, row, col);
            enemyDeck.RemoveFromHand(card);
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi pose {card.data.cardName} en ({row},{col})");

            TriggerOnPlaceKeyword(card, row, col, isPlayer: false);

            int ptsEnemy = gridManager.CheckAndScoreEnemy(row, col);
            if (ptsEnemy > 0) Log($"  Score ennemi +{ptsEnemy} pts (motif)");

            RefreshAllUI();
        }

        public void EnemyCastSpellOnUnit(CardInstance card, int row, int col)
        {
            var target = gridManager.GetUnit(row, col);
            if (target == null) return;
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi lance {card.data.cardName} sur {target.data.cardName}");
            ApplyAllEffectsEnemy(card, row, col);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        public void EnemyCastSpell(CardInstance card)
        {
            manaManager.Spend(card.data.manaCost);
            Log($"  Ennemi lance {card.data.cardName}");
            ApplyAllEffectsEnemy(card, -1, -1);
            enemyDeck.PlayCard(card);
            RefreshAllUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        // KEYWORDS À LA POSE
        // ─────────────────────────────────────────────────────────────────────

        private void TriggerOnPlaceKeyword(CardInstance unit, int r, int c, bool isPlayer)
        {
            switch (unit.data.keyword)
            {
                case UnitKeyword.Inspiration:
                    if (isPlayer)
                    {
                        playerDeck.DrawCards(1);
                        Log($"  [Inspiration] {unit.data.cardName} : pioche 1 carte");
                    }
                    break;

                case UnitKeyword.Bouclier:
                    unit.hasShield = true;
                    Log($"  [Bouclier] {unit.data.cardName} protégé pour la première attaque");
                    break;

                case UnitKeyword.Légion:
                {
                    int allies = CountAdjacentAllies(r, c, isPlayer);
                    if (allies > 0)
                    {
                        unit.currentCountdown = System.Math.Max(1, unit.currentCountdown - allies);
                        Log($"  [Légion] {unit.data.cardName} : CD réduit à {unit.currentCountdown}");
                    }
                    break;
                }

                case UnitKeyword.Ralliement:
                {
                    foreach (var a in gridManager.GetAllUnits(isPlayer))
                    {
                        if (a == unit) continue;
                        bool isAdjacent = false;
                        foreach (var (nr, nc) in ScoringSystem.GetOrthogonalNeighbors(r, c))
                            if (nr == a.gridRow && nc == a.gridCol) { isAdjacent = true; break; }
                        if (isAdjacent)
                        {
                            a.currentCountdown = System.Math.Max(1, a.currentCountdown - 1);
                            Log($"  [Ralliement] {a.data.cardName} CD réduit à {a.currentCountdown}");
                        }
                    }
                    break;
                }
            }
        }

        private int CountAdjacentAllies(int r, int c, bool isPlayer)
        {
            int count = 0;
            foreach (var (nr, nc) in ScoringSystem.GetOrthogonalNeighbors(r, c))
            {
                var u = gridManager.GetUnit(nr, nc);
                if (u != null && u.isPlayerCard == isPlayer) count++;
            }
            return count;
        }

        // ─────────────────────────────────────────────────────────────────────
        // RÉSOLUTION DE FIN DE MANCHE
        // ─────────────────────────────────────────────────────────────────────

        private IEnumerator ResolveRound()
        {
            Log("=== Fin de manche ===");

            // Dominance : +1 pt par unité survivante ayant le keyword
            int playerDom = gridManager.ScoreDominance(isPlayer: true);
            int enemyDom  = gridManager.ScoreDominance(isPlayer: false);
            if (playerDom > 0) Log($"  [Dominance] +{playerDom} pts joueur");
            if (enemyDom  > 0) Log($"  [Dominance] +{enemyDom} pts ennemi");

            int playerScore = gridManager.PlayerRoundScore;
            int enemyScore  = gridManager.EnemyRoundScore;
            int delta       = playerScore - enemyScore;

            Log($"  Score : Joueur {playerScore} — Ennemi {enemyScore}");

            if (delta > 0)
            {
                DamageEnemy(delta);
                Log($"  Joueur inflige {delta} dmg au héros ennemi ! ({enemyCurrentHP}/{enemyMaxHP})");
            }
            else if (delta < 0)
            {
                DamagePlayer(-delta);
                Log($"  Ennemi inflige {-delta} dmg à votre héros ! ({playerHP}/{playerMaxHP})");
            }
            else
            {
                Log("  Égalité — aucun dégât.");
            }

            if (playerHP <= 0) { OnDefeat(); yield break; }
            if (enemyCurrentHP <= 0) { OnVictory(); yield break; }

            // Vider la grille → défausse
            var survivors = gridManager.ClearGrid();
            foreach (var unit in survivors)
                SendToDiscard(unit);

            Log($"  {survivors.Count} unité(s) survivante(s) → défausse.");

            yield return new WaitForSeconds(0.5f);

            RefreshAllUI();
            StartNewRound(playerFirst: true); // le joueur commence toujours la manche suivante
        }

        // ─────────────────────────────────────────────────────────────────────
        // DÉGÂTS HÉROS
        // ─────────────────────────────────────────────────────────────────────

        public void DamagePlayer(int amount)
        {
            if (amount <= 0) return;
            playerHP = Mathf.Max(0, playerHP - amount);
            Log($"  Votre héros : {playerHP}/{playerMaxHP} HP");
        }

        public void DamageEnemy(int amount)
        {
            if (amount <= 0) return;
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - amount);
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
                        ApplyEffectToUnit(eff, t, t.gridRow, t.gridCol, playerCasting);
                    break;
                }

                case SpellTarget.AllAllyUnits:
                {
                    var targets = playerCasting
                        ? gridManager.GetAllUnits(true)
                        : gridManager.GetAllUnits(false);
                    foreach (var t in new List<CardInstance>(targets))
                        ApplyEffectToUnit(eff, t, t.gridRow, t.gridCol, playerCasting);
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

            var cellRT = GetCellUI(tr, tc)?.GetComponent<RectTransform>();

            switch (eff.effectType)
            {
                case EffectType.DestroyUnit:
                case EffectType.Damage:
                    Log($"  Sort : {target.data.cardName} détruit !");
                    InstantKillUnit(target, tr, tc, playerCasting);
                    break;
                case EffectType.ReduceCountdown:
                    target.currentCountdown = System.Math.Max(1, target.currentCountdown - eff.value);
                    Log($"  {target.data.cardName} CD réduit à {target.currentCountdown}");
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ÉTATS FINAUX
        // ─────────────────────────────────────────────────────────────────────

        private void OnVictory()
        {
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

        public void RefreshAllUI()
        {
            if (combatUI != null)
            {
                combatUI.RefreshPlayerInfo(playerHP, playerMaxHP,
                    manaManager.CurrentMana, manaManager.MaxMana,
                    playerDeck.DeckCount, playerDeck.Hand.Count);
                combatUI.RefreshEnemyHP(enemyCurrentHP, enemyMaxHP);
                combatUI.RefreshGold(RunPersistence.Instance?.PlayerGold ?? 0);
                combatUI.RefreshCemetery(0, playerDeck.DiscardCount);
                combatUI.RefreshScores(gridManager.PlayerRoundScore, gridManager.EnemyRoundScore);
                combatUI.RefreshRoundInfo(turnManager.CurrentRound, turnManager.PlayerTurnsLeft);
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
            var canvas = FindObjectOfType<Canvas>();
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
            var canvas = FindObjectOfType<Canvas>();
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
            var canvas = FindObjectOfType<Canvas>();
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
