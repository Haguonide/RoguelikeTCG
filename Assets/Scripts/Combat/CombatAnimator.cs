using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Handles all in-combat animations: attack, death, advance, clash.
    /// </summary>
    public class CombatAnimator : MonoBehaviour
    {
        public static CombatAnimator Instance { get; private set; }

        [Header("Board Slide (legacy — no longer used)")]
        public BoardNavigator boardNavigator;

        private static readonly Color ColAtk = new Color(1.00f, 0.82f, 0.22f);
        private static readonly Color ColHP  = new Color(1.00f, 0.40f, 0.40f);

        private int _animCount = 0;
        public bool IsAnimating => _animCount > 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Attack (wind-up + charge) ─────────────────────────────────────────

        /// <param name="isPlayerAttacking">
        /// True  → unit lunges upward   (player slots are below enemy slots).
        /// False → unit lunges downward (enemy attacking player).
        /// </param>
        public IEnumerator PlayAttackAnim(LaneSlotUI slot, bool isPlayerAttacking)
        {
            if (slot?.PlayedCard == null) yield break;

            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            Vector2 origin = rt.anchoredPosition;

            float windUpY = isPlayerAttacking ? -10f :  10f;
            float lungeY  = isPlayerAttacking ?  22f : -22f;

            yield return LerpPos(rt, origin, origin + new Vector2(0f, windUpY), 0.18f);
            AudioManager.Instance.PlaySFX("sfx_attack");
            yield return LerpPos(rt, origin + new Vector2(0f, windUpY),
                                      origin + new Vector2(0f, lungeY), 0.16f, easeOut: true);
            yield return LerpPos(rt, origin + new Vector2(0f, lungeY), origin, 0.28f);

            rt.anchoredPosition = origin;
            _animCount--;
        }

        // ── Death (shake → shrink) ────────────────────────────────────────────

        public IEnumerator PlayDeathAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;

            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_death");

            // Phase 1 — Shake + scale-up (0.12s)
            float elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.deltaTime;
                float p     = elapsed / 0.12f;
                float shake = Mathf.Sin(p * Mathf.PI * 6f) * 5f * (1f - p);
                float scale = 1f + Mathf.Sin(p * Mathf.PI) * 0.15f;
                rt.localEulerAngles = new Vector3(0f, 0f, shake);
                rt.localScale       = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // Phase 2 — Shrink + rotate (0.22s)
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.22f);
                float ease  = t * t;
                float scale = Mathf.Lerp(1f, 0f, ease);
                rt.localScale       = new Vector3(scale, scale, 1f);
                rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 40f, t));
                yield return null;
            }

            // Pas de reset — le PlayedCard sera détruit par Refresh() juste après
            _animCount--;
        }

        // ── Board slide transition ────────────────────────────────────────────

        /// <summary>
        /// Fait glisser le board <paramref name="fromIndex"/> hors de l'écran
        /// pendant que le board <paramref name="toIndex"/> entre depuis le côté opposé.
        /// Appeler SetActiveBoard() APRÈS cette coroutine.
        /// </summary>
        public IEnumerator PlayBoardSlide(int fromIndex, int toIndex)
        {
            if (boardNavigator == null) yield break;
            var views = boardNavigator.boardViews;
            if (views == null || fromIndex == toIndex) yield break;
            if (fromIndex < 0 || fromIndex >= views.Length) yield break;
            if (toIndex   < 0 || toIndex   >= views.Length) yield break;

            var fromView = views[fromIndex];
            var toView   = views[toIndex];
            if (fromView == null || toView == null) yield break;

            _animCount++;
            var fromRT = fromView.GetComponent<RectTransform>();
            var toRT   = toView.GetComponent<RectTransform>();

            float slideW = GetCanvasWidth();
            float dir = toIndex > fromIndex ? 1f : -1f;

            Vector2 fromOrigin = fromRT.anchoredPosition;
            Vector2 toOrigin   = toRT.anchoredPosition;

            toRT.anchoredPosition = new Vector2(dir * slideW, toOrigin.y);
            toView.SetActive(true);

            const float duration = 0.60f;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float ease = EaseInOut(t);
                fromRT.anchoredPosition = new Vector2(
                    Mathf.Lerp(fromOrigin.x, -dir * slideW, ease), fromOrigin.y);
                toRT.anchoredPosition = new Vector2(
                    Mathf.Lerp(dir * slideW, toOrigin.x, ease), toOrigin.y);
                yield return null;
            }

            fromRT.anchoredPosition = fromOrigin;
            toRT.anchoredPosition   = toOrigin;
            _animCount--;
        }

        // ── Hit Shake (survives) ──────────────────────────────────────────────

        /// <summary>
        /// Shake when a unit takes damage but survives.
        /// </summary>
        public IEnumerator PlayHitFlash(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_hit");

            float elapsed = 0f;
            while (elapsed < 0.10f)
            {
                elapsed += Time.deltaTime;
                float p     = elapsed / 0.10f;
                float shake = Mathf.Sin(p * Mathf.PI * 6f) * 5f * (1f - p);
                rt.localEulerAngles = new Vector3(0f, 0f, shake);
                yield return null;
            }
            rt.localEulerAngles = Vector3.zero;
            _animCount--;
        }

        // ── Card Play Anim (fly from hand + spin + top-down land) ────────────

        /// <summary>
        /// Flies a ghost card from the hand position to the slot with a full
        /// 360° Y-spin, then lands with a top-down drop effect on the real card.
        /// </summary>
        public IEnumerator PlayCardPlayAnim(Vector3 fromWorldPos, Vector2 cardSize,
                                            LaneSlotUI targetSlot, CardInstance cardInst)
        {
            // ─ 1. Build ghost card over canvas ───────────────────────────────
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) yield break;

            _animCount++;
            var ghost = BuildGhostCard(canvas.transform, cardInst, cardSize);
            ghost.transform.position = fromWorldPos;

            // ─ 2. Hide the real placed card until ghost lands ─────────────────
            var placed = targetSlot?.PlayedCard;
            if (placed?.animatedRoot != null)
                placed.animatedRoot.localScale = Vector3.zero;

            // ─ 3. Fly: arc + full Y-spin ──────────────────────────────────────
            Vector3 endPos    = targetSlot.transform.position;
            float   arcHeight = Mathf.Max(90f, Vector3.Distance(fromWorldPos, endPos) * 0.35f);
            Vector3 midPos    = Vector3.Lerp(fromWorldPos, endPos, 0.5f) + new Vector3(0f, arcHeight, 0f);

            const float flyDur    = 0.65f;
            const float scaleMin  = 1.00f;
            const float scaleHigh = 1.40f;
            float t = 0f;
            ghost.transform.localScale = new Vector3(scaleMin, scaleMin, 1f);
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / flyDur);
                float ease = EaseInOut(t);
                float u    = 1f - ease;
                Vector3 p  = u * u * fromWorldPos
                           + 2f * u * ease * midPos
                           + ease * ease * endPos;
                ghost.transform.position   = p;
                float s = Mathf.Lerp(scaleMin, scaleHigh, ease);
                ghost.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            ghost.transform.position   = endPos;
            ghost.transform.localScale = new Vector3(scaleHigh, scaleHigh, 1f);

            // Phase B — spin 360° on Y axis (0.52s)
            const float spinDur = 0.52f;
            t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / spinDur);
                float ease = EaseInOut(t);
                ghost.transform.localEulerAngles = new Vector3(0f, ease * 360f, 0f);
                yield return null;
            }
            ghost.transform.localEulerAngles = Vector3.zero;

            // ─ 4. Destroy ghost ───────────────────────────────────────────────
            Object.Destroy(ghost);

            // ─ 5. Top-down landing: card falls onto the table (1.40x → 0.90x → 1.0x)
            if (placed?.animatedRoot == null) { _animCount--; yield break; }
            AudioManager.Instance.PlaySFX("sfx_card_place");

            placed.animatedRoot.localScale = new Vector3(scaleHigh, scaleHigh, 1f);

            t = 0f;
            const float slamDur = 0.16f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / slamDur);
                float e = 1f - (1f - t) * (1f - t);
                float s = Mathf.Lerp(scaleHigh, 0.90f, e);
                placed.animatedRoot.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            t = 0f;
            const float reboundDur = 0.18f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / reboundDur);
                float e = 1f - (1f - t) * (1f - t);
                float s = Mathf.Lerp(0.90f, 1f, e);
                placed.animatedRoot.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            placed.animatedRoot.localScale = Vector3.one;
            _animCount--;
        }

        // ── Advance (slide one cell) ──────────────────────────────────────────

        /// <summary>Slides a unit's visual from one cell slot to the next.</summary>
        public IEnumerator PlayAdvanceAnim(LaneSlotUI fromSlot, LaneSlotUI toSlot, bool isPlayer)
        {
            // For MVP: just play a quick scale pulse on the unit to signal movement
            if (fromSlot?.PlayedCard == null) yield break;
            var rt = fromSlot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            // Small pulse indicating movement
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.12f);
                float e     = Mathf.Sin(t * Mathf.PI);
                float scale = 1f + e * 0.08f;
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
            _animCount--;
        }

        // ── Clash ─────────────────────────────────────────────────────────────

        /// <summary>Both units lunge toward each other simultaneously.</summary>
        public IEnumerator PlayClashAnim(LaneSlotUI playerSlot, LaneSlotUI enemySlot)
        {
            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_attack");

            var pRT = playerSlot?.PlayedCard?.animatedRoot;
            var eRT = enemySlot?.PlayedCard?.animatedRoot;

            Vector2 pOrigin = pRT != null ? pRT.anchoredPosition : Vector2.zero;
            Vector2 eOrigin = eRT != null ? eRT.anchoredPosition : Vector2.zero;

            // Lunge: player lunges right (+X), enemy lunges left (-X)
            const float lungeX = 16f;
            const float dur    = 0.18f;

            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / dur);
                float e = EaseInOut(t);
                if (pRT != null) pRT.anchoredPosition = Vector2.LerpUnclamped(pOrigin, pOrigin + new Vector2( lungeX, 0f), e);
                if (eRT != null) eRT.anchoredPosition = Vector2.LerpUnclamped(eOrigin, eOrigin + new Vector2(-lungeX, 0f), e);
                yield return null;
            }

            // Snap back
            t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / (dur * 1.4f));
                float e = EaseInOut(t);
                if (pRT != null) pRT.anchoredPosition = Vector2.LerpUnclamped(pOrigin + new Vector2( lungeX, 0f), pOrigin, e);
                if (eRT != null) eRT.anchoredPosition = Vector2.LerpUnclamped(eOrigin + new Vector2(-lungeX, 0f), eOrigin, e);
                yield return null;
            }

            if (pRT != null) pRT.anchoredPosition = pOrigin;
            if (eRT != null) eRT.anchoredPosition = eOrigin;
            _animCount--;
        }

        // ── Place Anim (bounce) — enemy card placement ───────────────────────

        public IEnumerator PlayPlaceAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.12f);
                float e     = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(0f, 1.15f, e);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.09f);
                float e     = 1f - (1f - t) * (1f - t);
                float scale = Mathf.Lerp(1.15f, 1f, e);
                rt.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
            _animCount--;
        }

        // ── Ghost Card ────────────────────────────────────────────────────────

        private GameObject BuildGhostCard(Transform canvasTransform, CardInstance card, Vector2 size)
        {
            bool isUnit = card.IsUnit;
            var cfg = Resources.Load<CardTemplateConfig>("CardTemplateConfig");

            var go = new GameObject("GhostCard", typeof(RectTransform));
            go.transform.SetParent(canvasTransform, false);
            go.transform.SetAsLastSibling();
            var rt       = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.pivot     = new Vector2(0.5f, 0.5f);

            // Couche 1 : fond
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite        = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImg.color         = Color.white;
            bgImg.raycastTarget = false;

            // Couche 2 : illustration
            var illuGO  = GhostMakeChild("Illustration", go);
            GhostSetAnchors(illuGO, 0f, 0f, 1f, 1f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.sprite        = card.data.artwork;
            illuImg.color         = Color.white;
            illuImg.raycastTarget = false;

            // Couche 3 : devant
            var frontGO  = GhostMakeChild("Front", go);
            GhostSetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

            // Couche 4 : textes
            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(go.transform, false);
            GhostSetAnchors(textsGO, 0f, 0f, 1f, 1f);

            var nameTMP = GhostMakeTMP("Name", textsGO,
                0.05f, 0.84f, 0.95f, 0.97f, 8f, FontStyles.Bold, TextAlignmentOptions.Center);
            nameTMP.enableWordWrapping = true;
            nameTMP.text = card.data.cardName;

            if (isUnit)
            {
                var statsTMP = GhostMakeTMP("Stats", textsGO,
                    0.03f, 0.04f, 0.97f, 0.22f, 10f, FontStyles.Bold, TextAlignmentOptions.Center);
                statsTMP.enableWordWrapping = false;
                statsTMP.richText = true;
                statsTMP.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ColAtk)}>⚔ {card.CurrentAttack}</color>" +
                                $"  <color=#{ColorUtility.ToHtmlStringRGB(ColHP)}>❤ {card.currentHP}</color>";
            }
            else
            {
                var manaTMP = GhostMakeTMP("Mana", textsGO,
                    0.03f, 0.20f, 0.97f, 0.34f, 9.5f, FontStyles.Bold, TextAlignmentOptions.Center);
                manaTMP.text = $"💎 {card.data.manaCost} mana";

                if (!string.IsNullOrEmpty(card.data.description))
                {
                    var descTMP = GhostMakeTMP("Desc", textsGO,
                        0.04f, 0.04f, 0.96f, 0.19f, 6f, FontStyles.Normal, TextAlignmentOptions.Center);
                    descTMP.enableWordWrapping = true;
                    descTMP.overflowMode = TextOverflowModes.Ellipsis;
                    descTMP.text = card.data.description;
                }
            }

            return go;
        }

        private static GameObject GhostMakeChild(string name, GameObject parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
            return go;
        }

        private static void GhostSetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt       = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI GhostMakeTMP(string name, GameObject parent,
            float xMin, float yMin, float xMax, float yMax,
            float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            GhostSetAnchors(go, xMin, yMin, xMax, yMax);
            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = style;
            tmp.alignment     = align;
            tmp.color         = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private float GetCanvasWidth()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                float w = canvas.GetComponent<RectTransform>().rect.width;
                if (w > 100f) return w;
            }
            return 1200f;
        }

        private static float EaseInOut(float t)
            => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        private IEnumerator LerpPos(RectTransform rt, Vector2 from, Vector2 to,
                                    float duration, bool easeOut = false)
        {
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / duration);
                float e = easeOut ? 1f - (1f - t) * (1f - t) : t;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
                yield return null;
            }
            rt.anchoredPosition = to;
        }
    }
}
