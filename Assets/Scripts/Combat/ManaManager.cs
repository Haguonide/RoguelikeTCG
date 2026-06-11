using System;

namespace RoguelikeTCG.Combat
{
    public class ManaManager
    {
        public const int MaxManaCap = 10;

        public int CurrentMana { get; private set; }
        public int MaxMana { get; private set; }

        public event Action<int, int> OnManaChanged; // current, max

        public void StartCombat()
        {
            MaxMana = 0;
            CurrentMana = 0;
        }

        // Appelé en début de chaque tour du propriétaire de ce ManaManager
        public void OnTurnStart()
        {
            if (MaxMana < MaxManaCap)
                MaxMana++;
            CurrentMana = MaxMana;
            OnManaChanged?.Invoke(CurrentMana, MaxMana);
        }

        public bool CanAfford(int cost) => CurrentMana >= cost;

        public bool Spend(int cost)
        {
            if (!CanAfford(cost)) return false;
            CurrentMana -= cost;
            OnManaChanged?.Invoke(CurrentMana, MaxMana);
            return true;
        }

        // Pour les effets temporaires (récompenses Terrain, reliques…)
        public void AddMana(int amount)
        {
            CurrentMana = Math.Min(CurrentMana + amount, MaxMana);
            OnManaChanged?.Invoke(CurrentMana, MaxMana);
        }
    }
}
