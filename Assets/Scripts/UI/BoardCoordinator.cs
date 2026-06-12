using UnityEngine;
using RoguelikeTCG.Combat;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.UI
{
    public class BoardCoordinator : MonoBehaviour
    {
        [Header("Player Slots")]
        public BoardSlotUI[] playerSlots;
        public BoardSlotUI playerTerrain;

        [Header("Enemy Slots")]
        public BoardSlotUI[] enemySlots;
        public BoardSlotUI enemyTerrain;

        private void Start()
        {
            if (CombatManager.Instance == null || CombatManager.Instance.Board == null)
                return;

            CombatManager.Instance.OnTurnStarted += OnTurnStarted;
            CombatManager.Instance.Board.OnBoardChanged += RefreshAll;
            CombatManager.Instance.Board.OnUnitDied += OnUnitDied;
            CombatManager.Instance.Board.OnTerrainDiscarded += OnTerrainDiscarded;
            CombatManager.Instance.Board.OnDamageDealt += OnDamageDealt;

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (CombatManager.Instance == null || CombatManager.Instance.Board == null)
                return;

            CombatManager.Instance.OnTurnStarted -= OnTurnStarted;
            CombatManager.Instance.Board.OnBoardChanged -= RefreshAll;
            CombatManager.Instance.Board.OnUnitDied -= OnUnitDied;
            CombatManager.Instance.Board.OnTerrainDiscarded -= OnTerrainDiscarded;
            CombatManager.Instance.Board.OnDamageDealt -= OnDamageDealt;
        }

        private void OnTurnStarted(TurnSide _) => RefreshAll();
        private void OnUnitDied(CardInstance _, TurnSide __) => RefreshAll();
        private void OnTerrainDiscarded(CardInstance _, TurnSide __) => RefreshAll();
        private void OnDamageDealt(int _, TurnSide __) => RefreshAll();

        private void RefreshAll()
        {
            if (CombatManager.Instance == null || CombatManager.Instance.Board == null)
                return;

            var board = CombatManager.Instance.Board;

            for (int i = 0; i < BoardManager.SlotCount; i++)
            {
                if (playerSlots != null && i < playerSlots.Length && playerSlots[i] != null)
                    playerSlots[i].Refresh(board.PlayerUnits[i]);

                if (enemySlots != null && i < enemySlots.Length && enemySlots[i] != null)
                    enemySlots[i].Refresh(board.EnemyUnits[i]);
            }

            if (playerTerrain != null)
                playerTerrain.Refresh(board.PlayerTerrain);

            if (enemyTerrain != null)
                enemyTerrain.Refresh(board.EnemyTerrain);
        }
    }
}
