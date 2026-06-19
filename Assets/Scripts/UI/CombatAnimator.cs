using UnityEngine;
using DG.Tweening;
using RoguelikeTCG.Combat;
using System;

namespace RoguelikeTCG.UI
{
    public class CombatAnimator : MonoBehaviour
    {
        public static CombatAnimator Instance { get; private set; }

        [Header("Refs")]
        public BoardCoordinator boardCoordinator;

        [Header("Timings")]
        public float placePunchDuration = 0.3f;
        public float attackSlideDist = 45f;
        public float attackSlideDuration = 0.15f;
        public float attackReturnDuration = 0.1f;
        public float deathDuration = 0.28f;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (CombatManager.Instance?.Board != null)
                CombatManager.Instance.Board.OnUnitPlaced += OnUnitPlaced;
        }

        private void OnUnitPlaced(int slot, TurnSide side)
        {
            var slotUI = GetSlot(slot, side);
            if (slotUI?.CurrentView == null) return;
            slotUI.CurrentView.transform.DOKill();
            slotUI.CurrentView.transform.DOPunchScale(Vector3.one * 0.25f, placePunchDuration, 6, 0.5f);
        }

        public void PlayAttackAnim(int slot, TurnSide side, Action onComplete)
        {
            var slotUI = GetSlot(slot, side);
            if (slotUI?.CurrentView == null) { onComplete?.Invoke(); return; }

            var panel = slotUI.CurrentView.transform;
            float dir = side == TurnSide.Player ? 1f : -1f;
            Vector3 origin = panel.localPosition;

            DOTween.Sequence()
                .Append(panel.DOLocalMoveY(origin.y + dir * attackSlideDist, attackSlideDuration).SetEase(Ease.OutQuad))
                .Append(panel.DOLocalMoveY(origin.y, attackReturnDuration).SetEase(Ease.InQuad))
                .OnComplete(() => onComplete?.Invoke());
        }

        public void PlayDeathAnim(int slot, TurnSide side, Action onComplete)
        {
            var slotUI = GetSlot(slot, side);
            if (slotUI?.CurrentView == null) { onComplete?.Invoke(); return; }

            var panel = slotUI.CurrentView.transform;
            var cg = slotUI.CurrentView.GetComponent<CanvasGroup>();
            if (cg == null) cg = slotUI.CurrentView.gameObject.AddComponent<CanvasGroup>();

            panel.DOKill();
            DOTween.Sequence()
                .Append(panel.DOScale(0f, deathDuration).SetEase(Ease.InBack))
                .Join(cg.DOFade(0f, deathDuration))
                .OnComplete(() =>
                {
                    // CurrentView sera détruit par Refresh() — pas besoin de reset
                    onComplete?.Invoke();
                });
        }

        private BoardSlotUI GetSlot(int slot, TurnSide side)
        {
            if (boardCoordinator == null) return null;
            if (side == TurnSide.Player)
                return (slot >= 0 && slot < boardCoordinator.playerSlots.Length)
                    ? boardCoordinator.playerSlots[slot] : null;
            else
                return (slot >= 0 && slot < boardCoordinator.enemySlots.Length)
                    ? boardCoordinator.enemySlots[slot] : null;
        }
    }
}
