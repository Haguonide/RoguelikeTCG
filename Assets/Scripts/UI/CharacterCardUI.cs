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
        public Image         portraitImage;

        private Outline _outline;
        private static readonly Color OutlineColor = new Color(1f, 0.85f, 0.1f, 1f);

        private void Awake()
        {
            if (portraitImage != null)
            {
                _outline = portraitImage.GetComponent<Outline>()
                           ?? portraitImage.gameObject.AddComponent<Outline>();
                _outline.effectColor    = OutlineColor;
                _outline.effectDistance = new Vector2(4f, -4f);
                _outline.enabled        = false;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_outline != null) _outline.enabled = selected;
        }
    }
}
