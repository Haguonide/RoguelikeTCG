using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class HandView : MonoBehaviour
    {
        private List<CardView> cardViews = new();
        private GameObject _bricolageGO;

        // ── API publique ──────────────────────────────────────────────────────

        public void RefreshHand(List<CardInstance> hand,
            CardData bricolageCardData = null,
            int gearCount = 0, int gearATK = 0, int gearHP = 0)
        {
            foreach (var cv in cardViews)
                if (cv != null) Destroy(cv.gameObject);
            cardViews.Clear();

            if (_bricolageGO != null) { Destroy(_bricolageGO); _bricolageGO = null; }

            bool hasBricolage  = bricolageCardData != null;
            int  regularCount  = hand?.Count ?? 0;
            int  totalCount    = regularCount + (hasBricolage ? 1 : 0);
            if (totalCount == 0) return;

            float cardW = 160f, cardH = 240f, spacing = 10f;
            float totalW = totalCount * cardW + (totalCount - 1) * spacing;
            float startX = -totalW / 2f + cardW / 2f;

            for (int i = 0; i < regularCount; i++)
            {
                var go = BuildCard(hand[i], i);
                go.transform.SetParent(transform, false);
                PlaceCard(go, i, startX, cardW, cardH, spacing);
                cardViews.Add(go.GetComponent<CardView>());
            }

            if (hasBricolage)
            {
                _bricolageGO = BuildBricolageCard(bricolageCardData, gearCount, gearATK, gearHP);
                _bricolageGO.transform.SetParent(transform, false);
                PlaceCard(_bricolageGO, regularCount, startX, cardW, cardH, spacing);
            }
        }

        // ── Construction ─────────────────────────────────────────────────────

        private static void PlaceCard(GameObject go, int index, float startX,
            float cardW, float cardH, float spacing)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(cardW, cardH);
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(startX + index * (cardW + spacing), 0);
        }

        private GameObject BuildCard(CardInstance card, int index)
        {
            bool isUnit = card.IsUnit;
            var cfg = Resources.Load<CardTemplateConfig>("CardTemplateConfig");

            var go = new GameObject("HandCard_" + index, typeof(RectTransform));
            var bgImage = go.AddComponent<Image>();
            bgImage.sprite = cfg != null ? (isUnit ? cfg.unitBackground : cfg.spellBackground) : null;
            bgImage.color  = Color.white;

            var illuGO  = MakeChild("Illustration", go);
            SetAnchors(illuGO, 0f, 0f, 1f, 1f);
            var illuImg = illuGO.GetComponent<Image>();
            illuImg.color         = Color.white;
            illuImg.raycastTarget = false;

            var frontGO  = MakeChild("Front", go);
            SetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg != null ? (isUnit ? cfg.unitFront : cfg.spellFront) : null;
            frontImg.color         = Color.white;
            frontImg.raycastTarget = false;

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

        private GameObject BuildBricolageCard(CardData data, int gearCount, int gearATK, int gearHP)
        {
            bool active = gearCount >= 3;
            var  cfg    = Resources.Load<CardTemplateConfig>("CardTemplateConfig");
            var  tint   = active ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.75f);

            var go = new GameObject("BricolageCard", typeof(RectTransform));
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = cfg?.unitBackground;
            bgImg.color  = tint;

            if (data?.artwork != null)
            {
                var illuGO  = MakeChild("Illustration", go);
                SetAnchors(illuGO, 0f, 0f, 1f, 1f);
                var illuImg = illuGO.GetComponent<Image>();
                illuImg.sprite        = data.artwork;
                illuImg.color         = tint;
                illuImg.raycastTarget = false;
            }

            var frontGO  = MakeChild("Front", go);
            SetAnchors(frontGO, 0f, 0f, 1f, 1f);
            var frontImg = frontGO.GetComponent<Image>();
            frontImg.sprite        = cfg?.unitFront;
            frontImg.color         = tint;
            frontImg.raycastTarget = false;

            var textsGO = new GameObject("Texts", typeof(RectTransform));
            textsGO.transform.SetParent(go.transform, false);
            SetAnchors(textsGO, 0f, 0f, 1f, 1f);

            var nameTMP = MakeTMP("Name", textsGO,
                0.05f, 0.84f, 0.95f, 0.97f, 8f, FontStyles.Bold, TextAlignmentOptions.Center);
            nameTMP.text              = data?.cardName ?? "Bricolage";
            nameTMP.color             = active ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            nameTMP.enableWordWrapping = true;
            nameTMP.raycastTarget      = false;

            var gearTMP = MakeTMP("Gears", textsGO,
                0.03f, 0.60f, 0.97f, 0.82f, 11f, FontStyles.Bold, TextAlignmentOptions.Center);
            gearTMP.text  = active ? "⚙ PRÊT !" : $"⚙ {gearCount}/3";
            gearTMP.color = active ? new Color(1f, 0.85f, 0.1f) : new Color(0.65f, 0.65f, 0.65f);
            gearTMP.raycastTarget = false;

            int cappedATK = Mathf.Min(gearATK, 5);
            int cappedHP  = Mathf.Min(gearHP,  8);

            var statsTMP = MakeTMP("Stats", textsGO,
                0.03f, 0.04f, 0.97f, 0.22f, 10f, FontStyles.Bold, TextAlignmentOptions.Center);
            statsTMP.text             = active ? $"{cappedATK}/{cappedHP}" : "?/?";
            statsTMP.color            = active ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            statsTMP.enableWordWrapping = false;
            statsTMP.richText           = true;
            statsTMP.raycastTarget      = false;

            if (!string.IsNullOrEmpty(data?.description))
            {
                var descTMP = MakeTMP("Desc", textsGO,
                    0.04f, 0.22f, 0.96f, 0.59f, 5.5f, FontStyles.Normal, TextAlignmentOptions.Center);
                descTMP.text              = data.description;
                descTMP.enableWordWrapping = true;
                descTMP.overflowMode       = TextOverflowModes.Ellipsis;
                descTMP.color              = active ? new Color(0.88f, 0.88f, 0.88f) : new Color(0.55f, 0.55f, 0.55f);
                descTMP.raycastTarget      = false;
            }

            // Button — only functional when 3 gears accumulated
            var btn = go.AddComponent<Button>();
            btn.interactable = active;
            if (active)
                btn.onClick.AddListener(() => CardSelector.Instance?.SelectBricolage());

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
