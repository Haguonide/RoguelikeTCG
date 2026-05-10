using System;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Keyword unique d'une unité dans le système grille 3×3.
    /// </summary>
    public enum UnitKeyword
    {
        Aucun       = 0,
        Impact      = 1,  // à la pose : inflige 1 dégât supplémentaire à la première cible touchée
        Épine       = 3,  // à la mort : inflige 1 dégât à l'unité attaquante (Rare+)
        Explosion   = 4,  // à la mort : inflige 1 dégât à toutes les unités adjacentes (Rare+)
        Combo       = 5,  // si la pose complète un motif actif → +1 pt bonus
        Inspiration = 6,  // à la pose : pioche 1 carte
        Essaim      = 7,  // +1 ATK à la pose par unité alliée adjacente
        Dominance   = 8,  // si encore en vie en fin de manche : +1 pt
        Percée      = 9,  // si kill → attaque aussi la case derrière dans la même direction
        Réveil      = 10, // à la pose : chaque unité alliée adjacente attaque à nouveau dans ses directions
    }

    public enum PositionalCondition { None, Corner, Edge, Center }

    public enum PositionalEffect { None, PlusOneATK, PlusOneHP, DrawCard, PlusOnePoint }

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
        Damage               = 0, // dégâts au héros cible, ou kill si cible = unité
        Heal                 = 1, // soins au héros joueur
        DrawCard             = 2, // pioche X cartes
        BuffNextUnitATK      = 3, // la prochaine unité jouée gagne +X ATK (à la pose)
        DestroyUnit          = 4, // détruit instantanément une unité ciblée
        BuffAllAllyHP        = 5, // +X HP à toutes les unités alliées sur la grille
        TriggerAllAllyAttack = 9, // toutes les unités alliées sur la grille attaquent à nouveau
        // Obsolètes — conservés pour compatibilité sérialisation
        BuffHP_Obsolete      = 6,
        Shield_Obsolete      = 7,
        ApplyPoison_Obsolete = 8,
    }
}
