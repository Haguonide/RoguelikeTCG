using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// One lane of the battlefield — 6 cells shared by both sides.
    /// Cells 0-2 are the player deployment zone (left), cells 3-5 are the enemy zone (right).
    /// Player units advance left→right (increasing cell index).
    /// Enemy units advance right→left (decreasing cell index).
    /// </summary>
    public class CombatLane : MonoBehaviour
    {
        public const int LANE_LENGTH = 6;
        public const int PLAYER_MAX_CELL   = 2;  // rightmost cell a player unit starts in (advance boundary)
        public const int ENEMY_MIN_CELL    = 3;  // leftmost cell an enemy unit starts in (advance boundary)
        public const int PLAYER_DEPLOY_CELL = 0; // only cell where player may place a unit
        public const int ENEMY_DEPLOY_CELL  = 5; // only cell where enemy may place a unit

        public int laneIndex;

        private readonly CardInstance[] cells = new CardInstance[LANE_LENGTH];

        // ── Read ──────────────────────────────────────────────────────────────

        public bool          IsOccupied(int cell) => cells[cell] != null;
        public CardInstance  GetUnit(int cell)    => cells[cell];

        public List<(int cell, CardInstance unit)> GetPlayerUnits()
        {
            var result = new List<(int, CardInstance)>();
            for (int i = 0; i < LANE_LENGTH; i++)
                if (cells[i] != null && cells[i].isPlayerCard)
                    result.Add((i, cells[i]));
            return result;
        }

        public List<(int cell, CardInstance unit)> GetEnemyUnits()
        {
            var result = new List<(int, CardInstance)>();
            for (int i = 0; i < LANE_LENGTH; i++)
                if (cells[i] != null && !cells[i].isPlayerCard)
                    result.Add((i, cells[i]));
            return result;
        }

        public bool HasAnyUnit()
        {
            for (int i = 0; i < LANE_LENGTH; i++)
                if (cells[i] != null) return true;
            return false;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public void PlaceUnit(CardInstance unit, int cell)
        {
            cells[cell] = unit;
            // gridRow/gridCol sont les nouveaux champs de position dans CardInstance
            // Pour compatibilité avec l'ancien système : on encode laneIndex dans gridRow et cell dans gridCol
            unit.gridRow = laneIndex;
            unit.gridCol = cell;
        }

        public void ClearCell(int cell)
        {
            if (cells[cell] == null) return;
            cells[cell].gridRow = -1;
            cells[cell].gridCol = -1;
            cells[cell]         = null;
        }

        public void ClearAll()
        {
            for (int i = 0; i < LANE_LENGTH; i++) ClearCell(i);
        }

        // ── Placement helpers ─────────────────────────────────────────────────

        /// Returns the leftmost empty cell in [min..max], or -1 if all occupied.
        public int FindFirstEmpty(int minCell, int maxCell)
        {
            for (int i = minCell; i <= maxCell; i++)
                if (cells[i] == null) return i;
            return -1;
        }

        /// Returns the rightmost empty cell in [min..max], or -1 if all occupied.
        public int FindLastEmpty(int minCell, int maxCell)
        {
            for (int i = maxCell; i >= minCell; i--)
                if (cells[i] == null) return i;
            return -1;
        }
    }
}
