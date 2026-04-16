using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Singleton qui affiche une bulle de tooltip à droite de l'icône de relique survolée.
    /// Show() est appelé par RelicIconHover ; Hide() est appelé en OnPointerExit.
    /// La bulle est construite en code car elle est purement dynamique (hover).
    /// </summary>
    public class RelicTooltipUI : MonoBehaviour
    {
        public static RelicTooltipUI Instance { get; private set; }

        private GameObject _tooltip;
        private Canvas     _canvas;

        // Dimensions de la bulle
        private const float TooltipW    = 260f;
        private const float PadX        = 14f;
        private const float PadY        = 12f;
        private const float TitleFontSz = 22f;
        private const float DescFontSz  = 18f;
        private const float GapTitleDesc = 6f;
        private const float OffsetX     = 12f; // espace entre icône et bulle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Affiche la bulle à droite du RectTransform de l'icône survolée.
        /// </summary>
        public void Show(RelicData relic, RectTransform iconRT)
        {
            if (relic == null) return;
            Hide(); // détruire l'ancienne bulle si elle existe

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return;

            // Hauteur dynamique selon longueur description
            float titleH = TitleFontSz + 6f;
            // Estimation : ~22 chars par ligne à cette taille / largeur
            int   descLines   = Mathf.Max(1, Mathf.CeilToInt(relic.description.Length / 22f));
            float descH       = descLines * (DescFontSz + 4f);
            float tooltipH    = PadY + titleH + GapTitleDesc + descH + PadY;

            // ── Panneau fond ─────────────────────────────────────────────────
            _tooltip = new GameObject("RelicTooltip", typeof(RectTransform));
            _tooltip.transform.SetParent(_canvas.transform, false);
            _tooltip.transform.SetAsLastSibling(); // par-dessus tout

            var rt = _tooltip.GetComponent<RectTransform>();
            // Ancre centrale : anchoredPosition == coordonnées locales canvas (0,0 = centre).
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0f, 1f);   // pivot haut-gauche du tooltip
            rt.sizeDelta = new Vector2(TooltipW, tooltipH);

            // Coin haut-droit de l'icône en coordonnées locales canvas
            Vector2 iconTopRight = GetIconCornerInCanvasSpace(iconRT, cornerIndex: 2);
            rt.anchoredPosition = iconTopRight + new Vector2(OffsetX, 0f);

            // Fond semi-transparent arrondi (Image plain)
            var bg = _tooltip.AddComponent<Image>();
            bg.color         = new Color(0.08f, 0.08f, 0.10f, 0.93f);
            bg.raycastTarget = false;

            // Contour via un outline (Image enfant légèrement plus grand)
            AddOutline(_tooltip, rt.sizeDelta);

            // ── Titre (nom) ───────────────────────────────────────────────────
            var titleGO = new GameObject("TooltipTitle", typeof(RectTransform));
            titleGO.transform.SetParent(_tooltip.transform, false);
            var trt = titleGO.GetComponent<RectTransform>();
            trt.anchorMin        = new Vector2(0f, 1f);
            trt.anchorMax        = new Vector2(1f, 1f);
            trt.pivot            = new Vector2(0f, 1f);
            trt.offsetMin        = new Vector2(PadX, -(PadY + titleH));
            trt.offsetMax        = new Vector2(-PadX, -PadY);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text          = relic.relicName;
            title.fontSize      = TitleFontSz;
            title.fontStyle     = FontStyles.Bold;
            title.color         = new Color(0.95f, 0.82f, 0.35f); // doré
            title.alignment     = TextAlignmentOptions.TopLeft;
            title.raycastTarget = false;
            title.enableWordWrapping = true;

            // ── Description ───────────────────────────────────────────────────
            var descGO = new GameObject("TooltipDesc", typeof(RectTransform));
            descGO.transform.SetParent(_tooltip.transform, false);
            var drt = descGO.GetComponent<RectTransform>();
            drt.anchorMin        = new Vector2(0f, 1f);
            drt.anchorMax        = new Vector2(1f, 1f);
            drt.pivot            = new Vector2(0f, 1f);
            float descTop = PadY + titleH + GapTitleDesc;
            drt.offsetMin        = new Vector2(PadX, -(descTop + descH));
            drt.offsetMax        = new Vector2(-PadX, -descTop);
            var desc = descGO.AddComponent<TextMeshProUGUI>();
            desc.text          = relic.description;
            desc.fontSize      = DescFontSz;
            desc.color         = new Color(0.88f, 0.88f, 0.88f);
            desc.alignment     = TextAlignmentOptions.TopLeft;
            desc.raycastTarget = false;
            desc.enableWordWrapping = true;
        }

        public void Hide()
        {
            if (_tooltip != null)
            {
                Destroy(_tooltip);
                _tooltip = null;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne le coin demandé du RectTransform en coordonnées locales du Canvas.
        /// cornerIndex : 0=bas-gauche, 1=haut-gauche, 2=haut-droit, 3=bas-droit
        /// </summary>
        private Vector2 GetIconCornerInCanvasSpace(RectTransform rt, int cornerIndex)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT,
                corners[cornerIndex],
                _canvas.worldCamera,
                out localPoint
            );
            return localPoint;
        }

        private void AddOutline(GameObject parent, Vector2 size)
        {
            var outlineGO = new GameObject("TooltipOutline", typeof(RectTransform));
            outlineGO.transform.SetParent(parent.transform, false);
            outlineGO.transform.SetAsFirstSibling();
            var ort = outlineGO.GetComponent<RectTransform>();
            ort.anchorMin = Vector2.zero;
            ort.anchorMax = Vector2.one;
            ort.offsetMin = new Vector2(-2f, -2f);
            ort.offsetMax = new Vector2(2f, 2f);
            var img = outlineGO.AddComponent<Image>();
            img.color         = new Color(0.55f, 0.45f, 0.15f, 0.85f); // contour doré foncé
            img.raycastTarget = false;
        }
    }
}
