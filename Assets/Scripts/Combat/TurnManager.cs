using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public enum TurnSide { Player, Enemy }

    public class TurnManager
    {
        public TurnSide CurrentSide { get; private set; }
        public int TurnNumber { get; private set; }

        public event Action<TurnSide> OnTurnStart;
        public event Action<TurnSide> OnTurnEnd;

        public void StartCombat()
        {
            TurnNumber = 0;
            CurrentSide = UnityEngine.Random.value >= 0.5f ? TurnSide.Player : TurnSide.Enemy;
            BeginCurrentTurn();
        }

        public void EndTurn()
        {
            OnTurnEnd?.Invoke(CurrentSide);
            CurrentSide = CurrentSide == TurnSide.Player ? TurnSide.Enemy : TurnSide.Player;
            if (CurrentSide == TurnSide.Player)
                TurnNumber++;
            BeginCurrentTurn();
        }

        private void BeginCurrentTurn()
        {
            OnTurnStart?.Invoke(CurrentSide);
        }
    }
}
