using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Attach to any hero portrait Image in the scene.
    /// When the player has a hero-targeting spell selected, clicking here resolves it.
    /// </summary>
    public class HeroPortraitUI : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("True = player's own portrait. False = enemy portrait.")]
        public bool isPlayerPortrait;

        [Tooltip("Image enfant qui affiche le portrait du héros (masqué par RectMask2D du parent).")]
        public Image portraitImage;

        private Outline _outline;
        private static readonly Color HeroHighlightColor = new Color(1f, 0.92f, 0.2f, 1f);

        private void Awake()
        {
            _outline = gameObject.AddComponent<Outline>();
            _outline.effectColor = HeroHighlightColor;
            _outline.effectDistance = new Vector2(6, -6);
            _outline.enabled = false;
        }

        public void SetHighlight(bool on)
        {
            if (_outline != null) _outline.enabled = on;
        }

        public void SetPortrait(Sprite sprite)
        {
            if (portraitImage != null)
                portraitImage.sprite = sprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                CardSelector.Instance?.OnHeroClicked(isPlayerPortrait);
        }
    }
}
