using UnityEngine;
using TMPro;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class HeroHPUI : MonoBehaviour
    {
        public TMP_Text playerHPText;
        public TMP_Text enemyHPText;

        private CombatManager _combat;

        private void Start()
        {
            _combat = CombatManager.Instance;
            if (_combat == null) return;

            _combat.OnHeroHPChanged += OnHeroHPChanged;

            RefreshPlayer();
            RefreshEnemy();
        }

        private void OnDestroy()
        {
            if (_combat != null) _combat.OnHeroHPChanged -= OnHeroHPChanged;
        }

        private void OnHeroHPChanged(TurnSide side, int newHP)
        {
            if (side == TurnSide.Player) RefreshPlayer();
            else RefreshEnemy();
        }

        private void RefreshPlayer()
        {
            if (playerHPText == null || _combat == null) return;
            int max = _combat.playerCharacter != null ? _combat.playerCharacter.maxHP : 0;
            playerHPText.text = $"HP : {_combat.PlayerHP} / {max}";
        }

        private void RefreshEnemy()
        {
            if (enemyHPText == null || _combat == null) return;
            int max = _combat.enemyData != null ? _combat.enemyData.maxHP : 0;
            enemyHPText.text = $"HP : {_combat.EnemyHP} / {max}";
        }
    }
}
