using UnityEngine;
using TMPro;
using DG.Tweening;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Floating number that pops up at a target slot and floats upward.
    /// Parented to the main Canvas so it always renders on top.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private static readonly Color ColDamage = new Color(1.00f, 0.28f, 0.18f);
        private static readonly Color ColHeal   = new Color(0.35f, 1.00f, 0.45f);
        private static readonly Color ColShield = new Color(0.45f, 0.72f, 1.00f);

        public static void ShowDamage(RectTransform anchor, int amount)
            => Spawn(anchor, $"-{amount}", ColDamage, 24f);

        public static void ShowHeal(RectTransform anchor, int amount)
            => Spawn(anchor, $"+{amount}", ColHeal, 22f);

        public static void ShowShield(RectTransform anchor, int amount)
            => Spawn(anchor, $"🛡 +{amount}", ColShield, 20f);

        // ──────────────────────────────────────────────────────────────────────

        private static void Spawn(RectTransform anchor, string text, Color color, float fontSize)
        {
#pragma warning disable CS0618
            var canvas = Object.FindObjectOfType<Canvas>();
#pragma warning restore CS0618
            if (canvas == null) return;

            var go = new GameObject("DamagePopup", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt       = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 50f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);

            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), screenPt, null, out Vector2 localPt);
            rt.anchoredPosition = localPt + new Vector2(0f, anchor.rect.height * 0.5f);

            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.color         = color;
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            Vector2 startPos   = rt.anchoredPosition;
            const float dur    = 0.85f;
            const float rise   = 60f;

            var seq = DOTween.Sequence().SetTarget(go);
            seq.Join(rt.DOAnchorPos(startPos + new Vector2(0f, rise), dur).SetEase(Ease.OutQuad));
            seq.Join(rt.DOPunchScale(new Vector3(0.50f, 0.50f, 0f), 0.20f, 5, 0.3f));
            seq.Insert(dur * 0.55f, tmp.DOFade(0f, dur * 0.45f).SetEase(Ease.InQuad));
            seq.OnComplete(() => Object.Destroy(go));
        }
    }
}
