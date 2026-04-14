using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.RunMap;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.SaveSystem
{
    // ── Structures de données sérialisables ───────────────────────────────────

    [Serializable]
    public class NodeSaveData
    {
        public int row, col, type, state;
    }

    [Serializable]
    public class EdgeSaveData
    {
        public int fromRow, fromCol, toRow, toCol;
    }

    [Serializable]
    public class RunSaveData
    {
        public bool   hasActiveRun;
        public int    playerHP;
        public int    playerMaxHP;
        public int    playerGold;
        public float  mapScrollPosition;
        public int    currentNodeRow = -1;
        public int    currentNodeCol = -1;
        public string selectedCharacterName;
        public string[] playerDeckCardNames;
        public string[] playerRelicNames;
        public NodeSaveData[] nodes;
        public EdgeSaveData[] edges;
    }

    // ── Logique de sauvegarde / chargement ────────────────────────────────────

    public static class DiskSave
    {
        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, "run_save.json");

        public static bool HasSave() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[DiskSave] Sauvegarde supprimée.");
            }
        }

        // ── Sauvegarde ────────────────────────────────────────────────────────

        public static void Save(RunPersistence p)
        {
            var data = new RunSaveData
            {
                hasActiveRun          = p.HasActiveRun,
                playerHP              = p.PlayerHP,
                playerMaxHP           = p.PlayerMaxHP,
                playerGold            = p.PlayerGold,
                mapScrollPosition     = p.MapScrollPosition,
                currentNodeRow        = p.CurrentNode?.row ?? -1,
                currentNodeCol        = p.CurrentNode?.col ?? -1,
                selectedCharacterName = p.SelectedCharacter?.characterName ?? "",
            };

            // Reliques
            if (p.PlayerRelics != null && p.PlayerRelics.Count > 0)
            {
                data.playerRelicNames = new string[p.PlayerRelics.Count];
                for (int i = 0; i < p.PlayerRelics.Count; i++)
                    data.playerRelicNames[i] = p.PlayerRelics[i] != null ? p.PlayerRelics[i].relicName : "";
            }

            // Deck
            if (p.PlayerDeck != null && p.PlayerDeck.Count > 0)
            {
                data.playerDeckCardNames = new string[p.PlayerDeck.Count];
                for (int i = 0; i < p.PlayerDeck.Count; i++)
                    data.playerDeckCardNames[i] = p.PlayerDeck[i] != null ? p.PlayerDeck[i].cardName : "";
            }

            // Carte de run
            if (p.Map != null)
            {
                var nodeList = new List<NodeSaveData>();
                var edgeList = new List<EdgeSaveData>();

                foreach (var row in p.Map)
                {
                    foreach (var node in row)
                    {
                        nodeList.Add(new NodeSaveData
                        {
                            row   = node.row,
                            col   = node.col,
                            type  = (int)node.type,
                            state = (int)node.state,
                        });

                        foreach (var child in node.children)
                            edgeList.Add(new EdgeSaveData
                            {
                                fromRow = node.row, fromCol = node.col,
                                toRow   = child.row, toCol   = child.col,
                            });
                    }
                }

                data.nodes = nodeList.ToArray();
                data.edges = edgeList.ToArray();
            }

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
            Debug.Log($"[DiskSave] Sauvegardé → {SavePath}");
        }

        // ── Chargement ────────────────────────────────────────────────────────

        public static void LoadInto(RunPersistence p)
        {
            if (!HasSave()) return;

            RunSaveData data;
            try
            {
                data = JsonUtility.FromJson<RunSaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[DiskSave] Impossible de lire la sauvegarde : {e.Message}");
                return;
            }

            if (!data.hasActiveRun) return;

            p.PlayerHP          = data.playerHP;
            p.PlayerMaxHP       = data.playerMaxHP;
            p.PlayerGold        = data.playerGold;
            p.MapScrollPosition = data.mapScrollPosition;

            // Personnage sélectionné
            if (!string.IsNullOrEmpty(data.selectedCharacterName))
            {
                var charReg = Resources.Load<CharacterRegistry>("CharacterRegistry");
                if (charReg != null)
                    p.SelectedCharacter = charReg.FindByName(data.selectedCharacterName);
            }

            // Reliques
            if (data.playerRelicNames != null && data.playerRelicNames.Length > 0)
            {
                var relicReg = Resources.Load<RelicRegistry>("RelicRegistry");
                if (relicReg != null)
                {
                    p.PlayerRelics = new List<RelicData>();
                    foreach (var name in data.playerRelicNames)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        var relic = relicReg.FindByName(name);
                        if (relic != null) p.PlayerRelics.Add(relic);
                        else Debug.LogWarning($"[DiskSave] Relique inconnue : '{name}'");
                    }
                }
            }

            // Deck
            if (data.playerDeckCardNames != null && data.playerDeckCardNames.Length > 0)
            {
                var registry = Resources.Load<CardRegistry>("CardRegistry");
                if (registry == null)
                {
                    Debug.LogError("[DiskSave] CardRegistry introuvable dans Resources !");
                }
                else
                {
                    p.PlayerDeck = new List<CardData>();
                    foreach (var name in data.playerDeckCardNames)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        var card = registry.FindByName(name);
                        if (card != null)
                            p.PlayerDeck.Add(card);
                        else
                            Debug.LogWarning($"[DiskSave] Carte inconnue dans la sauvegarde : '{name}'");
                    }
                }
            }

            // Carte de run
            if (data.nodes != null && data.nodes.Length > 0)
            {
                // 1. Déterminer les dimensions de la grille
                int maxRow = 0;
                var colCountPerRow = new Dictionary<int, int>();
                foreach (var nd in data.nodes)
                {
                    if (nd.row > maxRow) maxRow = nd.row;
                    if (!colCountPerRow.ContainsKey(nd.row) || nd.col + 1 > colCountPerRow[nd.row])
                        colCountPerRow[nd.row] = nd.col + 1;
                }

                // 2. Recréer les nœuds
                var nodeGrid = new Dictionary<(int, int), RunNode>();
                foreach (var nd in data.nodes)
                {
                    var node   = new RunNode(nd.row, nd.col, (NodeType)nd.type);
                    node.state = (NodeState)nd.state;
                    nodeGrid[(nd.row, nd.col)] = node;
                }

                // 3. Rétablir les arêtes
                if (data.edges != null)
                {
                    foreach (var edge in data.edges)
                    {
                        if (!nodeGrid.TryGetValue((edge.fromRow, edge.fromCol), out var from)) continue;
                        if (!nodeGrid.TryGetValue((edge.toRow,   edge.toCol),   out var to))   continue;
                        if (!from.children.Contains(to)) from.children.Add(to);
                        if (!to.parents.Contains(from))  to.parents.Add(from);
                    }
                }

                // 4. Reconstruire la liste de listes
                var map = new List<List<RunNode>>();
                for (int r = 0; r <= maxRow; r++)
                {
                    var rowList  = new List<RunNode>();
                    int colCount = colCountPerRow.ContainsKey(r) ? colCountPerRow[r] : 0;
                    for (int c = 0; c < colCount; c++)
                        if (nodeGrid.TryGetValue((r, c), out var n))
                            rowList.Add(n);
                    map.Add(rowList);
                }

                p.Map = map;

                // 5. Nœud courant
                if (data.currentNodeRow >= 0 &&
                    nodeGrid.TryGetValue((data.currentNodeRow, data.currentNodeCol), out var cn))
                    p.CurrentNode = cn;
            }

            Debug.Log("[DiskSave] Run chargée depuis le disque.");
        }
    }
}
