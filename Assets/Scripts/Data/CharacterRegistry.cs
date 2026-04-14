using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "CharacterRegistry", menuName = "RoguelikeTCG/CharacterRegistry")]
    public class CharacterRegistry : ScriptableObject
    {
        public List<CharacterData> allCharacters = new List<CharacterData>();

        public CharacterData FindByName(string characterName)
        {
            foreach (var c in allCharacters)
                if (c != null && c.characterName == characterName)
                    return c;
            return null;
        }
    }
}
