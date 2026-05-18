using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Grille 2x5 : Row 0 = ligne ennemie, Row 1 = ligne joueur.
    /// Chaque case [1,c] fait face à [0,c] — duel de colonne.
    /// 1 unité max par case.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public const int ROWS = 2;  // 0 = ennemi, 1 = joueur
        public const int COLS = 5;

        // Stockage interne : [row, col]
        private CardInstance[,] _grid = new CardInstance[ROWS, COLS];

        // ── Bounds ─────────────────────────────────────────────────────────────

        public static bool InBounds(int row, int col) =>
            row >= 0 && row < ROWS && col >= 0 && col < COLS;

        // ── Accès ──────────────────────────────────────────────────────────────

        public bool IsEmpty(int row, int col)
        {
            if (!InBounds(row, col)) return false;
            return _grid[row, col] == null;
        }

        public bool IsPlayerRow(int row) => row == 1;
        public bool IsEnemyRow(int row)  => row == 0;

        public CardInstance GetUnit(int row, int col)
        {
            if (!InBounds(row, col)) return null;
            return _grid[row, col];
        }

        // ── Placement ──────────────────────────────────────────────────────────

        /// <summary>Place une unité sur une case. Retourne false si occupée ou hors limites.</summary>
        public bool PlaceUnit(CardInstance card, int row, int col)
        {
            if (!InBounds(row, col)) return false;
            if (_grid[row, col] != null) return false;

            _grid[row, col] = card;
            card.row        = row;
            card.col        = col;
            return true;
        }

        /// <summary>Retire une unité de la case et libère row/col.</summary>
        public void RemoveUnit(int row, int col)
        {
            if (!InBounds(row, col)) return;
            var unit = _grid[row, col];
            if (unit != null) { unit.row = -1; unit.col = -1; }
            _grid[row, col] = null;
        }

        /// <summary>Retire l'unité par référence directe.</summary>
        public void RemoveUnit(CardInstance unit)
        {
            if (unit == null) return;
            if (InBounds(unit.row, unit.col))
                RemoveUnit(unit.row, unit.col);
        }

        /// <summary>Déplace une unité de (fromRow,fromCol) vers (toRow,toCol).</summary>
        public bool MoveUnit(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (!InBounds(fromRow, fromCol) || !InBounds(toRow, toCol)) return false;
            var unit = GetUnit(fromRow, fromCol);
            if (unit == null) return false;
            if (!IsEmpty(toRow, toCol)) return false;

            RemoveUnit(fromRow, fromCol);
            PlaceUnit(unit, toRow, toCol);
            return true;
        }

        // ── Enumération ────────────────────────────────────────────────────────

        public List<CardInstance> GetAllUnits(bool isPlayer)
        {
            var result = new List<CardInstance>();
            for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
            {
                var u = _grid[r, c];
                if (u != null && u.isPlayerCard == isPlayer)
                    result.Add(u);
            }
            return result;
        }

        public List<CardInstance> GetPlayerUnits() => GetAllUnits(true);
        public List<CardInstance> GetEnemyUnits()  => GetAllUnits(false);

        public List<CardInstance> GetAllUnitsOnGrid()
        {
            var result = new List<CardInstance>();
            for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                if (_grid[r, c] != null) result.Add(_grid[r, c]);
            return result;
        }

        // ── Adjacence (même ligne, gauche/droite) ─────────────────────────────

        /// <summary>Retourne les voisins gauche et droit d'une case (même ligne).</summary>
        public List<CardInstance> GetAdjacentSameRow(int row, int col)
        {
            var result = new List<CardInstance>();
            if (InBounds(row, col - 1) && _grid[row, col - 1] != null)
                result.Add(_grid[row, col - 1]);
            if (InBounds(row, col + 1) && _grid[row, col + 1] != null)
                result.Add(_grid[row, col + 1]);
            return result;
        }

        // ── Lane leak ──────────────────────────────────────────────────────────

        /// <summary>
        /// Case joueur [1,c] occupée ET case ennemie [0,c] vide
        /// → l'unité joueur pourrait faire un lane leak vers l'ennemi.
        /// </summary>
        public bool IsPlayerLaneLeak(int col) =>
            InBounds(1, col) && _grid[1, col] != null &&
            InBounds(0, col) && _grid[0, col] == null;

        /// <summary>
        /// Case ennemie [0,c] occupée ET case joueur [1,c] vide
        /// → l'unité ennemie pourrait faire un lane leak vers le joueur.
        /// </summary>
        public bool IsEnemyLaneLeak(int col) =>
            InBounds(0, col) && _grid[0, col] != null &&
            InBounds(1, col) && _grid[1, col] == null;

        // ── Vide la grille ─────────────────────────────────────────────────────

        /// <summary>Vide la grille et retourne toutes les unités (à envoyer en défausse).</summary>
        public List<CardInstance> ClearGrid()
        {
            var all = GetAllUnitsOnGrid();
            for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
            {
                var u = _grid[r, c];
                if (u != null) { u.row = -1; u.col = -1; }
                _grid[r, c] = null;
            }
            return all;
        }

        // ── Reset flags de tour ────────────────────────────────────────────────

        /// <summary>Reset les flags de tour (tookDamageThisTurn, isFrozen) sur toutes les unités.</summary>
        public void ResetTurnFlags()
        {
            for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
            {
                var u = _grid[r, c];
                if (u != null)
                {
                    u.tookDamageThisTurn = false;
                    u.isFrozen           = false;
                }
            }
        }

        // ── Helpers pour CombatAnimator ────────────────────────────────────────

        /// <summary>Retourne la grille brute (lecture seule, pour les animations).</summary>
        public CardInstance[,] GetGridRef() => _grid;

        // ── Compatibilité API (ScoringSystem) — supprimée ─────────────────────
        // Les méthodes liées aux motifs (CheckAndScorePlayer, etc.) sont supprimées.
        // GetAttackTargets est supprimée — remplacée par la logique colonne-vs-colonne dans CombatManager.
    }
}
