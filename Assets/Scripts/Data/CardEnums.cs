using System;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Keyword unique d'une unité dans le nouveau système grille 4×4.
    /// </summary>
    public enum UnitKeyword
    {
        Aucun       = 0,
        Hâte        = 1,  // CD fixe à 1 (le CardData.countdown doit être 1)
        Blindage    = 2,  // -1 dmg reçu de toutes sources
        Épine       = 3,  // à la mort : X dmg aux unités ENNEMIES adjacentes
        Explosion   = 4,  // à la mort : X dmg à TOUTES unités adjacentes (alliés compris)
        Combo       = 5,  // si la pose complète une ligne/diagonale/carré → +1 pt bonus
        Inspiration = 6,  // à la pose : pioche 1 carte
        Légion      = 7,  // +1 ATK par unité alliée adjacente (calculé au moment de l'attaque)
        Dominance   = 8,  // si vivante à la fin de la manche : +1 pt
        Percée      = 9,  // si kill → attaque aussi la case derrière dans la même direction
        Ralliement  = 10, // à la pose : +1 ATK à toutes les unités alliées adjacentes
    }

    /// <summary>
    /// Directions d'attaque d'une unité sur la grille 4×4.
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

    public enum CardType { Unit, Spell }

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
        Damage,
        Heal,
        BuffAttack,
        BuffHP,
        DrawCard,
        Shield,
        ApplyPoison,
        SlowUnit,
    }
}
