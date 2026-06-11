using System;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public class TerrainSystem
    {
        private BoardManager _board;
        private DeckManager _playerDeck;
        private DeckManager _enemyDeck;

        private int _playerProgress;
        private int _enemyProgress;
        private int _playerDamageThisTurn;
        private int _enemyDamageThisTurn;

        // (camp dont la mission est accomplie)
        public event Action<TurnSide> OnMissionCompleted;

        public void Initialize(BoardManager board, DeckManager playerDeck, DeckManager enemyDeck)
        {
            _board = board;
            _playerDeck = playerDeck;
            _enemyDeck = enemyDeck;

            board.OnUnitDied += HandleUnitDied;
            board.OnDamageDealt += HandleDamageDealt;
            board.OnTerrainDiscarded += HandleTerrainDiscarded;
        }

        // Appelé par CombatManager quand une unité est posée (HaveAlliedUnitsOnBoard)
        public void NotifyUnitPlaced(TurnSide side)
        {
            CheckCompletion(side);
        }

        // Appelé par TurnManager.OnTurnEnd via CombatManager
        public void OnTurnEnd(TurnSide side)
        {
            if (side == TurnSide.Player) _playerDamageThisTurn = 0;
            else _enemyDamageThisTurn = 0;
        }

        // ── Handlers BoardManager ────────────────────────────────────────────

        private void HandleUnitDied(CardInstance unit, TurnSide ownerSide)
        {
            // Une unité ennemie est morte → progress pour le joueur (et vice versa)
            TurnSide missionOwner = ownerSide == TurnSide.Enemy ? TurnSide.Player : TurnSide.Enemy;
            var terrain = GetTerrain(missionOwner);
            if (terrain != null && terrain.Data.missionType == TerrainMissionType.KillEnemyUnits)
            {
                IncrementProgress(missionOwner);
                CheckCompletion(missionOwner);
            }
        }

        private void HandleDamageDealt(int damage, TurnSide attackingSide)
        {
            if (attackingSide == TurnSide.Player) _playerDamageThisTurn += damage;
            else _enemyDamageThisTurn += damage;

            var terrain = GetTerrain(attackingSide);
            if (terrain != null && terrain.Data.missionType == TerrainMissionType.DealDamageInOneTurn)
                CheckCompletion(attackingSide);
        }

        private void HandleTerrainDiscarded(CardInstance terrain, TurnSide side)
        {
            ResetProgress(side);
        }

        // ── Vérification & complétion ────────────────────────────────────────

        private void CheckCompletion(TurnSide side)
        {
            var terrain = GetTerrain(side);
            if (terrain == null) return;

            bool completed = terrain.Data.missionType switch
            {
                TerrainMissionType.KillEnemyUnits =>
                    GetProgress(side) >= terrain.Data.missionTarget,

                TerrainMissionType.HaveAlliedUnitsOnBoard =>
                    CountAliveUnits(side) >= terrain.Data.missionTarget,

                TerrainMissionType.DealDamageInOneTurn =>
                    GetDamageThisTurn(side) >= terrain.Data.missionTarget,

                _ => false
            };

            if (completed) CompleteMission(side, terrain);
        }

        private void CompleteMission(TurnSide side, CardInstance terrain)
        {
            GrantReward(side, terrain);
            OnMissionCompleted?.Invoke(side);

            if (terrain.Data.loopsOnCompletion)
            {
                ResetProgress(side);
            }
            else
            {
                _board.ClearTerrain(side);
                ResetProgress(side);
            }
        }

        private void GrantReward(TurnSide side, CardInstance terrain)
        {
            var deck = side == TurnSide.Player ? _playerDeck : _enemyDeck;
            int value = terrain.Data.rewardValue;

            switch (terrain.Data.rewardType)
            {
                case TerrainRewardType.DrawCards:
                    deck.DrawCards(value);
                    break;
                case TerrainRewardType.GetEphemeralSpell:
                    // CombatManager souscrit à OnMissionCompleted pour gérer ce cas
                    // (nécessite un pool de sorts éphémères défini sur l'EnemyData/CharacterData)
                    break;
                case TerrainRewardType.BonusMana:
                    // CombatManager souscrit à OnMissionCompleted pour appeler ManaManager.AddMana
                    break;
                case TerrainRewardType.HealHero:
                    // CombatManager souscrit à OnMissionCompleted pour soigner le héros
                    break;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private CardInstance GetTerrain(TurnSide side) =>
            side == TurnSide.Player ? _board.PlayerTerrain : _board.EnemyTerrain;

        private int GetProgress(TurnSide side) =>
            side == TurnSide.Player ? _playerProgress : _enemyProgress;

        private int GetDamageThisTurn(TurnSide side) =>
            side == TurnSide.Player ? _playerDamageThisTurn : _enemyDamageThisTurn;

        private void IncrementProgress(TurnSide side)
        {
            if (side == TurnSide.Player) _playerProgress++;
            else _enemyProgress++;
        }

        private void ResetProgress(TurnSide side)
        {
            if (side == TurnSide.Player) _playerProgress = 0;
            else _enemyProgress = 0;
        }

        private int CountAliveUnits(TurnSide side)
        {
            int count = 0;
            foreach (var unit in _board.GetUnits(side))
                if (unit != null) count++;
            return count;
        }
    }
}
