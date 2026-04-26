using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gère la grille 4×4 partagée entre joueur et ennemi.
    /// N'est PAS un singleton — composant attaché à un GO en scène, référencé par CombatManager.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public const int GRID_SIZE = 4;

        // Grille interne
        private CardInstance[,] _grid = new CardInstance[GRID_SIZE, GRID_SIZE];

        // Tracking des combos déjà scorés (reset à chaque manche)
        private HashSet<string> _playerScoredCombos = new();
        private HashSet<string> _enemyScoredCombos  = new();

        // Scores de la manche courante (modifiables depuis CombatManager pour les kills)
        public int PlayerRoundScore { get; set; }
        public int EnemyRoundScore  { get; set; }

        // ── Accès à la grille ─────────────────────────────────────────────────

        public bool IsEmpty(int r, int c)
        {
            if (!InBounds(r, c)) return false;
            return _grid[r, c] == null;
        }

        public CardInstance GetUnit(int r, int c)
        {
            if (!InBounds(r, c)) return null;
            return _grid[r, c];
        }

        /// <summary>
        /// Place une unité sur une case. Retourne false si occupée ou hors limites.
        /// Met à jour gridRow/gridCol sur l'instance.
        /// </summary>
        public bool PlaceUnit(CardInstance card, int r, int c)
        {
            if (!InBounds(r, c)) return false;
            if (_grid[r, c] != null) return false;

            _grid[r, c]   = card;
            card.gridRow  = r;
            card.gridCol  = c;
            return true;
        }

        /// <summary>
        /// Retire une unité de la case et libère gridRow/gridCol.
        /// N'envoie PAS en défausse — c'est la responsabilité de l'appelant.
        /// </summary>
        public void RemoveUnit(int r, int c)
        {
            if (!InBounds(r, c)) return;
            var unit = _grid[r, c];
            if (unit != null)
            {
                unit.gridRow = -1;
                unit.gridCol = -1;
            }
            _grid[r, c] = null;
        }

        /// <summary>
        /// Retire l'unité par référence directe (utile quand on connaît l'instance mais pas la case).
        /// </summary>
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
            for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                var u = _grid[r, c];
                if (u != null && u.isPlayerCard == isPlayer)
                    result.Add(u);
            }
            return result;
        }

        public List<CardInstance> GetAllUnitsOnGrid()
        {
            var result = new List<CardInstance>();
            for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
                if (_grid[r, c] != null) result.Add(_grid[r, c]);
            return result;
        }

        // ── Countdown & attaques ──────────────────────────────────────────────

        /// <summary>
        /// Décrémente tous les CD de 1. Retourne la liste des unités dont le CD atteint 0.
        /// </summary>
        public List<CardInstance> TickCountdowns()
        {
            var readyToAttack = new List<CardInstance>();
            for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                var u = _grid[r, c];
                if (u == null) continue;
                u.currentCountdown--;
                if (u.currentCountdown <= 0)
                    readyToAttack.Add(u);
            }
            return readyToAttack;
        }

        /// <summary>
        /// Réinitialise le CD d'une unité à sa valeur de base (CardData.countdown).
        /// </summary>
        public void ResetCountdown(CardInstance unit)
        {
            unit.currentCountdown = unit.data.countdown;
        }

        // ── Scoring ───────────────────────────────────────────────────────────

        /// <summary>
        /// Vérifie les nouvelles combinaisons joueur, les score immédiatement.
        /// Retourne les points gagnés lors de cet appel.
        /// </summary>
        public int CheckAndScorePlayer()
        {
            int pts = ScoringSystem.CheckAndScore(_grid, isPlayer: true, _playerScoredCombos);
            PlayerRoundScore += pts;
            return pts;
        }

        /// <summary>
        /// Vérifie les nouvelles combinaisons ennemies, les score immédiatement.
        /// Retourne les points gagnés lors de cet appel.
        /// </summary>
        public int CheckAndScoreEnemy()
        {
            int pts = ScoringSystem.CheckAndScore(_grid, isPlayer: false, _enemyScoredCombos);
            EnemyRoundScore += pts;
            return pts;
        }

        /// <summary>
        /// Score le keyword Dominance : +1 pt par unité alliée survivante en fin de manche.
        /// </summary>
        public int ScoreDominance(bool isPlayer)
        {
            int pts = 0;
            var units = GetAllUnits(isPlayer);
            foreach (var u in units)
                if (u.data.keyword == UnitKeyword.Dominance)
                    pts++;
            if (isPlayer) PlayerRoundScore += pts;
            else          EnemyRoundScore  += pts;
            return pts;
        }

        // ── Fin de manche ─────────────────────────────────────────────────────

        /// <summary>
        /// Vide entièrement la grille. Retourne toutes les unités retirées (à envoyer en défausse).
        /// Reset les scores de manche et les combos trackés.
        /// </summary>
        public List<CardInstance> ClearGrid()
        {
            var survivors = GetAllUnitsOnGrid();
            for (int r = 0; r < GRID_SIZE; r++)
            for (int c = 0; c < GRID_SIZE; c++)
            {
                var u = _grid[r, c];
                if (u != null) { u.gridRow = -1; u.gridCol = -1; }
                _grid[r, c] = null;
            }
            return survivors;
        }

        /// <summary>
        /// Reset les scores et combos trackés pour la nouvelle manche.
        /// </summary>
        public void ResetRoundTracking()
        {
            PlayerRoundScore = 0;
            EnemyRoundScore  = 0;
            _playerScoredCombos.Clear();
            _enemyScoredCombos.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool InBounds(int r, int c) =>
            r >= 0 && r < GRID_SIZE && c >= 0 && c < GRID_SIZE;

        /// <summary>
        /// Retourne la grille brute (lecture seule pour ScoringSystem et animations).
        /// </summary>
        public CardInstance[,] GetGridRef() => _grid;

        /// <summary>
        /// Retourne les voisins orthogonaux d'une case (pour attaques directionnelles).
        /// Filtre selon les AttackDirection de l'unité.
        /// </summary>
        public List<(int r, int c)> GetAttackTargets(int r, int c, AttackDirection dirs)
        {
            var targets = new List<(int, int)>();
            if ((dirs & AttackDirection.Up)    != 0 && r > 0)         targets.Add((r - 1, c));
            if ((dirs & AttackDirection.Down)  != 0 && r < GRID_SIZE - 1) targets.Add((r + 1, c));
            if ((dirs & AttackDirection.Left)  != 0 && c > 0)         targets.Add((r, c - 1));
            if ((dirs & AttackDirection.Right) != 0 && c < GRID_SIZE - 1) targets.Add((r, c + 1));
            return targets;
        }
    }
}
