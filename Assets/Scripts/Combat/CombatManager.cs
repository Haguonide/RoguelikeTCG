using System;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Data")]
        public CharacterData playerCharacter;
        public EnemyData enemyData;

        [Header("References")]
        public EnemyAI enemyAI;

        // ── Systèmes ─────────────────────────────────────────────────────────
        public TurnManager TurnManager { get; private set; }
        public BoardManager Board { get; private set; }
        public TerrainSystem Terrain { get; private set; }
        public RuneSystem Runes { get; private set; }
        public DeckManager PlayerDeck { get; private set; }
        public DeckManager EnemyDeck { get; private set; }
        public ManaManager PlayerMana { get; private set; }
        public ManaManager EnemyMana { get; private set; }

        // ── HP héros ─────────────────────────────────────────────────────────
        public int PlayerHP { get; private set; }
        public int EnemyHP { get; private set; }

        // ── Événements UI ────────────────────────────────────────────────────
        public event Action<TurnSide> OnTurnStarted;
        public event Action<TurnSide> OnTurnEnded;
        public event Action<TurnSide, int> OnHeroHPChanged;    // side, new hp
        public event Action<bool> OnCombatEnded;               // true = player won
        public event Action<TurnSide, CardInstance> OnCardDrawn;
        public event Action<TurnSide> OnMissionCompleted;

        private bool _combatOver;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Initialisation ────────────────────────────────────────────────────

        public void StartCombat(CharacterData player, EnemyData enemy, RuneSystem existingRunes)
        {
            playerCharacter = player;
            enemyData = enemy;
            _combatOver = false;

            PlayerHP = player.maxHP;
            EnemyHP = enemy.maxHP;

            PlayerDeck = new DeckManager();
            EnemyDeck = new DeckManager();
            PlayerDeck.Initialize(player.startingDeck);
            EnemyDeck.Initialize(enemy.deck);

            PlayerMana = new ManaManager();
            EnemyMana = new ManaManager();
            PlayerMana.StartCombat();
            EnemyMana.StartCombat();

            Board = new BoardManager();
            Board.OnUnitDied += HandleUnitDied;
            Board.OnDirectDamageToHero += HandleDirectDamage;

            Terrain = new TerrainSystem();
            Terrain.Initialize(Board, PlayerDeck, EnemyDeck);
            Terrain.OnMissionCompleted += HandleMissionCompleted;

            Runes = existingRunes ?? new RuneSystem();

            if (enemyAI != null) enemyAI.Initialize(this);

            TurnManager = new TurnManager();
            TurnManager.OnTurnStart += HandleTurnStart;
            TurnManager.OnTurnEnd += HandleTurnEnd;

            // Main de départ
            PlayerDeck.DrawCards(4);
            EnemyDeck.DrawCards(4);

            TurnManager.StartCombat();
        }

        // ── Tour ──────────────────────────────────────────────────────────────

        private void HandleTurnStart(TurnSide side)
        {
            if (_combatOver) return;

            if (side == TurnSide.Player) PlayerMana.OnTurnStart();
            else EnemyMana.OnTurnStart();

            var drawn = side == TurnSide.Player
                ? PlayerDeck.DrawCard()
                : EnemyDeck.DrawCard();

            if (drawn != null) OnCardDrawn?.Invoke(side, drawn);

            // Terrain ennemi en boucle : rejouer si nécessaire
            if (side == TurnSide.Enemy && Board.EnemyTerrain == null && enemyData.loopingTerrain != null)
                PlayEnemyTerrain();

            OnTurnStarted?.Invoke(side);

            if (side == TurnSide.Enemy && enemyAI != null)
                enemyAI.PlayTurn();
        }

        private void HandleTurnEnd(TurnSide side)
        {
            if (_combatOver) return;
            Terrain.OnTurnEnd(side);
            Board.ResolveAttacks(side);
            OnTurnEnded?.Invoke(side);
            CheckWinCondition();
        }

        // Bouton "Fin de Tour" dans l'UI
        public void EndPlayerTurn()
        {
            if (_combatOver || TurnManager.CurrentSide != TurnSide.Player) return;
            TurnManager.EndTurn();
        }

        // ── Jouer des cartes ──────────────────────────────────────────────────

        public bool PlayUnit(CardInstance card, int slot)
        {
            if (!CanPlay(card, TurnSide.Player)) return false;
            if (!Board.PlaceUnit(card, slot, TurnSide.Player)) return false;

            PlayerMana.Spend(card.CurrentCost);
            PlayerDeck.PlayCard(card);
            Terrain.NotifyUnitPlaced(TurnSide.Player);
            return true;
        }

        public bool PlayTerrain(CardInstance card)
        {
            if (!CanPlay(card, TurnSide.Player)) return false;

            PlayerMana.Spend(card.CurrentCost);
            PlayerDeck.PlayCard(card);
            Board.PlaceTerrain(card, TurnSide.Player);
            return true;
        }

        public bool PlaySpell(CardInstance card)
        {
            if (!CanPlay(card, TurnSide.Player)) return false;

            PlayerMana.Spend(card.CurrentCost);
            PlayerDeck.PlayCard(card);
            ResolveSpellEffect(card, TurnSide.Player);
            PlayerDeck.SendToDiscard(card);
            return true;
        }

        private bool CanPlay(CardInstance card, TurnSide side)
        {
            if (_combatOver) return false;
            if (TurnManager.CurrentSide != side) return false;
            var mana = side == TurnSide.Player ? PlayerMana : EnemyMana;
            return mana.CanAfford(card.CurrentCost);
        }

        // ── IA ennemie ────────────────────────────────────────────────────────

        public void PlayEnemyCard(CardInstance card, int slot)
        {
            if (!CanPlay(card, TurnSide.Enemy)) return;
            EnemyMana.Spend(card.CurrentCost);
            EnemyDeck.PlayCard(card);

            if (card.IsUnit) Board.PlaceUnit(card, slot, TurnSide.Enemy);
            else if (card.IsTerrain) Board.PlaceTerrain(card, TurnSide.Enemy);
            else if (card.IsSpell) { ResolveSpellEffect(card, TurnSide.Enemy); EnemyDeck.SendToDiscard(card); }
        }

        public void EndEnemyTurn()
        {
            if (_combatOver || TurnManager.CurrentSide != TurnSide.Enemy) return;
            TurnManager.EndTurn();
        }

        private void PlayEnemyTerrain()
        {
            var terrain = new CardInstance(enemyData.loopingTerrain);
            Board.PlaceTerrain(terrain, TurnSide.Enemy);
        }

        // ── Effets sorts (placeholder — à étoffer lors de la phase mécanique) ──

        private void ResolveSpellEffect(CardInstance card, TurnSide caster)
        {
            // Implémentation des effets sort à venir
        }

        // ── Handlers événements ───────────────────────────────────────────────

        private void HandleUnitDied(CardInstance unit, TurnSide ownerSide)
        {
            var deck = ownerSide == TurnSide.Player ? PlayerDeck : EnemyDeck;
            deck.SendToDiscard(unit);

            // Gain de rune pour le joueur si une unité ennemie meurt
            if (ownerSide == TurnSide.Enemy)
            {
                var runeType = unit.Data.elementType switch
                {
                    ElementType.Fire   => RuneType.Fire,
                    ElementType.Shadow => RuneType.Shadow,
                    ElementType.Nature => RuneType.Nature,
                    ElementType.Ice    => RuneType.Ice,
                    _                  => RuneType.Neutral,
                };
                Runes.Add(runeType);
            }
        }

        private void HandleDirectDamage(int damage, TurnSide victimSide)
        {
            if (victimSide == TurnSide.Player)
            {
                PlayerHP = Mathf.Max(0, PlayerHP - damage);
                OnHeroHPChanged?.Invoke(TurnSide.Player, PlayerHP);
            }
            else
            {
                EnemyHP = Mathf.Max(0, EnemyHP - damage);
                OnHeroHPChanged?.Invoke(TurnSide.Enemy, EnemyHP);
            }
        }

        private void HandleMissionCompleted(TurnSide side)
        {
            OnMissionCompleted?.Invoke(side);
            var terrain = side == TurnSide.Player ? Board.PlayerTerrain : Board.EnemyTerrain;
            if (terrain == null) return;

            var mana = side == TurnSide.Player ? PlayerMana : EnemyMana;
            switch (terrain.Data.rewardType)
            {
                case TerrainRewardType.BonusMana:
                    mana.AddMana(terrain.Data.rewardValue);
                    break;
                case TerrainRewardType.HealHero:
                    HealHero(side, terrain.Data.rewardValue);
                    break;
                case TerrainRewardType.GetEphemeralSpell:
                    // TODO : définir le pool de sorts éphémères sur CharacterData/EnemyData
                    break;
            }
        }

        private void HealHero(TurnSide side, int amount)
        {
            int max = side == TurnSide.Player ? playerCharacter.maxHP : enemyData.maxHP;
            if (side == TurnSide.Player)
            {
                PlayerHP = Mathf.Min(PlayerHP + amount, max);
                OnHeroHPChanged?.Invoke(TurnSide.Player, PlayerHP);
            }
            else
            {
                EnemyHP = Mathf.Min(EnemyHP + amount, max);
                OnHeroHPChanged?.Invoke(TurnSide.Enemy, EnemyHP);
            }
        }

        private void CheckWinCondition()
        {
            if (_combatOver) return;
            if (EnemyHP <= 0) { _combatOver = true; OnCombatEnded?.Invoke(true); }
            else if (PlayerHP <= 0) { _combatOver = true; OnCombatEnded?.Invoke(false); }
        }
    }
}
