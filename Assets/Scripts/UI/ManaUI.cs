using UnityEngine;
using TMPro;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class ManaUI : MonoBehaviour
    {
        public TMP_Text manaText;

        private ManaManager _mana;

        private void Start()
        {
            var combat = CombatManager.Instance;
            if (combat == null) return;

            _mana = combat.PlayerMana;
            _mana.OnManaChanged += OnManaChanged;

            OnManaChanged(_mana.CurrentMana, _mana.MaxMana);
        }

        private void OnDestroy()
        {
            if (_mana != null) _mana.OnManaChanged -= OnManaChanged;
        }

        private void OnManaChanged(int current, int max)
        {
            if (manaText != null) manaText.text = $"{current} / {max}";
        }
    }
}
