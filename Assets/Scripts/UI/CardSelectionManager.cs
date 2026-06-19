using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class CardSelectionManager : MonoBehaviour
    {
        public static CardSelectionManager Instance { get; private set; }

        public CardInstance SelectedCard { get; private set; }
        private CardView _selectedView;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Deselect();
        }

        // Appelé par HandView au clic sur une carte
        public void ToggleSelect(CardInstance card, CardView view)
        {
            if (CombatManager.Instance?.IsResolvingCombat == true) return;
            if (SelectedCard == card) { Deselect(); return; }

            SetHighlight(_selectedView, false);
            SelectedCard = card;
            _selectedView = view;
            SetHighlight(view, true);
        }

        public void Deselect()
        {
            SetHighlight(_selectedView, false);
            SelectedCard = null;
            _selectedView = null;
        }

        // Slot unité joueur (index 0–4)
        public void OnPlayerUnitSlotClicked(int slotIndex)
        {
            if (CombatManager.Instance?.IsResolvingCombat == true) return;
            if (SelectedCard == null || !SelectedCard.IsUnit) return;
            if (!IsPlayerTurn()) return;

            if (CombatManager.Instance.PlayUnit(SelectedCard, slotIndex))
                Deselect();
        }

        // Case Terrain joueur
        public void OnPlayerTerrainSlotClicked()
        {
            if (CombatManager.Instance?.IsResolvingCombat == true) return;
            if (SelectedCard == null || !SelectedCard.IsTerrain) return;
            if (!IsPlayerTurn()) return;

            if (CombatManager.Instance.PlayTerrain(SelectedCard))
                Deselect();
        }

        // Zone ennemie (sorts — pas de ciblage précis pour l'instant)
        public void OnEnemyZoneClicked()
        {
            if (CombatManager.Instance?.IsResolvingCombat == true) return;
            if (SelectedCard == null || !SelectedCard.IsSpell) return;
            if (!IsPlayerTurn()) return;

            if (CombatManager.Instance.PlaySpell(SelectedCard))
                Deselect();
        }

        private bool IsPlayerTurn()
        {
            var tm = CombatManager.Instance?.TurnManager;
            return tm != null && tm.CurrentSide == TurnSide.Player;
        }

        private void SetHighlight(CardView view, bool on)
        {
            if (view == null) return;
            view.transform.localScale = on ? Vector3.one * 1.08f : Vector3.one;
        }
    }
}
