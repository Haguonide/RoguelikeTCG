namespace RoguelikeTCG.Data
{
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
        Shield
    }
}
