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

    [Serializable]
    public class UnitPassive
    {
        public UnitPassiveType passiveType;
        public int value;
        [UnityEngine.Tooltip("Keyword displayed in card description (e.g. 'Épine', 'Vigilance')")]
        public string keyword;
    }
}
