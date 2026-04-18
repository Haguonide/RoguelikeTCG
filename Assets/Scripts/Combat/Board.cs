using UnityEngine;

namespace RoguelikeTCG.Combat
{
    public class Board : MonoBehaviour
    {
        public int boardIndex;
        public int enemyMaxHP = 60;
        public int enemyCurrentHP;

        public Lane[] playerLanes = new Lane[3];
        public Lane[] enemyLanes = new Lane[3];

        public bool isActive    = true;
        public bool IsDefeated => enemyCurrentHP <= 0;

        private void Awake()
        {
            enemyCurrentHP = enemyMaxHP; // valeur par défaut — sera écrasée par CombatManager.ConfigureBoardCount
        }

        public void TakeDamage(int amount)
        {
            enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - amount);
        }

        public bool HasDangerousEnemyUnit()
        {
            foreach (var lane in enemyLanes)
            {
                if (lane != null && lane.IsOccupied)
                {
                    int i = lane.laneIndex;
                    if (i < playerLanes.Length && playerLanes[i] != null && !playerLanes[i].IsOccupied)
                        return true;
                }
            }
            return false;
        }
    }
}
