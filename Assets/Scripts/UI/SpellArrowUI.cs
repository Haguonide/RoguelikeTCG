using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class SpellArrowUI : MaskableGraphic
    {
        [SerializeField] private int   ribbonSegments = 30;
        [SerializeField] private float ribbonWidth    = 20f;
        [SerializeField] private float arrowheadSize  = 36f;

        // ── État interne ─────────────────────────────────────────────────────

        private RectTransform _originRT;
        private Vector2 _p0, _p1, _p2;
        private bool _hasPoints;

        // ── Lifecycle ────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;

            var s = Shader.Find("RoguelikeTCG/RibbonArrow");
            if (s != null)
                material = new Material(s) { hideFlags = HideFlags.DontSave };

            gameObject.SetActive(false);
        }

        // ── API publique (identique à l'ancienne version MonoBehaviour) ──────

        public void SetArrowColor(Color c)
        {
            // Graphic.color déclenche SetVerticesDirty + SetMaterialDirty automatiquement
            color = c;
        }

        public void Show(RectTransform origin)
        {
            _originRT = origin;
            gameObject.SetActive(true);
            UpdateArrow(Input.mousePosition);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _originRT  = null;
            _hasPoints = false;
        }

        public void UpdateArrow(Vector2 mouseScreenPos)
        {
            if (_originRT == null) return;

            var selfRT = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            var cam    = canvas != null ? canvas.worldCamera : null;

            // Origine : centre haut de la carte source
            Rect    rect      = _originRT.rect;
            Vector3 topCenter = _originRT.TransformPoint(new Vector3(0f, rect.yMax, 0f));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selfRT,
                RectTransformUtility.WorldToScreenPoint(cam, topCenter),
                cam,
                out _p0);

            // Destination : souris en espace local du canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selfRT, mouseScreenPos, cam, out _p2);

            // Point de contrôle Bézier : au-dessus du point médian
            float height = Mathf.Abs(_p2.x - _p0.x) * 0.4f + 80f;
            _p1 = (_p0 + _p2) * 0.5f + Vector2.up * height;

            _hasPoints = true;
            SetVerticesDirty();
        }

        // ── Rendu mesh ───────────────────────────────────────────────────────

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!_hasPoints) return;

            Color32 c = color;
            int     N = ribbonSegments;

            // ── Ruban (N quads) ──────────────────────────────────────────────
            for (int i = 0; i < N; i++)
            {
                float t0 = (float)i       / N;
                float t1 = (float)(i + 1) / N;

                Vector2 pos0  = Bezier(_p0, _p1, _p2, t0);
                Vector2 pos1  = Bezier(_p0, _p1, _p2, t1);
                Vector2 perp0 = Perp(BezierTangent(_p0, _p1, _p2, t0));
                Vector2 perp1 = Perp(BezierTangent(_p0, _p1, _p2, t1));

                // Le ruban s'élargit légèrement de la queue vers la pointe
                float w0 = Mathf.Lerp(ribbonWidth * 0.4f, ribbonWidth, t0);
                float w1 = Mathf.Lerp(ribbonWidth * 0.4f, ribbonWidth, t1);

                // Rétrécit sur les 2 derniers segments avant la tête de flèche
                if (i >= N - 2)
                    w1 = Mathf.Lerp(w1, 0f, (float)(i - N + 2) / 2f);

                // u = progression le long de la courbe, v = 0/1 pour les bords
                int b = vh.currentVertCount;
                vh.AddVert(MakeVert(pos0 - perp0 * w0, c, t0, 0f));
                vh.AddVert(MakeVert(pos0 + perp0 * w0, c, t0, 1f));
                vh.AddVert(MakeVert(pos1 + perp1 * w1, c, t1, 1f));
                vh.AddVert(MakeVert(pos1 - perp1 * w1, c, t1, 0f));
                vh.AddTriangle(b,     b + 1, b + 2);
                vh.AddTriangle(b,     b + 2, b + 3);
            }

            // ── Tête de flèche (chevron triangulaire) ────────────────────────
            Vector2 tip     = Bezier(_p0, _p1, _p2, 1f);
            Vector2 tipTan  = BezierTangent(_p0, _p1, _p2, 0.96f).normalized;
            Vector2 tipPerp = new Vector2(-tipTan.y, tipTan.x);
            float   ah      = arrowheadSize;
            Vector2 aBase   = tip - tipTan * ah * 0.7f;

            int ai = vh.currentVertCount;
            vh.AddVert(MakeVert(tip,                          c, 1f,    0.5f));
            vh.AddVert(MakeVert(aBase - tipPerp * ah * 0.6f, c, 0.85f, 0f));
            vh.AddVert(MakeVert(aBase + tipPerp * ah * 0.6f, c, 0.85f, 1f));
            vh.AddTriangle(ai, ai + 1, ai + 2);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static UIVertex MakeVert(Vector2 pos, Color32 col, float u, float v) =>
            new UIVertex { position = pos, color = col, uv0 = new Vector2(u, v) };

        private static Vector2 Perp(Vector2 v) =>
            new Vector2(-v.y, v.x).normalized;

        private static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float inv = 1f - t;
            return inv * inv * p0 + 2f * inv * t * p1 + t * t * p2;
        }

        private static Vector2 BezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t) =>
            2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }
}
