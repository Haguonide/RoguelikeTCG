using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.RunMap
{
    /// <summary>
    /// Génère un arbre de nœuds procédural pour la carte de run.
    /// 10 lignes, maximum 4 nœuds par ligne.
    /// Ligne 0 = départ (1 Combat), Ligne 9 = boss (1 Boss).
    /// </summary>
    public static class RunMapGenerator
    {
        private const int ROWS             = 10;
        private const int MAX_NODES_PER_ROW = 4;
        private const int MIN_NODES_PER_ROW = 1;

        public static List<List<RunNode>> Generate(int seed = -1)
        {
            if (seed >= 0) Random.InitState(seed);

            var map = new List<List<RunNode>>();

            // 1. Créer les nœuds ligne par ligne
            for (int r = 0; r < ROWS; r++)
            {
                int count;
                if (r == 0 || r == ROWS - 1) count = 1;
                else                          count = Random.Range(MIN_NODES_PER_ROW, MAX_NODES_PER_ROW + 1);

                var row = new List<RunNode>();
                for (int c = 0; c < count; c++)
                    row.Add(new RunNode(r, c, NodeType.Combat)); // type défini plus bas

                map.Add(row);
            }

            // 2. Connecter les lignes (sans croisements)
            for (int r = 0; r < ROWS - 1; r++)
                ConnectRows(map[r], map[r + 1]);

            // 3. Assigner les types de nœuds
            AssignTypes(map);

            // 4. Ligne 0 disponible au départ
            foreach (var node in map[0])
                node.state = NodeState.Available;

            return map;
        }

        // -------------------------------------------------------
        // Connexions sans croisements
        // -------------------------------------------------------
        private static void ConnectRows(List<RunNode> current, List<RunNode> next)
        {
            int nc = current.Count;
            int nn = next.Count;

            // Chaque nœud courant reçoit un enfant proportionnel
            for (int i = 0; i < nc; i++)
            {
                int j = nn == 1 ? 0 : Mathf.RoundToInt((float)i / (nc - 1) * (nn - 1));
                j = Mathf.Clamp(j, 0, nn - 1);
                AddEdge(current[i], next[j]);
            }

            // S'assurer que chaque nœud de la ligne suivante a au moins un parent
            for (int j = 0; j < nn; j++)
            {
                if (next[j].parents.Count == 0)
                {
                    int i = nc == 1 ? 0 : Mathf.RoundToInt((float)j / (nn - 1) * (nc - 1));
                    i = Mathf.Clamp(i, 0, nc - 1);
                    AddEdge(current[i], next[j]);
                }
            }

            // 0–1 connexion bonus aléatoire
            int extras = Random.Range(0, 2);
            for (int e = 0; e < extras; e++)
            {
                int i = Random.Range(0, nc);
                // Index enfant dans une plage proportionnelle pour limiter les croisements
                int jMin = Mathf.Max(0, Mathf.FloorToInt((float)i / nc * nn) - 1);
                int jMax = Mathf.Min(nn - 1, Mathf.CeilToInt((float)(i + 1) / nc * nn));
                int j = Random.Range(jMin, jMax + 1);
                AddEdge(current[i], next[j]);
            }
        }

        private static void AddEdge(RunNode parent, RunNode child)
        {
            if (!parent.children.Contains(child)) parent.children.Add(child);
            if (!child.parents.Contains(parent))  child.parents.Add(parent);
        }

        // -------------------------------------------------------
        // Assignation des types selon la ligne
        // -------------------------------------------------------
        private static void AssignTypes(List<List<RunNode>> map)
        {
            for (int r = 0; r < map.Count; r++)
                foreach (var node in map[r])
                    node.type = PickTypeForRow(r);
        }

        private static NodeType PickTypeForRow(int r)
        {
            if (r == 0)           return NodeType.Start;
            if (r == ROWS - 1)    return NodeType.Boss;

            // Avant-dernière ligne : repos ou boutique garantis
            if (r == ROWS - 2)
                return Random.value < 0.5f ? NodeType.Rest : NodeType.Shop;

            float roll = Random.value;

            // Lignes 1–3 : combat + événements légers
            if (r <= 3)
            {
                if (roll < 0.50f) return NodeType.Combat;
                if (roll < 0.68f) return NodeType.Event;
                if (roll < 0.78f) return NodeType.Mystery;
                if (roll < 0.88f) return NodeType.Shop;
                return NodeType.Rest;
            }
            // Lignes 4–6 : combat + élite + variété
            if (r <= 6)
            {
                if (roll < 0.30f) return NodeType.Combat;
                if (roll < 0.48f) return NodeType.Elite;
                if (roll < 0.62f) return NodeType.Event;
                if (roll < 0.72f) return NodeType.Shop;
                if (roll < 0.82f) return NodeType.Forge;
                if (roll < 0.91f) return NodeType.Rest;
                return NodeType.Mystery;
            }
            // Ligne 7 : combat + élite
            if (roll < 0.40f) return NodeType.Combat;
            if (roll < 0.65f) return NodeType.Elite;
            if (roll < 0.78f) return NodeType.Event;
            if (roll < 0.88f) return NodeType.Forge;
            return NodeType.Mystery;
        }
    }
}
