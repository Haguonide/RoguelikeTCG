using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;
using RoguelikeTCG.RunMap;
using RoguelikeTCG.SaveSystem;

namespace RoguelikeTCG.Core
{
    /// <summary>
    /// Singleton persistant entre les scènes.
    /// Conserve l'état de la run en cours (carte, HP joueur, deck).
    /// </summary>
    public class RunPersistence : MonoBehaviour
    {
        public static RunPersistence Instance { get; private set; }

        // ── État de la carte de run ───────────────────────────────────────────
        public List<List<RunNode>> Map      { get; set; }
        public RunNode             CurrentNode { get; set; }

        // ── Cache sprites RunMap (empêche le déchargement entre scènes) ─────────
        // Resources.UnloadUnusedAssets() est appelé lors des transitions de scènes ;
        // stocker ici les sprites référencés par RunMapUI les maintient en mémoire.
        public Sprite[] CachedNodeIcons;        // ordre : Start,Combat,Elite,Boss,Event,Shop,Forge,Rest,Mystery
        public Sprite   CachedBackground;
        public Color    CachedBackgroundColor = Color.white;

        // ── Position de scroll de la RunMap ──────────────────────────────────
        // -1 = non défini (nouveau run → scroll en bas par défaut)
        public float MapScrollPosition = -1f;

        // ── Intro animation RunMap ─────────────────────────────────────────────
        // True uniquement quand on arrive depuis la sélection de personnage (nouvelle partie).
        // Consommé (remis à false) dès que l'animation a joué.
        public bool IsNewRun { get; set; } = false;

        // ── État du joueur ────────────────────────────────────────────────────
        public int PlayerHP    = -1;   // -1 = non initialisé → utiliser CharacterData
        public int PlayerMaxHP = 80;

        // ── Deck persistant entre les combats ─────────────────────────────────
        // null = pas encore initialisé → le premier combat utilise le deck de départ
        public List<CardData> PlayerDeck;

        // ── Or ────────────────────────────────────────────────────────────────
        public int PlayerGold = 0;

        // ── Personnage sélectionné ────────────────────────────────────────────
        public CharacterData SelectedCharacter { get; set; }

        // ── Reliques ──────────────────────────────────────────────────────────
        public List<RelicData> PlayerRelics = new List<RelicData>();

        // ── Pool de cartes effectif (deck de base + épiques débloquées via leveling) ─
        // Reconstruit à chaque démarrage / chargement de run. N'est pas sauvegardé sur disque.
        public List<CardData> EffectiveCardPool = new List<CardData>();

        // ── Statistiques de run (pour le calcul d'XP en fin de run) ──────────
        public int NodesVisited = 0;
        public int CombatWins   = 0;
        public int EliteWins    = 0;
        public int BossWins     = 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Si pas de run en mémoire, tenter de charger depuis le disque
            if (!HasActiveRun && DiskSave.HasSave())
            {
                DiskSave.LoadInto(this);
                // Reconstruire le pool effectif pour la run chargée
                if (HasActiveRun && SelectedCharacter != null)
                    BuildEffectiveCardPool();
            }
        }

        public bool HasActiveRun => Map != null;

        // ── Initialisation d'une nouvelle run ─────────────────────────────────

        /// <summary>
        /// Point d'entrée unique pour démarrer une nouvelle run.
        /// Définit le personnage, les HP, le pool effectif et les reliques de leveling.
        /// </summary>
        public void InitRun(CharacterData character)
        {
            SelectedCharacter = character;
            PlayerMaxHP       = character.maxHP;
            PlayerHP          = character.maxHP;
            BuildEffectiveCardPool();
            ApplyLevelingRelics();
        }

        /// <summary>
        /// Reconstruit EffectiveCardPool = cardPool de base + cartes épiques
        /// débloquées via le système de leveling pour le personnage sélectionné.
        /// </summary>
        private void BuildEffectiveCardPool()
        {
            EffectiveCardPool = SelectedCharacter?.cardPool != null
                ? new List<CardData>(SelectedCharacter.cardPool)
                : new List<CardData>();

            var rewards = AccountData.Instance.GetUnlockedRewards(SelectedCharacter);
            foreach (var r in rewards)
            {
                if (r.rewardType != AccountRewardType.UnlockCardInPool) continue;
                if (r.cardReward == null) continue;
                if (!EffectiveCardPool.Contains(r.cardReward))
                    EffectiveCardPool.Add(r.cardReward);
            }
        }

