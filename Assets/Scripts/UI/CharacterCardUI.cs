using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Composant attaché à chaque carte personnage dans la scène CharacterSelect.
    /// Peuple automatiquement les champs texte depuis le CharacterData associé.
    /// </summary>
    public class CharacterCardUI : MonoBehaviour
    {
        public CharacterData character;
        public Image         portraitImage;

        [Header("Textes auto-peuplés depuis CharacterData")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI relicText;

        private Outline _outline;
        private static readonly Color OutlineColor = new Color(1f, 0.85f, 0.1f, 1f);

        private void Awake()
        {
            if (portraitImage != null)
            {
                _outline = portraitImage.GetComponent<Outline>()
                           ?? portraitImage.gameObject.AddComponent<Outline>();
                _outline.effectColor    = OutlineColor;
                _outline.effectDistance = new Vector2(4f, -4f);
                _outline.enabled        = false;
            }
        }

        private void Start()
        {
            if (character == null) return;

            if (portraitImage != null && character.portrait != null)
                portraitImage.sprite = character.portrait;

            if (nameText  != null) nameText.text  = character.characterName;
            if (descText  != null) descText.text  = character.description;
            if (hpText    != null) hpText.text    = $"{character.maxHP} HP";
            if (relicText != null)
                relicText.text = GetRelicDisplayText();
        }

        public void SetSelected(bool selected)
        {
            if (_outline != null) _outline.enabled = selected;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string GetRelicDisplayText()
        {
            if (character == null) return "—";

            var acct = AccountData.Instance;
            var rewards = acct.GetUnlockedRewards(character);

            // Chercher une StartingRelic déjà débloquée pour ce personnage
            foreach (var r in rewards)
            {
                if (r.rewardType == AccountRewardType.StartingRelic && r.relicReward != null)
                    return r.relicReward.relicName;
            }

            // Aucune relique débloquée — trouver le prochain palier
            var allRewards = Resources.LoadAll<AccountLevelReward>("LevelRewards");
            int nextLevel = int.MaxValue;
            foreach (var r in allRewards)
            {
                if (r == null) continue;
                if (r.rewardType != AccountRewardType.StartingRelic) continue;
                if (r.characterFilter != null && r.characterFilter != character) continue;
                if (r.requiredLevel > acct.AccountLevel && r.requiredLevel < nextLevel)
                    nextLevel = r.requiredLevel;
            }

            return nextLevel != int.MaxValue ? $"Niv.{nextLevel} (relique)" : "—";
        }
    }
}
