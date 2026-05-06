using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gestion du deck, de la main, et de la défausse.
    /// Plus de cimetière : toutes les morts vont en défausse (recyclable).
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        public int maxHandSize = 10;
        public int drawPerTurn = 1;    // 1 carte par tour joueur dans le nouveau système
        public int initialDraw = 4;

        private List<CardInstance> deck    = new();
        private List<CardInstance> hand    = new();
        private List<CardInstance> discard = new();  // toutes les cartes jouées/mortes

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

        /// <summary>Unité morte OU unité ayant survécu à une manche → défausse.</summary>
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

        /// <summary>Réinitialise les CD de toutes les cartes de la défausse pour la prochaine manche.</summary>
        public void ResetDiscardCountdowns()
        {
            foreach (var c in discard)
                c.currentCountdown = c.data.countdown;
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
