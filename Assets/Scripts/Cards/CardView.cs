using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.Cards
{
    public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI statsText;
        public TextMeshProUGUI manaCostText;
        public Image artwork;
        public Image rarityBorder;

        [Header("Zoom")]
        public GameObject zoomPanel; // fullscreen zoomed view

        private CardInstance cardInstance;
        private bool isZoomed;

        // Hover lift state
        private RectTransform _rt;
        private Coroutine     _hoverCo;
        private Vector2       _basePos;
        private bool          _baseCaptured;

        public CardInstance CardInstance => cardInstance;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public void Setup(CardInstance instance)
        {
            cardInstance = instance;
            Refresh();
        }

        public void Refresh()
        {
            if (cardInstance == null) return;
            var data = cardInstance.data;

            if (cardNameText)  cardNameText.text  = data.cardName;
            if (descriptionText) descriptionText.text = data.description;
            if (artwork && data.artwork) artwork.sprite = data.artwork;
            if (manaCostText)  manaCostText.text  = data.cardType == CardType.Spell ? $"{data.manaCost}" : "";

            if (statsText)
            {
                statsText.text = data.cardType == CardType.Unit
                    ? $"{cardInstance.CurrentAttack}/{cardInstance.currentHP}"
                    : "";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                CardSelector.Instance?.SelectCard(this);
            else if (eventData.button == PointerEventData.InputButton.Right)
                ToggleZoom();
        }

        private void ToggleZoom()
        {
            var panel = RoguelikeTCG.UI.CardZoomPanel.Instance;
            if (panel == null) return;

            if (panel.gameObject.activeSelf)
                panel.Hide();
            else
                panel.Show(cardInstance);
        }

        // ── Hover lift ────────────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_rt == null) return;
            if (!_baseCaptured) { _basePos = _rt.anchoredPosition; _baseCaptured = true; }
            if (_hoverCo != null) StopCoroutine(_hoverCo);
            _hoverCo = StartCoroutine(HoverLerp(_basePos + new Vector2(0f, 18f), 1.06f));
            AudioManager.Instance.PlaySFX("sfx_card_hover");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_rt == null || !_baseCaptured) return;
            if (_hoverCo != null) StopCoroutine(_hoverCo);
            _hoverCo = StartCoroutine(HoverLerp(_basePos, 1f));
        }

        private IEnumerator HoverLerp(Vector2 targetPos, float targetScale)
        {
            Vector2 fromPos   = _rt.anchoredPosition;
            float   fromScale = _rt.localScale.x;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.12f);
                float e = 1f - (1f - t) * (1f - t);
                _rt.anchoredPosition = Vector2.Lerp(fromPos, targetPos, e);
                float s = Mathf.Lerp(fromScale, targetScale, e);
                _rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }
    }
}
