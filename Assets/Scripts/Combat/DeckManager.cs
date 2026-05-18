using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gestion du deck, de la main, et de la défausse.
    /// Pas de cimetière : toutes les morts vont en défausse (recyclable).
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        public int maxHandSize = 10;
        public int drawPerTurn = 2;    // 2 cartes par tour dans le nouveau système
        public int initialDraw = 4;    // 4 cartes au premier tour

        private List<CardInstance> deck    = new();
        private List<CardInstance> hand    = new();
        private List<CardInstance> discard = new();

        public List<CardInstance> Hand         => hand;
        public int                DeckCount    => deck.Count;
        public int                DiscardCount => discard.Count;
        public int                CemeteryCount => 0; // supprimé — compatibilité CombatUI

        public void InitializeDeck(List<CardData> cards, bool isPlayerDeck)
        {
            deck.Clear(); hand.Clear(); discard.Clear();
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

        /// <summary>Sort joué depuis la main → défausse.</summary>
        public void PlayCard(CardInstance card)
        {
            hand.Remove(card);
            discard.Add(card);
        }

        /// <summary>Unité posée sur la grille → retirée de la main seulement.</summary>
        public void RemoveFromHand(CardInstance card) => hand.Remove(card);

        /// <summary>Unité morte → défausse (recyclable).</summary>
        public void AddToDiscard(CardInstance card)
        {
            hand.Remove(card);
            if (!discard.Contains(card))
                discard.Add(card);
        }

        /// <summary>Compatibilité ancienne API — redirige vers AddToDiscard.</summary>
        public void AddToCemetery(CardInstance card) => AddToDiscard(card);

        public void DiscardHand()
        {
            discard.AddRange(hand);
            hand.Clear();
        }

        /// <summary>
        /// Carte Repioche : mélange la main dans le deck, pioche autant de cartes.
        /// La carte Repioche elle-même doit avoir été retirée de la main avant l'appel.
        /// </summary>
        public void ReshuffleHandAndRedraw()
        {
            int count = hand.Count;
            deck.AddRange(hand);
            hand.Clear();
            Shuffle(deck);
            DrawCards(count);
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
