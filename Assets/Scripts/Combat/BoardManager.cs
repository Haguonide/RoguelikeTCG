// Legacy stub — multi-board management removed; use CombatManager.lanes directly.
using System.Collections.Generic;

namespace RoguelikeTCG.Combat
{
    [System.Obsolete("Multi-board system removed — use CombatLane + CombatManager directly")]
    public class BoardManager : UnityEngine.MonoBehaviour
    {
        public List<Board> boards = new();
        public Board ActiveBoard      => boards.Count > 0 ? boards[0] : null;
        public int   ActiveBoardIndex => 0;

        public void SetActiveBoard(int index) { }
        public void SetActiveBoardCount(int count) { }
        public bool AllBoardsDefeated() => false;

        public event System.Action<int> OnActiveBoardChanged;
    }
}
