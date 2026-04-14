using UnityEngine;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Singleton pour les assets partagés des cartes.
    /// Le cardTemplate n'est plus utilisé — le design est entièrement programmatique.
    /// </summary>
    public class CardAssets : MonoBehaviour
    {
        public static CardAssets Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }
    }
}
