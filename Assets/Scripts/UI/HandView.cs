using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class HandView : MonoBehaviour
    {
        public Transform cardContainer;
        public CardView cardPrefab;

        private DeckManager _deck;

        private void Start()
        {
            var combat = CombatManager.Instance;
            if (combat == null) return;

            _deck = combat.PlayerDeck;
            _deck.OnHandChanged += RebuildHand;

            RebuildHand(_deck.Hand);
        }

        private void OnDestroy()
        {
            if (_deck != null) _deck.OnHandChanged -= RebuildHand;
        }

        private void RebuildHand(List<CardInstance> hand)
        {
            foreach (Transform child in cardContainer)
                Destroy(child.gameObject);

            foreach (var instance in hand)
            {
                var view = Instantiate(cardPrefab, cardContainer);
                view.Bind(instance);

                var btn = view.GetComponent<Button>();
                if (btn == null) btn = view.gameObject.AddComponent<Button>();

                var captured = instance;
                btn.onClick.AddListener(() => OnCardClicked(captured));
            }
        }

        private void OnCardClicked(CardInstance instance)
        {
            var combat = CombatManager.Instance;
            if (combat == null) return;

            var data = instance.Data;

            if (instance.IsUnit)
            {
                int slot = combat.Board.GetFirstEmptySlot(TurnSide.Player);
                if (slot != -1) combat.PlayUnit(instance, slot);
            }
            else if (instance.IsTerrain)
            {
                combat.PlayTerrain(instance);
            }
            else if (instance.IsSpell)
            {
                combat.PlaySpell(instance);
            }
        }
    }
}
