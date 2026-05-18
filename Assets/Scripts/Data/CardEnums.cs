using System;

namespace RoguelikeTCG.Data
{
    public enum CardType { Unit, Spell, Utility }

    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }

    public enum Element { None = 0, Fire = 1, Ice = 2, Lightning = 3, Shadow = 4, Nature = 5 }

    public enum SpellTarget
    {
        PlayerHero,
        EnemyHero,
        AllyUnit,
        EnemyUnit,
        AllEnemyUnits,
        AllAllyUnits,
    }

    public enum EffectType
    {
        Damage       = 0,
        Heal         = 1,
        DrawCard     = 2,
        BuffATK      = 3,
        DestroyUnit  = 4,
        BuffHP       = 5,
        Freeze       = 6,
        // Conservés pour compatibilité sérialisation avec les anciens assets
        BuffNextUnitATK      = 10,
        BuffAllAllyHP        = 11,
        TriggerAllAllyAttack = 12,
    }

    public enum UtilityEffect { Deplacement = 0, Repioche = 1 }

    // ── Keywords et passifs positionnels — conservés pour les scripts de combat existants ──

    /// <summary>Keyword d'une unité — conservé pour le système de combat grille 3×3.</summary>
    public enum UnitKeyword
    {
        Aucun       = 0,
        Impact      = 1,
        Épine       = 3,
        Explosion   = 4,
        Combo       = 5,
        Inspiration = 6,
        Essaim      = 7,
        Dominance   = 8,
        Percée      = 9,
        Réveil      = 10,
    }

    public enum PositionalCondition { None, Corner, Edge, Center }

    public enum PositionalEffect { None, PlusOneATK, PlusOneHP, DrawCard, PlusOnePoint }

    /// <summary>Directions d'attaque d'une unité sur la grille.</summary>
    [Flags]
    public enum AttackDirection
    {
        None  = 0,
        Up    = 1,
        Down  = 2,
        Left  = 4,
        Right = 8,
    }
}
