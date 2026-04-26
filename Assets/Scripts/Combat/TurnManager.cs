using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public enum TurnPhase { PlayerTurn, Resolving, EnemyTurn }

    /// <summary>
    /// Gère les tours et les manches.
    /// 10 tours par joueur par manche (20 alternances totales).
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public const int TURNS_PER_ROUND = 10;

        private TurnPhase _currentPhase;

        public TurnPhase CurrentPhase     => _currentPhase;
        public bool      IsPlayerTurn     => _currentPhase == TurnPhase.PlayerTurn;
        public bool      CanPlayCard      => true; // seul le mana limite les joutes

        public int  PlayerTurnsLeft  { get; private set; } = TURNS_PER_ROUND;
        public int  EnemyTurnsLeft   { get; private set; } = TURNS_PER_ROUND;
        public int  CurrentRound     { get; private set; } = 1;
        public int  TurnNumber       { get; private set; }

        public bool IsRoundOver => PlayerTurnsLeft <= 0 && EnemyTurnsLeft <= 0;

        public event Action OnPlayerTurnStart;
        public event Action OnEnemyTurnStart;
        public event Action OnRoundOver;

        // ── API ───────────────────────────────────────────────────────────────

        public void StartPlayerTurn()
        {
            TurnNumber++;
            _currentPhase = TurnPhase.PlayerTurn;
            OnPlayerTurnStart?.Invoke();
        }

        public void EndPlayerTurn()
        {
            _currentPhase = TurnPhase.Resolving;
            PlayerTurnsLeft = Mathf.Max(0, PlayerTurnsLeft - 1);
        }

        public void StartEnemyTurn()
        {
            _currentPhase = TurnPhase.EnemyTurn;
            OnEnemyTurnStart?.Invoke();
        }

        public void EndEnemyTurn()
        {
            EnemyTurnsLeft = Mathf.Max(0, EnemyTurnsLeft - 1);
            if (IsRoundOver) OnRoundOver?.Invoke();
        }

        /// <summary>Reset les compteurs de tours pour une nouvelle manche.</summary>
        public void ResetRound()
        {
            CurrentRound++;
            PlayerTurnsLeft = TURNS_PER_ROUND;
            EnemyTurnsLeft  = TURNS_PER_ROUND;
        }

        /// <summary>Compatibilité ancienne API.</summary>
        public void RegisterCardPlayed() { }
    }
}
