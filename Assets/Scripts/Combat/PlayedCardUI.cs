using UnityEngine;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Représentation visuelle d'une unité posée sur un slot.
    /// Instancie CardPrefab dans le slot, délègue l'affichage à CardView.
    /// </summary>
    public class PlayedCardUI : MonoBehaviour
    {
        public RectTransform animatedRoot;
        public CardView      cardView;

        // ── Factory ───────────────────────────────────────────────────────────

        public static PlayedCardUI Create(Transform slotTransform, CardInstance card)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Cards/CardPrefab");

            var rootGO = prefab != null
                ? Instantiate(prefab)
                : new GameObject("PlayedCard", typeof(RectTransform));

            rootGO.name = "PlayedCard";
            rootGO.transform.SetParent(slotTransform, false);

            var rootRT       = rootGO.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;

            var cv = rootGO.GetComponent<CardView>() ?? rootGO.AddComponent<CardView>();
            cv.Setup(card);

            var ui          = rootGO.AddComponent<PlayedCardUI>();
            ui.animatedRoot = rootRT;
            ui.cardView     = cv;
            return ui;
        }

        // ── Runtime ───────────────────────────────────────────────────────────

        public void Refresh(CardInstance card)
        {
            cardView?.Setup(card);
        }
    }
}
