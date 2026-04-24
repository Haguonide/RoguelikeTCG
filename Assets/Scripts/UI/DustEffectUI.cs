using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RoguelikeTCG.UI
{
    public class DustEffectUI : MonoBehaviour
    {
        public static DustEffectUI Instance { get; private set; }

        [SerializeField] private Sprite dustSprite;
        [SerializeField] private int    particleCount = 12;

        private RectTransform _selfRT;
        private RectTransform[] _particles;
        private Image[]         _images;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            _selfRT  = GetComponent<RectTransform>();

            _particles = new RectTransform[particleCount];
            _images    = new Image[particleCount];

            for (int i = 0; i < particleCount; i++)
            {
                var go  = new GameObject($"DustParticle_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);

                var img = go.GetComponent<Image>();
                img.sprite        = dustSprite;
                img.color         = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = false;

                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(36f, 36f);

                _particles[i] = rt;
                _images[i]    = img;
                go.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void Play(RectTransform slotRT)
        {
            if (_selfRT == null || slotRT == null) return;

            // Convert slot world position to our canvas local space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _selfRT,
                RectTransformUtility.WorldToScreenPoint(null, slotRT.position),
                null,
                out Vector2 center);

            for (int i = 0; i < particleCount; i++)
                SpawnParticle(i, center);
        }

        // ── Internals ────────────────────────────────────────────────────────

        private void SpawnParticle(int i, Vector2 center)
        {
            var rt  = _particles[i];
            var img = _images[i];

            DOTween.Kill(rt);
            DOTween.Kill(img);

            // Random spread — slight upward bias for dust that rises
            float angle = Random.Range(-160f, -20f); // upper half arc (160° spread, centered upward)
            float dist  = Random.Range(28f, 68f);
            float size  = Random.Range(22f, 52f);
            float dur   = Random.Range(0.30f, 0.50f);

            Vector2 dir    = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 endPos = center + dir * dist;

            // Reset state
            rt.anchoredPosition = center;
            rt.sizeDelta        = new Vector2(size, size);
            rt.localScale       = Vector3.zero;
            rt.localRotation    = Quaternion.Euler(0f, 0f, Random.Range(-25f, 25f));
            img.color           = new Color(1f, 1f, 1f, 0f);
            rt.gameObject.SetActive(true);

            // Movement: burst outward, decelerates
            rt.DOAnchorPos(endPos, dur * 0.85f).SetEase(Ease.OutCubic).SetTarget(rt);

            // Slow upward drift in the tail end
            rt.DOAnchorPos(endPos + Vector2.up * Random.Range(6f, 16f), dur * 0.15f)
              .SetEase(Ease.InQuad).SetDelay(dur * 0.85f).SetTarget(rt);

            // Scale: fast pop-in, slow shrink
            var scaleSeq = DOTween.Sequence().SetTarget(rt);
            scaleSeq.Append(rt.DOScale(1f,  dur * 0.22f).SetEase(Ease.OutBack, 2.2f));
            scaleSeq.Append(rt.DOScale(0f,  dur * 0.78f).SetEase(Ease.InQuad));
            scaleSeq.OnComplete(() => rt.gameObject.SetActive(false));

            // Alpha: flash in, linger briefly, fade out
            var colorSeq = DOTween.Sequence().SetTarget(img);
            colorSeq.Append(img.DOFade(0.80f, dur * 0.18f).SetEase(Ease.OutQuad));
            colorSeq.Append(img.DOFade(0.55f, dur * 0.20f).SetEase(Ease.Linear));
            colorSeq.Append(img.DOFade(0f,    dur * 0.62f).SetEase(Ease.InQuad));
        }
    }
}
