using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    public class CardInstance
    {
        public CardData data;
        public bool isPlayerCard;

        // Position sur la grille 3x3 (-1 = pas en jeu)
        public int gridRow = -1;
        public int gridCol = -1;

        // Countdown (décrémenté chaque fin de tour, attaque à 0)
        public int currentCountdown;

        // Bouclier : absorbe la première attaque reçue cette manche
        public bool hasShield = false;

        public int manaCost => data.manaCost;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data             = data;
            this.isPlayerCard     = isPlayerCard;
            this.currentCountdown = data.countdown;
        }

        public bool IsUnit   => data.cardType == CardType.Unit;
        public bool IsAlive  => IsOnGrid;
        public bool IsOnGrid => gridRow >= 0 && gridCol >= 0;
    }
}
