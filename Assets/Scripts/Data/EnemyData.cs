using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "RoguelikeTCG/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName;
        public Sprite portrait;

        [Header("Stats")]
        public int maxHP;

        [Header("Deck")]
        public CardData[] deck;

        [Header("Terrain")]
        // Terrain unique rejoué en boucle. Null = pas de terrain.
        public CardData loopingTerrain;

        [Header("Behaviour")]
        public EnemyBehaviorData behavior;
    }
}
