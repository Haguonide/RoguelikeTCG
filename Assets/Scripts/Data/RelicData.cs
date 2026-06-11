using UnityEngine;

namespace RoguelikeTCG.Data
{
    public enum RelicEffectType
    {
        // Passifs de run (à étoffer lors de la phase mécanique)
        BonusStartingMana,       // +X mana au premier tour
        BonusStartingGold,       // +X or en début de run
        BonusStartingHP,         // +X HP max
        DrawExtraCardPerTurn,    // piocher +X carte par tour
        BonusAtkAllUnits,        // +X ATK à toutes les unités alliées
        GainRuneOnKill,          // +X rune supplémentaire par unité tuée
    }

    [CreateAssetMenu(fileName = "NewRelic", menuName = "RoguelikeTCG/Relic")]
    public class RelicData : ScriptableObject
    {
        [Header("Identity")]
        public string relicName;
        public Sprite icon;
        [TextArea(1, 3)]
        public string description;

        [Header("Effect")]
        public RelicEffectType effectType;
        public int effectValue;

        [Header("Flags")]
        // True = relique de départ déverrouillable par héros ; False = relique de run (boss)
        public bool isStarterRelic;
    }
}
