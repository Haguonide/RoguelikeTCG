using System;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Keyword unique d'une unité dans le système grille 3×3.
    /// </summary>
    public enum UnitKeyword
    {
        Aucun       = 0,
        Hâte        = 1,  // CD fixe à 1 (le CardData.countdown doit être 1)
        Épine       = 3,  // à la mort : inflige 1 dégât à l'unité attaquante (Rare+)
        Explosion   = 4,  // à la mort : inflige 1 dégât à toutes les unités adjacentes (Rare+)
        Combo       = 5,  // si la pose complète un motif actif → +1 pt bonus
        Inspiration = 6,  // à la pose : pioche 1 carte
        Légion      = 7,  // CD -1 par unité alliée adjacente (minimum 1)
        Dominance   = 8,  // si encore en vie en fin de manche : +1 pt
        Percée      = 9,  // si kill → attaque aussi la case derrière dans la même direction
        Ralliement  = 10, // à la pose : -1 CD à toutes les unités alliées adjacentes
    }

    public enum PositionalCondition { None, Corner, Edge, Center }

    public enum PositionalEffect { None, PlusOneATK, MinusOneCD, DrawCard, PlusOnePoint }

    /// <summary>
    /// Directions d'attaque d'une unité sur la grille 3×3.
    /// Flags combinables : une unité peut attaquer plusieurs directions.
    /// </summary>
    [Flags]
    public enum AttackDirection
    {
        None  = 0,
        Up    = 1,
        Down  = 2,
        Left  = 4,
        Right = 8,
    }

    // ── Conservés de l'ancien système ────────────────────────────────────────

    public enum CardType { Unit, Spell, Utility }

    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }

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
        Damage,          // dégâts au héros cible, ou kill instantané si cible = unité
        Heal,            // soins au héros joueur uniquement
        DrawCard,        // pioche X cartes
        ReduceCountdown, // réduit le CD d'une unité de X (min 1)
        DestroyUnit,     // détruit instantanément une unité ciblée
        BuffAttack,      // obsolète — conservé pour compatibilité sérialisation
        BuffHP,          // obsolète — conservé pour compatibilité sérialisation
        Shield,          // obsolète — conservé pour compatibilité sérialisation
        ApplyPoison,     // obsolète — conservé pour compatibilité sérialisation
        SlowUnit,        // obsolète — conservé pour compatibilité sérialisation
    }
}
