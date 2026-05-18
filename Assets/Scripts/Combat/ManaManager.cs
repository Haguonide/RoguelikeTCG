using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Mana croissant : 1 au tour 1, +1 par tour joueur, cap à 6.
    /// Régénère entièrement au début de chaque tour joueur.
    /// Les unités ont manaCost=0 et ne consomment pas de mana.
    /// </summary>
    public class ManaManager : MonoBehaviour
    {
        private const int MAX_CAP = 6;

        private int _turnManaLevel; // niveau croissant : 1, 2, 3, ... 6
        private int _currentMana;

        public int CurrentMana => _currentMana;
        public int MaxMana     => _turnManaLevel;

        public event Action OnManaChanged;

        /// <summary>Initialise le ManaManager (appelé une fois au démarrage du combat).</summary>
        public void Initialize()
        {
            _turnManaLevel = 0;
            _currentMana   = 0;
            OnManaChanged?.Invoke();
        }

        /// <summary>
        /// Appelé au début de chaque tour joueur.
        /// Incrémente le niveau de mana (1, 2, 3 … 6) et régénère entièrement.
        /// </summary>
        public void OnPlayerTurnStart()
        {
            _turnManaLevel = Mathf.Min(_turnManaLevel + 1, MAX_CAP);
            _currentMana   = _turnManaLevel;
            OnManaChanged?.Invoke();
        }

        /// <summary>Bonus de relique — ajoute du mana sans changer le niveau de tour.</summary>
        public void AddBonus(int amount)
        {
            _currentMana = Mathf.Min(_currentMana + amount, _turnManaLevel + amount);
            OnManaChanged?.Invoke();
        }

        public bool CanAfford(int cost) => _currentMana >= cost;

        public void Spend(int cost)
        {
            _currentMana = Mathf.Max(0, _currentMana - cost);
            OnManaChanged?.Invoke();
        }

        /// <summary>L'ennemi n'a pas de mana dans le nouveau système.</summary>
        public void EnemyTurnRegen() { }

        // ── Compatibilité ancienne API ─────────────────────────────────────────

        /// <summary>Compatibilité — redirige vers OnPlayerTurnStart.</summary>
        public void PlayerTurnRegen() => OnPlayerTurnStart();

        /// <summary>Compatibilité — no-op (plus de manches).</summary>
        public void ResetForNewRound() { }
    }
}
