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
        [Tooltip("Valeur initiale du countdown (1-3). À 0 l'unité attaque puis revient à cette valeur.")]
        [Range(1, 3)]
        public int countdown = 2;
        [EnumFlags]
        [Tooltip("Directions attaquées (flags combinables : Up, Down, Left, Right)")]
        public AttackDirection attackDirections = AttackDirection.Right;
        [Tooltip("Keyword unique de l'unité")]
        public UnitKeyword keyword = UnitKeyword.Aucun;
        [Tooltip("Valeur numérique du keyword (ex: Épine=1 signifie 1 dégât à la mort)")]
        public int keywordValue = 0;

        [Header("Spell Targeting & Effects (Spell only)")]
        public SpellTarget spellTarget;
        public List<CardEffect> effects;

        [Header("Upgrade")]
        public CardData upgradedVersion; // null if no upgrade or already upgraded
    }
}
