using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public class EnemyAI : MonoBehaviour
    {
        [Tooltip("Délai entre chaque carte jouée par l'IA (secondes)")]
        public float playDelay = 0.8f;

        private CombatManager _combat;

        public void Initialize(CombatManager combat)
        {
            _combat = combat;
        }

        public void PlayTurn()
        {
            StartCoroutine(PlayTurnRoutine());
        }

        private IEnumerator PlayTurnRoutine()
        {
            var style = _combat.enemyData.behavior != null
                ? _combat.enemyData.behavior.style
                : BehaviorStyle.Balanced;

            var hand = new List<CardInstance>(_combat.EnemyDeck.Hand);
            SortByPriority(hand, style);

            foreach (var card in hand)
            {
                if (!TryPlay(card, style)) continue;
                yield return new WaitForSeconds(playDelay);
            }

            _combat.EndEnemyTurn();
        }

        private bool TryPlay(CardInstance card, BehaviorStyle style)
        {
            var mana = _combat.EnemyMana;
            var behavior = _combat.enemyData.behavior;
            int reserve = behavior != null ? behavior.manaReserve : 0;

            if (!mana.CanAfford(card.CurrentCost)) return false;
            if (mana.CurrentMana - card.CurrentCost < reserve) return false;

            if (card.IsUnit)
            {
                int slot = PickSlot(style);
                if (slot == -1) return false;
                _combat.PlayEnemyCard(card, slot);
                return true;
            }

            if (card.IsSpell)
            {
                float chance = behavior != null ? behavior.spellPlayChance : 0.5f;
                if (Random.value > chance) return false;
                _combat.PlayEnemyCard(card, 0);
                return true;
            }

            // Les terrains ennemis sont gérés par CombatManager (loopingTerrain) — pas joués depuis la main
            return false;
        }

        private int PickSlot(BehaviorStyle style)
        {
            var board = _combat.Board;

            return style switch
            {
                BehaviorStyle.Aggressive => PickAggressiveSlot(board),
                BehaviorStyle.Defensive  => PickDefensiveSlot(board),
                _                        => board.GetFirstEmptySlot(TurnSide.Enemy),
            };
        }

        // Priorité aux colonnes où le joueur a une unité (pour bloquer)
        private int PickAggressiveSlot(BoardManager board)
        {
            for (int i = 0; i < BoardManager.SlotCount; i++)
                if (board.EnemyUnits[i] == null && board.PlayerUnits[i] != null)
                    return i;
            return board.GetFirstEmptySlot(TurnSide.Enemy);
        }

        // Priorité aux colonnes où le joueur n'a pas d'unité (attaque directe garantie)
        private int PickDefensiveSlot(BoardManager board)
        {
            for (int i = 0; i < BoardManager.SlotCount; i++)
                if (board.EnemyUnits[i] == null && board.PlayerUnits[i] == null)
                    return i;
            return board.GetFirstEmptySlot(TurnSide.Enemy);
        }

        private void SortByPriority(List<CardInstance> hand, BehaviorStyle style)
        {
            hand.Sort((a, b) =>
            {
                // Sorts éphémères toujours en dernier
                if (a.IsEphemeral != b.IsEphemeral)
                    return a.IsEphemeral ? 1 : -1;

                return style switch
                {
                    BehaviorStyle.Aggressive => b.CurrentATK.CompareTo(a.CurrentATK),
                    BehaviorStyle.Defensive  => b.Data.maxHP.CompareTo(a.Data.maxHP),
                    _                        => b.CurrentCost.CompareTo(a.CurrentCost),
                };
            });
        }
    }
}
