namespace RoguelikeTCG.Data
{
    public enum UnitPassiveType
    {
        // ── On-entry triggers ────────────────────────────────────────────────
        DrawOnEntry,             // Inspiration : pioche X à l'entrée
        ATKDebuffRandomOnEntry,  // Contrebande : -X ATK à une unité ennemie aléatoire à l'entrée
        BuffAlliesOnEntry,       // Ralliement  : +X ATK à toutes les unités alliées déjà présentes
        LegionBonusATK,          // Légion      : +X ATK si ≥1 allié déjà présent
        AoEAllLanesOnEntry,      // Souffle     : X dmg à toutes les unités ennemies sur toutes les lanes

        // ── On-death triggers ────────────────────────────────────────────────
        ThornsOnDeath,           // Épine               : X dmg à l'unité qui a tué
        ATKDebuffOnDeath,        // Contagion           : -X ATK à l'unité ennemie adjacente à la mort
        DamageHeroOnDeath,       // Sacrifice offensif  : X dmg directs aux HP ennemis à la mort
        AoEOnDeath,              // Explosion radioact. : X dmg à toutes les unités ennemies à la mort

        // ── Combat modifiers ─────────────────────────────────────────────────
        Vigilance,                   // ATK×2 si cette unité traverse sans aucun clash
        ExcessDamageBreakthrough,    // Percée   : l'excès de dégâts saigne sur les HP ennemis
        BonusDmgIfEnemyWeakened,     // Exploiter : +X dmg directs si l'unité en face est à ≤0 ATK
        LifestealOnKill,             // Conquête  : soigne X HP héros quand tue une unité
        DmgReduction,                // Blindage  : -X dmg reçus de toutes les sources

        // ── End-of-round triggers ────────────────────────────────────────────
        HealHeroIfAlive,         // Résilience : soigne X HP héros si a survécu à un clash ce tour
        HealHeroIfEnemyWeak,     // Synthèse   : soigne X HP héros si ≥1 ennemi à ≤0 ATK en fin de tour

        // ── Start-of-turn triggers ───────────────────────────────────────────
        AoEStartOfTurn,          // Irradiation : X dmg à toutes les unités ennemies au début du tour

        // ── Movement modifiers ───────────────────────────────────────────────
        ChargeNoSickness,        // Charge : peut avancer le tour où posé (pas de summoning sickness)
        FastAdvance,             // Rapide : avance de 2 cases par tour au lieu de 1
    }

    public enum CardType  { Unit, Spell }

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
