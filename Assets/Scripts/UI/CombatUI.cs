using UnityEngine;
using TMPro;

namespace RoguelikeTCG.UI
{
    public class CombatUI : MonoBehaviour
    {
        [Header("Player Info (top left)")]
        public TextMeshProUGUI playerHPText;
        public TextMeshProUGUI playerManaText;
        public TextMeshProUGUI playerDeckCountText;
        public TextMeshProUGUI playerHandCountText;
        public TextMeshProUGUI playerGoldText;

        [Header("Enemy Info (top right) — one per board")]
        public TextMeshProUGUI[] enemyHPTexts;

        [Header("Single Enemy HP (new lane system)")]
        public TextMeshProUGUI enemySingleHPText;

        [Header("Cemetery / Discard counts")]
        public TextMeshProUGUI cemeteryCountText;
        public TextMeshProUGUI discardCountText;

        [Header("In-board Enemy HP — label inside each BoardArea (legacy)")]
        public TextMeshProUGUI[] boardAreaHPTexts;

        public void RefreshPlayerInfo(int hp, int maxHP, int mana, int maxMana, int deckCount, int handCount, int shield = 0)
        {
            if (playerHPText)        playerHPText.text        = shield > 0 ? $"HP: {hp}/{maxHP}  \ud83d\udee1 {shield}" : $"HP: {hp}/{maxHP}";
            if (playerManaText)      playerManaText.text      = $"Mana: {mana}/{maxMana}";
            if (playerDeckCountText) playerDeckCountText.text = $"Deck: {deckCount}";
            if (playerHandCountText) playerHandCountText.text = $"Main: {handCount}";
        }

        public void RefreshGold(int gold)
        {
            if (playerGoldText) playerGoldText.text = $"Or: {gold}";
        }

        public void RefreshEnemyHP(int current, int max)
        {
            if (enemySingleHPText) enemySingleHPText.text = $"Ennemi : {current}/{max} HP";
        }

        public void RefreshCemetery(int cemCount, int discardCount)
        {
            if (cemeteryCountText) cemeteryCountText.text = $"Cim: {cemCount}";
            if (discardCountText)  discardCountText.text  = $"Def: {discardCount}";
        }

        public void ClearEnemyBoard(int boardIndex)
        {
            if (boardIndex < enemyHPTexts.Length && enemyHPTexts[boardIndex] != null)
                enemyHPTexts[boardIndex].text = "";
            if (boardAreaHPTexts != null && boardIndex < boardAreaHPTexts.Length && boardAreaHPTexts[boardIndex] != null)
                boardAreaHPTexts[boardIndex].text = "";
        }

        public void RefreshEnemyBoard(int boardIndex, int hp, int maxHP, bool hasDangerUnit)
        {
            if (boardIndex < enemyHPTexts.Length && enemyHPTexts[boardIndex] != null)
            {
                string danger = hasDangerUnit ? " (!)" : "";
                enemyHPTexts[boardIndex].text = $"B{boardIndex + 1}: {hp}/{maxHP}{danger}";
            }

            if (boardAreaHPTexts != null && boardIndex < boardAreaHPTexts.Length && boardAreaHPTexts[boardIndex] != null)
                boardAreaHPTexts[boardIndex].text = $"{hp}/{maxHP} HP";
        }
    }
}
