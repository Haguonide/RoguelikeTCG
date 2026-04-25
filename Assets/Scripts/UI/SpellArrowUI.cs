using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    public class SpellArrowUI : MonoBehaviour
    {
        [SerializeField] private Sprite segmentSprite;
        [SerializeField] private Sprite arrowheadSprite;
        [SerializeField] private int    dotCount = 20;

        private RectTransform   _originRT;
        private RectTransform   _selfRT;
        private Camera          _canvasCamera;

        private RectTransform[] _segments;
        private Image[]         _segmentImages;
        private RectTransform   _arrowheadRT;
        private Image           _arrowheadImage;

        private Color _arrowColor = new Color(1f, 0.9f, 0.4f, 0.85f);

        // Segment dimensions — length varies slightly near tip for depth effect
        private const float SegW    = 22f;
        private const float SegH    = 9f;
        private const float AheadSz = 40f;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            _selfRT = GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            _canvasCamera = canvas != null ? canvas.worldCamera : null;

            _segments      = new RectTransform[dotCount];
            _segmentImages = new Image[dotCount];

            for (int i = 0; i < dotCount; i++)
            {
                var go  = new GameObject($"SpellSeg_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);

                var img = go.GetComponent<Image>();
                img.sprite        = segmentSprite;
                img.color         = _arrowColor;
                img.raycastTarget = false;
                img.preserveAspect = false;

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(SegW, SegH);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);

                _segments[i]      = rt;
                _segmentImages[i] = img;

                go.SetActive(false);
            }

            var ahead = new GameObject("SpellArrowhead", typeof(RectTransform), typeof(Image));
            ahead.transform.SetParent(transform, false);

            _arrowheadImage               = ahead.GetComponent<Image>();
            _arrowheadImage.sprite        = arrowheadSprite;
            _arrowheadImage.color         = _arrowColor;
            _arrowheadImage.raycastTarget = false;
            _arrowheadImage.preserveAspect = true;

            _arrowheadRT          = ahead.GetComponent<RectTransform>();
            _arrowheadRT.sizeDelta = new Vector2(AheadSz, AheadSz);
            _arrowheadRT.anchorMin = new Vector2(0.5f, 0.5f);
            _arrowheadRT.anchorMax = new Vector2(0.5f, 0.5f);
            _arrowheadRT.pivot     = new Vector2(0.5f, 0.5f);

            ahead.SetActive(false);
            gameObject.SetActive(false);
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void SetArrowColor(Color color)
        {
            _arrowColor = color;
            for (int i = 0; i < dotCount; i++)
                if (_segmentImages[i] != null) _segmentImages[i].color = color;
            if (_arrowheadImage != null) _arrowheadImage.color = color;
        }

        public void Show(RectTransform origin)
        {
            _originRT = origin;
            gameObject.SetActive(true);
            for (int i = 0; i < dotCount; i++)
                _segments[i].gameObject.SetActive(true);
            _arrowheadRT.gameObject.SetActive(true);
        }

        public void Hide()
        {
            for (int i = 0; i < dotCount; i++)
                _segments[i].gameObject.SetActive(false);
            _arrowheadRT.gameObject.SetActive(false);
            gameObject.SetActive(false);
            _originRT = null;
        }

        public void UpdateArrow(Vector2 mouseScreenPos)
        {
            if (_originRT == null || _selfRT == null) return;

            Vector2 p0 = OriginLocalPoint();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _selfRT, mouseScreenPos, _canvasCamera, out Vector2 p2);

            // Control point: above midpoint, curvature scales with horizontal distance
            float height = Mathf.Abs(p2.x - p0.x) * 0.4f + 80f;
            Vector2 mid  = (p0 + p2) * 0.5f;
            Vector2 p1   = mid + Vector2.up * height;

            for (int i = 0; i < dotCount; i++)
            {
                float t  = (float)i / (dotCount - 1);
                Vector2 pt      = Bezier(p0, p1, p2, t);
                Vector2 tangent = BezierTangent(p0, p1, p2, t).normalized;

                _segments[i].anchoredPosition = pt;

                // Rotate segment to align with curve direction
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                _segments[i].localRotation = Quaternion.Euler(0f, 0f, angle);

                // Slight scale-up near the tip for a depth effect
                float scale = Mathf.Lerp(0.75f, 1.1f, t);
                _segments[i].localScale = new Vector3(scale, scale, 1f);
            }

            // Arrowhead at curve tip, rotated along final tangent
            Vector2 tip         = Bezier(p0, p1, p2, 1f);
            Vector2 tipTangent  = BezierTangent(p0, p1, p2, 0.98f);
            float   aheadAngle  = Mathf.Atan2(tipTangent.y, tipTangent.x) * Mathf.Rad2Deg - 90f;

            _arrowheadRT.anchoredPosition = tip;
            _arrowheadRT.localRotation    = Quaternion.Euler(0f, 0f, aheadAngle);
        }

        // ── Bézier math ──────────────────────────────────────────────────────

        private static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float inv = 1f - t;
            return inv * inv * p0 + 2f * inv * t * p1 + t * t * p2;
        }

        private static Vector2 BezierTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Vector2 OriginLocalPoint()
        {
            Rect    rect      = _originRT.rect;
            Vector3 topCenter = _originRT.TransformPoint(new Vector3(0f, rect.yMax, 0f));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _selfRT,
                RectTransformUtility.WorldToScreenPoint(_canvasCamera, topCenter),
                _canvasCamera,
                out Vector2 local);
            return local;
        }
    }
}
