using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Panneau de zoom plein écran. Clic droit sur une carte → agrandie.
    /// Clic n'importe où → ferme.
    /// Instancie le CardPrefab et le scale pour un rendu identique à la main/plateau.
    /// </summary>
    public class CardZoomPanel : MonoBehaviour, IPointerClickHandler
    {
        public static CardZoomPanel Instance { get; private set; }

        private const float ZoomScale = 3f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            BuildOverlay();
            gameObject.SetActive(false);
        }

        // ── API publique ──────────────────────────────────────────────────────

        public void Show(CardInstance card)
        {
            if (card == null) return;

            foreach (Transform child in transform)
                Destroy(child.gameObject);
            BuildCardContainer(card);

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Hide() => gameObject.SetActive(false);

        public void OnPointerClick(PointerEventData _) => Hide();

        // ── Construction ─────────────────────────────────────────────────────

        private void BuildOverlay()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.80f);
        }

        private void BuildCardContainer(CardInstance card)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Cards/CardPrefab");
            if (prefab == null)
            {
                Debug.LogError("[CardZoomPanel] CardPrefab introuvable dans Resources/Prefabs/Cards/");
                return;
            }

            var cardGO = Instantiate(prefab, transform);
            var rt = cardGO.GetComponent<RectTransform>();
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale       = new Vector3(ZoomScale, ZoomScale, 1f);

            var view = cardGO.GetComponent<CardView>();
            if (view == null) return;
            view.Setup(card);
            view.SetZoomMode(true);
        }
    }
}
