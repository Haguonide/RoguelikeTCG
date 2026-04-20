using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public enum TurnPhase { PlayerTurn, Resolving, EnemyTurn }

    public class TurnManager : MonoBehaviour
    {
        public int maxCardsPerTurn = 3;

        private TurnPhase currentPhase;
        private int cardsPlayedThisTurn;

        public TurnPhase CurrentPhase   => currentPhase;
        public bool IsPlayerTurn        => currentPhase == TurnPhase.PlayerTurn;
        public bool CanPlayCard         => cardsPlayedThisTurn < maxCardsPerTurn;
        public int  CardsRemaining      => maxCardsPerTurn - cardsPlayedThisTurn;
        public int  TurnNumber          { get; private set; }

        public event Action OnPlayerTurnStart;
        public event Action OnEnemyTurnStart;

        public void StartPlayerTurn()
        {
            TurnNumber++;
            currentPhase        = TurnPhase.PlayerTurn;
            cardsPlayedThisTurn = 0;
            OnPlayerTurnStart?.Invoke();
        }

        public void RegisterCardPlayed() => cardsPlayedThisTurn++;

        public void EndPlayerTurn() => currentPhase = TurnPhase.Resolving;

        public void StartEnemyTurn()
        {
            currentPhase        = TurnPhase.EnemyTurn;
            cardsPlayedThisTurn = 0;
            OnEnemyTurnStart?.Invoke();
        }
    }
}
