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
        // Ruée : attaque directement à l'invocation (avant la fin de tour normale)
        Ruee,
    }
}
