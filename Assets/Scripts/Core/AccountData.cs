using UnityEngine;
using RoguelikeTCG.SaveSystem;

namespace RoguelikeTCG.Core
{
    /// <summary>
    /// Singleton persistant entre les sessions.
    /// Gère le niveau de compte et l'expérience du joueur, indépendants des runs.
    /// </summary>
    public class AccountData : MonoBehaviour
    {
        private static AccountData _instance;

        public static AccountData Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AccountData");
                    _instance = go.AddComponent<AccountData>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // ── Données exposées ──────────────────────────────────────────────────
        public int AccountLevel       { get; private set; } = 1;
        public int CurrentXP          { get; private set; } = 0;
        public int TotalXPEarned      { get; private set; } = 0;
        public int TotalRunsCompleted { get; private set; } = 0;

        /// <summary>Déclenché après chaque changement d'XP ou de niveau.</summary>
        public event System.Action OnProgressChanged;

        // ── Cycle de vie Unity ────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            AccountSave.LoadInto(this);
        }

        // ── XP et niveaux ─────────────────────────────────────────────────────

        /// <summary>
        /// XP nécessaire pour passer du niveau <paramref name="level"/> au suivant.
        /// Formule : niveau × 100 (niv1→2 = 100, niv2→3 = 200, etc.)
        /// </summary>
        public static int GetXPRequiredForLevel(int level) => level * 100;

        /// <summary>
        /// XP total cumulé pour atteindre le niveau <paramref name="level"/> depuis le niveau 1.
        /// Formule : 100 × (level-1) × level / 2
        /// </summary>
        public static int GetTotalXPForLevel(int level)
        {
            if (level <= 1) return 0;
            int n = level - 1;
            return 100 * n * (n + 1) / 2;
        }

        /// <summary>Progression 0–1 dans le niveau actuel.</summary>
        public float GetLevelProgress()
        {
            int required = GetXPRequiredForLevel(AccountLevel);
            if (required <= 0) return 1f;
            return Mathf.Clamp01((float)CurrentXP / required);
        }

        /// <summary>Ajoute de l'XP, gère les montées de niveau, sauvegarde.</summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            TotalXPEarned      += amount;
            TotalRunsCompleted += 1;
            CurrentXP          += amount;

            // Montées de niveau
            while (CurrentXP >= GetXPRequiredForLevel(AccountLevel))
            {
                CurrentXP -= GetXPRequiredForLevel(AccountLevel);
                AccountLevel++;
                OnLevelUp(AccountLevel);
            }

            AccountSave.Save(this);
            OnProgressChanged?.Invoke();

            Debug.Log($"[AccountData] +{amount} XP → Niv.{AccountLevel} ({CurrentXP}/{GetXPRequiredForLevel(AccountLevel)} XP)");
        }

        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"[AccountData] *** NIVEAU {newLevel} ATTEINT ! ***");
            // Future : distribuer les récompenses du niveau (AccountLevelReward)
        }

        // ── Initialisation interne (appelée depuis AccountSave.LoadInto) ──────

        internal void SetData(int level, int xp, int totalXP, int totalRuns)
        {
            AccountLevel       = Mathf.Max(1, level);
            CurrentXP          = Mathf.Max(0, xp);
            TotalXPEarned      = Mathf.Max(0, totalXP);
            TotalRunsCompleted = Mathf.Max(0, totalRuns);
        }
    }
}
