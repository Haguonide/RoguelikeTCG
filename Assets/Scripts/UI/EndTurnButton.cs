using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class EndTurnButton : MonoBehaviour
    {
        public Button button;

        private TurnManager _turn;

        private void Start()
        {
            var combat = CombatManager.Instance;
            if (combat == null) return;

            _turn = combat.TurnManager;
            _turn.OnTurnStart += OnTurnStart;

            if (button != null)
                button.onClick.AddListener(OnClick);

            RefreshInteractable();
        }

        private void OnDestroy()
        {
            if (_turn != null) _turn.OnTurnStart -= OnTurnStart;
            if (button != null) button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            CombatManager.Instance?.EndPlayerTurn();
        }

        private void OnTurnStart(TurnSide side)
        {
            RefreshInteractable();
        }

        private void RefreshInteractable()
        {
            if (button == null || _turn == null) return;
            button.interactable = _turn.CurrentSide == TurnSide.Player;
        }
    }
}
