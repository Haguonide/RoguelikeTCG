using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RoguelikeTCG.Cards;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Case de la grille 3×3.
    /// Affiche l'unité via un CardPrefab instancié comme enfant.
    /// Forwarde les clics au CardSelector.
    /// </summary>
    public class GridCellUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Position dans la grille")]
        public int row;
        public int col;

        [Header("Highlight overlay (Image sur le GO racine ou enfant dédié)")]
        [SerializeField] private Image highlightImage;

        private static readonly Color ColorEmpty      = Color.clear;
        private static readonly Color ColorHighlight  = new Color(0.2f, 1f, 0.35f, 0.7f);

        private CardInstance _currentUnit;
        private CardView     _placedCard;

        private void Awake() { }

        // ── Refresh ───────────────────────────────────────────────────────────

        public void Refresh(CardInstance unit)
        {
            _currentUnit = unit;

            if (unit == null)
            {
                if (_placedCard != null) { Destroy(_placedCard.gameObject); _placedCard = null; }
                return;
            }

            // Mise à jour d'une carte déjà présente (HP, CD…)
            if (_placedCard != null)
            {
                _placedCard.Refresh();
                return;
            }

            // Spawn instantané (carte ennemie, ou joueur sans animation)
            SpawnCardPrefab(unit);
        }

        /// <summary>
        /// Adopte un CardView qui arrive depuis la main (animation vol).
        /// Le reparente à cette case et le met en mode "posé sur grille".
        /// </summary>
        public void SetPlacedCard(CardView cv)
        {
            if (_placedCard != null && _placedCard != cv)
                Destroy(_placedCard.gameObject);

            _placedCard = cv;
            cv.transform.SetParent(transform, false);
            var rt = cv.GetComponent<RectTransform>();
            rt.anchorMin    = Vector2.zero;
            rt.anchorMax    = Vector2.one;
            rt.offsetMin    = rt.offsetMax = Vector2.zero;
            rt.localScale   = Vector3.one;
            rt.localPosition = Vector3.zero;
            cv.SetOnGrid(true);
        }

        // ── Highlight ─────────────────────────────────────────────────────────

        public void SetHighlight(bool active, Color color) { }

        public void StartShake() { }
        public void StopShake()  { }

        // ── Propriétés ────────────────────────────────────────────────────────

        public bool IsOccupied    => _currentUnit != null;
        public bool HasPlayerUnit => _currentUnit != null &&  _currentUnit.isPlayerCard;
        public bool HasEnemyUnit  => _currentUnit != null && !_currentUnit.isPlayerCard;
        public CardInstance Unit  => _currentUnit;

        // ── Pointer events ────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (_currentUnit != null) CardZoomPanel.Instance?.Show(_currentUnit);
                return;
            }
            CardSelector.Instance?.OnGridCellClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData) => CardSelector.Instance?.OnGridCellHoverEnter(this);
        public void OnPointerExit(PointerEventData eventData)  => CardSelector.Instance?.OnGridCellHoverExit(this);

        // ── Spawn ─────────────────────────────────────────────────────────────

        private void SpawnCardPrefab(CardInstance unit)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Cards/CardPrefab");
            if (prefab == null)
            {
                Debug.LogWarning("[GridCellUI] CardPrefab introuvable dans Resources/Prefabs/Cards/");
                return;
            }

            var go = Instantiate(prefab, transform);
            go.name = "PlacedCard";
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin    = Vector2.zero;
            rt.anchorMax    = Vector2.one;
            rt.offsetMin    = rt.offsetMax = Vector2.zero;

            _placedCard = go.GetComponent<CardView>();
            if (_placedCard != null)
            {
                _placedCard.Setup(unit);
                _placedCard.SetOnGrid(true);
            }
        }
    }
}
