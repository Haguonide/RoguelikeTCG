using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Growing mana system : cap starts at 0, increases by 1 each player-turn start up to MAX_CAP (6).
    /// Each turn (player or enemy) fully refills mana to the current cap.
    /// </summary>
    public class ManaManager : MonoBehaviour
    {
        private const int MAX_CAP = 6;

        private int manaCap;
        private int currentMana;

        public int CurrentMana => currentMana;
        public int MaxMana     => manaCap;

        public event Action OnManaChanged;

        public void Initialize()
        {
            manaCap     = 0;
            currentMana = 0;
            OnManaChanged?.Invoke();
        }

        /// Call once per round at the START of the player's turn.
        /// Grows the cap by 1 (capped at MAX_CAP), then refills.
        public void PlayerTurnRegen()
        {
            if (manaCap < MAX_CAP) manaCap++;
            currentMana = manaCap;
            OnManaChanged?.Invoke();
        }

        /// Call at the start of the enemy's turn — refills to current cap without growing it.
        public void EnemyTurnRegen()
        {
            currentMana = manaCap;
            OnManaChanged?.Invoke();
        }

        /// Relic or other bonus — adds mana on top of the current value (does not change cap).
        public void AddBonus(int amount)
        {
            currentMana = Mathf.Min(manaCap + amount, currentMana + amount);
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
