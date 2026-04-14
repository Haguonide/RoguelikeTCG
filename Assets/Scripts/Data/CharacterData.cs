using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "RoguelikeTCG/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        [TextArea] public string description;
        public Sprite portrait;

        [Header("Stats")]
        public int maxHP;

        [Header("Starting Deck")]
        public List<CardData> startingDeck; // 20 cards

        [Header("Starting Relic")]
        public RelicData startingRelic;
    }
}
