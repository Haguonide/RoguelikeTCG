using System.Collections.Generic;
using RoguelikeTCG.Data;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    public class DeckManager
    {
        public List<CardInstance> Deck { get; private set; } = new();
        public List<CardInstance> Hand { get; private set; } = new();
        public List<CardInstance> Discard { get; private set; } = new();

        public int DeckCount => Deck.Count;
        public int HandCount => Hand.Count;
        public int DiscardCount => Discard.Count;

        public void Initialize(CardData[] cardDatas)
        {
            Deck.Clear();
            Hand.Clear();
            Discard.Clear();

            foreach (var data in cardDatas)
                Deck.Add(new CardInstance(data));

            Shuffle(Deck);
        }

        public CardInstance DrawCard()
        {
            if (Deck.Count == 0)
            {
                if (Discard.Count == 0) return null;
                ReshuffleDiscardIntoDeck();
            }

            var card = Deck[0];
            Deck.RemoveAt(0);
            Hand.Add(card);
            return card;
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
                DrawCard();
        }

        // Retire la carte de la main quand elle est jouée (vers le board ou résolue)
        public void PlayCard(CardInstance card)
        {
            Hand.Remove(card);
        }

        // Envoie une carte en défausse (sorts éphémères sont détruits, pas mis en défausse)
        public void SendToDiscard(CardInstance card)
        {
            Hand.Remove(card);
            if (!card.IsEphemeral)
                Discard.Add(card);
        }

        // Appelé par TerrainSystem quand une mission est complétée
        public void AddEphemeralToHand(CardData spellData)
        {
            Hand.Add(new CardInstance(spellData, isEphemeral: true));
        }

        public void ReshuffleDiscardIntoDeck()
        {
            Deck.AddRange(Discard);
            Discard.Clear();
            Shuffle(Deck);
        }

        private void Shuffle(List<CardInstance> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
