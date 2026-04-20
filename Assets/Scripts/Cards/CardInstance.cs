using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    public class CardInstance
    {
        public CardData data;
        public int currentHP;
        public int shieldHP;
        public int bonusAttack;
        public bool isPlayerCard;

        // Board position (-1 = not on board)
        public int cellPosition = -1;
        public int laneIndex    = -1;

        // Per-turn flags
        public bool placedThisTurn;          // summoning sickness — reset after player or enemy turn resolves
        public bool survivedClashThisTurn;   // used by Résilience (HealHeroIfAlive)
        public bool hadClashThisTurn;        // used by Vigilance (no traverse bonus if clashed)
        public bool slowed;                  // can't advance next turn (from SlowUnit effect)

        // Status effects
        public int poisonStacks;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data        = data;
            this.currentHP   = data.maxHP;
            this.isPlayerCard = isPlayerCard;
        }

        public int  CurrentAttack => System.Math.Max(0, data.attackPower + bonusAttack);
        public bool IsUnit        => data.cardType == CardType.Unit;
        public bool IsAlive       => currentHP > 0;
    }
}
