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

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Si pas de run en mémoire, tenter de charger depuis le disque
            if (!HasActiveRun && DiskSave.HasSave())
                DiskSave.LoadInto(this);
        }

        public bool HasActiveRun => Map != null;

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

        public void ResetRun()
        {
            Map              = null;
            CurrentNode      = null;
            PlayerHP         = -1;
            PlayerDeck       = null;
            PlayerGold        = 0;
            PlayerRelics      = new List<RelicData>();
            SelectedCharacter = null;
            DiskSave.DeleteSave();
        }
    }
}
