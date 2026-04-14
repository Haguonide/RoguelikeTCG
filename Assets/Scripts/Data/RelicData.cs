using UnityEngine;

namespace RoguelikeTCG.Data
{
    public enum RelicEffect
    {
        None,
        DrawExtraCardPerTurn,  // Pioche +value cartes au début de chaque tour joueur
        StartWithBonusMana,    // Commence chaque combat avec +value mana
        MaxHPBonus,            // +value HP max (appliqué une fois à l'acquisition)
        HealAfterCombat,       // Récupère +value HP après chaque victoire
    }

    [CreateAssetMenu(fileName = "NewRelic", menuName = "RoguelikeTCG/Relic")]
    public class RelicData : ScriptableObject
    {
        [Header("General")]
        public string relicName;
        [TextArea] public string description;
        public Sprite artwork;

        [Header("Effect")]
        public RelicEffect effect;
        public int effectValue;
    }
}
