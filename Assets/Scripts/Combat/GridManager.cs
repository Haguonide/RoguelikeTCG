using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gère la grille 3x3 partagée entre joueur et ennemi.
    /// Les cases sont indexées 0-8 en row-major (0=TL, 4=centre, 8=BR).
    /// N'est PAS un singleton — composant attaché à un GO en scène, référencé par CombatManager.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public const int GRID_SIZE  = 3;
        public const int CELL_COUNT = GRID_SIZE * GRID_SIZE; // 9

        [Header("Système de motifs")]
        public PatternManager patternManager;

        // Grille interne — tableau 1D, index = row*GRID_SIZE + col
        private CardInstance[] _grid = new CardInstance[CELL_COUNT];

        // Scores de la manche courante
        public int PlayerRoundScore { get; set; }
        public int EnemyRoundScore  { get; set; }

        // ── Conversion index ↔ row/col ────────────────────────────────────────

        public static int ToIndex(int r, int c) => r * GRID_SIZE + c;
        public static (int r, int c) ToRowCol(int index) => (index / GRID_SIZE, index % GRID_SIZE);
        private static bool InBounds(int index) => index >= 0 && index < CELL_COUNT;
        public static bool InBounds(int r, int c) => r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE;

        // ── Accès à la grille ─────────────────────────────────────────────────

        public bool IsEmpty(int index)
        {
            if (!InBounds(index)) return false;
            return _grid[index] == null;
        }

        public bool IsEmpty(int r, int c) => IsEmpty(ToIndex(r, c));

        public CardInstance GetUnit(int index)
        {
            if (!InBounds(index)) return null;
            return _grid[index];
        }

        public CardInstance GetUnit(int r, int c) => GetUnit(ToIndex(r, c));

        /// <summary>
        /// Place une unité sur une case. Retourne false si occupée ou hors limites.
        /// Met à jour gridRow/gridCol sur l'instance.
        /// </summary>
        public bool PlaceUnit(CardInstance card, int r, int c)
        {
            int index = ToIndex(r, c);
            if (!InBounds(index)) return false;
            if (_grid[index] != null) return false;

            _grid[index]  = card;
            card.gridRow  = r;
            card.gridCol  = c;
            return true;
        }

        public bool PlaceUnit(CardInstance card, int index)
        {
            var (r, c) = ToRowCol(index);
            return PlaceUnit(card, r, c);
        }

        /// <summary>
        /// Retire une unité de la case et libère gridRow/gridCol.
        /// N'envoie PAS en défausse — responsabilité de l'appelant.
        /// </summary>
        public void RemoveUnit(int r, int c)
        {
            int index = ToIndex(r, c);
            if (!InBounds(index)) return;
            var unit = _grid[index];
            if (unit != null)
            {
                unit.gridRow = -1;
                unit.gridCol = -1;
            }
            _grid[index] = null;
        }

        public void RemoveUnit(int index)
        {
            var (r, c) = ToRowCol(index);
            RemoveUnit(r, c);
        }

        /// <summary>Retire l'unité par référence directe.</summary>
        public void RemoveUnit(CardInstance unit)
        {
            if (unit == null) return;
            if (InBounds(unit.gridRow, unit.gridCol))
                RemoveUnit(unit.gridRow, unit.gridCol);
        }

        // ── Enumération ───────────────────────────────────────────────────────

        public List<CardInstance> GetAllUnits(bool isPlayer)
        {
            var result = new List<CardInstance>();
            for (int i = 0; i < CELL_COUNT; i++)
            {
                var u = _grid[i];
                if (u != null && u.isPlayerCard == isPlayer)
                    result.Add(u);
            }
            return result;
        }

        public List<CardInstance> GetAllUnitsOnGrid()
        {
            var result = new List<CardInstance>();
            for (int i = 0; i < CELL_COUNT; i++)
                if (_grid[i] != null) result.Add(_grid[i]);
            return result;
        }

        // ── Countdown & attaques ──────────────────────────────────────────────

        /// <summary>Décrémente tous les CD de 1. Retourne les unités dont CD atteint 0.</summary>
        public List<CardInstance> TickCountdowns()
        {
            var readyToAttack = new List<CardInstance>();
            for (int i = 0; i < CELL_COUNT; i++)
            {
                var u = _grid[i];
                if (u == null) continue;
                u.currentCountdown--;
                if (u.currentCountdown <= 0)
                    readyToAttack.Add(u);
            }
            return readyToAttack;
        }

        /// <summary>Réinitialise le CD d'une unité à sa valeur de base.</summary>
        public void ResetCountdown(CardInstance unit)
        {
            unit.currentCountdown = unit.data.countdown;
        }

        // ── Scoring via PatternManager ────────────────────────────────────────

        /// <summary>
        /// Vérifie si la pose en (r,c) complète un motif ouvert côté joueur.
        /// Retourne les points encaissés et met à jour PlayerRoundScore.
        /// </summary>
        public int CheckAndScorePlayer(int r, int c)
        {
            if (patternManager == null) return 0;
            int pts = patternManager.CheckAndScore(
                ToIndex(r, c),
                isPlayer: true,
                GetOwnerAt);
            PlayerRoundScore += pts;
            return pts;
        }

        /// <summary>
        /// Vérifie si la pose en (r,c) complète un motif ouvert côté ennemi.
        /// Retourne les points encaissés et met à jour EnemyRoundScore.
        /// </summary>
        public int CheckAndScoreEnemy(int r, int c)
        {
            if (patternManager == null) return 0;
            int pts = patternManager.CheckAndScore(
                ToIndex(r, c),
                isPlayer: false,
                GetOwnerAt);
            EnemyRoundScore += pts;
            return pts;
        }

        /// <summary>
        /// Score le keyword Dominance : +1 pt par unité survivante ayant ce keyword.
        /// </summary>
        public int ScoreDominance(bool isPlayer)
        {
            int pts = 0;
            foreach (var u in GetAllUnits(isPlayer))
                if (u.data.keyword == UnitKeyword.Dominance)
                    pts++;
            if (isPlayer) PlayerRoundScore += pts;
            else          EnemyRoundScore  += pts;
            return pts;
        }

        // ── Fin de manche ─────────────────────────────────────────────────────

        /// <summary>
        /// Vide la grille et retourne toutes les unités (à envoyer en défausse).
        /// </summary>
        public List<CardInstance> ClearGrid()
        {
            var survivors = GetAllUnitsOnGrid();
            for (int i = 0; i < CELL_COUNT; i++)
            {
                var u = _grid[i];
                if (u != null) { u.gridRow = -1; u.gridCol = -1; }
                _grid[i] = null;
            }
            return survivors;
        }

        /// <summary>Reset les scores de manche et réouvre les motifs.</summary>
        public void ResetRoundTracking()
        {
            PlayerRoundScore = 0;
            EnemyRoundScore  = 0;
            patternManager?.ResetRound();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne le propriétaire d'une case : -1=vide, 0=joueur, 1=ennemi.
        /// Utilisé par PatternManager.CheckAndScore.
        /// </summary>
        public int GetOwnerAt(int cellIndex)
        {
            if (!InBounds(cellIndex)) return -1;
            var u = _grid[cellIndex];
            if (u == null) return -1;
            return u.isPlayerCard ? 0 : 1;
        }

        /// <summary>Retourne les cibles d'attaque selon les directions de l'unité.</summary>
        public List<(int r, int c)> GetAttackTargets(int r, int c, AttackDirection dirs)
        {
            var targets = new List<(int, int)>();
            if ((dirs & AttackDirection.Up)    != 0 && r > 0)              targets.Add((r - 1, c));
            if ((dirs & AttackDirection.Down)  != 0 && r < GRID_SIZE - 1)  targets.Add((r + 1, c));
            if ((dirs & AttackDirection.Left)  != 0 && c > 0)              targets.Add((r, c - 1));
            if ((dirs & AttackDirection.Right) != 0 && c < GRID_SIZE - 1)  targets.Add((r, c + 1));
            return targets;
        }

        /// <summary>
        /// Retourne la grille brute (lecture seule, pour les animations).
        /// Index = row*GRID_SIZE + col.
        /// </summary>
        public CardInstance[] GetGridRef() => _grid;
    }
}
