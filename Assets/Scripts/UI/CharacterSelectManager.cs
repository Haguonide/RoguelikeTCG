using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RoguelikeTCG.Data;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Gère la logique de sélection du personnage.
    /// L'UI est entièrement construite dans la scène — ce script ne crée rien.
    /// </summary>
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Cartes personnages")]
        public CharacterCardUI[] cards;

        [Header("Bouton confirmer")]
        public Button confirmButton;
        public Image  confirmBtnImage;

        private static readonly Color BtnInactive    = new Color(0.22f, 0.22f, 0.26f);
        private static readonly Color BtnActive      = new Color(0.22f, 0.48f, 0.22f);

        private CharacterData _selected;

        private void Start()
        {
            SetConfirmState(false);
        }

        // ── Appelé par le onClick persistant de chaque carte ─────────────────

        public void SelectCard0() => SelectCard(0);
        public void SelectCard1() => SelectCard(1);
        public void SelectCard2() => SelectCard(2);
        public void SelectCard3() => SelectCard(3);

        private void SelectCard(int index)
        {
            if (cards == null || index >= cards.Length) return;
            _selected = cards[index].character;

            for (int i = 0; i < cards.Length; i++)
                cards[i].SetSelected(i == index);

            SetConfirmState(true);
        }

        // ── Appelé par le onClick persistant du bouton Confirmer ─────────────

        public void OnConfirm()
        {
            if (_selected == null) return;

            var persistence = RunPersistence.Instance;
            if (persistence != null)
            {
                persistence.SelectedCharacter = _selected;
                persistence.PlayerMaxHP       = _selected.maxHP;
                persistence.PlayerHP          = _selected.maxHP;
            }

            if (persistence != null)
                persistence.IsNewRun = true;

            SceneManager.LoadScene("RunMap");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetConfirmState(bool active)
        {
            if (confirmButton)    confirmButton.interactable = active;
            if (confirmBtnImage)  confirmBtnImage.color      = active ? BtnActive : BtnInactive;
        }
    }
}
