using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    public class CardInstance
    {
        public CardData Data { get; private set; }

        // Stats runtime (peuvent être modifiées par effets/reliques)
        public int CurrentHP { get; set; }
        public int CurrentATK { get; set; }
        public int CurrentCost { get; set; }

        // Sorts éphémères obtenus via récompense Terrain — hors deck, usage unique
        public bool IsEphemeral { get; private set; }

        public CardInstance(CardData data, bool isEphemeral = false)
        {
            Data = data;
            CurrentHP = data.maxHP;
            CurrentATK = data.atk;
            CurrentCost = data.cost;
            IsEphemeral = isEphemeral;
        }

        public bool IsUnit => Data.cardType == CardType.Unit;
        public bool IsTerrain => Data.cardType == CardType.Terrain;
        public bool IsSpell => Data.cardType == CardType.Spell;
        public bool IsAlive => CurrentHP > 0;

        public bool HasKeyword(KeywordType keyword)
        {
            if (Data.keywords == null) return false;
            foreach (var k in Data.keywords)
                if (k == keyword) return true;
            return false;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP < 0) CurrentHP = 0;
        }
    }
}
