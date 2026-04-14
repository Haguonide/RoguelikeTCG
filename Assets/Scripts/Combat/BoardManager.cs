using System.Collections.Generic;
using System;
using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public class BoardManager : MonoBehaviour
    {
        public List<Board> boards = new();

        private int activeBoardIndex = 0;

        public Board ActiveBoard => boards[activeBoardIndex];
        public int ActiveBoardIndex => activeBoardIndex;

        public event Action<int> OnActiveBoardChanged;

        public void SetActiveBoard(int index)
        {
            if (index >= 0 && index < boards.Count)
            {
                activeBoardIndex = index;
                OnActiveBoardChanged?.Invoke(activeBoardIndex);
            }
        }

        public bool AllBoardsDefeated()
        {
            foreach (var board in boards)
                if (!board.IsDefeated) return false;
            return true;
        }
    }
}
