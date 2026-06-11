using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "RoguelikeTCG/Card")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public string cardName;
        public CardType cardType;
        public ElementType elementType;
        public int cost;
        public Sprite artwork;

        [Header("Unit")]
        public int atk;
        public int maxHP;

        [Header("Spell")]
        public SpellOrigin spellOrigin;

        [Header("Terrain")]
        [TextArea(1, 2)]
        public string missionText;
        public TerrainMissionType missionType;
        public int missionTarget;
        [TextArea(1, 2)]
        public string rewardText;
        public TerrainRewardType rewardType;
        public int rewardValue;
        // Si true : se réactive après complétion au lieu d'aller en défausse (terrains ennemis)
        public bool loopsOnCompletion;

        [Header("Effect")]
        [TextArea(2, 4)]
        public string description;
        public KeywordType[] keywords;
    }
}
