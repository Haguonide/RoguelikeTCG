using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Represents ONE cell (out of 6) in a CombatLane.
    /// cellIndex 0-2 = player deployment zone (left side).
    /// cellIndex 3-5 = enemy deployment zone (right side).
    /// </summary>
    public class LaneSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Logic")]
        public CombatLane lane;
        public int        cellIndex;

        [Header("Slot Visual")]
        public Image            background;
        public Sprite           slotSprite;
        public TextMeshProUGUI  emptyHintText;

        // ── Derived state ─────────────────────────────────────────────────────

        public bool         IsPlayerDeployZone => cellIndex == CombatLane.PLAYER_DEPLOY_CELL;
        public bool         IsOccupied         => lane != null && lane.IsOccupied(cellIndex);
        public CardInstance Occupant           => lane?.GetUnit(cellIndex);
        public bool         HasPlayerUnit      => Occupant?.isPlayerCard == true;
        public bool         HasEnemyUnit       => Occupant != null && !Occupant.isPlayerCard;

        // ── Children ──────────────────────────────────────────────────────────

        public PlayedCardUI PlayedCard => _playedCard;
        private PlayedCardUI _playedCard;

        private Image _highlightOverlay;

        private static readonly Color TintEmpty = new Color(1f, 1f, 1f, 0.45f);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            var go = new GameObject("HighlightOverlay", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
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
                CardSelector.Instance?.OnCellClicked(this);
            else if (eventData.button == PointerEventData.InputButton.Right)
                if (IsOccupied)
                    RoguelikeTCG.UI.CardZoomPanel.Instance?.Show(Occupant);
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

            if (background != null)
            {
                if (slotSprite != null && background.sprite == null) background.sprite = slotSprite;
                background.color = TintEmpty;
            }

            if (IsOccupied)
            {
                SetHighlight(false, Color.clear);

                if (_playedCard == null)
                    _playedCard = PlayedCardUI.CreateProgrammatic(transform, Occupant, HasPlayerUnit);
                else
                    _playedCard.Refresh(Occupant);

                if (emptyHintText) emptyHintText.gameObject.SetActive(false);
            }
            else
            {
                if (_playedCard != null) { Destroy(_playedCard.gameObject); _playedCard = null; }
                if (emptyHintText)
                {
                    emptyHintText.text = IsPlayerDeployZone ? "Poser\nune unité" : "";
                    emptyHintText.gameObject.SetActive(IsPlayerDeployZone);
                }
            }
        }
    }
}
