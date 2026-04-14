using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public class ManaManager : MonoBehaviour
    {
        public int maxMana = 5;
        public int regenPerTurn = 1;

        private int currentMana;
        public int CurrentMana => currentMana;

        public event Action OnManaChanged;

        public void Initialize()
        {
            currentMana = 0;
            OnManaChanged?.Invoke();
        }

        public void RegenTurn()
        {
            currentMana = Mathf.Min(maxMana, currentMana + regenPerTurn);
            OnManaChanged?.Invoke();
        }

        public void AddBonus(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke();
        }

        public bool CanAfford(int cost) => currentMana >= cost;

        public void Spend(int cost)
        {
            currentMana = Mathf.Max(0, currentMana - cost);
            OnManaChanged?.Invoke();
        }
    }
}
