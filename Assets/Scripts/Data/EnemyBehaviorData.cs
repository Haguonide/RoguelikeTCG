using UnityEngine;

namespace RoguelikeTCG.Data
{
    public enum BehaviorStyle
    {
        Aggressive,  // privilégie les unités haut ATK, joue vite
        Defensive,   // privilégie les unités haut HP, économise le mana
        Balanced,    // mix équilibré
    }

    [CreateAssetMenu(fileName = "NewBehavior", menuName = "RoguelikeTCG/Enemy Behavior")]
    public class EnemyBehaviorData : ScriptableObject
    {
        public BehaviorStyle style;

        [Tooltip("Probabilité (0-1) que l'ennemi joue un sort s'il en a un en main")]
        [Range(0f, 1f)]
        public float spellPlayChance = 0.5f;

        [Tooltip("Mana minimum que l'ennemi garde en réserve avant de jouer des cartes coûteuses")]
        public int manaReserve = 0;
    }
}
