namespace RoguelikeTCG.Data
{
    public enum UnitPassiveType
    {
        // Triggered when this unit is placed on the board
        DrawOnEntry,            // Draw value cards
        AoEOnEntry,             // Deal value dmg to all enemy units on this board
        DamageOnEntry,          // Deal value dmg to the unit in the opposite lane (or hero if empty)
        ATKDebuffRandomOnEntry, // Apply -value ATK to a random enemy unit
        BuffAlliesOnEntry,      // Give +value ATK to all other ally units already on this board
        LegionBonusATK,         // Gain +value ATK if at least 1 other ally unit is already on this board

        // Triggered when this unit dies
        ThornsOnDeath,          // Deal value dmg to the unit that killed this unit
        ATKDebuffOnDeath,       // Apply -value ATK to the unit that killed this unit
        DamageHeroOnDeath,      // Deal value dmg to the opposite hero
        AoEOnDeath,             // Deal value dmg to all units on the opposite side

        // Modifiers applied during attack resolution
        DoubleATKIfLaneEmpty,      // When attacking an empty lane, deal ATK×2 direct damage
        ExcessDamageBreakthrough,  // Excess damage after killing a unit bleeds through to the hero
        BonusDmgIfEnemyWeakened,   // If the unit in the opposite lane has CurrentATK ≤ 0, deal +value direct dmg to enemy hero
        LifestealOnKill,           // Heal own hero by value when this unit kills an enemy
        DmgReduction,              // Reduce incoming damage by value (min 0)
        AuraAlliesReduceDmg,       // All ally units on this board take -value dmg from attacks

        // Triggered at end of round (after all enemy attacks)
        HealHeroIfAlive,        // If this unit survived an attack this round, heal ally hero by value
        HealHeroIfEnemyWeak,    // If 1+ enemy unit has CurrentATK ≤ 0 on this board, heal ally hero by value

        // Triggered at the start of the player's attack phase
        AoEStartOfTurn,         // Deal value dmg to all enemy units on this board

        // Triggered when this unit successfully hits an enemy unit (player side only)
        ApplyPoisonOnHit,       // Apply value poison stacks to the enemy unit that was hit
    }

    public enum CardType
    {
        Unit,
        Spell
    }

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum SpellTarget
    {
        PlayerHero,
        EnemyHero,
        AllyUnit,
        EnemyUnit,
        AllEnemyUnits
    }

    public enum EffectType
    {
        Damage,
        Heal,
        BuffAttack,
        BuffHP,
        DrawCard,
        Shield,
        ApplyPoison     // Apply value poison stacks to the target unit
    }
}
