using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    public class CardInstance
    {
        public CardData data;
        public int currentHP;
        public int shieldHP;
        public int bonusAttack;   // accumulated ATK buffs — never touches the ScriptableObject
        public bool isPlayerCard;

        public CardInstance(CardData data, bool isPlayerCard)
        {
            this.data = data;
            this.currentHP = data.maxHP;
            this.isPlayerCard = isPlayerCard;
        }

        /// <summary>Base ATK from the card definition plus any in-combat buffs. Clamped to 0 (debuffs can't go negative).</summary>
        public int CurrentAttack => System.Math.Max(0, data.attackPower + bonusAttack);

        public bool IsUnit => data.cardType == CardType.Unit;
        public bool IsAlive => currentHP > 0;
    }
}
