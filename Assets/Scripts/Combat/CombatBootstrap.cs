using UnityEngine;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    // S'exécute avant les scripts UI (Start à exécution -1) pour que les managers
    // soient initialisés quand ManaUI, HeroHPUI, etc. se connectent dans leur Start().
    [DefaultExecutionOrder(-1)]
    public class CombatBootstrap : MonoBehaviour
    {
        public CharacterData playerCharacter;
        public EnemyData enemyData;

        private void Start()
        {
            if (playerCharacter == null || enemyData == null)
            {
                Debug.LogError("[CombatBootstrap] playerCharacter ou enemyData non assigné dans l'Inspector.");
                return;
            }

            var combat = CombatManager.Instance;
            if (combat == null)
            {
                Debug.LogError("[CombatBootstrap] CombatManager.Instance introuvable dans la scène.");
                return;
            }

            combat.StartCombat(playerCharacter, enemyData, null);
        }
    }
}
