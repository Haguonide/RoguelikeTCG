using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    public class LaneSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
        private PlayedCardUI  _playedCard;
        private Outline       _outline;
        private RectTransform _slotVisualRT;
        private Tweener       _shakeTween;
        private bool          _isHighlighted;

        private static readonly Color TintEmpty = Color.clear;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            var sv = transform.Find("SlotVisual");
            if (sv != null)
            {
                _slotVisualRT = sv.GetComponent<RectTransform>();
                _outline = sv.GetComponent<Outline>() ?? sv.gameObject.AddComponent<Outline>();
                _outline.effectColor    = Color.clear;
                _outline.effectDistance = new Vector2(8, -8);
                _outline.useGraphicAlpha = false;
                _outline.enabled        = false;
            }
            else
            {
                Debug.LogWarning($"[LaneSlotUI] SlotVisual child not found on {gameObject.name}", this);
            }
        }

        private void Start()    => Refresh();
        private void OnEnable() => Refresh();

        private void OnDestroy() => StopShake();

        // ── Click / Hover ──────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                CardSelector.Instance?.OnCellClicked(this);
            else if (eventData.button == PointerEventData.InputButton.Right)
                if (IsOccupied)
                    RoguelikeTCG.UI.CardZoomPanel.Instance?.Show(Occupant);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isHighlighted)
                CardSelector.Instance?.OnSlotHoverEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHighlighted)
                CardSelector.Instance?.OnSlotHoverExit(this);
        }

        // ── Highlight ─────────────────────────────────────────────────────────

        public void SetHighlight(bool on, Color color)
        {
            _isHighlighted = on;
            if (_outline == null) return;
            _outline.effectColor = color;
            _outline.enabled     = on;
            if (!on) StopShake();
        }

        // ── Shake ─────────────────────────────────────────────────────────────

        public void StartShake()
        {
            if (_slotVisualRT == null) return;
            StopShake();
            _shakeTween = _slotVisualRT
                .DOShakeAnchorPos(0.45f, new Vector2(2.5f, 2f), 18, 90f, false, true)
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopShake()
        {
            if (_shakeTween == null || !_shakeTween.IsActive()) return;
            _shakeTween.Kill(true);
            _shakeTween = null;
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
                if (emptyHintText) emptyHintText.gameObject.SetActive(false);
            }
        }
    }
}
