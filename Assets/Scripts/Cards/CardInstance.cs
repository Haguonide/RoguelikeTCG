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

        // HP actuels (initialisés depuis data.hp)
        public int currentHP;

        // Boost ATK du passif positionnel (0 ou 1)
        public int currentATKBoost;

        // Passif positionnel actif ?
        public bool positionalPassiveActive;

        public int manaCost => data.manaCost;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data             = data;
            this.isPlayerCard     = isPlayerCard;
            this.currentCountdown = data.countdown;
            this.currentHP        = data.hp;
        }

        /// <summary>Applique des dégâts. Retourne true si l'unité meurt (HP <= 0).</summary>
        public bool TakeDamage(int amount)
        {
            currentHP -= amount;
            return currentHP <= 0;
        }

        public bool IsUnit   => data.cardType == CardType.Unit;
        public bool IsAlive  => IsOnGrid;
        public bool IsOnGrid => gridRow >= 0 && gridCol >= 0;
    }
}
