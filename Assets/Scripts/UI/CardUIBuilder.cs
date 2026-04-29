using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Utilitaire statique : applique le système 4 couches (fond template → illustration →
    /// devant template → textes) sur un GameObject existant disposant d'un RectTransform.
    /// Utilisé par les écrans de sélection de cartes (récompenses, forge, shop).
    /// </summary>
    public static class CardUIBuilder
    {
        private static readonly Color ColAtk = new Color(1.00f, 0.82f, 0.22f);
        private static readonly Color ColHP  = new Color(1.00f, 0.40f, 0.40f);

        /// <summary>
        /// Applique le visuel 4 couches à <paramref name="cardGO"/>.
        /// L'Image existante sur cardGO est reconfigurée comme fond (son raycastTarget
        /// n'est pas touché afin de conserver la fonctionnalité des Button).
        /// </summary>
        public static void ApplyTemplate(CardData card, GameObject cardGO)
        {
            bool isUnit = card.cardType == CardType.Unit;
            var  cfg    = Resources.Load<CardTemplateConfig>("CardTemplateConfig");

            // ── Couche 1 : fond (sur le root) ──────────────────────────────────
            var bgImg = cardGO.GetComponent<Image>();
            if (bgImg == null) bgImg = cardGO.AddComponent<Image>();
            bgImg.sprite = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImg.color  = Color.white;
            // raycastTarget non modifié : reste true pour les Button présents sur ce GO

            // ── Couche 2 : illustration ─────────────────────────────────────────
            var illuGO  = MakeChild("Illustration", cardGO);
            SetAnchors(illuGO, 0f, 0f, 1f, 1f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.sprite        = card.artwork;
            illuImg.color         = card.artwork != null ? Color.white : Color.clear;
            illuImg.raycastTarget = false;

            // ── Couche 3 : devant ───────────────────────────────────────────────
            var frontGO  = MakeChild("Front", cardGO);
            SetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

            // ── Couche 4 : textes ───────────────────────────────────────────────
            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(cardGO.transform, false);
            SetAnchors(textsGO, 0f, 0f, 1f, 1f);

            var nameTMP = MakeTMP("Name", textsGO,
                0.05f, 0.84f, 0.95f, 0.97f, 9f, FontStyles.Bold, TextAlignmentOptions.Center);
            nameTMP.enableWordWrapping = true;
            nameTMP.text = card.cardName;

            if (isUnit)
            {
                var statsTMP = MakeTMP("Stats", textsGO,
                    0.03f, 0.04f, 0.97f, 0.22f, 10f, FontStyles.Bold, TextAlignmentOptions.Center);
                statsTMP.richText = true;
                string kwLabel = card.keyword != UnitKeyword.Aucun ? $"  [{card.keyword}]" : "";
                statsTMP.text = $"CD {card.countdown}{kwLabel}";
            }
            else
            {
                var manaTMP = MakeTMP("Mana", textsGO,
                    0.03f, 0.20f, 0.97f, 0.34f, 9f, FontStyles.Bold, TextAlignmentOptions.Center);
                manaTMP.text = $"💎 {card.manaCost} mana";

                if (!string.IsNullOrEmpty(card.description))
                {
                    var descTMP = MakeTMP("Desc", textsGO,
                        0.04f, 0.04f, 0.96f, 0.19f, 6f, FontStyles.Normal, TextAlignmentOptions.Center);
                    descTMP.enableWordWrapping = true;
                    descTMP.overflowMode       = TextOverflowModes.Ellipsis;
                    descTMP.text               = card.description;
                }
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
