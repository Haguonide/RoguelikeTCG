using UnityEngine;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// ScriptableObject décrivant la stratégie et le deck d'un ennemi.
    /// Assigné à EnemyAI dans la scène pour personnaliser le comportement.
    /// </summary>
    [CreateAssetMenu(menuName = "RoguelikeTCG/EnemyBehavior", fileName = "NewEnemyBehavior")]
    public class EnemyBehaviorData : ScriptableObject
    {
        public enum Strategy
        {
            Aggressive,    // Priorité : placer en face des unités joueur
            Defensive,     // Priorité : remplir les colonnes vides pour bloquer les lane leaks
            SynergySeeker, // Priorité : maximiser les bonds entre unités adjacentes
        }

        [Header("Stratégie")]
        public Strategy strategy = Strategy.Aggressive;

        [Header("Deck de l'ennemi")]
        public CharacterData character;

        [Header("HP (0 = utiliser valeur par défaut selon node type : 20/35/50)")]
        public int hp = 0;
    }
}
