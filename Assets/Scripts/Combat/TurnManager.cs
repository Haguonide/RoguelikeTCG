using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public enum TurnPhase { PlayerTurn, Resolving, EnemyTurn }

    public class TurnManager : MonoBehaviour
    {
        private TurnPhase currentPhase;

        public TurnPhase CurrentPhase => currentPhase;
        public bool IsPlayerTurn      => currentPhase == TurnPhase.PlayerTurn;
        public bool CanPlayCard       => true; // only mana limits plays
        public int  TurnNumber        { get; private set; }

        public event Action OnPlayerTurnStart;
        public event Action OnEnemyTurnStart;

        public void StartPlayerTurn()
        {
            TurnNumber++;
            currentPhase = TurnPhase.PlayerTurn;
            OnPlayerTurnStart?.Invoke();
        }

        public void RegisterCardPlayed() { } // kept for call-site compatibility

        public void EndPlayerTurn() => currentPhase = TurnPhase.Resolving;

        public void StartEnemyTurn()
        {
            currentPhase = TurnPhase.EnemyTurn;
            OnEnemyTurnStart?.Invoke();
        }
    }
}
