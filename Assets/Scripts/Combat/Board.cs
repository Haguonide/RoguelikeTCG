// Legacy stub — boards no longer exist in the new lane-advancement system.
namespace RoguelikeTCG.Combat
{
    [System.Obsolete("Multi-board system removed — use CombatLane + CombatManager directly")]
    public class Board : UnityEngine.MonoBehaviour
    {
        public int  boardIndex;
        public int  enemyMaxHP     = 30;
        public int  enemyCurrentHP = 30;
        public bool isActive       = true;
        public bool IsDefeated     => enemyCurrentHP <= 0;

        public Lane[] playerLanes = new Lane[0];
        public Lane[] enemyLanes  = new Lane[0];

        public void TakeDamage(int amount)
        {
            enemyCurrentHP = UnityEngine.Mathf.Max(0, enemyCurrentHP - amount);
        }

        public bool HasDangerousEnemyUnit() => false;
    }
}
