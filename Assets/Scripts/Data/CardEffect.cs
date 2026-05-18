using System;

namespace RoguelikeTCG.Data
{
    [Serializable]
    public class CardEffect
    {
        public EffectType effectType;
        public SpellTarget target;
        public int value;
    }
}
