using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche les 3 motifs actifs dans un cadre CRT unique (vert phosphore).
    /// Les 3 panel refs (panel0/1/2) sont arrangés en tiers horizontaux à l'intérieur
    /// d'un écran partagé, séparés par des pointillés verts.
    /// </summary>
    public class PatternDisplayUI : MonoBehaviour
    {
        [Header("3 zones de motifs (assignées depuis la scène)")]
        [SerializeField] private RectTransform panel0;
        [SerializeField] private RectTransform panel1;
        [SerializeField] private RectTransform panel2;

        // CRT green phosphor palette
        private static readonly Color CRTGreen      = new Color(0f,    1f,    0.255f, 1f);
        private static readonly Color CRTGreenFaint  = new Color(0f,    0.09f, 0.02f,  1f);
        private static readonly Color CRTGreenBright = new Color(0.3f,  1f,    0.4f,   1f);
        private static readonly Color CRTRed         = new Color(1f,    0.25f, 0.25f,  1f);
        private static readonly Color FrameColor     = new Color(0.16f, 0.16f, 0.16f,  1f);
        private static readonly Color ScreenColor    = new Color(0.02f, 0.02f, 0.02f,  1f);
        private static readonly Color TintClosedP    = new Color(0f,    0.4f,  0.08f,  0.28f);
        private static readonly Color TintClosedE    = new Color(0.4f,  0.05f, 0.05f,  0.28f);

        private RectTransform[]   _panels;
        private Image[]           _panelTints;
        private Image[][]         _cells;
        private TextMeshProUGUI[] _nameTMPs;
        private TextMeshProUGUI[] _statusTMPs;
        private bool              _built;

        private void Awake()
        {
            _panels     = new[] { panel0, panel1, panel2 };
            _panelTints = new Image[3];
            _cells      = new Image[3][];
            _nameTMPs   = new TextMeshProUGUI[3];
            _statusTMPs = new TextMeshProUGUI[3];

            // Cadre anthracite sur ce GO
            var frameImg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            frameImg.color = FrameColor;

            // Écran noir partagé (sibling 0 → derrière tout)
            var screenGO = new GameObject("Screen", typeof(RectTransform));
            screenGO.transform.SetParent(transform, false);
            screenGO.transform.SetSiblingIndex(0);
            var screenRT = screenGO.GetComponent<RectTransform>();
            screenRT.anchorMin = Vector2.zero;
            screenRT.anchorMax = Vector2.one;
            screenRT.offsetMin = new Vector2(3f, 3f);
            screenRT.offsetMax = new Vector2(-3f, -3f);
            var screenImg = screenGO.AddComponent<Image>();
            screenImg.color = ScreenColor;
            screenImg.raycastTarget = false;

            // Arrange les 3 panneaux en tiers horizontaux
            float[] xMins = { 0f, 1f / 3f, 2f / 3f };
            float[] xMaxs = { 1f / 3f, 2f / 3f, 1f };
            for (int p = 0; p < 3; p++)
            {
                if (_panels[p] == null) continue;
                var rt = _panels[p];
                rt.anchorMin = new Vector2(xMins[p], 0f);
                rt.anchorMax = new Vector2(xMaxs[p], 1f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                BuildPanelContent(p, rt);
            }

            // Pointillés verts aux séparations
            BuildDottedSeparator(1f / 3f);
            BuildDottedSeparator(2f / 3f);

            _built = true;
        }

        private void BuildPanelContent(int p, RectTransform panel)
        {
            const float cellSize = 17f;
            const float cellGap  = 2f;
            const float labelH   = 14f;
            const float statusH  = 12f;

            // Tint overlay transparent (coloré à la fermeture du motif)
            var tintImg = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            tintImg.color = Color.clear;
            tintImg.raycastTarget = false;
            _panelTints[p] = tintImg;

            // Nom du motif (haut)
            var nameGO = new GameObject("PatternName", typeof(RectTransform));
            nameGO.transform.SetParent(panel, false);
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 1f);
            nameRT.anchorMax = new Vector2(1f, 1f);
            nameRT.pivot     = new Vector2(0.5f, 1f);
            nameRT.anchoredPosition = new Vector2(0f, -4f);
            nameRT.sizeDelta = new Vector2(0f, labelH);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.fontSize  = 7f;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color     = CRTGreen;
            nameTMP.raycastTarget = false;
            _nameTMPs[p] = nameTMP;

            // Mini-grille 3×3 centrée
            float gridW = 3 * cellSize + 2 * cellGap;
            float gridH = 3 * cellSize + 2 * cellGap;
            var gridGO = new GameObject("MiniGrid", typeof(RectTransform));
            gridGO.transform.SetParent(panel, false);
            var gridRT = gridGO.GetComponent<RectTransform>();
            gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.5f);
            gridRT.pivot     = new Vector2(0.5f, 0.5f);
            gridRT.anchoredPosition = new Vector2(0f, 3f);
            gridRT.sizeDelta = new Vector2(gridW, gridH);

            _cells[p] = new Image[9];
            for (int i = 0; i < 9; i++)
            {
                int row = i / 3, col = i % 3;
                var cellGO = new GameObject($"C{i}", typeof(RectTransform));
                cellGO.transform.SetParent(gridGO.transform, false);
                var cellRT = cellGO.GetComponent<RectTransform>();
                cellRT.anchorMin = cellRT.anchorMax = new Vector2(0f, 1f);
                cellRT.pivot     = new Vector2(0f, 1f);
                cellRT.anchoredPosition = new Vector2(col * (cellSize + cellGap), -row * (cellSize + cellGap));
                cellRT.sizeDelta = new Vector2(cellSize, cellSize);
                var img = cellGO.AddComponent<Image>();
                img.color = CRTGreenFaint;
                img.raycastTarget = false;
                _cells[p][i] = img;
            }

            // Statut / points (bas)
            var statusGO = new GameObject("PatternStatus", typeof(RectTransform));
            statusGO.transform.SetParent(panel, false);
            var statusRT = statusGO.GetComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 0f);
            statusRT.anchorMax = new Vector2(1f, 0f);
            statusRT.pivot     = new Vector2(0.5f, 0f);
            statusRT.anchoredPosition = new Vector2(0f, 3f);
            statusRT.sizeDelta = new Vector2(0f, statusH);
            var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
            statusTMP.fontSize  = 6.5f;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.color     = CRTGreen;
            statusTMP.raycastTarget = false;
            _statusTMPs[p] = statusTMP;
        }

        private void BuildDottedSeparator(float xNormalized)
        {
            var sepGO = new GameObject("DotSep", typeof(RectTransform));
            sepGO.transform.SetParent(transform, false);
            var sepRT = sepGO.GetComponent<RectTransform>();
            sepRT.anchorMin = new Vector2(xNormalized, 0f);
            sepRT.anchorMax = new Vector2(xNormalized, 1f);
            sepRT.pivot     = new Vector2(0.5f, 0.5f);
            sepRT.sizeDelta = new Vector2(4f, 0f);

            const int numDots = 12;
            for (int i = 0; i < numDots; i++)
            {
                float yCenter = (i + 0.5f) / numDots;
                var dotGO = new GameObject($"D{i}", typeof(RectTransform));
                dotGO.transform.SetParent(sepGO.transform, false);
                var dotRT = dotGO.GetComponent<RectTransform>();
                dotRT.anchorMin = new Vector2(0f, yCenter - 0.025f);
                dotRT.anchorMax = new Vector2(1f, yCenter + 0.025f);
                dotRT.offsetMin = dotRT.offsetMax = Vector2.zero;
                var dotImg = dotGO.AddComponent<Image>();
                dotImg.color = CRTGreen;
                dotImg.raycastTarget = false;
            }
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

                // Tint du panneau selon état
                if (_panelTints[p] != null)
                    _panelTints[p].color = status == -1 ? Color.clear :
                                           status == 0  ? TintClosedP : TintClosedE;

                // Nom
                if (_nameTMPs[p] != null)
                    _nameTMPs[p].text = pattern?.patternName ?? "—";

                // Statut
                if (_statusTMPs[p] != null)
                {
                    if (status == -1)
                    {
                        _statusTMPs[p].text  = pattern != null ? $"+{pattern.Points} PTS" : "";
                        _statusTMPs[p].color = CRTGreen;
                    }
                    else if (status == 0)
                    {
                        _statusTMPs[p].text  = "SCORED";
                        _statusTMPs[p].color = CRTGreenBright;
                    }
                    else
                    {
                        _statusTMPs[p].text  = "BLOCKED";
                        _statusTMPs[p].color = CRTRed;
                    }
                }

                // Cellules de la mini-grille
                if (_cells[p] == null) continue;
                var inPattern = pattern != null
                    ? new System.Collections.Generic.HashSet<int>(pattern.cellIndices)
                    : new System.Collections.Generic.HashSet<int>();

                Color activeCell = status == -1 ? CRTGreen :
                                   status == 0  ? CRTGreenBright : CRTRed;

                for (int i = 0; i < 9; i++)
                {
                    if (_cells[p][i] == null) continue;
                    _cells[p][i].color = inPattern.Contains(i) ? activeCell : CRTGreenFaint;
                }
            }
        }
    }
}
