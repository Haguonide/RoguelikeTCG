using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Composant attaché à chaque carte personnage dans la scène CharacterSelect.
    /// Porte les références visuelles nécessaires au highlight de sélection.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        public CharacterData character;
        public Image         borderImage;
        public Image         bgImage;
    }
}
