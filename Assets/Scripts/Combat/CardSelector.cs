using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Manages card selection in hand and spell targeting state machine.
    /// </summary>
    public class CardSelector : MonoBehaviour
    {
        public static CardSelector Instance { get; private set; }

        private enum TargetingMode { None, AwaitingLane, AwaitingHero, AwaitingAllyUnit, AwaitingEnemyUnit, AwaitingAoEConfirm, AwaitingBricolage }

        private CardView     selectedView;
        private TargetingMode mode = TargetingMode.None;

        private static readonly Color HighlightColor = new Color(1f, 0.85f, 0.1f, 1f);
        private static readonly Color HighlightSlot  = new Color(0.15f, 1f, 0.3f, 0.45f);

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool HasSelection => selectedView != null || mode == TargetingMode.AwaitingBricolage;

        // ── Select a card from hand ────────────────────────────────────────────

        public void SelectCard(CardView view)
        {
            // Toggle off same card
            if (selectedView == view) { Deselect(); return; }
            AudioManager.Instance.PlaySFX("sfx_card_select");

            Deselect();
            selectedView = view;
            ApplyHighlight(true);

            var card = selectedView.CardInstance;
            if (card == null) { Deselect(); return; }

            if (card.IsUnit)
            {
                mode = TargetingMode.AwaitingLane;
            }
            else
            {
                // Route spell to correct targeting mode
                switch (card.data.spellTarget)
                {
                    case SpellTarget.PlayerHero:
                    case SpellTarget.EnemyHero:
                        mode = TargetingMode.AwaitingHero;
                        break;
                    case SpellTarget.AllyUnit:
                        mode = TargetingMode.AwaitingAllyUnit;
                        break;
                    case SpellTarget.EnemyUnit:
                        mode = TargetingMode.AwaitingEnemyUnit;
                        break;
                    case SpellTarget.AllEnemyUnits:
                        mode = TargetingMode.AwaitingAoEConfirm;
                        break;
                }
            }
            RefreshHighlights();
        }

        public void SelectBricolage()
        {
            var cm = CombatManager.Instance;
            if (cm == null || cm.gearCount < 3 || !cm.IsDeVinciRun()) return;
            Deselect();
            mode = TargetingMode.AwaitingBricolage;
            RefreshHighlights();
            AudioManager.Instance.PlaySFX("sfx_card_select");
        }

        public void Deselect()
        {
            if (selectedView == null) return;
            ApplyHighlight(false);
            ClearHighlights();
            selectedView = null;
            mode = TargetingMode.None;
        }

        // ── Called by LaneSlotUI ───────────────────────────────────────────────

        public void OnLaneClicked(LaneSlotUI slot)
        {
            if (slot == null) return;

            if (mode == TargetingMode.AwaitingBricolage)
            {
                if (!slot.isPlayerLane || slot.Lane == null || slot.Lane.IsOccupied) return;
                if (IsOnDefeatedBoard(slot)) return;
                CombatManager.Instance?.TryPlayBricolage(slot.Lane);
                mode = TargetingMode.None;
                ClearHighlights();
                return;
            }

            if (selectedView == null) return;
            var card = selectedView.CardInstance;
            if (card == null) return;

            switch (mode)
            {
                case TargetingMode.AwaitingLane:
                    // Unit placement: must be player lane, empty
                    if (!slot.isPlayerLane || slot.Lane == null) return;
                    {
                        // Capture hand card position + size BEFORE TryPlayUnit destroys the hand
                        Vector3 handWorldPos = selectedView.transform.position;
                        Vector2 handCardSize = selectedView.GetComponent<RectTransform>().rect.size;
                        var     cardInst     = card;

                        if (CombatManager.Instance.TryPlayUnit(card, slot.Lane))
                        {
                            Deselect();
                            slot.Refresh();
                            if (CombatAnimator.Instance != null)
                                CombatAnimator.Instance.StartCoroutine(
                                    CombatAnimator.Instance.PlayCardPlayAnim(
                                        handWorldPos, handCardSize, slot, cardInst));
                        }
                    }
                    break;

                case TargetingMode.AwaitingAllyUnit:
                    // Spell targeting ally unit: must be player lane, occupied
                    if (!slot.isPlayerLane || slot.Lane == null || !slot.Lane.IsOccupied) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, slot.Lane))
                    {
                        Deselect();
                        slot.Refresh();
                    }
                    break;

                case TargetingMode.AwaitingEnemyUnit:
                    // Spell targeting enemy unit: must be enemy lane, occupied
                    if (slot.isPlayerLane || slot.Lane == null || !slot.Lane.IsOccupied) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, slot.Lane))
                    {
                        Deselect();
                        slot.Refresh();
                    }
                    break;

                case TargetingMode.AwaitingAoEConfirm:
                    // AoE spell: clicking any enemy slot confirms the cast
                    if (slot.isPlayerLane) return;
                    if (CombatManager.Instance.TryPlayAoESpell(card))
                        Deselect();
                    break;

            }
        }

        // ── Called by HeroPortraitUI ───────────────────────────────────────────

        public void OnHeroClicked(bool isPlayerPortrait)
        {
            if (selectedView == null || mode != TargetingMode.AwaitingHero) return;
            var card = selectedView.CardInstance;
            if (card == null) return;

            // Validate portrait matches spell target
            bool wantsPlayer = card.data.spellTarget == SpellTarget.PlayerHero;
            if (isPlayerPortrait != wantsPlayer)
            {
                Debug.Log($"[CardSelector] Ce sort cible {(wantsPlayer ? "le héros allié" : "le héros ennemi")}.");
                return;
            }

            if (CombatManager.Instance.TryPlaySpellOnHero(card, isPlayerPortrait))
                Deselect();
        }

        // ── Target Highlights ─────────────────────────────────────────────────

        private void RefreshHighlights()
        {
            ClearHighlights();
            var allSlots  = FindObjectsOfType<LaneSlotUI>(true);
            var allHeroes = FindObjectsOfType<HeroPortraitUI>(true);

            switch (mode)
            {
                case TargetingMode.AwaitingLane:
                    foreach (var s in allSlots)
                        if (s.isPlayerLane && s.Lane != null && !s.Lane.IsOccupied && !IsOnDefeatedBoard(s))
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case TargetingMode.AwaitingAllyUnit:
                    foreach (var s in allSlots)
                        if (s.isPlayerLane && s.Lane != null && s.Lane.IsOccupied && !IsOnDefeatedBoard(s))
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case TargetingMode.AwaitingEnemyUnit:
                    foreach (var s in allSlots)
                        if (!s.isPlayerLane && s.Lane != null && s.Lane.IsOccupied && !IsOnDefeatedBoard(s))
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case TargetingMode.AwaitingHero:
                    if (selectedView?.CardInstance == null) break;
                    bool wantsPlayer = selectedView.CardInstance.data.spellTarget == SpellTarget.PlayerHero;
                    foreach (var h in allHeroes)
                        if (h.isPlayerPortrait == wantsPlayer)
                            h.SetHighlight(true);
                    break;

                case TargetingMode.AwaitingAoEConfirm:
                    foreach (var s in allSlots)
                        if (!s.isPlayerLane && !IsOnDefeatedBoard(s))
                            s.SetHighlight(true, new Color(1f, 0.35f, 0.1f, 0.45f));
                    break;

                case TargetingMode.AwaitingBricolage:
                    foreach (var s in allSlots)
                        if (s.isPlayerLane && s.Lane != null && !s.Lane.IsOccupied && !IsOnDefeatedBoard(s))
                            s.SetHighlight(true, HighlightSlot);
                    break;
            }
        }

        private static bool IsOnDefeatedBoard(LaneSlotUI slot)
        {
            if (slot.Lane == null || CombatManager.Instance == null) return false;
            foreach (var board in CombatManager.Instance.boardManager.boards)
            {
                foreach (var l in board.playerLanes)
                    if (l == slot.Lane) return board.IsDefeated;
                foreach (var l in board.enemyLanes)
                    if (l == slot.Lane) return board.IsDefeated;
            }
            return false;
        }

        private void ClearHighlights()
        {
            foreach (var s in FindObjectsOfType<LaneSlotUI>(true))
                s.SetHighlight(false, Color.clear);
            foreach (var h in FindObjectsOfType<HeroPortraitUI>(true))
                h.SetHighlight(false);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ApplyHighlight(bool on)
        {
            if (selectedView == null) return;
            var outline = selectedView.GetComponent<Outline>();
            if (outline == null && on) outline = selectedView.gameObject.AddComponent<Outline>();
            if (outline == null) return;
            outline.effectColor   = HighlightColor;
            outline.effectDistance = new Vector2(4, -4);
            outline.enabled       = on;
        }
    }
}
