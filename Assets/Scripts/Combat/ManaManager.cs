using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Mana croissant 1→6, reset complet au début de chaque manche (pas chaque tour).
    /// +1 par tour joueur, cap à 6.
    /// </summary>
    public class ManaManager : MonoBehaviour
    {
        private const int MAX_CAP = 6;

        private int _manaCap;
        private int _currentMana;
        private int _turnCount; // nombre de tours joueur dans la manche courante

        public int CurrentMana => _currentMana;
        public int MaxMana     => _manaCap;

        public event Action OnManaChanged;

        /// <summary>Initialise le ManaManager (appelé une fois au démarrage du combat).</summary>
        public void Initialize()
        {
            _manaCap     = 0;
            _currentMana = 0;
            _turnCount   = 0;
            OnManaChanged?.Invoke();
        }

        /// <summary>
        /// Appelé au début de chaque manche.
        /// Reset mana à 1 et repart du turnCount = 1.
        /// </summary>
        public void ResetForNewRound()
        {
            _turnCount   = 1;
            _manaCap     = 1;
            _currentMana = 1;
            OnManaChanged?.Invoke();
        }

        /// <summary>
        /// Appelé au début de chaque tour joueur dans une manche.
        /// Mana = min(turnCount, 6), puis turnCount++.
        /// </summary>
        public void OnPlayerTurnStart()
        {
            _manaCap     = Mathf.Min(_turnCount, MAX_CAP);
            _currentMana = _manaCap;
            _turnCount++;
            OnManaChanged?.Invoke();
        }

        /// <summary>Compatibilité ancienne API.</summary>
        public void PlayerTurnRegen() => OnPlayerTurnStart();

        /// <summary>L'ennemi utilise le même pool de mana.</summary>
        public void EnemyTurnRegen()
        {
            _currentMana = _manaCap;
            OnManaChanged?.Invoke();
        }

        /// <summary>Bonus de relique — ajoute du mana sans changer le cap.</summary>
        public void AddBonus(int amount)
        {
            _currentMana = Mathf.Min(_manaCap + amount, _currentMana + amount);
            OnManaChanged?.Invoke();
        }

        public bool CanAfford(int cost) => _currentMana >= cost;

        public void Spend(int cost)
        {
            _currentMana = Mathf.Max(0, _currentMana - cost);
            OnManaChanged?.Invoke();
        }
    }
}
