using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    /// <summary>
    /// Instance en mémoire d'une carte (état mutable pendant le combat).
    /// </summary>
    public class CardInstance
    {
        public CardData data;
        public int  currentHP;
        public int  bonusAttack;
        public bool isPlayerCard;

        // Position sur la grille 4×4 (-1 = pas en jeu)
        public int gridRow = -1;
        public int gridCol = -1;

        // Countdown (décrémenté chaque fin de tour, attaque à 0)
        public int currentCountdown;

        // Status effects
        public int poisonStacks = 0;

        // Mana cost (peut être modifié par des effets en cours de run)
        public int manaCost => data.manaCost;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data         = data;
            this.isPlayerCard = isPlayerCard;
            this.currentHP    = data.maxHP;
            // Hâte : le CD est déjà 1 sur CardData.countdown
            this.currentCountdown = data.countdown;
        }

        public int  CurrentAttack => System.Math.Max(0, data.attackPower + bonusAttack);
        public bool IsUnit        => data.cardType == CardType.Unit;
        public bool IsAlive       => currentHP > 0;
        public bool IsOnGrid      => gridRow >= 0 && gridCol >= 0;
    }
}
