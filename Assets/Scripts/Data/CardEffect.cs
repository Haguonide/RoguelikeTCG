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

    /// <summary>
    /// Conservé uniquement pour compatibilité de sérialisation avec les anciens assets.
    /// Ne plus utiliser — le système de keyword est géré via CardData.keyword (UnitKeyword).
    /// </summary>
    [Serializable]
    [Obsolete("Remplacé par UnitKeyword sur CardData. Conservé pour compatibilité sérialisation.")]
    public class UnitPassive
    {
        public UnitPassiveType passiveType;
        public int value;
        [UnityEngine.Tooltip("Keyword displayed in card description")]
        public string keyword;
    }

    /// <summary>
    /// Conservé pour compatibilité de sérialisation avec les anciens assets.
    /// </summary>
    [Obsolete("Remplacé par UnitKeyword dans CardEnums.cs.")]
    public enum UnitPassiveType
    {
        DrawOnEntry,
        ATKDebuffRandomOnEntry,
        BuffAlliesOnEntry,
        LegionBonusATK,
        AoEAllLanesOnEntry,
        ThornsOnDeath,
        ATKDebuffOnDeath,
        DamageHeroOnDeath,
        AoEOnDeath,
        Vigilance,
        ExcessDamageBreakthrough,
        BonusDmgIfEnemyWeakened,
        LifestealOnKill,
        DmgReduction,
        HealHeroIfAlive,
        HealHeroIfEnemyWeak,
        AoEStartOfTurn,
        ChargeNoSickness,
        FastAdvance,
        VenomOnClash,
    }
}
