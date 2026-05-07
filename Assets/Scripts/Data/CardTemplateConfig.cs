using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "CardTemplateConfig", menuName = "RoguelikeTCG/CardTemplateConfig")]
    public class CardTemplateConfig : ScriptableObject
    {
        [Header("Templates Unités")]
        public Sprite unitBackground;
        public Sprite unitFront;

        [Header("Templates Sorts")]
        public Sprite spellBackground;
        public Sprite spellFront;

        [Header("Illustrations placeholder (en attendant les vrais arts)")]
        public Sprite playerFallbackArt;
        public Sprite enemyFallbackArt;
    }
}
