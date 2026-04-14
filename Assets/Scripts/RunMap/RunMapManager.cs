using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.RunMap
{
    public class RunMapManager : MonoBehaviour
    {
        public static RunMapManager Instance { get; private set; }

        [Header("Config")]
        [Tooltip("-1 = graine aléatoire")]
        public int mapSeed = -1;

        public List<List<RunNode>> Map      { get; private set; }
        public RunNode             CurrentNode { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            AudioManager.Instance.PlayMusic("music_runmap");

            // Créer RunPersistence s'il n'existe pas encore (point d'entrée de la run)
            if (RunPersistence.Instance == null)
            {
                var go = new GameObject("RunPersistence");
                go.AddComponent<RunPersistence>();
            }

            var persistence = RunPersistence.Instance;

            Debug.Log($"[RunMapManager] Awake — HasActiveRun={persistence?.HasActiveRun}, persistenceMap={persistence?.Map?.Count} rows");
            if (persistence != null && persistence.HasActiveRun)
            {
                // Reprendre la run existante
                Map         = persistence.Map;
                CurrentNode = persistence.CurrentNode;
                Debug.Log($"[RunMapManager] Run reprise — {Map?.Count} rows");
            }
            else
            {
                // Nouvelle run
                Map = RunMapGenerator.Generate(mapSeed);

                var startNode    = Map[0][0];
                startNode.state  = NodeState.Visited;
                CurrentNode      = startNode;
                foreach (var child in startNode.children)
                    child.state = NodeState.Available;

                // Sauvegarder dans la persistance
                if (persistence != null)
                {
                    persistence.Map         = Map;
                    persistence.CurrentNode = CurrentNode;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Visite d'un nœud ──────────────────────────────────────────────────

        public bool CanVisit(RunNode node) => node.state == NodeState.Available;

        public void VisitNode(RunNode node)
        {
            if (!CanVisit(node)) return;
            if (RunMapUI.Instance != null && RunMapUI.Instance.IsIntroPlaying) return;
            AudioManager.Instance.PlaySFX("sfx_node_select");

            var fromNode = CurrentNode;   // capturer avant mise à jour

            node.state  = NodeState.Visited;
            CurrentNode = node;
            RunPersistence.Instance?.RecordNodeVisited();

            foreach (var row in Map)
                foreach (var n in row)
                    if (n.state == NodeState.Available)
                        n.state = NodeState.Locked;

            foreach (var child in node.children)
                child.state = NodeState.Available;

            var persistence = RunPersistence.Instance;
            if (persistence != null)
                persistence.CurrentNode = CurrentNode;

            RunMapUI.Instance?.RefreshNodeStates();
            persistence?.SaveToDisk();

            // Animation caméra + verdissement arête, puis navigation
            var ui = RunMapUI.Instance;
            if (ui != null && fromNode != null)
                ui.PlayNavigationAnimation(fromNode, node, () => NavigateToNode(node));
            else
                NavigateToNode(node);
        }

        // ── Navigation ────────────────────────────────────────────────────────

        private void NavigateToNode(RunNode node)
        {
            switch (node.type)
            {
                case NodeType.Start:
                    break;
                case NodeType.Combat:
                case NodeType.Elite:
                case NodeType.Boss:
                    SceneManager.LoadScene("Combat");
                    break;
                case NodeType.Rest:
                case NodeType.Forge:
                case NodeType.Shop:
                case NodeType.Event:
                case NodeType.Mystery:
                    NodeEventManager.Instance?.ShowNode(node.type);
                    break;
            }
        }
    }
}
