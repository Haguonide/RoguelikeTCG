using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
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

        [Header("Zones (CardPrefab)")]
        public TextMeshProUGUI hpText;    // HPZone/HPText — "2/3"
        public GameObject      manaZone;  // caché quand la carte est posée sur la grille
        public GameObject      cdZone;    // caché pour les sorts/utilitaires
        public GameObject      hpZone;    // caché pour les sorts/utilitaires

        [Header("Legacy — unused on CardPrefab, kept pour compatibilité")]
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI statsText;
        public TextMeshProUGUI keywordText;

        [Header("Flèches d'attaque (auto-trouvées si null)")]
        public GameObject arrowUp;
        public GameObject arrowDown;
        public GameObject arrowLeft;
        public GameObject arrowRight;

        [Header("Zoom")]
        public GameObject zoomPanel;

        private CardInstance cardInstance;
        private bool isZoomed;
        private bool _onGrid; // true quand posée sur la grille — bloque hover/select
        private bool _arrowsFound;

        private RectTransform _rt;
        private Coroutine     _hoverCo;
        private Vector2       _basePos;
        private float         _baseRot;
        private bool          _baseCaptured;

        public CardInstance CardInstance => cardInstance;

        /// <summary>Appelé par GridCellUI quand la carte est posée sur la grille.</summary>
        public void SetOnGrid(bool onGrid)
        {
            _onGrid = onGrid;
            Refresh(); // manaZone se cache (IsOnGrid devient true)
        }

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            FindArrows();
        }

        private void FindArrows()
        {
            if (_arrowsFound) return;
            var arrowsRoot = transform.Find("Arrows");
            if (arrowsRoot == null) return;
            if (arrowUp    == null) arrowUp    = arrowsRoot.Find("ArrowUp")?.gameObject;
            if (arrowDown  == null) arrowDown  = arrowsRoot.Find("ArrowDown")?.gameObject;
            if (arrowLeft  == null) arrowLeft  = arrowsRoot.Find("ArrowLeft")?.gameObject;
            if (arrowRight == null) arrowRight = arrowsRoot.Find("ArrowRight")?.gameObject;
            _arrowsFound = true;
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
            bool isUnit = data.cardType == CardType.Unit;

            if (artwork)
            {
                if (data.artwork != null)
                    artwork.sprite = data.artwork;
                else
                {
                    var cfg = Resources.Load<CardTemplateConfig>("CardTemplateConfig");
                    if (cfg != null)
                        artwork.sprite = cardInstance.isPlayerCard ? cfg.playerFallbackArt : cfg.enemyFallbackArt;
                }
            }
            if (manaCostText) manaCostText.text = $"{data.manaCost}";

            // ManaZone : visible uniquement en main (pas encore posée sur la grille)
            if (manaZone) manaZone.SetActive(!cardInstance.IsOnGrid);

            // CDZone + CDText : uniquement pour les unités
            if (cdZone) cdZone.SetActive(false);
            if (cdText) cdText.text = "";

            // HPZone + HPText : "currentHP/maxHP" pour les unités
            if (hpZone) hpZone.SetActive(isUnit);
            if (hpText) hpText.text = isUnit ? $"{cardInstance.currentHP}/{data.hp}" : "";

            // Icônes cœurs (ancien affichage HP, coexiste avec hpText)
            if (heartImages != null)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] == null) continue;
                    bool withinMax = isUnit && i < data.hp;
                    heartImages[i].gameObject.SetActive(withinMax);
                    if (withinMax)
                        heartImages[i].color = i < cardInstance.currentHP
                            ? Color.white
                            : new Color(1f, 1f, 1f, 0.2f);
                }
            }

            // Flèches d'attaque : affichées uniquement en jeu, selon les directions réelles
            if (!_arrowsFound) FindArrows();
            bool onGrid = isUnit && cardInstance.IsOnGrid;
            var dirs = data.attackDirections;
            if (arrowUp)    arrowUp.SetActive(onGrid    && (dirs & AttackDirection.Up)    != 0);
            if (arrowDown)  arrowDown.SetActive(onGrid  && (dirs & AttackDirection.Down)  != 0);
            if (arrowLeft)  arrowLeft.SetActive(onGrid  && (dirs & AttackDirection.Left)  != 0);
            if (arrowRight) arrowRight.SetActive(onGrid && (dirs & AttackDirection.Right) != 0);

            // Legacy fields — null-safe, ignorés si non assignés dans le prefab
            if (cardNameText)    cardNameText.text    = data.cardName;
            if (descriptionText) descriptionText.text = data.description;
            if (statsText)       statsText.text       = isUnit
                ? $"HP {cardInstance.currentHP}/{data.hp}"
                : "";
            if (keywordText)     keywordText.text     = (isUnit && data.keyword != UnitKeyword.Aucun)
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

        public void CancelHover()
        {
            if (_hoverCo != null) { StopCoroutine(_hoverCo); _hoverCo = null; }
            if (_baseCaptured && _rt != null)
            {
                _rt.anchoredPosition = _basePos;
                _rt.localEulerAngles = new Vector3(0f, 0f, _baseRot);
                _rt.localScale       = Vector3.one;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_onGrid) return;
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
            if (_onGrid || _rt == null) return;
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
            if (_onGrid || _rt == null || !_baseCaptured) return;
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
