using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Represents a single lane slot on the board.
    /// The slot is ONLY a position anchor with a neutral empty-state visual.
    /// When a unit is placed, a PlayedCardUI is spawned as an independent child;
    /// all card visuals (colour, name, stats, animations) live there.
    /// </summary>
    public class LaneSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Logic")]
        public Lane lane;
        public bool isPlayerLane = true;

        [Header("Slot Visual (empty state)")]
        public Image  background;   // always shows the slot sprite at neutral tint
        public Sprite slotSprite;
        public TextMeshProUGUI emptyHintText;

        // ── Played card ───────────────────────────────────────────────────────

        /// <summary>The card currently displayed on this slot, or null if empty.</summary>
        public PlayedCardUI PlayedCard => _playedCard;
        private PlayedCardUI _playedCard;

        // ── Highlight overlay ─────────────────────────────────────────────────

        private Image _highlightOverlay;

        private static readonly Color TintEmpty = new Color(1f, 1f, 1f, 0.55f);

        public Lane Lane => lane;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            var go = new GameObject("HighlightOverlay", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            _highlightOverlay = go.AddComponent<Image>();
            _highlightOverlay.color = Color.clear;
            _highlightOverlay.raycastTarget = false;
        }

        private void Start()    => Refresh();
        private void OnEnable() => Refresh();

        // ── Click ─────────────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                CardSelector.Instance?.OnLaneClicked(this);
            else if (eventData.button == PointerEventData.InputButton.Right)
                if (lane != null && lane.IsOccupied)
                    RoguelikeTCG.UI.CardZoomPanel.Instance?.Show(lane.Occupant);
        }

        // ── Highlight ─────────────────────────────────────────────────────────

        public void SetHighlight(bool on, Color color)
        {
            if (_highlightOverlay == null) return;
            _highlightOverlay.color = on ? color : Color.clear;
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        public void Refresh()
        {
            if (lane == null) return;

            // Slot background — always neutral (position marker only)
            if (background != null)
            {
                if (slotSprite != null && background.sprite == null)
                    background.sprite = slotSprite;
                background.color = TintEmpty;
            }

            if (lane.IsOccupied)
            {
                // Spawn the card if not yet created
                if (_playedCard == null)
                    _playedCard = PlayedCardUI.CreateProgrammatic(transform, lane.Occupant, isPlayerLane);
                else
                    _playedCard.Refresh(lane.Occupant);

                if (emptyHintText) emptyHintText.gameObject.SetActive(false);
            }
            else
            {
                // Destroy the card object if the lane was just cleared
                if (_playedCard != null)
                {
                    Destroy(_playedCard.gameObject);
                    _playedCard = null;
                }

                if (emptyHintText)
                {
                    emptyHintText.text = isPlayerLane ? "Poser\nune unité" : "";
                    emptyHintText.gameObject.SetActive(isPlayerLane);
                }
            }
        }
    }
}
