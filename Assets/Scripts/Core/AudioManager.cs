using UnityEngine;

namespace RoguelikeTCG.Core
{
    /// <summary>
    /// Singleton audio manager — auto-created on first use, persists across scenes.
    /// Audio clips are loaded from Assets/Resources/Audio/Music/ and Assets/Resources/Audio/SFX/.
    /// If a clip file is missing, the call is silently ignored (nothing breaks).
    ///
    /// ── Convention des noms de fichiers ──────────────────────────────────────
    ///
    /// MUSIQUES (Assets/Resources/Audio/Music/) :
    ///   music_menu        — écran titre
    ///   music_runmap      — carte de run (boucle taverne)
    ///   music_combat      — combat (boucle tendue)
    ///   music_victory     — fanfare victoire
    ///   music_defeat      — motif défaite
    ///
    /// SFX (Assets/Resources/Audio/SFX/) :
    ///   sfx_card_place    — unité posée sur un slot
    ///   sfx_card_draw     — carte piochée
    ///   sfx_card_hover    — survol d'une carte en main
    ///   sfx_card_select   — clic de sélection d'une carte
    ///   sfx_attack        — charge d'une unité
    ///   sfx_hit           — unité touchée (survit)
    ///   sfx_death         — destruction d'une unité
    ///   sfx_spell_cast    — sort lancé (générique)
    ///   sfx_spell_aoe     — sort zone
    ///   sfx_spell_heal    — soin appliqué
    ///   sfx_spell_shield  — bouclier appliqué
    ///   sfx_end_turn      — fin de tour joueur
    ///   sfx_victory       — victoire
    ///   sfx_defeat        — défaite
    ///   sfx_node_select   — nœud sélectionné sur la RunMap
    ///   sfx_button_click  — clic UI générique
    ///
    /// ─────────────────────────────────────────────────────────────────────────
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Range(0f, 1f)] public float musicVolume = 0.55f;
        [Range(0f, 1f)] public float sfxVolume   = 0.80f;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _musicSource       = gameObject.AddComponent<AudioSource>();
            _musicSource.loop  = true;
            _musicSource.volume = musicVolume;

            _sfxSource        = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop   = false;
            _sfxSource.volume = sfxVolume;
        }

        // ── Musique ───────────────────────────────────────────────────────────

        public void PlayMusic(string clipName)
        {
            var clip = Resources.Load<AudioClip>($"Audio/Music/{clipName}");
            if (clip == null) return;
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;
            _musicSource.clip = clip;
            _musicSource.volume = musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        // ── Effets sonores ────────────────────────────────────────────────────

        public void PlaySFX(string clipName)
        {
            var clip = Resources.Load<AudioClip>($"Audio/SFX/{clipName}");
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // ── Volume ────────────────────────────────────────────────────────────

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (_musicSource != null) _musicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
        }
    }
}
