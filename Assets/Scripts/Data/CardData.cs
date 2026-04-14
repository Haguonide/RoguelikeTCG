using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "RoguelikeTCG/Card")]
    public class CardData : ScriptableObject
    {
        [Header("General")]
        public string cardName;
        [TextArea] public string description;
        public Sprite artwork;
        public CardType cardType;
        public CardRarity rarity;

        [Header("Cost")]
        public int manaCost; // Always 0 for units

        [Header("Unit Stats (Unit only)")]
        public int attackPower;
        public int maxHP;

        [Header("Spell Targeting & Effects (Spell only)")]
        public SpellTarget spellTarget;
        public List<CardEffect> effects;

        [Header("Upgrade")]
        public CardData upgradedVersion; // null if no upgrade or already upgraded
    }
}
