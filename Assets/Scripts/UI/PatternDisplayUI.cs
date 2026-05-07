using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche les 3 motifs actifs du combat sous forme de mini-grilles 3x3.
    /// S'auto-construit dans Awake() — les 3 patternPanels sont assignés depuis la scène.
    /// </summary>
    public class PatternDisplayUI : MonoBehaviour
    {
        [Header("3 panneaux (assignés depuis la scène)")]
        [SerializeField] private RectTransform panel0;
        [SerializeField] private RectTransform panel1;
        [SerializeField] private RectTransform panel2;

        // Couleurs
        private static readonly Color CellPattern  = new Color(1f,   0.85f, 0.2f,  0.95f); // or vif
        private static readonly Color CellEmpty    = new Color(0.2f, 0.2f,  0.2f,  0.6f);  // gris foncé
        private static readonly Color PanelOpen    = new Color(0.12f,0.12f, 0.12f, 0.85f);
        private static readonly Color PanelClosedP = new Color(0.08f,0.28f, 0.08f, 0.92f); // vert joueur
        private static readonly Color PanelClosedE = new Color(0.28f,0.08f, 0.08f, 0.92f); // rouge ennemi

        private RectTransform[]   _panels;
        private Image[][]         _cells;       // [panelIdx][cellIdx 0-8]
        private TextMeshProUGUI[] _nameTMPs;
        private TextMeshProUGUI[] _statusTMPs;
        private bool              _built;

        private void Awake()
        {
            _panels    = new[] { panel0, panel1, panel2 };
            _cells     = new Image[3][];
            _nameTMPs  = new TextMeshProUGUI[3];
            _statusTMPs= new TextMeshProUGUI[3];

            for (int p = 0; p < 3; p++)
            {
                if (_panels[p] == null) continue;
                BuildPanel(p, _panels[p]);
            }
            _built = true;
        }

        private void BuildPanel(int p, RectTransform panel)
        {
            float cellSize  = 18f;
            float cellGap   = 2f;
            float gridW     = 3 * cellSize + 2 * cellGap;
            float gridH     = 3 * cellSize + 2 * cellGap;
            float labelH    = 16f;
            float statusH   = 14f;
            float panelPad  = 6f;

            // Fond du panneau
            var bg = panel.GetComponent<Image>();
            if (bg == null) bg = panel.gameObject.AddComponent<Image>();
            bg.color = PanelOpen;

            // Label nom (haut du panneau)
            var nameGO  = new GameObject("PatternName", typeof(RectTransform));
            nameGO.transform.SetParent(panel, false);
            var nameRT  = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 1f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.pivot     = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0f, -panelPad);
            nameRT.sizeDelta = new Vector2(0f, labelH);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.fontSize  = 8f;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color     = Color.white;
            nameTMP.raycastTarget = false;
            _nameTMPs[p] = nameTMP;

            // Mini-grille (centrée)
            var gridGO = new GameObject("MiniGrid", typeof(RectTransform));
            gridGO.transform.SetParent(panel, false);
            var gridRT = gridGO.GetComponent<RectTransform>();
            gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.pivot     = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = new Vector2(0f, 4f);
            gridRT.sizeDelta = new Vector2(gridW, gridH);

            _cells[p] = new Image[9];
            for (int i = 0; i < 9; i++)
            {
                int r = i / 3;
                int c = i % 3;
                var cellGO = new GameObject($"Cell_{i}", typeof(RectTransform));
                cellGO.transform.SetParent(gridGO.transform, false);
                var cellRT = cellGO.GetComponent<RectTransform>();
                cellRT.anchorMin = cellRT.anchorMax = new Vector2(0f, 1f);
                cellRT.pivot     = new Vector2(0f, 1f);
                cellRT.anchoredPosition = new Vector2(c * (cellSize + cellGap), -r * (cellSize + cellGap));
                cellRT.sizeDelta = new Vector2(cellSize, cellSize);
                var img = cellGO.AddComponent<Image>();
                img.color = CellEmpty;
                img.raycastTarget = false;
                _cells[p][i] = img;
            }

            // Label statut (bas du panneau)
            var statusGO = new GameObject("PatternStatus", typeof(RectTransform));
            statusGO.transform.SetParent(panel, false);
            var statusRT = statusGO.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 0f);
            statusRT.anchorMax = new Vector2(1f, 0f);
            statusRT.pivot     = new Vector2(0.5f, 0f);
            statusRT.anchoredPosition = new Vector2(0f, panelPad);
            statusRT.sizeDelta = new Vector2(0f, statusH);
            var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
            statusTMP.fontSize  = 7.5f;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.color     = new Color(1f, 0.85f, 0.2f);
            statusTMP.raycastTarget = false;
            _statusTMPs[p] = statusTMP;
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        public void Refresh(PatternData[] patterns, int[] closedBy)
        {
            if (!_built || patterns == null) return;

            for (int p = 0; p < 3; p++)
            {
                if (_panels[p] == null) continue;

                var pattern = p < patterns.Length ? patterns[p] : null;
                int status  = closedBy != null && p < closedBy.Length ? closedBy[p] : -1;

                // Fond panneau
                var bg = _panels[p].GetComponent<Image>();
                if (bg != null)
                    bg.color = status == -1 ? PanelOpen :
                               status == 0  ? PanelClosedP : PanelClosedE;

                // Nom
                if (_nameTMPs[p] != null)
                    _nameTMPs[p].text = pattern?.patternName ?? "—";

                // Statut / points
                if (_statusTMPs[p] != null)
                {
                    _statusTMPs[p].text = status == -1
                        ? (pattern != null ? $"+{pattern.Points} pts" : "")
                        : status == 0 ? "OK Vous"
                        : "OK Ennemi";
                    _statusTMPs[p].color = status == -1 ? new Color(1f, 0.85f, 0.2f) :
                                           status == 0  ? new Color(0.4f, 1f, 0.4f) :
                                                          new Color(1f, 0.4f, 0.4f);
                }

                // Cellules de la mini-grille
                if (_cells[p] == null) continue;
                var inPattern = pattern != null
                    ? new System.Collections.Generic.HashSet<int>(pattern.cellIndices)
                    : new System.Collections.Generic.HashSet<int>();

                for (int i = 0; i < 9; i++)
                {
                    if (_cells[p][i] == null) continue;
                    _cells[p][i].color = inPattern.Contains(i) ? CellPattern : CellEmpty;
                }
            }
        }
    }
}
