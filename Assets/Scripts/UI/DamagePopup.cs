using System.Collections;
using UnityEngine;
using TMPro;

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
            => Spawn(anchor, $"\ud83d\udee1 +{amount}", ColShield, 20f);

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

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 50f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);

            // Convert anchor world position to canvas local position
            Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), screenPt, null, out Vector2 localPt);
            rt.anchoredPosition = localPt + new Vector2(0f, anchor.rect.height * 0.5f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.color         = color;
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            var popup = go.AddComponent<DamagePopup>();
            popup.StartCoroutine(popup.Animate(rt, tmp));
        }

        private IEnumerator Animate(RectTransform rt, TextMeshProUGUI tmp)
        {
            const float duration   = 0.85f;
            const float risePixels = 60f;
            Vector2 startPos   = rt.anchoredPosition;
            Color   startColor = tmp.color;

            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);

                // Rise with ease-out
                float rise = risePixels * (1f - (1f - t) * (1f - t));
                rt.anchoredPosition = startPos + new Vector2(0f, rise);

                // Scale: pop in 0→1.2 (first 15%), settle 1.2→1 (next 10%), hold
                float scale = t < 0.15f ? Mathf.Lerp(0f,   1.2f, t / 0.15f)
                            : t < 0.25f ? Mathf.Lerp(1.2f, 1.0f, (t - 0.15f) / 0.10f)
                            : 1f;
                rt.localScale = new Vector3(scale, scale, 1f);

                // Fade out in last 40%
                float alpha = t < 0.60f ? 1f
                            : Mathf.Lerp(1f, 0f, (t - 0.60f) / 0.40f);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
