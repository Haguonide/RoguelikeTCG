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
        public int manaCost;

        [Header("Unit — Grille 3x3")]
        [Range(1, 3)]
        public int hp = 1;
        [EnumFlags]
        [Tooltip("Directions attaquées (flags combinables : Up, Down, Left, Right)")]
        public AttackDirection attackDirections = AttackDirection.Right;
        [Tooltip("Keyword unique de l'unité")]
        public UnitKeyword keyword = UnitKeyword.Aucun;

        [Header("Passif positionnel")]
        public PositionalCondition positionalCondition = PositionalCondition.None;
        public PositionalEffect    positionalEffect    = PositionalEffect.None;

        [Header("Utility subtype (Utility only)")]
        public UtilityEffect utilityEffect = UtilityEffect.Deplacement;

        [Header("Spell Targeting & Effects (Spell only)")]
        public SpellTarget spellTarget;
        public List<CardEffect> effects;

        [Header("Upgrade")]
        public CardData upgradedVersion; // null if no upgrade or already upgraded
    }
}
