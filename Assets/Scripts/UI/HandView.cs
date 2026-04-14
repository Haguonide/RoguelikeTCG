using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche la main du joueur.
    /// 4 couches : fond (template) → illustration → devant (template) → textes.
    /// </summary>
    public class HandView : MonoBehaviour
    {
        private List<CardView> cardViews = new();

        // ── API publique ──────────────────────────────────────────────────────

        public void RefreshHand(List<CardInstance> hand)
        {
            foreach (var cv in cardViews)
                if (cv != null) Destroy(cv.gameObject);
            cardViews.Clear();

            if (hand == null || hand.Count == 0) return;

            float cardW = 160f, cardH = 240f, spacing = 10f;
            float totalW = hand.Count * cardW + (hand.Count - 1) * spacing;
            float startX = -totalW / 2f + cardW / 2f;

            for (int i = 0; i < hand.Count; i++)
            {
                var go = BuildCard(hand[i], i);
                go.transform.SetParent(transform, false);

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta        = new Vector2(cardW, cardH);
                rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot            = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(startX + i * (cardW + spacing), 0);

                cardViews.Add(go.GetComponent<CardView>());
            }
        }

        // ── Construction ─────────────────────────────────────────────────────

        private GameObject BuildCard(CardInstance card, int index)
        {
            bool isUnit = card.IsUnit;
            var cfg = Resources.Load<CardTemplateConfig>("CardTemplateConfig");

            // Racine — couche 1 : fond
            var go = new GameObject("HandCard_" + index, typeof(RectTransform));
            var bgImage = go.AddComponent<Image>();
            bgImage.sprite = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImage.color  = Color.white;

            // Couche 2 : illustration
            var illuGO  = MakeChild("Illustration", go);
            SetAnchors(illuGO, 0.03f, 0.38f, 0.97f, 0.80f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.color          = Color.white;
            illuImg.preserveAspect = true;
            illuImg.raycastTarget  = false;

            // Couche 3 : devant
            var frontGO  = MakeChild("Front", go);
            SetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

            // Couche 4 : textes
            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(go.transform, false);
            SetAnchors(textsGO, 0f, 0f, 1f, 1f);

            var nameTMP = MakeTMP("Name", textsGO,
                0.05f, 0.84f, 0.95f, 0.97f, 8f, FontStyles.Bold, TextAlignmentOptions.Center);
            nameTMP.enableWordWrapping = true;

            TextMeshProUGUI statsTMP = null;
            TextMeshProUGUI manaTMP  = null;
            TextMeshProUGUI descTMP  = null;

            if (isUnit)
            {
                statsTMP = MakeTMP("Stats", textsGO,
                    0.03f, 0.04f, 0.97f, 0.22f, 10f, FontStyles.Bold, TextAlignmentOptions.Center);
                statsTMP.enableWordWrapping = false;
                statsTMP.richText = true;
            }
            else
            {
                manaTMP = MakeTMP("Mana", textsGO,
                    0.03f, 0.20f, 0.97f, 0.34f, 9f, FontStyles.Bold, TextAlignmentOptions.Center);

                if (!string.IsNullOrEmpty(card.data.description))
                {
                    descTMP = MakeTMP("Desc", textsGO,
                        0.04f, 0.04f, 0.96f, 0.19f, 6f, FontStyles.Normal, TextAlignmentOptions.Center);
                    descTMP.enableWordWrapping = true;
                    descTMP.overflowMode = TextOverflowModes.Ellipsis;
                }
            }

            var cv = go.AddComponent<CardView>();
            cv.cardNameText    = nameTMP;
            cv.statsText       = statsTMP;
            cv.manaCostText    = manaTMP;
            cv.descriptionText = descTMP;
            cv.artwork         = illuImg;
            cv.Setup(card);
            return go;
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
