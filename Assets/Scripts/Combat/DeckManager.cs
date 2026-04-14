using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    public class DeckManager : MonoBehaviour
    {
        public int maxHandSize = 8;
        public int drawPerTurn = 2;

        private List<CardInstance> deck = new();
        private List<CardInstance> hand = new();
        private List<CardInstance> discard = new();

        public List<CardInstance> Hand => hand;
        public int DeckCount => deck.Count;
        public int DiscardCount => discard.Count;

        public void InitializeDeck(List<CardData> cards, bool isPlayerDeck)
        {
            deck.Clear();
            hand.Clear();
            discard.Clear();

            foreach (var cardData in cards)
                deck.Add(new CardInstance(cardData, isPlayerDeck));

            Shuffle(deck);
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (hand.Count >= maxHandSize) break;
                if (deck.Count == 0) RecycleDiscard();
                if (deck.Count == 0) break;

                var card = deck[0];
                deck.RemoveAt(0);
                hand.Add(card);
            }
        }

        public void PlayCard(CardInstance card)
        {
            hand.Remove(card);
            discard.Add(card);
        }

        /// <summary>Remet une carte directement en main (retour du terrain).
        /// Si la main est pleine, va en défausse.</summary>
        public void ReturnToHand(CardInstance card)
        {
            if (card == null) return;
            if (hand.Count < maxHandSize)
                hand.Add(card);
            else
                discard.Add(card);
        }

        public void DiscardHand()
        {
            discard.AddRange(hand);
            hand.Clear();
        }

        private void RecycleDiscard()
        {
            deck.AddRange(discard);
            discard.Clear();
            Shuffle(deck);
            Debug.Log("Deck recyclé depuis la défausse.");
        }

        private void Shuffle(List<CardInstance> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
