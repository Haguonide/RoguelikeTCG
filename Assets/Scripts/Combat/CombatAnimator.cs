using System.Collections;
using System.Collections.Generic;
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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (shakeTarget != null) DOTween.Kill(shakeTarget);
            DOTween.KillAll();
        }

        // ── Attack / Traverse (lunge → impact → return) ──────────────────────

        public IEnumerator PlayAttackAnim(LaneSlotUI slot, bool isPlayerAttacking)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            Vector2 origin = rt.anchoredPosition;

            float pullX  = isPlayerAttacking ? -18f :  18f;
            float lungeX = isPlayerAttacking ?  52f : -52f;

            // ── Phase 1 — Anticipation: pull back + squash (0.10s) ───────────
            var anticipate = DOTween.Sequence().SetTarget(rt);
            anticipate.Append(rt.DOAnchorPos(origin + new Vector2(pullX, -4f), 0.10f).SetEase(Ease.OutCubic));
            anticipate.Join(rt.DOScale(new Vector3(0.78f, 1.20f, 1f), 0.10f).SetEase(Ease.OutCubic));
            yield return anticipate.WaitForCompletion();

            // ── Phase 2 — Lunge: burst forward + horizontal stretch (0.08s) ──
            AudioManager.Instance.PlaySFX("sfx_attack");
            var lunge = DOTween.Sequence().SetTarget(rt);
            lunge.Append(rt.DOAnchorPos(origin + new Vector2(lungeX, 0f), 0.08f).SetEase(Ease.OutExpo));
            lunge.Join(rt.DOScale(new Vector3(1.30f, 0.74f, 1f), 0.08f).SetEase(Ease.OutExpo));
            yield return lunge.WaitForCompletion();

            // ── Phase 3 — Impact: hard punch + screen shake (0.14s) ──────────
            if (shakeTarget != null)
                shakeTarget.DOShakeAnchorPos(0.18f, 10f, 16, 60f, false, true).SetTarget(shakeTarget);
            yield return rt.DOPunchScale(new Vector3(0.32f, 0.32f, 0f), 0.14f, 6, 0.40f).SetTarget(rt)
                           .WaitForCompletion();

            // ── Phase 4 — Return: snap back with elastic overshoot (0.20s) ───
            var ret = DOTween.Sequence().SetTarget(rt);
            ret.Append(rt.DOAnchorPos(origin, 0.20f).SetEase(Ease.OutBack, 1.8f));
            ret.Join(rt.DOScale(Vector3.one, 0.20f).SetEase(Ease.OutBack, 1.8f));
            yield return ret.WaitForCompletion();

            rt.anchoredPosition = origin;
            rt.localScale       = Vector3.one;
            _animCount--;
        }

        // ── Death (stagger → shatter → violent collapse) ─────────────────────

        public IEnumerator PlayDeathAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_death");

            // ── Phase 1 — Stagger: sharp recoil backward + vertical squash (0.09s)
            bool isPlayer = slot.HasPlayerUnit;
            float recoilX = isPlayer ? -20f : 20f;
            var stagger = DOTween.Sequence().SetTarget(rt);
            stagger.Append(rt.DOAnchorPos(rt.anchoredPosition + new Vector2(recoilX, -8f), 0.09f)
                             .SetEase(Ease.OutExpo));
            stagger.Join(rt.DOScale(new Vector3(1.10f, 0.80f, 1f), 0.09f).SetEase(Ease.OutExpo));
            yield return stagger.WaitForCompletion();

            // ── Phase 2 — Shatter: bloat + violent tremble + heavy shake (0.16s)
            if (shakeTarget != null)
                shakeTarget.DOShakeAnchorPos(0.22f, 12f, 20, 65f, false, true).SetTarget(shakeTarget);
            var shatter = DOTween.Sequence().SetTarget(rt);
            shatter.Append(rt.DOScale(new Vector3(1.40f, 1.40f, 1f), 0.10f).SetEase(Ease.OutQuad).SetTarget(rt));
            shatter.Join(rt.DOPunchPosition(new Vector3(12f, 9f, 0f), 0.16f, 16, 0.7f, false).SetTarget(rt));
            shatter.Join(rt.DOLocalRotate(new Vector3(0f, 0f, 15f), 0.10f).SetEase(Ease.OutQuad).SetTarget(rt));
            yield return shatter.WaitForCompletion();

            // ── Phase 3 — Collapse: wild spin-out + shrink (0.24s) ────────────
            float spinDir = isPlayer ? -1f : 1f;
            var collapse = DOTween.Sequence().SetTarget(rt);
            collapse.Append(rt.DOScale(Vector3.zero, 0.24f).SetEase(Ease.InBack, 2.5f));
            collapse.Join(rt.DOLocalRotate(new Vector3(0f, 0f, spinDir * 250f), 0.24f).SetEase(Ease.InCubic));
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

        // ── Hit Flash (survives damage — short recoil only) ──────────────────

        public IEnumerator PlayHitFlash(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            AudioManager.Instance.PlaySFX("sfx_hit");

            // Intentionally short and snappy — distinct from clash impact
            bool isPlayer = slot.HasPlayerUnit;
            float nudgeX  = isPlayer ? -8f : 8f;
            var seq = DOTween.Sequence().SetTarget(rt);
            seq.Append(rt.DOPunchScale(new Vector3(0.16f, 0.16f, 0f), 0.15f, 4, 0.5f).SetTarget(rt));
            seq.Join(rt.DOPunchPosition(new Vector3(nudgeX, 2f, 0f), 0.15f, 6, 0.35f, false).SetTarget(rt));
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

            var landSeq = DOTween.Sequence().SetTarget(rt);
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

            var seq = DOTween.Sequence().SetTarget(animRoot);
            seq.Append(animRoot.DOMove(to, 0.20f).SetEase(Ease.OutCubic));
            seq.Join(animRoot.DOScale(new Vector3(1.18f, 0.88f, 1f), 0.20f).SetEase(Ease.OutQuad));
            seq.Append(animRoot.DOScale(new Vector3(0.88f, 1.14f, 1f), 0.07f).SetEase(Ease.OutQuad));
            seq.Append(animRoot.DOScale(Vector3.one, 0.13f).SetEase(Ease.OutBack, 1.8f));

            yield return seq.WaitForCompletion();

            DOTween.Kill(animRoot);
            if (animGO != null) Object.Destroy(animGO);
            _animCount--;
        }

        // ── Clash (both units fight — 4-phase squash-stretch + damage popups) ─
        // dmgToPlayer  : damage the player unit receives (shown above playerSlot)
        // dmgToEnemy   : damage the enemy unit receives (shown above enemySlot)
        // playerDies / enemyDies : popup gets "fatal" color if unit will die

        public IEnumerator PlayClashAnim(
            LaneSlotUI playerSlot, LaneSlotUI enemySlot,
            int dmgToPlayer = 0, int dmgToEnemy = 0,
            bool playerDies = false, bool enemyDies = false)
        {
            _animCount++;

            var pRT = playerSlot?.PlayedCard?.animatedRoot;
            var eRT = enemySlot?.PlayedCard?.animatedRoot;

            Vector2 pOrigin = pRT != null ? pRT.anchoredPosition : Vector2.zero;
            Vector2 eOrigin = eRT != null ? eRT.anchoredPosition : Vector2.zero;

            // ── Phase 1 — Both pull back away from each other (0.11s) ─────────
            var anticipate = DOTween.Sequence().SetAutoKill(true);
            if (pRT != null)
            {
                anticipate.Append(pRT.DOAnchorPos(pOrigin + new Vector2(-18f, -3f), 0.11f).SetEase(Ease.OutCubic).SetTarget(pRT));
                anticipate.Join(pRT.DOScale(new Vector3(0.78f, 1.22f, 1f), 0.11f).SetEase(Ease.OutCubic).SetTarget(pRT));
            }
            if (eRT != null)
            {
                anticipate.Join(eRT.DOAnchorPos(eOrigin + new Vector2(15f, -3f), 0.11f).SetEase(Ease.OutCubic).SetTarget(eRT));
                anticipate.Join(eRT.DOScale(new Vector3(0.80f, 1.18f, 1f), 0.11f).SetEase(Ease.OutCubic).SetTarget(eRT));
            }
            yield return anticipate.WaitForCompletion();

            // ── Phase 2 — Both lunge toward each other (0.09s) ───────────────
            AudioManager.Instance.PlaySFX("sfx_attack");
            var lunge = DOTween.Sequence().SetAutoKill(true);
            if (pRT != null)
            {
                lunge.Append(pRT.DOAnchorPos(pOrigin + new Vector2(30f, 0f), 0.09f).SetEase(Ease.OutExpo).SetTarget(pRT));
                lunge.Join(pRT.DOScale(new Vector3(1.28f, 0.76f, 1f), 0.09f).SetEase(Ease.OutExpo).SetTarget(pRT));
            }
            if (eRT != null)
            {
                lunge.Join(eRT.DOAnchorPos(eOrigin + new Vector2(-26f, 0f), 0.09f).SetEase(Ease.OutExpo).SetTarget(eRT));
                lunge.Join(eRT.DOScale(new Vector3(1.24f, 0.78f, 1f), 0.09f).SetEase(Ease.OutExpo).SetTarget(eRT));
            }
            yield return lunge.WaitForCompletion();

            // ── Phase 3 — Simultaneous impact + screen shake + damage popups ──
            if (shakeTarget != null)
                shakeTarget.DOShakeAnchorPos(0.22f, 14f, 20, 65f, false, true).SetTarget(shakeTarget);

            if (dmgToEnemy > 0 && enemySlot != null)
                RoguelikeTCG.UI.DamagePopup.ShowClashDamage(
                    (RectTransform)enemySlot.transform, dmgToEnemy, enemyDies);
            if (dmgToPlayer > 0 && playerSlot != null)
                RoguelikeTCG.UI.DamagePopup.ShowClashDamage(
                    (RectTransform)playerSlot.transform, dmgToPlayer, playerDies);

            var impact = DOTween.Sequence().SetAutoKill(true);
            if (pRT != null) impact.Append(pRT.DOPunchScale(new Vector3(0.30f, 0.30f, 0f), 0.18f, 6, 0.38f).SetTarget(pRT));
            if (eRT != null) impact.Join(eRT.DOPunchScale(new Vector3(0.30f, 0.30f, 0f), 0.18f, 6, 0.38f).SetTarget(eRT));
            yield return impact.WaitForCompletion();

            // ── Phase 4 — Both return — player snaps back harder (OutBack) ────
            var ret = DOTween.Sequence().SetAutoKill(true);
            if (pRT != null)
            {
                ret.Append(pRT.DOAnchorPos(pOrigin, 0.22f).SetEase(Ease.OutBack, 1.6f).SetTarget(pRT));
                ret.Join(pRT.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack, 1.6f).SetTarget(pRT));
            }
            if (eRT != null)
            {
                ret.Join(eRT.DOAnchorPos(eOrigin, 0.22f).SetEase(Ease.OutBack, 1.2f).SetTarget(eRT));
                ret.Join(eRT.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack, 1.2f).SetTarget(eRT));
            }
            yield return ret.WaitForCompletion();

            if (pRT != null) { pRT.anchoredPosition = pOrigin; pRT.localScale = Vector3.one; }
            if (eRT != null) { eRT.anchoredPosition = eOrigin; eRT.localScale = Vector3.one; }
            _animCount--;
        }

        // ── Place Anim (enemy card placement — drop from top + squash landing) ─

        public IEnumerator PlayPlaceAnim(LaneSlotUI slot)
        {
            if (slot?.PlayedCard == null) yield break;
            var rt = slot.PlayedCard.animatedRoot;
            if (rt == null) yield break;

            _animCount++;
            rt.localScale    = new Vector3(0.70f, 0.70f, 1f);
            Vector2 origin   = rt.anchoredPosition;
            rt.anchoredPosition = origin + new Vector2(0f, 40f);   // starts above slot

            // Phase 1 — fall down: tall stretch as it drops (0.11s)
            // Phase 2 — squash on landing: wide and flat (0.09s)
            // Phase 3 — rebound: tall snap-back (0.07s)
            // Phase 4 — settle (0.10s)
            var seq = DOTween.Sequence().SetTarget(rt);
            seq.Append(rt.DOAnchorPos(origin, 0.11f).SetEase(Ease.InCubic));
            seq.Join(rt.DOScale(new Vector3(0.82f, 1.26f, 1f), 0.11f).SetEase(Ease.InCubic));
            seq.AppendCallback(() => AudioManager.Instance.PlaySFX("sfx_card_place"));
            seq.Append(rt.DOScale(new Vector3(1.26f, 0.74f, 1f), 0.09f).SetEase(Ease.OutQuad));
            seq.Append(rt.DOScale(new Vector3(0.90f, 1.12f, 1f), 0.07f).SetEase(Ease.OutSine));
            seq.Append(rt.DOScale(Vector3.one,                   0.10f).SetEase(Ease.OutBack, 1.6f));
            yield return seq.WaitForCompletion();

            rt.localScale       = Vector3.one;
            rt.anchoredPosition = origin;
            _animCount--;
        }

        // ── Draw Cards (ghost flies from deck → hand slot, existing cards slide) ──

        public IEnumerator PlayDrawCardsAnim(
            List<CardInstance> fullHand, int drawnCount, int totalLayoutCount,
            RectTransform deckRT, HandView handView)
        {
            if (drawnCount <= 0 || handView == null || deckRT == null || _canvas == null)
                yield break;

            _animCount++;

            int existingCount = fullHand.Count - drawnCount;
            var newCards      = fullHand.GetRange(existingCount, drawnCount);

            // Slide existing hand cards sideways to make room (fire-and-forget)
            handView.SlideExistingCards(totalLayoutCount);

            // Create real card GOs at their final positions, invisible (scale=0)
            var targetRTs = handView.InsertCardsInvisible(newCards, existingCount, totalLayoutCount);

            // Fly each card from deck, staggered
            const float stagger = 0.07f;
            Vector3 deckPos = deckRT.position;
            for (int n = 0; n < drawnCount; n++)
                StartCoroutine(FlyCardIn(newCards[n], deckPos, targetRTs[n], n * stagger));

            yield return new WaitForSeconds((drawnCount - 1) * stagger + 0.38f + 0.27f);
            _animCount--;
        }

        private IEnumerator FlyCardIn(CardInstance card, Vector3 fromPos,
            RectTransform targetRT, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (_canvas == null) yield break;

            // Ghost spawned at deck position, slightly smaller than final card
            var ghost = BuildGhostCard(_canvas.transform, card, new Vector2(160f, 240f));
            ghost.transform.position   = fromPos;
            ghost.transform.localScale = Vector3.one * 0.72f;

            Vector3 endPos = targetRT.position;
            float   dist   = Vector3.Distance(fromPos, endPos);
            float   arc    = Mathf.Max(55f, dist * 0.30f);
            Vector3 midPos = Vector3.Lerp(fromPos, endPos, 0.5f) + new Vector3(0f, arc, 0f);

            const float flyDur = 0.38f;

            // Slight Z-tilt during flight
            DOTween.Sequence().SetTarget(ghost)
                .Append(ghost.transform.DORotate(new Vector3(0f, 0f, -9f), flyDur * 0.45f)
                    .SetEase(Ease.OutQuad))
                .Append(ghost.transform.DORotate(Vector3.zero, flyDur * 0.55f)
                    .SetEase(Ease.InSine));

            // Scale: launch small → grow → compress on impact
            DOTween.Sequence().SetTarget(ghost)
                .Append(ghost.transform.DOScale(1.08f, flyDur * 0.40f).SetEase(Ease.OutCubic))
                .Append(ghost.transform.DOScale(0.88f, flyDur * 0.60f).SetEase(Ease.InCubic));

            // Quadratic Bezier arc
            float t = 0f;
            yield return DOTween.To(() => t, v => {
                t = v;
                float u = 1f - t;
                ghost.transform.position = u * u * fromPos + 2f * u * t * midPos + t * t * endPos;
            }, 1f, flyDur).SetEase(Ease.InOutSine).WaitForCompletion();

            DOTween.Kill(ghost);
            Object.Destroy(ghost);
            AudioManager.Instance.PlaySFX("sfx_card_place");

            // Squash-stretch landing on the real card
            var land = DOTween.Sequence().SetTarget(targetRT);
            land.Append(targetRT.DOScale(new Vector3(1.14f, 0.80f, 1f), 0.07f).SetEase(Ease.OutQuint));
            land.Append(targetRT.DOScale(new Vector3(0.88f, 1.18f, 1f), 0.07f).SetEase(Ease.OutSine));
            land.Append(targetRT.DOScale(new Vector3(1.05f, 0.95f, 1f), 0.05f).SetEase(Ease.OutSine));
            land.Append(targetRT.DOScale(Vector3.one,                   0.08f).SetEase(Ease.OutBack, 1.4f));
            yield return land.WaitForCompletion();
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