        /// <summary>
        /// Applique les reliques débloquées via leveling au démarrage d'une nouvelle run.
        /// À appeler uniquement à l'init (pas au chargement d'une run sauvegardée).
        /// </summary>
        private void ApplyLevelingRelics()
        {
            PlayerRelics = new List<RelicData>();
            var rewards = AccountData.Instance.GetUnlockedRewards(SelectedCharacter);
            foreach (var r in rewards)
            {
                if (r.rewardType == AccountRewardType.StartingRelic && r.relicReward != null)
                    AddRelic(r.relicReward);
            }
        }

        public void SavePlayerHP(int hp, int maxHP)
        {
            PlayerHP    = hp;
            PlayerMaxHP = maxHP;
            DiskSave.Save(this);
        }

        public void AddGold(int amount)
        {
            PlayerGold += amount;
            DiskSave.Save(this);
        }

        public void AddRelic(RelicData relic)
        {
            if (relic == null) return;
            if (PlayerRelics == null) PlayerRelics = new List<RelicData>();
            PlayerRelics.Add(relic);

            // Effets immédiats à l'acquisition
            if (relic.effect == RelicEffect.MaxHPBonus)
            {
                PlayerMaxHP += relic.effectValue;
                if (PlayerHP > 0) PlayerHP = Mathf.Min(PlayerHP + relic.effectValue, PlayerMaxHP);
            }

            DiskSave.Save(this);
        }

        public void SpendGold(int amount)
        {
            PlayerGold = Mathf.Max(0, PlayerGold - amount);
            DiskSave.Save(this);
        }

        public void AddCardToDeck(CardData card)
        {
            if (card == null) return;
            if (PlayerDeck == null) PlayerDeck = new List<CardData>();
            PlayerDeck.Add(card);
            DiskSave.Save(this);
        }

        public void SaveToDisk() => DiskSave.Save(this);

        // ── Tracking de run ───────────────────────────────────────────────────

        /// <summary>Appeler depuis RunMapManager.VisitNode() à chaque visite de nœud joueur.</summary>
        public void RecordNodeVisited()
        {
            NodesVisited++;
        }

        /// <summary>Appeler depuis CombatManager.OnVictory() avec le type du nœud vaincu.</summary>
        public void RecordCombatWin(NodeType type)
        {
            switch (type)
            {
                case NodeType.Elite: EliteWins++;  break;
                case NodeType.Boss:  BossWins++;   break;
                default:             CombatWins++; break;
            }
        }

        /// <summary>
        /// Calcule l'XP gagnée lors de la run, l'attribue à AccountData, puis réinitialise la run.
        /// À utiliser à la place de ResetRun() pour tout fin de run (victoire boss ou défaite).
        /// </summary>
        public void AwardRunXPAndReset()
        {
            int xp = NodesVisited * 10
                   + CombatWins   * 15
                   + EliteWins    * 30
                   + BossWins     * 50;

            Debug.Log($"[RunPersistence] Fin de run — {NodesVisited} nœuds, "
                    + $"{CombatWins}+{EliteWins}(élite)+{BossWins}(boss) combats → {xp} XP");

            // AccountData est un singleton auto-créé (ne nécessite pas de présence en scène)
            AccountData.Instance.AddXP(xp);

            ResetRun();
        }

        public void ResetRun()
        {
            Map               = null;
            CurrentNode       = null;
            PlayerHP          = -1;
            PlayerDeck        = null;
            PlayerGold        = 0;
            PlayerRelics      = new List<RelicData>();
            EffectiveCardPool = new List<CardData>();
            SelectedCharacter = null;
            NodesVisited      = 0;
            CombatWins        = 0;
            EliteWins         = 0;
            BossWins          = 0;
            DiskSave.DeleteSave();
        }
    }
}
