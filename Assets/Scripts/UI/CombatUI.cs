using UnityEngine;
using TMPro;

namespace RoguelikeTCG.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Joueur — bas gauche")]
        public TextMeshProUGUI playerHPText;
        public TextMeshProUGUI playerManaText;
        public TextMeshProUGUI playerDeckCountText;
        public TextMeshProUGUI playerHandCountText;
        public TextMeshProUGUI playerGoldText;
        public TextMeshProUGUI discardCountText;

        [Header("Ennemi — portrait droit")]
        public TextMeshProUGUI enemySingleHPText;
        public TextMeshProUGUI enemyManaText;
        public TextMeshProUGUI enemyDeckCountText;
        public TextMeshProUGUI enemyDiscardCountText;

        [Header("Score — Flip Scoreboard")]
        public FlipScoreUI flipScoreUI;

        [Header("Score — fallback TMP (si FlipScoreUI absent)")]
        public TextMeshProUGUI playerScoreText;
        public TextMeshProUGUI enemyScoreText;

        [Header("Tour / Manche")]
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI turnsLeftText;

        [Header("Legacy — ne plus utiliser, conserver pour compatibilité")]
        public TextMeshProUGUI cemeteryCountText;
        public TextMeshProUGUI[] enemyHPTexts;       // vide dans le nouveau système
        public TextMeshProUGUI[] boardAreaHPTexts;   // vide dans le nouveau système

        // ── Refresh ───────────────────────────────────────────────────────────

        public void RefreshPlayerInfo(int hp, int maxHP, int mana, int maxMana, int deckCount, int handCount, int shield = 0)
        {
            if (playerHPText)        playerHPText.text        = $"{hp}";
            if (playerManaText)      playerManaText.text      = $"{mana}/{maxMana}";
            if (playerDeckCountText) playerDeckCountText.text = $"{deckCount}";
            if (playerHandCountText) playerHandCountText.text = $"{handCount}";
        }

        public void RefreshGold(int gold)
        {
            if (playerGoldText) playerGoldText.text = $"Or: {gold}";
        }

        public void RefreshEnemyHP(int current, int max)
        {
            if (enemySingleHPText) enemySingleHPText.text = $"{current}";
        }

        public void RefreshEnemyInfo(int deckCount, int discardCount, int mana, int maxMana)
        {
            if (enemyDeckCountText)    enemyDeckCountText.text    = $"{deckCount}";
            if (enemyDiscardCountText) enemyDiscardCountText.text = $"{discardCount}";
            if (enemyManaText)         enemyManaText.text         = $"{mana}/{maxMana}";
        }

        public void RefreshCemetery(int cemCount, int discardCount)
        {
            if (cemeteryCountText) cemeteryCountText.text = "";
            if (discardCountText)  discardCountText.text  = $"{discardCount}";
        }

        public void RefreshScores(int playerScore, int enemyScore)
        {
            if (flipScoreUI != null)
            {
                flipScoreUI.SetScores(playerScore, enemyScore);
                return;
            }
            if (playerScoreText) playerScoreText.text = $"Score: {playerScore}";
            if (enemyScoreText)  enemyScoreText.text  = $"Score: {enemyScore}";
        }

        public void RefreshRoundInfo(int round, int turnsLeft)
        {
            if (roundText)     roundText.text     = $"Manche {round}";
            if (turnsLeftText) turnsLeftText.text = $"Tours: {turnsLeft}";
        }

        // ── Legacy (compatibilité ancienne scène — no-op) ─────────────────────

        public void ClearEnemyBoard(int boardIndex) { }

        public void RefreshEnemyBoard(int boardIndex, int hp, int maxHP, bool hasDangerUnit) { }
    }
}
