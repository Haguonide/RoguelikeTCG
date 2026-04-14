using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche le niveau de compte et la barre d'XP sur le menu principal.
    /// Attacher à un GameObject de la scène MainMenu.
    /// Refs à câbler dans l'Inspector : LevelText, XPBar, XPText.
    /// </summary>
    public class AccountLevelUI : MonoBehaviour
    {
        [Header("Composants UI")]
        [Tooltip("TextMeshPro affichant 'Niveau X'")]
        public TextMeshProUGUI levelText;

        [Tooltip("Slider représentant la progression d'XP (Min=0, Max=1)")]
        public Slider xpBar;

        [Tooltip("TextMeshPro affichant 'X / Y XP'")]
        public TextMeshProUGUI xpText;

        private void OnEnable()
        {
            // S'assurer qu'AccountData existe
            _ = AccountData.Instance;

            AccountData.Instance.OnProgressChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (AccountData.Instance != null)
                AccountData.Instance.OnProgressChanged -= Refresh;
        }

        private void Refresh()
        {
            var acct = AccountData.Instance;
            if (acct == null) return;

            int required = AccountData.GetXPRequiredForLevel(acct.AccountLevel);

            if (levelText != null)
                levelText.text = $"Niveau {acct.AccountLevel}";

            if (xpBar != null)
            {
                xpBar.minValue = 0f;
                xpBar.maxValue = 1f;
                xpBar.value    = acct.GetLevelProgress();
            }

            if (xpText != null)
                xpText.text = $"{acct.CurrentXP} / {required} XP";
        }
    }
}
