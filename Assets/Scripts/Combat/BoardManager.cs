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

        public void SetActiveBoardCount(int count)
        {
            for (int i = 0; i < boards.Count; i++)
                boards[i].isActive = i < count;
            activeBoardIndex = 0;
        }

        public bool AllBoardsDefeated()
        {
            foreach (var board in boards)
                if (board.isActive && !board.IsDefeated) return false;
            return true;
        }
    }
}
