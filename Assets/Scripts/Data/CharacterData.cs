using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "RoguelikeTCG/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterName;
        public ElementType elementType;
        public Sprite portrait;
        [TextArea(1, 3)]
        public string description;

        [Header("Stats")]
        public int maxHP;

        [Header("Deck")]
        public CardData[] startingDeck;
    }
}
