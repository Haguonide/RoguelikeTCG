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
        public TextMeshProUGUI manaCostText;
        public TextMeshProUGUI cdText;
        public Image artwork;
        public Image rarityBorder;

        [Header("HP Hearts (3 slots, index 0-2)")]
        public Image[] heartImages;

        [Header("Legacy — unused on CardPrefab, kept pour compatibilité")]
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI statsText;
        public TextMeshProUGUI keywordText;

        [Header("Zoom")]
        public GameObject zoomPanel;

        private CardInstance cardInstance;
        private bool isZoomed;

        private RectTransform _rt;
        private Coroutine     _hoverCo;
        private Vector2       _basePos;
        private float         _baseRot;
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

            if (artwork && data.artwork) artwork.sprite = data.artwork;
            if (manaCostText) manaCostText.text = $"{data.manaCost}";
            if (cdText) cdText.text = $"{cardInstance.currentCountdown}";

            if (heartImages != null)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] == null) continue;
                    bool withinMax = data.cardType == CardType.Unit && i < data.hp;
                    heartImages[i].gameObject.SetActive(withinMax);
                    if (withinMax)
                        heartImages[i].color = i < cardInstance.currentHP
                            ? Color.white
                            : new Color(1f, 1f, 1f, 0.2f);
                }
            }

            // Legacy fields — null-safe, ignorés si non assignés dans le prefab
            if (cardNameText)    cardNameText.text    = data.cardName;
            if (descriptionText) descriptionText.text = data.description;
            if (statsText)       statsText.text       = data.cardType == CardType.Unit
                ? $"HP {cardInstance.currentHP}/{data.hp}  CD {cardInstance.currentCountdown}"
                : "";
            if (keywordText)     keywordText.text     = (data.cardType == CardType.Unit && data.keyword != UnitKeyword.Aucun)
                ? data.keyword.ToString()
                : "";
        }

        // Called by HandView after placing the card in the arc layout.
        public void SetHandBase(Vector2 pos, float zRot)
        {
            _basePos      = pos;
            _baseRot      = zRot;
            _baseCaptured = true;
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
            if (!_baseCaptured)
            {
                _basePos = _rt.anchoredPosition;
                _baseRot = NormalizeAngle(_rt.localEulerAngles.z);
                _baseCaptured = true;
            }
            if (_hoverCo != null) StopCoroutine(_hoverCo);
            transform.SetAsLastSibling();
            _hoverCo = StartCoroutine(HoverLerp(_basePos + new Vector2(0f, 50f), 0f, 1.1f));
            AudioManager.Instance.PlaySFX("sfx_card_hover");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_rt == null || !_baseCaptured) return;
            if (_hoverCo != null) StopCoroutine(_hoverCo);
            _hoverCo = StartCoroutine(HoverLerp(_basePos, _baseRot, 1f));
        }

        private IEnumerator HoverLerp(Vector2 targetPos, float targetRot, float targetScale)
        {
            Vector2 fromPos   = _rt.anchoredPosition;
            float   fromRot   = NormalizeAngle(_rt.localEulerAngles.z);
            float   fromScale = _rt.localScale.x;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / 0.12f);
                float e = 1f - (1f - t) * (1f - t);
                _rt.anchoredPosition = Vector2.Lerp(fromPos, targetPos, e);
                float s = Mathf.Lerp(fromScale, targetScale, e);
                _rt.localScale = new Vector3(s, s, 1f);
                _rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(fromRot, targetRot, e));
                yield return null;
            }
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}
