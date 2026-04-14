using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public enum TurnPhase
    {
        PlayerTurn,
        ResolvingAttacks,
        EnemyTurn
    }

    public class TurnManager : MonoBehaviour
    {
        public int maxCardsPerTurn = 2;

        private TurnPhase currentPhase;
        private int cardsPlayedThisTurn;
        private int _bonusThisTurn;  // bonus actif ce tour
        private int _pendingBonus;   // bonus accordé pour le prochain tour

        public TurnPhase CurrentPhase  => currentPhase;
        public bool CanPlayCard        => cardsPlayedThisTurn < maxCardsPerTurn + _bonusThisTurn;
        public bool IsPlayerTurn       => currentPhase == TurnPhase.PlayerTurn;
        public int  CardsRemaining     => (maxCardsPerTurn + _bonusThisTurn) - cardsPlayedThisTurn;

        public event Action OnPlayerTurnStart;
        public event Action OnEnemyTurnStart;

        public void StartPlayerTurn()
        {
            currentPhase        = TurnPhase.PlayerTurn;
            cardsPlayedThisTurn = 0;
            _bonusThisTurn      = _pendingBonus;
            _pendingBonus       = 0;
            OnPlayerTurnStart?.Invoke();
        }

        public void RegisterCardPlayed() => cardsPlayedThisTurn++;

        public void EndPlayerTurn() => currentPhase = TurnPhase.ResolvingAttacks;

        public void StartEnemyTurn()
        {
            currentPhase        = TurnPhase.EnemyTurn;
            cardsPlayedThisTurn = 0;
            OnEnemyTurnStart?.Invoke();
        }

        /// <summary>Accorde +1 action pour le prochain tour joueur uniquement.</summary>
        public void GrantBonusNextTurn() => _pendingBonus++;
    }
}
