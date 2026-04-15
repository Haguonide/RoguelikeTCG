using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Composant attaché à chaque carte personnage dans la scène CharacterSelect.
    /// Peuple automatiquement les champs texte depuis le CharacterData associé.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        public CharacterData character;
        public Image         portraitImage;

        [Header("Textes auto-peuplés depuis CharacterData")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI relicText;

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

        private void Start()
        {
            if (character == null) return;

            if (portraitImage != null && character.portrait != null)
                portraitImage.sprite = character.portrait;

            if (nameText  != null) nameText.text  = character.characterName;
            if (descText  != null) descText.text  = character.description;
            if (hpText    != null) hpText.text    = $"❤ {character.maxHP} HP";
            if (relicText != null)
                relicText.text = character.startingRelic != null
                    ? character.startingRelic.relicName
                    : "—";
        }

        public void SetSelected(bool selected)
        {
            if (_outline != null) _outline.enabled = selected;
        }
    }
}
