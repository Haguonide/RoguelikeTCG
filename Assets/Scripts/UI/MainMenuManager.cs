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
            RunPersistence.Instance?.ResetRun();
            SceneManager.LoadScene("CharacterSelect");
        }
    }
}
