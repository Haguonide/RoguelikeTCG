namespace RoguelikeTCG.Data
{
    public enum CardType
    {
        Unit,
        Terrain,
        Spell
    }

    public enum SpellOrigin
    {
        Permanent,
        Ephemeral
    }

    public enum ElementType
    {
        Neutral,
        Fire,
        Shadow,
        Nature,
        Ice
    }

    public enum TerrainMissionType
    {
        KillEnemyUnits,          // tuer X unités ennemies
        HaveAlliedUnitsOnBoard,  // avoir X unités alliées simultanément
        DealDamageInOneTurn,     // infliger X dégâts en un seul tour
    }

    public enum TerrainRewardType
    {
        DrawCards,           // piocher X cartes
        GetEphemeralSpell,   // obtenir un sort éphémère (défini séparément)
        BonusMana,           // +X mana ce tour
        HealHero,            // soigner X HP au héros
    }

    public enum RuneType
    {
        // Types à définir lors de la phase mécanique
        Fire,
        Shadow,
        Nature,
        Ice,
        Neutral,
    }

    public enum KeywordType
    {
        Charge,                 // attaque le tour où elle est posée (pas de summoning sickness)
        Epine,                  // à la mort, X dmg à l'unité tueuse
        Inspiration,            // pioche X à l'entrée en jeu
        Vigilance,              // dégâts ×2 si attaque une colonne vide
        Percee,                 // overkill saigne sur HP héros ennemi
        Resilience,             // soigne X HP héros si survit à une attaque ce tour
        Legion,                 // +1 ATK à l'entrée si ≥1 allié présent
        Conquete,               // soigne X HP héros quand cette unité tue
        SacrificeOffensif,      // à la mort, inflige X dmg directs au héros ennemi
        Irradiation,            // 1 dmg AoE à toutes les unités ennemies au début de chaque tour
        ExplosionRadioactive,   // AoE X dmg à toutes les unités ennemies à la mort
        Contagion,              // à la mort, réduit l'ATK de l'unité ennemie en face de X
        Exploiter,              // +2 dmg sur l'unité attaquée si elle est à 0 ATK
        Blindage,               // réduit de 1 tous les dégâts reçus
        Ralliement,             // +1 ATK à toutes les unités alliées présentes à l'entrée
    }
}
