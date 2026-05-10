using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Panneau de zoom plein écran. Clic droit sur une carte → agrandie.
    /// Clic n'importe où → ferme.
    /// 4 couches : fond (template) → illustration → devant (template) → textes.
    /// </summary>
    public class CardZoomPanel : MonoBehaviour, IPointerClickHandler
    {
        public static CardZoomPanel Instance { get; private set; }

        private static readonly Color ColAtk = new Color(1.00f, 0.82f, 0.22f);
        private static readonly Color ColHP  = new Color(1.00f, 0.40f, 0.40f);

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
            bool isUnit = card.IsUnit;
            var cfg = Resources.Load<CardTemplateConfig>("CardTemplateConfig");

            // Conteneur carte centré 320×480
            var cardGO = new GameObject("Card", typeof(RectTransform));
            cardGO.transform.SetParent(transform, false);
            var cardRT    = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot     = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(320f, 480f);

            // Couche 1 : fond
            var bgImg = cardGO.AddComponent<Image>();
            bgImg.sprite        = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImg.color         = Color.white;
            bgImg.raycastTarget = false;

            // Couche 2 : illustration
            var illuGO  = MakeChild("Illustration", cardGO);
            SetAnchors(illuGO, 0f, 0f, 1f, 1f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.sprite        = card.data.artwork;
            illuImg.color         = card.data.artwork != null ? Color.white : Color.clear;
            illuImg.raycastTarget = false;

            // Couche 3 : devant
            var frontGO  = MakeChild("Front", cardGO);
            SetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

            // Couche 4 : textes
            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(cardGO.transform, false);
            SetAnchors(textsGO, 0f, 0f, 1f, 1f);

            var nameTMP = MakeTMP("Name", textsGO,
                0.05f, 0.85f, 0.95f, 0.98f, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
            nameTMP.textWrappingMode = TextWrappingModes.Normal;
            nameTMP.text = card.data.cardName;

            if (isUnit)
            {
                var statsTMP = MakeTMP("Stats", textsGO,
                    0.03f, 0.28f, 0.97f, 0.44f, 28f, FontStyles.Bold, TextAlignmentOptions.Center);
                statsTMP.richText = true;
                string kwLabel = card.data.keyword != UnitKeyword.Aucun ? $"  [{card.data.keyword}]" : "";
                statsTMP.text = $"HP {card.currentHP}/{card.data.hp}{kwLabel}";
            }
            else
            {
                var manaTMP = MakeTMP("Mana", textsGO,
                    0.03f, 0.32f, 0.97f, 0.44f, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
                manaTMP.text = $"💎 {card.data.manaCost} mana";
            }

            if (!string.IsNullOrEmpty(card.data.description))
            {
                float yMin = isUnit ? 0.04f : 0.05f;
                float yMax = isUnit ? 0.27f : 0.31f;
                var descTMP = MakeTMP("Desc", textsGO,
                    0.05f, yMin, 0.95f, yMax, 14f, FontStyles.Normal, TextAlignmentOptions.Center);
                descTMP.textWrappingMode = TextWrappingModes.Normal;
                descTMP.overflowMode = TextOverflowModes.Ellipsis;
                descTMP.text = card.data.description;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static GameObject MakeChild(string name, GameObject parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
            return go;
        }

        private static TextMeshProUGUI MakeTMP(string name, GameObject parent,
            float xMin, float yMin, float xMax, float yMax,
            float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize      = fontSize;
            tmp.fontStyle     = style;
            tmp.alignment     = align;
            tmp.color         = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
