using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    public class CardSelector : MonoBehaviour
    {
        public static CardSelector Instance { get; private set; }

        private enum Mode { None, AwaitingCell, AwaitingHero, AwaitingAllyUnit, AwaitingEnemyUnit, AwaitingAoEConfirm, AwaitingAllAllyUnits, AwaitingBricolage }

        private CardView selectedView;
        private Mode     mode = Mode.None;

        private static readonly Color HighlightCard  = new Color(1f, 0.85f, 0.1f, 1f);
        private static readonly Color HighlightSlot  = new Color(0.15f, 1f, 0.3f, 0.45f);
        private static readonly Color HighlightEnemy = new Color(1f, 0.35f, 0.1f, 0.45f);

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool HasSelection => selectedView != null || mode == Mode.AwaitingBricolage;

        // ── Select from hand ──────────────────────────────────────────────────

        public void SelectCard(CardView view)
        {
            if (selectedView == view) { Deselect(); return; }
            AudioManager.Instance.PlaySFX("sfx_card_select");

            Deselect();
            selectedView = view;
            ApplyCardHighlight(true);

            var card = selectedView.CardInstance;
            if (card == null) { Deselect(); return; }

            if (card.IsUnit)
            {
                mode = Mode.AwaitingCell;
            }
            else
            {
                mode = card.data.spellTarget switch
                {
                    SpellTarget.PlayerHero    => Mode.AwaitingHero,
                    SpellTarget.EnemyHero     => Mode.AwaitingHero,
                    SpellTarget.AllyUnit      => Mode.AwaitingAllyUnit,
                    SpellTarget.EnemyUnit     => Mode.AwaitingEnemyUnit,
                    SpellTarget.AllEnemyUnits => Mode.AwaitingAoEConfirm,
                    SpellTarget.AllAllyUnits  => Mode.AwaitingAllAllyUnits,
                    _                         => Mode.None,
                };
            }

            RefreshHighlights();
        }

        public void SelectBricolage()
        {
            var cm = CombatManager.Instance;
            if (cm == null || !cm.CanUseBricolage) return;
            Deselect();
            mode = Mode.AwaitingBricolage;
            RefreshHighlights();
            AudioManager.Instance.PlaySFX("sfx_card_select");
        }

        public void Deselect()
        {
            ApplyCardHighlight(false);
            ClearHighlights();
            selectedView = null;
            mode = Mode.None;
        }

        // ── Cell click (from LaneSlotUI) ──────────────────────────────────────

        public void OnCellClicked(LaneSlotUI slot)
        {
            if (slot == null) return;

            if (mode == Mode.AwaitingBricolage)
            {
                if (!slot.IsPlayerDeployZone || slot.IsOccupied) return;
                CombatManager.Instance?.TryPlayBricolage(slot.lane, slot.cellIndex);
                ClearHighlights();
                mode = Mode.None;
                return;
            }

            if (selectedView == null) return;
            var card = selectedView.CardInstance;
            if (card == null) return;

            switch (mode)
            {
                case Mode.AwaitingCell:
                    if (!slot.IsPlayerDeployZone || slot.IsOccupied) return;
                    {
                        Vector3 handPos   = selectedView.transform.position;
                        Vector2 handSize  = selectedView.GetComponent<RectTransform>().rect.size;
                        var     cardInst  = card;

                        if (CombatManager.Instance.TryPlayUnit(card, slot.lane, slot.cellIndex))
                        {
                            Deselect();
                            slot.Refresh();
                            if (CombatAnimator.Instance != null)
                                CombatAnimator.Instance.StartCoroutine(
                                    CombatAnimator.Instance.PlayCardPlayAnim(handPos, handSize, slot, cardInst));
                        }
                    }
                    break;

                case Mode.AwaitingAllyUnit:
                    if (!slot.HasPlayerUnit) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, slot.lane, slot.cellIndex))
                        Deselect();
                    break;

                case Mode.AwaitingEnemyUnit:
                    if (!slot.HasEnemyUnit) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, slot.lane, slot.cellIndex))
                        Deselect();
                    break;

                case Mode.AwaitingAoEConfirm:
                    if (!slot.HasEnemyUnit) return;   // must click any enemy slot to confirm
                    if (CombatManager.Instance.TryPlayAoESpell(card))
                        Deselect();
                    break;

                case Mode.AwaitingAllAllyUnits:
                    if (!slot.HasPlayerUnit) return;  // click any ally slot to confirm
                    if (CombatManager.Instance.TryPlayAoESpell(card))  // AllAllyUnits spells routed via TryPlayAoESpell
                        Deselect();
                    break;
            }
        }

        // ── Hero click (from HeroPortraitUI) ──────────────────────────────────

        public void OnHeroClicked(bool isPlayerPortrait)
        {
            if (selectedView == null || mode != Mode.AwaitingHero) return;
            var card = selectedView.CardInstance;
            if (card == null) return;

            bool wantsPlayer = card.data.spellTarget == SpellTarget.PlayerHero;
            if (isPlayerPortrait != wantsPlayer) return;

            if (CombatManager.Instance.TryPlaySpellOnHero(card, isPlayerPortrait))
                Deselect();
        }

        // ── Highlights ────────────────────────────────────────────────────────

        private void RefreshHighlights()
        {
            ClearHighlights();
            var allSlots  = FindObjectsOfType<LaneSlotUI>(true);
            var allHeroes = FindObjectsOfType<HeroPortraitUI>(true);

            switch (mode)
            {
                case Mode.AwaitingCell:
                    foreach (var s in allSlots)
                        if (s.IsPlayerDeployZone && !s.IsOccupied)
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case Mode.AwaitingAllyUnit:
                    foreach (var s in allSlots)
                        if (s.HasPlayerUnit)
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case Mode.AwaitingEnemyUnit:
                    foreach (var s in allSlots)
                        if (s.HasEnemyUnit)
                            s.SetHighlight(true, HighlightEnemy);
                    break;

                case Mode.AwaitingHero:
                    if (selectedView?.CardInstance == null) break;
                    bool wantsPlayer = selectedView.CardInstance.data.spellTarget == SpellTarget.PlayerHero;
                    foreach (var h in allHeroes)
                        if (h.isPlayerPortrait == wantsPlayer) h.SetHighlight(true);
                    break;

                case Mode.AwaitingAoEConfirm:
                    foreach (var s in allSlots)
                        if (s.HasEnemyUnit)
                            s.SetHighlight(true, HighlightEnemy);
                    break;

                case Mode.AwaitingAllAllyUnits:
                    foreach (var s in allSlots)
                        if (s.HasPlayerUnit)
                            s.SetHighlight(true, HighlightSlot);
                    break;

                case Mode.AwaitingBricolage:
                    foreach (var s in allSlots)
                        if (s.IsPlayerDeployZone && !s.IsOccupied)
                            s.SetHighlight(true, HighlightSlot);
                    break;
            }
        }

        private void ClearHighlights()
        {
            foreach (var s in FindObjectsOfType<LaneSlotUI>(true))
                s.SetHighlight(false, Color.clear);
            foreach (var h in FindObjectsOfType<HeroPortraitUI>(true))
                h.SetHighlight(false);
        }

        private void ApplyCardHighlight(bool on)
        {
            if (selectedView == null) return;
            var outline = selectedView.GetComponent<Outline>();
            if (outline == null && on) outline = selectedView.gameObject.AddComponent<Outline>();
            if (outline == null) return;
            outline.effectColor    = HighlightCard;
            outline.effectDistance = new Vector2(4, -4);
            outline.enabled        = on;
        }
    }
}
