using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    public class CombatAnimator : MonoBehaviour
    {
        public static CombatAnimator Instance { get; private set; }

        [Header("Board Slide (legacy — no longer used)")]
        public BoardNavigator boardNavigator;

        [Header("Screen Shake")]
        public RectTransform shakeTarget;

        private Canvas _canvas;

        private static readonly Color ColAtk = new Color(1.00f, 0.82f, 0.22f);
        private static readonly Color ColHP  = new Color(1.00f, 0.40f, 0.40f);

        private int _animCount = 0;
        public bool IsAnimating => _animCount > 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
#pragma warning disable CS0618
            _canvas = Object.FindObjectOfType<Canvas>();
#pragma warning restore CS0618
        }

        // ── Attack / Traverse (lunge → impact → return) ──────────────────────

        public IEnumerator PlayAttackAnim(LaneSlotUI slot, bool isPlayerAttacking)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            Vector2 origin = rt.anchoredPosition;

            float pullX  = isPlayerAttacking ? -13f :  13f;
            float lungeX = isPlayerAttacking ?  40f : -40f;

            // ── Phase 1 — Anticipation: pull back + squash (0.11s) ───────────
            var anticipate = DOTween.Sequence();
            anticipate.Append(rt.DOAnchorPos(origin + new Vector2(pullX, 0f), 0.11f).SetEase(Ease.OutQuad));
            anticipate.Join(rt.DOScale(new Vector3(0.82f, 1.16f, 1f), 0.11f).SetEase(Ease.OutQuad));
            yield return anticipate.WaitForCompletion();

            // ── Phase 2 — Lunge: burst forward + horizontal stretch (0.09s) ──
            AudioManager.Instance.PlaySFX("sfx_attack");
            var lunge = DOTween.Sequence();
            lunge.Append(rt.DOAnchorPos(origin + new Vector2(lungeX, 0f), 0.09f).SetEase(Ease.OutQuint));
            lunge.Join(rt.DOScale(new Vector3(1.22f, 0.80f, 1f), 0.09f).SetEase(Ease.OutQuint));
            yield return lunge.WaitForCompletion();

            // ── Phase 3 — Impact punch + screen shake ────────────────────────
            shakeTarget?.DOShakeAnchorPos(0.16f, 8f, 14, 55f, false, true);
            yield return rt.DOPunchScale(new Vector3(0.28f, 0.28f, 0f), 0.16f, 5, 0.45f)
                           .WaitForCompletion();

            // ── Phase 4 — Return with slight overshoot (0.22s) ───────────────
            var ret = DOTween.Sequence();
            ret.Append(rt.DOAnchorPos(origin, 0.22f).SetEase(Ease.OutBack, 1.3f));
            ret.Join(rt.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack, 1.3f));
            yield return ret.WaitForCompletion();

            rt.anchoredPosition = origin;
            rt.localScale       = Vector3.one;
            _animCount--;
        }

        // ── Death (flash → shatter → collapse) ───────────────────────────────

        public IEnumerator PlayDeathAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_death");

            // ── Phase 1 — Impact flash: hard punch (0.13s) ───────────────────
            yield return rt.DOPunchScale(new Vector3(0.55f, 0.55f, 0f), 0.13f, 6, 0.4f)
                           .WaitForCompletion();

            // ── Phase 2 — Shatter: bloat + tremble + screen shake (0.14s) ────
            shakeTarget?.DOShakeAnchorPos(0.18f, 9f, 16, 55f, false, true);
            var shatter = DOTween.Sequence();
            shatter.Append(rt.DOScale(new Vector3(1.32f, 1.32f, 1f), 0.14f).SetEase(Ease.OutQuad));
            shatter.Join(rt.DOPunchPosition(new Vector3(9f, 7f, 0f), 0.14f, 12, 0.6f, false));
            yield return shatter.WaitForCompletion();

            // ── Phase 3 — Collapse: spin + shrink to nothing (0.26s) ─────────
            var collapse = DOTween.Sequence();
            collapse.Append(rt.DOScale(Vector3.zero, 0.26f).SetEase(Ease.InCubic));
            collapse.Join(rt.DOLocalRotate(new Vector3(0f, 0f, -170f), 0.26f).SetEase(Ease.InCubic));
            yield return collapse.WaitForCompletion();

            _animCount--;
        }

        // ── Board slide transition ────────────────────────────────────────────

        public IEnumerator PlayBoardSlide(int fromIndex, int toIndex)
        {
            if (boardNavigator == null || _canvas == null) yield break;
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

            float slideW = _canvas.GetComponent<RectTransform>().rect.width;
            float dir    = toIndex > fromIndex ? 1f : -1f;

            Vector2 fromOrigin = fromRT.anchoredPosition;
            Vector2 toOrigin   = toRT.anchoredPosition;

            toRT.anchoredPosition = new Vector2(dir * slideW, toOrigin.y);
            toView.SetActive(true);

            const float duration = 0.45f;
            var seq = DOTween.Sequence();
            seq.Append(fromRT.DOAnchorPos(new Vector2(-dir * slideW, fromOrigin.y), duration)
                             .SetEase(Ease.InOutCubic));
            seq.Join(toRT.DOAnchorPos(toOrigin, duration).SetEase(Ease.InOutCubic));
            yield return seq.WaitForCompletion();

            fromRT.anchoredPosition = fromOrigin;
            toRT.anchoredPosition   = toOrigin;
            _animCount--;
        }

        // ── Hit Flash (survives damage) ───────────────────────────────────────

        public IEnumerator PlayHitFlash(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_hit");

            var seq = DOTween.Sequence();
            seq.Append(rt.DOPunchScale(new Vector3(0.22f, 0.22f, 0f), 0.20f, 5, 0.5f));
            seq.Join(rt.DOPunchPosition(new Vector3(6f, 3f, 0f), 0.20f, 8, 0.4f, false));
            yield return seq.WaitForCompletion();

            _animCount--;
        }

        // ── Card Play Anim (arc flight + Z-tilt + squash-stretch landing) ────

        public IEnumerator PlayCardPlayAnim(Vector3 fromWorldPos, Vector2 cardSize,
                                            LaneSlotUI targetSlot, CardInstance cardInst)
        {
            if (_canvas == null) yield break;

            _animCount++;

            // ─ 1. Ghost card ──────────────────────────────────────────────────
            var ghost = BuildGhostCard(_canvas.transform, cardInst, cardSize);
            ghost.transform.position   = fromWorldPos;
            ghost.transform.localScale = Vector3.one;

            // ─ 2. Hide real placed card ───────────────────────────────────────
            var placed = targetSlot?.PlayedCard;
            if (placed?.animatedRoot != null)
                placed.animatedRoot.localScale = Vector3.zero;

            // ─ 3. Arc flight via quadratic Bezier ────────────────────────────
            Vector3 endPos    = targetSlot.transform.position;
            float   arcHeight = Mathf.Max(80f, Vector3.Distance(fromWorldPos, endPos) * 0.38f);
            Vector3 midPos    = Vector3.Lerp(fromWorldPos, endPos, 0.5f)
                                + new Vector3(0f, arcHeight, 0f);

            const float flyDur = 0.42f;

            var rotSeq = DOTween.Sequence().SetTarget(ghost);
            rotSeq.Append(ghost.transform.DORotate(new Vector3(0f, 0f, 14f), flyDur * 0.44f).SetEase(Ease.OutQuad));
            rotSeq.Append(ghost.transform.DORotate(Vector3.zero, flyDur * 0.56f).SetEase(Ease.InOutSine));

            var scaleSeq = DOTween.Sequence().SetTarget(ghost);
            scaleSeq.Append(ghost.transform.DOScale(1.22f, flyDur * 0.50f).SetEase(Ease.OutQuad));
            scaleSeq.Append(ghost.transform.DOScale(0.95f, flyDur * 0.50f).SetEase(Ease.InCubic));

            float arcT = 0f;
            var moveTween = DOTween.To(() => arcT, v =>
            {
                arcT = v;
                float u = 1f - arcT;
                ghost.transform.position = u * u * fromWorldPos
                                         + 2f * u * arcT * midPos
                                         + arcT * arcT * endPos;
            }, 1f, flyDur).SetEase(Ease.InOutSine).SetTarget(ghost);

            yield return moveTween.WaitForCompletion();

            // ─ 4. Kill all ghost tweens, destroy ghost ────────────────────────
            DOTween.Kill(ghost);
            Object.Destroy(ghost);
            AudioManager.Instance.PlaySFX("sfx_card_place");

            // ─ 5. Squash-stretch landing on real card ─────────────────────────
            if (placed?.animatedRoot == null) { _animCount--; yield break; }
            var rt = placed.animatedRoot;

            var landSeq = DOTween.Sequence();
            landSeq.Append(rt.DOScale(new Vector3(1.22f, 1.22f, 1f), 0.11f).SetEase(Ease.OutQuint));
            landSeq.Append(rt.DOScale(new Vector3(1.16f, 0.80f, 1f), 0.09f).SetEase(Ease.OutQuad));
            landSeq.Append(rt.DOScale(new Vector3(0.92f, 1.12f, 1f), 0.08f).SetEase(Ease.OutSine));
            landSeq.Append(rt.DOScale(Vector3.one, 0.09f).SetEase(Ease.InOutSine));

            yield return landSeq.WaitForCompletion();
            _animCount--;
        }

        // ── Advance (slide one cell) ──────────────────────────────────────────

        public IEnumerator PlayAdvanceAnim(LaneSlotUI fromSlot, LaneSlotUI toSlot, bool isPlayer)
        {
            if (fromSlot?.PlayedCard == null) yield break;
            var animRoot = fromSlot.PlayedCard.animatedRoot;
            if (animRoot == null) yield break;
            if (_canvas == null) { yield break; }

            _animCount++;

            var animGO = animRoot.gameObject;

            var slotRt   = fromSlot.GetComponent<RectTransform>();
            Vector2 size = slotRt.rect.size;
            Vector3 from = animGO.transform.position;
            Vector3 to   = toSlot.transform.position;

            animGO.transform.SetParent(_canvas.transform, true);
            animGO.transform.SetAsLastSibling();

            animRoot.anchorMin = new Vector2(0.5f, 0.5f);
            animRoot.anchorMax = new Vector2(0.5f, 0.5f);
            animRoot.pivot     = new Vector2(0.5f, 0.5f);
            animRoot.sizeDelta = size;
            animRoot.position  = from;

            var seq = DOTween.Sequence();
            seq.Append(animRoot.DOMove(to, 0.20f).SetEase(Ease.OutCubic));
            seq.Join(animRoot.DOScale(new Vector3(1.18f, 0.88f, 1f), 0.20f).SetEase(Ease.OutQuad));
            seq.Append(animRoot.DOScale(new Vector3(0.88f, 1.14f, 1f), 0.07f).SetEase(Ease.OutQuad));
            seq.Append(animRoot.DOScale(Vector3.one, 0.13f).SetEase(Ease.OutBack, 1.8f));

            yield return seq.WaitForCompletion();

            DOTween.Kill(animGO.transform);
            Object.Destroy(animGO);
            _animCount--;
        }

        // ── Clash (both units fight — 4-phase squash-stretch) ────────────────

        public IEnumerator PlayClashAnim(LaneSlotUI playerSlot, LaneSlotUI enemySlot)
        {
            _animCount++;

            var pRT = playerSlot?.PlayedCard?.animatedRoot;
            var eRT = enemySlot?.PlayedCard?.animatedRoot;

            Vector2 pOrigin = pRT != null ? pRT.anchoredPosition : Vector2.zero;
            Vector2 eOrigin = eRT != null ? eRT.anchoredPosition : Vector2.zero;

            // ── Phase 1 — Both pull back away from each other (squash) ────────
            var anticipate = DOTween.Sequence();
            if (pRT != null)
            {
                anticipate.Append(pRT.DOAnchorPos(pOrigin + new Vector2(-13f, 0f), 0.12f).SetEase(Ease.OutQuad));
                anticipate.Join(pRT.DOScale(new Vector3(0.82f, 1.16f, 1f), 0.12f).SetEase(Ease.OutQuad));
            }
            if (eRT != null)
            {
                anticipate.Join(eRT.DOAnchorPos(eOrigin + new Vector2(13f, 0f), 0.12f).SetEase(Ease.OutQuad));
                anticipate.Join(eRT.DOScale(new Vector3(0.82f, 1.16f, 1f), 0.12f).SetEase(Ease.OutQuad));
            }
            yield return anticipate.WaitForCompletion();

            // ── Phase 2 — Both lunge toward each other (horizontal stretch) ───
            AudioManager.Instance.PlaySFX("sfx_attack");
            var lunge = DOTween.Sequence();
            if (pRT != null)
            {
                lunge.Append(pRT.DOAnchorPos(pOrigin + new Vector2(22f, 0f), 0.10f).SetEase(Ease.OutQuint));
                lunge.Join(pRT.DOScale(new Vector3(1.22f, 0.80f, 1f), 0.10f).SetEase(Ease.OutQuint));
            }
            if (eRT != null)
            {
                lunge.Join(eRT.DOAnchorPos(eOrigin + new Vector2(-22f, 0f), 0.10f).SetEase(Ease.OutQuint));
                lunge.Join(eRT.DOScale(new Vector3(1.22f, 0.80f, 1f), 0.10f).SetEase(Ease.OutQuint));
            }
            yield return lunge.WaitForCompletion();

            // ── Phase 3 — Simultaneous impact punch + screen shake ────────────
            shakeTarget?.DOShakeAnchorPos(0.20f, 12f, 18, 60f, false, true);
            var impact = DOTween.Sequence();
            if (pRT != null) impact.Append(pRT.DOPunchScale(new Vector3(0.26f, 0.26f, 0f), 0.16f, 5, 0.4f));
            if (eRT != null) impact.Join(eRT.DOPunchScale(new Vector3(0.26f, 0.26f, 0f), 0.16f, 5, 0.4f));
            yield return impact.WaitForCompletion();

            // ── Phase 4 — Both return with slight overshoot ───────────────────
            var ret = DOTween.Sequence();
            if (pRT != null)
            {
                ret.Append(pRT.DOAnchorPos(pOrigin, 0.20f).SetEase(Ease.OutBack, 1.3f));
                ret.Join(pRT.DOScale(Vector3.one, 0.20f).SetEase(Ease.OutBack, 1.3f));
            }
            if (eRT != null)
            {
                ret.Join(eRT.DOAnchorPos(eOrigin, 0.20f).SetEase(Ease.OutBack, 1.3f));
                ret.Join(eRT.DOScale(Vector3.one, 0.20f).SetEase(Ease.OutBack, 1.3f));
            }
            yield return ret.WaitForCompletion();

            if (pRT != null) { pRT.anchoredPosition = pOrigin; pRT.localScale = Vector3.one; }
            if (eRT != null) { eRT.anchoredPosition = eOrigin; eRT.localScale = Vector3.one; }
            _animCount--;
        }

        // ── Place Anim (enemy card placement — squash-stretch drop-in) ───────

        public IEnumerator PlayPlaceAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            rt.localScale = Vector3.zero;

            // Phase 1 — pop in with tall stretch: card "drops" from above (0.09s)
            // Phase 2 — squash on landing: wide and flat (0.08s)
            // Phase 3 — rebound tall: elastic bounce (0.07s)
            // Phase 4 — settle to rest (0.09s)
            var seq = DOTween.Sequence();
            seq.Append(rt.DOScale(new Vector3(0.86f, 1.24f, 1f), 0.09f).SetEase(Ease.OutQuint));
            seq.AppendCallback(() => AudioManager.Instance.PlaySFX("sfx_card_place"));
            seq.Append(rt.DOScale(new Vector3(1.20f, 0.78f, 1f), 0.08f).SetEase(Ease.OutQuad));
            seq.Append(rt.DOScale(new Vector3(0.92f, 1.10f, 1f), 0.07f).SetEase(Ease.OutSine));
            seq.Append(rt.DOScale(Vector3.one,                   0.09f).SetEase(Ease.InOutSine));
            yield return seq.WaitForCompletion();

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

            var bgImg = go.AddComponent<Image>();
            bgImg.sprite        = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImg.color         = Color.white;
            bgImg.raycastTarget = false;

            var illuGO  = GhostMakeChild("Illustration", go);
            GhostSetAnchors(illuGO, 0f, 0f, 1f, 1f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.sprite        = card.data.artwork;
            illuImg.color         = Color.white;
            illuImg.raycastTarget = false;

            var frontGO  = GhostMakeChild("Front", go);
            GhostSetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

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
    }
}
