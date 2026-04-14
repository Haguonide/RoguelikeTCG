using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RoguelikeTCG.Core;
using RoguelikeTCG.SaveSystem;

namespace RoguelikeTCG.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Boutons")]
        public Button continueButton;
        public Button newGameButton;

        [Header("Objets conditionnels")]
        [Tooltip("GameObject du bouton Continuer — masqué si pas de sauvegarde")]
        public GameObject continueButtonGO;

        private void Start()
        {
            AudioManager.Instance.PlayMusic("music_menu");

            // S'assurer que RunPersistence existe (charge la save depuis son Awake)
            if (RunPersistence.Instance == null)
            {
                var go = new GameObject("RunPersistence");
                go.AddComponent<RunPersistence>();
            }

            // S'assurer qu'AccountData existe (auto-créé si absent, charge depuis le disque)
            _ = AccountData.Instance;

            bool hasSave = DiskSave.HasSave();
            if (continueButtonGO != null)
                continueButtonGO.SetActive(hasSave);
        }

        public void OnContinue()
        {
            SceneManager.LoadScene("RunMap");
        }

        public void OnNewGame()
        {
            // Si une run était en cours lors d'un abandon (retour au menu sans défaite
            // ni victoire boss), attribuer l'XP des nœuds déjà visités avant de réinitialiser.
            if (RunPersistence.Instance != null && RunPersistence.Instance.HasActiveRun)
                RunPersistence.Instance.AwardRunXPAndReset();
            else
                RunPersistence.Instance?.ResetRun();

            SceneManager.LoadScene("CharacterSelect");
        }
    }
}
