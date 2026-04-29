using System.Collections.Generic;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Utilitaires statiques de géométrie pour la grille 3x3.
    /// Le scoring par motifs est délégué à PatternManager (via GridManager).
    /// Cette classe conserve uniquement les helpers de voisinage et de direction
    /// utilisés par CombatManager (Épine, Explosion, Percée, Légion, Ralliement).
    /// </summary>
    public static class ScoringSystem
    {
        // ── Voisinage ─────────────────────────────────────────────────────────

        /// <summary>Retourne les voisins orthogonaux valides d'une case (grille 3x3).</summary>
        public static List<(int r, int c)> GetOrthogonalNeighbors(int r, int c)
        {
            var list = new List<(int, int)>();
            if (r > 0)                          list.Add((r - 1, c));
            if (r < GridManager.GRID_SIZE - 1)  list.Add((r + 1, c));
            if (c > 0)                          list.Add((r, c - 1));
            if (c < GridManager.GRID_SIZE - 1)  list.Add((r, c + 1));
            return list;
        }

        /// <summary>Retourne tous les voisins (orthogonaux + diagonaux) d'une case (grille 3x3).</summary>
        public static List<(int r, int c)> GetAllNeighbors(int r, int c)
        {
            var list = new List<(int, int)>();
            for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (GridManager.InBounds(nr, nc))
                    list.Add((nr, nc));
            }
            return list;
        }

        /// <summary>
        /// Case adjacente dans une direction donnée (pour Percée).
        /// Retourne (-1,-1) si hors grille.
        /// </summary>
        public static (int r, int c) GetCellInDirection(int r, int c, AttackDirection dir)
        {
            return dir switch
            {
                AttackDirection.Up    => (r - 1, c),
                AttackDirection.Down  => (r + 1, c),
                AttackDirection.Left  => (r, c - 1),
                AttackDirection.Right => (r, c + 1),
                _ => (-1, -1),
            };
        }
    }
}
