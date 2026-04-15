using UnityEngine;
using UnityEngine.SceneManagement;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("Fenêtre")]
        public GameObject window;

        public bool IsOpen => window != null && window.activeSelf;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            if (window != null) window.SetActive(false);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // Priorité : fermer l'OptionsPanel s'il est ouvert
            if (OptionsPanel.Instance != null && OptionsPanel.Instance.IsOpen)
            {
                OptionsPanel.Instance.Hide();
                return;
            }

            Toggle();
        }

        public void Toggle() { if (IsOpen) Hide(); else Show(); }

        public void Show()
        {
            Time.timeScale = 0f;
            if (window != null) window.SetActive(true);
        }

        public void Hide()
        {
            Time.timeScale = 1f;
            if (window != null) window.SetActive(false);
        }

        // ── Boutons ───────────────────────────────────────────────────────────

        public void OnResume() => Hide();

        public void OnSaveRun()
        {
            RunPersistence.Instance?.SaveToDisk();
            Hide();
        }

        public void OnAbandonRun()
        {
            Time.timeScale = 1f;
            if (RunPersistence.Instance != null && RunPersistence.Instance.HasActiveRun)
                RunPersistence.Instance.AwardRunXPAndReset();
            SceneManager.LoadScene("MainMenu");
        }

        public void OnOptions() => OptionsPanel.Instance?.Show();

        public void OnQuitGame() => Application.Quit();
    }
}
