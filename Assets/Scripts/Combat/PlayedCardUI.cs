using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Représentation visuelle d'une unité posée sur un slot.
    /// 4 couches : fond (template) → illustration → devant (template) → textes.
    /// </summary>
    public class PlayedCardUI : MonoBehaviour
    {
        [HideInInspector] public RectTransform   animatedRoot;
        [HideInInspector] public Image           background;
        [HideInInspector] public TextMeshProUGUI cardNameText;
        [HideInInspector] public TextMeshProUGUI statsText;

        private static readonly Color ColAtk = new Color(1.00f, 0.82f, 0.22f);
        private static readonly Color ColHP  = new Color(1.00f, 0.40f, 0.40f);

        // ── Factory ───────────────────────────────────────────────────────────

        public static PlayedCardUI CreateProgrammatic(
            Transform slotTransform, CardInstance card, bool isPlayerCard)
        {
            var cfg     = Resources.Load<CardTemplateConfig>("CardTemplateConfig");
            bool isUnit = card.IsUnit;

            // Racine statique (remplit le slot)
            var rootGO = new GameObject("PlayedCard", typeof(RectTransform));
            rootGO.transform.SetParent(slotTransform, false);
            FillParent(rootGO.GetComponent<RectTransform>());

            // Racine animée (attaque / mort)
            var animGO = new GameObject("CardVisual", typeof(RectTransform));
            animGO.transform.SetParent(rootGO.transform, false);
            var animRT = animGO.GetComponent<RectTransform>();
            FillParent(animRT);

            // ── Couche 1 : fond ───────────────────────────────────────────────
            var bgImage = animGO.AddComponent<Image>();
            bgImage.sprite        = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImage.color         = Color.white;
            bgImage.raycastTarget = false;

            // ── Couche 2 : illustration ───────────────────────────────────────
            var illuGO  = MakeChild("Illustration", animGO);
            SetAnchors(illuGO, 0.03f, 0.38f, 0.97f, 0.80f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.sprite         = card.data.artwork;
            illuImg.color          = Color.white;
            illuImg.preserveAspect = true;
            illuImg.raycastTarget  = false;

            // ── Couche 3 : devant ─────────────────────────────────────────────
            var frontGO  = MakeChild("Front", animGO);
            FillParent(frontGO.GetComponent<RectTransform>());
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

            // ── Couche 4 : textes ─────────────────────────────────────────────
            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(animGO.transform, false);
            FillParent(textsGO.GetComponent<RectTransform>());

            var nameTMP  = MakeText("Name",  textsGO.transform,
                new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.97f), 8f, true);
            var statsTMP = MakeText("Stats", textsGO.transform,
                new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.20f), 9f, true);
            statsTMP.richText = true;

            var ui          = rootGO.AddComponent<PlayedCardUI>();
            ui.animatedRoot = animRT;
            ui.background   = bgImage;
            ui.cardNameText = nameTMP;
            ui.statsText    = statsTMP;
            ui.Refresh(card);
            return ui;
        }

        // ── Runtime ───────────────────────────────────────────────────────────

        public void Refresh(CardInstance card)
        {
            if (card == null) return;
            if (cardNameText) cardNameText.text = card.data.cardName;
            if (statsText)
            {
                string shieldStr = card.shieldHP > 0
                    ? $"  <color=#55AAFF>🛡 {card.shieldHP}</color>"
                    : "";
                statsText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ColAtk)}>⚔ {card.CurrentAttack}</color>" +
                                 $"  <color=#{ColorUtility.ToHtmlStringRGB(ColHP)}>❤ {card.currentHP}</color>" +
                                 shieldStr;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void FillParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static GameObject MakeChild(string name, GameObject parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
            return go;
        }

        private static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI MakeText(
            string goName, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            float fontSize, bool bold)
        {
            var go = new GameObject(goName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize           = fontSize;
            tmp.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.alignment          = TextAlignmentOptions.Center;
            tmp.color              = Color.white;
            tmp.raycastTarget      = false;
            tmp.enableWordWrapping = true;
            return tmp;
        }
    }
}
