using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Représentation visuelle d'une case de la grille 3x3.
    /// Forwarde les clics au CardSelector.
    /// Affiche l'unité présente, son CD, et les flèches d'attaque.
    /// </summary>
    public class GridCellUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
    {
        [Header("Position dans la grille")]
        public int row;
        public int col;

        [Header("Références UI")]
        [SerializeField] private Image     bgImage;
        [SerializeField] private Image     cardImage;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI statsText;      // "ATK/HP"
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private TextMeshProUGUI keywordText;
        [SerializeField] private GameObject arrowUp;
        [SerializeField] private GameObject arrowDown;
        [SerializeField] private GameObject arrowLeft;
        [SerializeField] private GameObject arrowRight;
        [SerializeField] private Image      highlightImage;

        // Couleurs de fond par état
        private static readonly Color ColorEmpty       = Color.clear;
        private static readonly Color ColorPlayerUnit  = new Color(0.10f, 0.30f, 0.12f, 0.95f);
        private static readonly Color ColorEnemyUnit   = new Color(0.30f, 0.10f, 0.10f, 0.95f);

        // Unité actuellement affichée (peut être null)
        private CardInstance _currentUnit;

        // ── Refresh ───────────────────────────────────────────────────────────

        public void Refresh(CardInstance unit)
        {
            _currentUnit = unit;

            if (unit == null)
            {
                SetBackground(ColorEmpty);
                SetCardVisuals(false, null, "", "", 0, AttackDirection.None);
                return;
            }

            Color bg = unit.isPlayerCard ? ColorPlayerUnit : ColorEnemyUnit;
            SetBackground(bg);

            string stats = $"HP {unit.currentHP}/{unit.data.hp}  CD {unit.currentCountdown}";
            string kw    = unit.data.keyword != UnitKeyword.Aucun
                         ? unit.data.keyword.ToString()
                         : "";
            SetCardVisuals(true, unit.data.artwork, unit.data.cardName, stats,
                           unit.currentCountdown, unit.data.attackDirections, kw);
        }

        // ── Highlight ─────────────────────────────────────────────────────────

        public void SetHighlight(bool active, Color color)
        {
            if (highlightImage == null) return;
            highlightImage.enabled = active;
            highlightImage.color   = active ? color : Color.clear;
        }

        public void StartShake()
        {
            // Hook pour CombatAnimator si besoin — laissé vide pour l'instant
        }

        public void StopShake()
        {
            // Hook pour CombatAnimator
        }

        // ── Propriétés publiques ──────────────────────────────────────────────

        public bool IsOccupied      => _currentUnit != null;
        public bool HasPlayerUnit   => _currentUnit != null && _currentUnit.isPlayerCard;
        public bool HasEnemyUnit    => _currentUnit != null && !_currentUnit.isPlayerCard;
        public CardInstance Unit    => _currentUnit;

        // ── Pointer events ────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Zoom sur la carte — délégué au CardZoomPanel si disponible
                if (_currentUnit != null)
                    CardZoomPanel.Instance?.Show(_currentUnit);
                return;
            }

            // Clic gauche : forwarde au CardSelector
            CardSelector.Instance?.OnGridCellClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CardSelector.Instance?.OnGridCellHoverEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CardSelector.Instance?.OnGridCellHoverExit(this);
        }

        // ── Helpers visuels ───────────────────────────────────────────────────

        private void SetBackground(Color color)
        {
            if (bgImage == null) return;
            bgImage.enabled = color.a > 0f;
            bgImage.color = color;
        }

        // ── Drop target (drag & drop depuis la main) ──────────────────────────

        public void OnDrop(PointerEventData eventData)
        {
            var cv = eventData.pointerDrag?.GetComponent<CardView>();
            if (cv == null || !cv.IsDragging) return;

            var card = cv.CardInstance;
            if (card == null || !card.IsUnit) return;
            if (IsOccupied) return;

            if (CombatManager.Instance.TryPlayUnit(card, row, col))
            {
                cv.NotifyDragSuccess();
                Refresh(CombatManager.Instance.gridManager.GetUnit(row, col));
            }
        }

        private void SetCardVisuals(bool visible, Sprite artwork,
            string unitName, string stats, int cd,
            AttackDirection dirs, string keyword = "")
        {
            if (cardImage != null)
            {
                cardImage.enabled = visible && artwork != null;
                if (artwork != null) cardImage.sprite = artwork;
            }
            if (unitNameText  != null) { unitNameText.text  = unitName;  unitNameText.gameObject.SetActive(visible); }
            if (statsText     != null) { statsText.text     = stats;     statsText.gameObject.SetActive(visible); }
            if (countdownText != null) { countdownText.text = visible ? $"CD:{cd}" : ""; }
            if (keywordText   != null) { keywordText.text   = keyword;   keywordText.gameObject.SetActive(visible && keyword.Length > 0); }

            bool showArrows = visible;
            if (arrowUp    != null) arrowUp.SetActive(showArrows    && (dirs & AttackDirection.Up)    != 0);
            if (arrowDown  != null) arrowDown.SetActive(showArrows  && (dirs & AttackDirection.Down)  != 0);
            if (arrowLeft  != null) arrowLeft.SetActive(showArrows  && (dirs & AttackDirection.Left)  != 0);
            if (arrowRight != null) arrowRight.SetActive(showArrows && (dirs & AttackDirection.Right) != 0);
        }
    }
}
