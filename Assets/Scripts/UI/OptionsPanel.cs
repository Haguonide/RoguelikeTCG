using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoguelikeTCG.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        public static OptionsPanel Instance { get; private set; }

        [Header("Fenêtre")]
        public GameObject window;

        [Header("Contrôles")]
        public Slider volumeSlider;
        public TextMeshProUGUI languageButtonLabel;
        public TextMeshProUGUI fullscreenButtonLabel;

        private static readonly string[] LanguageNames    = { "Français", "English" };
        private static readonly string[] FullscreenNames  = { "Fenêtré", "Plein écran" };

        private const string KEY_VOLUME     = "opt_volume";
        private const string KEY_LANGUAGE   = "opt_language";
        private const string KEY_FULLSCREEN = "opt_fullscreen";

        public bool IsOpen => window != null && window.activeSelf;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            if (window != null) window.SetActive(false);
        }

        private void Start()
        {
            ApplyStoredSettings();
        }

        public void Show()
        {
            RefreshUI();
            if (window != null) window.SetActive(true);
        }

        public void Hide()
        {
            if (window != null) window.SetActive(false);
        }

        // Appelé par le Slider (onValueChanged)
        public void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(KEY_VOLUME, value);
        }

        // Appelé par le bouton Langue (onClick)
        public void OnLanguageCycleClicked()
        {
            int next = (PlayerPrefs.GetInt(KEY_LANGUAGE, 0) + 1) % LanguageNames.Length;
            PlayerPrefs.SetInt(KEY_LANGUAGE, next);
            if (languageButtonLabel != null)
                languageButtonLabel.text = LanguageNames[next];
        }

        // Appelé par le bouton Plein écran (onClick)
        public void OnFullscreenCycleClicked()
        {
            int next = 1 - PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0);
            Screen.fullScreen = next == 1;
            PlayerPrefs.SetInt(KEY_FULLSCREEN, next);
            if (fullscreenButtonLabel != null)
                fullscreenButtonLabel.text = FullscreenNames[next];
        }

        // Applique volume + fullscreen sans toucher l'UI (appelé au démarrage)
        private void ApplyStoredSettings()
        {
            AudioListener.volume = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);
            Screen.fullScreen    = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        }

        // Met à jour les contrôles UI (appelé à chaque Show)
        private void RefreshUI()
        {
            if (volumeSlider != null)
                volumeSlider.value = PlayerPrefs.GetFloat(KEY_VOLUME, 1f);

            if (languageButtonLabel != null)
                languageButtonLabel.text = LanguageNames[PlayerPrefs.GetInt(KEY_LANGUAGE, 0)];

            if (fullscreenButtonLabel != null)
                fullscreenButtonLabel.text = FullscreenNames[PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0)];
        }
    }
}
