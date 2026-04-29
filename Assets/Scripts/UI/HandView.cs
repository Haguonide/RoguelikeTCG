using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class HandView : MonoBehaviour
    {
        [Header("Hand Fan")]
        [SerializeField] private float maxRotation = 12f;
        [SerializeField] private float arcDepth    = 20f;
        [SerializeField] private float sinkY       = -60f;
        [SerializeField] private float cardSpacing = -20f; // négatif = overlap entre cartes

        private List<CardView> cardViews = new();
        private GameObject _bricolageGO;

        // ── API publique ──────────────────────────────────────────────────────

        public IReadOnlyList<CardView> CardViews => cardViews;

        public void SlideExistingCards(int totalFinalCount)
        {
            if (cardViews.Count == 0) return;
            float cardW = 160f, spacing = cardSpacing;
            float totalW = totalFinalCount * cardW + (totalFinalCount - 1) * spacing;
            float startX = -totalW / 2f + cardW / 2f;

            for (int i = 0; i < cardViews.Count; i++)
            {
                var cv = cardViews[i];
                if (cv == null) continue;
                var (pos, rot) = ArcTransform(i, totalFinalCount, startX, cardW, spacing);
                cv.GetComponent<RectTransform>()
                    .DOAnchorPos(pos, 0.18f).SetEase(Ease.OutCubic);
                cv.transform.DOLocalRotate(new Vector3(0f, 0f, rot), 0.18f).SetEase(Ease.OutCubic);
                cv.SetHandBase(pos, rot);
            }
        }

        public RectTransform[] InsertCardsInvisible(
            List<CardInstance> newCards, int existingCount, int totalFinalCount)
        {
            float cardW = 160f, cardH = 240f, spacing = cardSpacing;
            float totalW = totalFinalCount * cardW + (totalFinalCount - 1) * spacing;
            float startX = -totalW / 2f + cardW / 2f;

            var rts = new RectTransform[newCards.Count];
            for (int i = 0; i < newCards.Count; i++)
            {
                int globalIdx = existingCount + i;
                var go = BuildCard(newCards[i], globalIdx);
                go.transform.SetParent(transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta  = new Vector2(cardW, cardH);
                rt.anchorMin  = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot      = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.zero;

                var (pos, rot) = ArcTransform(globalIdx, totalFinalCount, startX, cardW, spacing);
                rt.anchoredPosition  = pos;
                rt.localEulerAngles  = new Vector3(0f, 0f, rot);

                var cv = go.GetComponent<CardView>();
                cv?.SetHandBase(pos, rot);
                cardViews.Add(cv);
                rts[i] = rt;
            }
            return rts;
        }

        public void RefreshHand(List<CardInstance> hand)
        {
            foreach (var cv in cardViews)
                if (cv != null) Destroy(cv.gameObject);
            cardViews.Clear();

            if (_bricolageGO != null) { Destroy(_bricolageGO); _bricolageGO = null; }

            int count = hand?.Count ?? 0;
            if (count == 0) return;

            float cardW = 160f, cardH = 240f, spacing = cardSpacing;
            float totalW = count * cardW + (count - 1) * spacing;
            float startX = -totalW / 2f + cardW / 2f;

            for (int i = 0; i < count; i++)
            {
                var go = BuildCard(hand[i], i);
                go.transform.SetParent(transform, false);
                ApplyArcTransform(go, i, count, startX, cardW, cardH, spacing);
                cardViews.Add(go.GetComponent<CardView>());
            }
        }

        // ── Arc helpers ───────────────────────────────────────────────────────

        private (Vector2 pos, float rot) ArcTransform(int index, int total, float startX, float cardW, float spacing)
        {
            float t   = total > 1 ? (index / (total - 1f)) * 2f - 1f : 0f; // -1..+1
            float x   = startX + index * (cardW + spacing);
            float y   = sinkY - t * t * arcDepth;
            float rot = -t * maxRotation;
            return (new Vector2(x, y), rot);
        }

        private void ApplyArcTransform(GameObject go, int index, int total,
            float startX, float cardW, float cardH, float spacing)
        {
            var (pos, rot) = ArcTransform(index, total, startX, cardW, spacing);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(cardW, cardH);
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.localEulerAngles = new Vector3(0f, 0f, rot);
            go.GetComponent<CardView>()?.SetHandBase(pos, rot);
        }

        // ── Construction ─────────────────────────────────────────────────────

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
            bool active = gearCount >= 2;
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
            gearTMP.text  = active ? "⚙ PRÊT !" : $"⚙ {gearCount}/2";
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
