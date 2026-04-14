using System;

namespace RoguelikeTCG.Data
{
    [Serializable]
    public class CardEffect
    {
        public EffectType effectType;
        public int value;
        public SpellTarget target;
    }
}
