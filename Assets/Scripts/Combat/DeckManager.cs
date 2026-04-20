using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    public class DeckManager : MonoBehaviour
    {
        public int maxHandSize  = 10;
        public int drawPerTurn  = 2;
        public int initialDraw  = 5;

        private List<CardInstance> deck     = new();
        private List<CardInstance> hand     = new();
        private List<CardInstance> discard  = new();  // recyclable (traversed units + played spells)
        private List<CardInstance> cemetery = new();  // permanent (killed in combat)

        public List<CardInstance> Hand       => hand;
        public int DeckCount                 => deck.Count;
        public int DiscardCount              => discard.Count;
        public int CemeteryCount             => cemetery.Count;
        public List<CardInstance> Cemetery   => cemetery;

        public void InitializeDeck(List<CardData> cards, bool isPlayerDeck)
        {
            deck.Clear(); hand.Clear(); discard.Clear(); cemetery.Clear();
            foreach (var cd in cards) deck.Add(new CardInstance(cd, isPlayerDeck));
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

        /// Call when a SPELL is played — removes from hand, adds to discard.
        public void PlayCard(CardInstance card)
        {
            hand.Remove(card);
            discard.Add(card);
        }

        /// Call when a UNIT is placed on the board — only removes from hand.
        /// The unit goes to Cemetery or Discard when it leaves the board.
        public void RemoveFromHand(CardInstance card) => hand.Remove(card);

        /// Unit traversed the lane — goes to DISCARD (recyclable).
        public void AddToDiscard(CardInstance card)
        {
            discard.Add(card);
        }

        /// Unit killed in combat — goes to CEMETERY (never returns).
        public void AddToCemetery(CardInstance card)
        {
            hand.Remove(card);
            discard.Remove(card);
            cemetery.Add(card);
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
            Debug.Log("[DeckManager] Défausse mélangée dans le deck.");
        }

        private static void Shuffle(List<CardInstance> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
