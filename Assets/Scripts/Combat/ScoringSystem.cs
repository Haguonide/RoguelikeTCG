using System.Collections.Generic;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Classe statique utilitaire : détecte et score les combinaisons sur la grille 4×4.
    /// Scoring :
    ///   3 en ligne H/V    → 2 pts
    ///   3 en diagonale    → 3 pts
    ///   Carré 2×2         → 2 pts
    ///   Kill d'une unité ennemie (géré par CombatManager) → 1 pt
    ///   Dominance (fin de manche) → 1 pt par unité survivante
    ///
    /// Chaque combinaison spécifique ne score qu'une fois par manche.
    /// La clé de combo est une string "r0c0-r1c1-r2c2" avec les cases triées.
    /// </summary>
    public static class ScoringSystem
    {
        // ── Clé unique ────────────────────────────────────────────────────────

        public static string ComboKey((int r, int c) a, (int r, int c) b, (int r, int c) c2)
        {
            // Tri des 3 cases pour une clé ordre-indépendante
            var arr = new[] { a, b, c2 };
            System.Array.Sort(arr, (x, y) => x.r != y.r ? x.r.CompareTo(y.r) : x.c.CompareTo(y.c));
            return $"{arr[0].r}{arr[0].c}-{arr[1].r}{arr[1].c}-{arr[2].r}{arr[2].c}";
        }

        private static string ComboKey4_Internal((int r, int c)[] cells)
        {
            System.Array.Sort(cells, (a, b) => a.r != b.r ? a.r.CompareTo(b.r) : a.c.CompareTo(b.c));
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < cells.Length; i++)
            {
                if (i > 0) sb.Append('-');
                sb.Append(cells[i].r).Append(cells[i].c);
            }
            return sb.ToString();
        }

        // ── Vérification complète ─────────────────────────────────────────────

        /// <summary>
        /// Vérifie toutes les combinaisons possibles pour le camp indiqué.
        /// Retourne les points gagnés et met à jour alreadyScored.
        /// </summary>
        public static int CheckAndScore(CardInstance[,] grid, bool isPlayer,
                                        HashSet<string> alreadyScored)
        {
            int pts = 0;

            // Lignes horizontales — 4 lignes de 4, cherche toutes les triplets H
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c <= 1; c++) // 4-3 = 1 : triplets (c,c+1,c+2) et (c+1,c+2,c+3)
                {
                    // Triplet c, c+1, c+2
                    if (IsAlly(grid, r, c, isPlayer) &&
                        IsAlly(grid, r, c + 1, isPlayer) &&
                        IsAlly(grid, r, c + 2, isPlayer))
                    {
                        var key = ComboKey((r, c), (r, c + 1), (r, c + 2));
                        if (alreadyScored.Add(key)) pts += 2;
                    }
                }
                // Triplet [0,1,2,3] → aussi [1,2,3]
                if (IsAlly(grid, r, 1, isPlayer) &&
                    IsAlly(grid, r, 2, isPlayer) &&
                    IsAlly(grid, r, 3, isPlayer))
                {
                    var key = ComboKey((r, 1), (r, 2), (r, 3));
                    if (alreadyScored.Add(key)) pts += 2;
                }
            }

            // Lignes verticales
            for (int c = 0; c < 4; c++)
            {
                for (int r = 0; r <= 1; r++)
                {
                    if (IsAlly(grid, r, c, isPlayer) &&
                        IsAlly(grid, r + 1, c, isPlayer) &&
                        IsAlly(grid, r + 2, c, isPlayer))
                    {
                        var key = ComboKey((r, c), (r + 1, c), (r + 2, c));
                        if (alreadyScored.Add(key)) pts += 2;
                    }
                }
                if (IsAlly(grid, 1, c, isPlayer) &&
                    IsAlly(grid, 2, c, isPlayer) &&
                    IsAlly(grid, 3, c, isPlayer))
                {
                    var key = ComboKey((1, c), (2, c), (3, c));
                    if (alreadyScored.Add(key)) pts += 2;
                }
            }

            // Diagonales descendantes (top-left → bottom-right)
            for (int r = 0; r <= 1; r++)
            {
                for (int c = 0; c <= 1; c++)
                {
                    if (IsAlly(grid, r, c, isPlayer) &&
                        IsAlly(grid, r + 1, c + 1, isPlayer) &&
                        IsAlly(grid, r + 2, c + 2, isPlayer))
                    {
                        var key = ComboKey((r, c), (r + 1, c + 1), (r + 2, c + 2));
                        if (alreadyScored.Add(key)) pts += 3;
                    }
                }
            }
            // Diag desc : [1,1][2,2][3,3]
            if (IsAlly(grid, 1, 1, isPlayer) && IsAlly(grid, 2, 2, isPlayer) && IsAlly(grid, 3, 3, isPlayer))
            {
                var key = ComboKey((1, 1), (2, 2), (3, 3));
                if (alreadyScored.Add(key)) pts += 3;
            }

            // Diagonales montantes (bottom-left → top-right)
            for (int r = 2; r < 4; r++)
            {
                for (int c = 0; c <= 1; c++)
                {
                    if (IsAlly(grid, r, c, isPlayer) &&
                        IsAlly(grid, r - 1, c + 1, isPlayer) &&
                        IsAlly(grid, r - 2, c + 2, isPlayer))
                    {
                        var key = ComboKey((r, c), (r - 1, c + 1), (r - 2, c + 2));
                        if (alreadyScored.Add(key)) pts += 3;
                    }
                }
            }
            // Diag mont : [2,1][1,2][0,3]
            if (IsAlly(grid, 2, 1, isPlayer) && IsAlly(grid, 1, 2, isPlayer) && IsAlly(grid, 0, 3, isPlayer))
            {
                var key = ComboKey((2, 1), (1, 2), (0, 3));
                if (alreadyScored.Add(key)) pts += 3;
            }

            // Carrés 2×2 — 9 possibles sur une grille 4×4
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (IsAlly(grid, r, c, isPlayer) &&
                        IsAlly(grid, r, c + 1, isPlayer) &&
                        IsAlly(grid, r + 1, c, isPlayer) &&
                        IsAlly(grid, r + 1, c + 1, isPlayer))
                    {
                        var key = ComboKey4((r, c), (r, c + 1), (r + 1, c), (r + 1, c + 1));
                        if (alreadyScored.Add(key)) pts += 2;
                    }
                }
            }

            return pts;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool IsAlly(CardInstance[,] grid, int r, int c, bool isPlayer)
        {
            if (r < 0 || r >= 4 || c < 0 || c >= 4) return false;
            var u = grid[r, c];
            return u != null && u.IsAlive && u.isPlayerCard == isPlayer;
        }

        /// <summary>
        /// Retourne les voisins orthogonaux valides d'une case.
        /// </summary>
        public static List<(int r, int c)> GetOrthogonalNeighbors(int r, int c)
        {
            var list = new List<(int, int)>();
            if (r > 0) list.Add((r - 1, c));
            if (r < 3) list.Add((r + 1, c));
            if (c > 0) list.Add((r, c - 1));
            if (c < 3) list.Add((r, c + 1));
            return list;
        }

        /// <summary>
        /// Retourne tous les voisins (orthogonaux + diagonaux) d'une case.
        /// </summary>
        public static List<(int r, int c)> GetAllNeighbors(int r, int c)
        {
            var list = new List<(int, int)>();
            for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < 4 && nc >= 0 && nc < 4)
                    list.Add((nr, nc));
            }
            return list;
        }

        /// <summary>
        /// Case adjacente dans une direction donnée (pour Percée).
        /// Retourne (-1,-1) si hors grille.
        /// </summary>
        public static (int r, int c) GetCellInDirection(int r, int c, Data.AttackDirection dir)
        {
            return dir switch
            {
                Data.AttackDirection.Up    => (r - 1, c),
                Data.AttackDirection.Down  => (r + 1, c),
                Data.AttackDirection.Left  => (r, c - 1),
                Data.AttackDirection.Right => (r, c + 1),
                _ => (-1, -1),
            };
        }

        /// <summary>
        /// Clé pour les combos de carrés 2×2 (4 cases).
        /// </summary>
        public static string ComboKey4(
            (int r, int c) a, (int r, int c) b,
            (int r, int c) c2, (int r, int c) d)
        {
            return ComboKey4_Internal(new[] { a, b, c2, d });
        }
    }
}
