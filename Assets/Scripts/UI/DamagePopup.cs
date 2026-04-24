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
        private static readonly Color ColDamage      = new Color(1.00f, 0.28f, 0.18f);
        private static readonly Color ColHeal        = new Color(0.35f, 1.00f, 0.45f);
        private static readonly Color ColShield      = new Color(0.45f, 0.72f, 1.00f);
        private static readonly Color ColClashDmg    = new Color(1.00f, 0.22f, 0.10f);   // vivid red
        private static readonly Color ColClashFatal  = new Color(1.00f, 0.55f, 0.05f);   // orange — lethal blow

        // ── Standard popups (spells, direct hero hits) ────────────────────────

        public static void ShowDamage(RectTransform anchor, int amount)
            => Spawn(anchor, $"-{amount}", ColDamage, 24f, rise: 60f, duration: 0.85f);

        public static void ShowHeal(RectTransform anchor, int amount)
            => Spawn(anchor, $"+{amount}", ColHeal, 22f, rise: 60f, duration: 0.85f);

        public static void ShowShield(RectTransform anchor, int amount)
            => Spawn(anchor, $"🛡 +{amount}", ColShield, 20f, rise: 60f, duration: 0.85f);

        // ── Clash popup — bigger, punchier, rises higher ──────────────────────
        // fatal = true when the unit will die from this hit (orange instead of red)

        public static void ShowClashDamage(RectTransform anchor, int amount, bool fatal = false)
        {
            Color col = fatal ? ColClashFatal : ColClashDmg;
            SpawnClash(anchor, $"-{amount}", col, 36f);
        }

        // ──────────────────────────────────────────────────────────────────────

        private static Canvas GetRootCanvas(RectTransform anchor)
        {
            var c = anchor.GetComponentInParent<Canvas>();
            return c != null ? c.rootCanvas : null;
        }

        private static void Spawn(RectTransform anchor, string text, Color color,
                                   float fontSize, float rise, float duration)
        {
            var canvas = GetRootCanvas(anchor);
            if (canvas == null) return;

            var go = new GameObject("DamagePopup", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt       = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120f, 50f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            // In Screen Space Overlay, world position == screen position in pixels.
            // Set position directly — no coordinate conversion needed.
            rt.position = anchor.position + new Vector3(0f, anchor.rect.height * 0.5f, 0f);

            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.color         = color;
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            Vector2 startPos = rt.anchoredPosition;

            var seq = DOTween.Sequence().SetTarget(go.transform).SetAutoKill(true);
            seq.Join(rt.DOAnchorPos(startPos + new Vector2(0f, rise), duration).SetEase(Ease.OutQuad).SetTarget(go.transform));
            seq.Join(rt.DOPunchScale(new Vector3(0.50f, 0.50f, 0f), 0.20f, 5, 0.3f).SetTarget(go.transform));
            seq.Insert(duration * 0.55f, tmp.DOFade(0f, duration * 0.45f).SetEase(Ease.InQuad).SetTarget(go.transform));
            seq.OnComplete(() => { if (go != null) Object.Destroy(go); });
        }

        // ── Clash variant — punch-in scale, longer hold, fast fade ───────────

        private static void SpawnClash(RectTransform anchor, string text, Color color, float fontSize)
        {
            var canvas = GetRootCanvas(anchor);
            if (canvas == null) return;

            var go = new GameObject("ClashPopup", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt       = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 70f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.zero;

            // In Screen Space Overlay, world position == screen position in pixels.
            rt.position = anchor.position + new Vector3(0f, anchor.rect.height * 0.6f, 0f);

            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.color         = color;
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            Vector2 startPos = rt.anchoredPosition;
            const float holdDur  = 0.28f;   // how long the number is fully visible
            const float riseDur  = 0.70f;   // total rise duration
            const float totalDur = 0.98f;

            var seq = DOTween.Sequence().SetTarget(go.transform).SetAutoKill(true);

            // Pop-in punch: scale from 0 → overshoot → settle
            seq.Append(rt.DOScale(new Vector3(1.40f, 1.40f, 1f), 0.10f).SetEase(Ease.OutExpo).SetTarget(go.transform));
            seq.Append(rt.DOScale(Vector3.one, 0.08f).SetEase(Ease.OutBack, 2f).SetTarget(go.transform));

            // Rise upward throughout
            seq.Join(rt.DOAnchorPos(startPos + new Vector2(0f, 80f), riseDur)
                       .SetEase(Ease.OutCubic).SetDelay(0f).SetTarget(go.transform));

            // Hold visible for holdDur, then fade
            seq.Insert(holdDur, tmp.DOFade(0f, totalDur - holdDur).SetEase(Ease.InCubic).SetTarget(go.transform));

            seq.OnComplete(() => { if (go != null) Object.Destroy(go); });
        }
    }
}
