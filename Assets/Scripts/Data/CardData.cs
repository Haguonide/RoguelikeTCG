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
        public Element element;

        [Header("Cost")]
        public int manaCost;

        [Header("Unit stats (Unit only)")]
        public int atk = 1;
        [Range(1, 5)]
        public int hp = 1;

        [Header("Spell (Spell only)")]
        public SpellTarget spellTarget;
        public List<CardEffect> effects;

        [Header("Upgrade")]
        public CardData upgradedVersion;

        // ── Champs legacy — conservés pour compatibilité avec les scripts de combat existants ──

        [Header("Legacy — Grille 3x3 (ne plus utiliser pour de nouvelles cartes)")]
        [Tooltip("Directions d'attaque (flags). Conservé pour le système de combat existant.")]
        public AttackDirection attackDirections = AttackDirection.Right;
        [Tooltip("Keyword unique de l'unité. Conservé pour le système de combat existant.")]
        public UnitKeyword keyword = UnitKeyword.Aucun;
        [Tooltip("Condition de passif positionnel. Conservé pour le système de combat existant.")]
        public PositionalCondition positionalCondition = PositionalCondition.None;
        [Tooltip("Effet de passif positionnel. Conservé pour le système de combat existant.")]
        public PositionalEffect    positionalEffect    = PositionalEffect.None;
        [Tooltip("Sous-type utilitaire. Conservé pour le système de combat existant.")]
        public UtilityEffect utilityEffect = UtilityEffect.Deplacement;
    }
}
