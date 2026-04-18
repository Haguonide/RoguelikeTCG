using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.AI
{
    public class EnemyAI : MonoBehaviour
    {
        private DeckManager  enemyDeck;
        private BoardManager boardManager;
        private ManaManager  manaManager;
        private const int MaxCardsPerTurn = 2;

        public void Initialize(DeckManager deck, BoardManager boards, ManaManager mana)
        {
            enemyDeck    = deck;
            boardManager = boards;
            manaManager  = mana;
        }

        // ── Tour IA ──────────────────────────────────────────────────────────

        public void PlayTurn()
        {
            enemyDeck.DrawCards(enemyDeck.drawPerTurn);

            for (int i = 0; i < MaxCardsPerTurn; i++)
            {
                var action = FindBestAction();
                if (action == null) break;
                ExecuteAction(action);
            }
        }

        // ── Sélection de la meilleure action ─────────────────────────────────

        private AIAction FindBestAction()
        {
            AIAction best = null;

            foreach (var card in new List<CardInstance>(enemyDeck.Hand))
            {
                var candidate = card.IsUnit ? EvaluateUnit(card) : EvaluateSpell(card);
                if (candidate != null && (best == null || candidate.Score > best.Score))
                    best = candidate;
            }

            return (best != null && best.Score > 0f) ? best : null;
        }

        // ── Évaluation : unités ───────────────────────────────────────────────

        private AIAction EvaluateUnit(CardInstance unit)
        {
            PlaceUnitAction best = null;

            for (int bi = 0; bi < boardManager.boards.Count; bi++)
            {
                var board = boardManager.boards[bi];
                if (!board.isActive || board.IsDefeated) continue;

                foreach (var eLane in board.enemyLanes)
                {
                    if (eLane == null || eLane.IsOccupied) continue;

                    float score = ScorePlacement(unit, eLane, board, bi);
                    if (best == null || score > best.Score)
                        best = new PlaceUnitAction { unit = unit, lane = eLane, boardIdx = bi, Score = score };
                }
            }

            return best;
        }

        private float ScorePlacement(CardInstance unit, Lane eLane, Board board, int boardIdx)
        {
            float score = 5f; // valeur de base : poser une unité vaut toujours quelque chose

            var pLane = board.playerLanes[eLane.laneIndex];
            bool playerHasUnit = pLane != null && pLane.IsOccupied;

            if (playerHasUnit)
            {
                // Bloquer une unité qui attaque sinon directement → forte priorité
                score += 15f + pLane.Occupant.CurrentAttack;
                // Bonus si notre unité peut tuer l'opposant ce tour
                if (unit.CurrentAttack >= pLane.Occupant.currentHP) score += 10f;
            }
            else
            {
                // Lane vide en face : dégâts directs au prochain tour → offensif
                score += 8f + unit.CurrentAttack;
            }

            // Légère préférence pour le board actif (le joueur le regarde)
            if (boardIdx == boardManager.ActiveBoardIndex) score += 2f;

            return score;
        }

        // ── Évaluation : sorts ────────────────────────────────────────────────

        /// <summary>
        /// Convention : SpellTarget est défini du point de vue du JOUEUR.
        /// Du côté IA : EnemyHero = héros du joueur, PlayerHero = héros de l'IA,
        /// EnemyUnit = unité du joueur, AllyUnit = unité de l'IA, AllEnemyUnits = toutes unités du joueur.
        /// </summary>
        private AIAction EvaluateSpell(CardInstance spell)
        {
            if (!manaManager.CanAfford(spell.data.manaCost)) return null;

            switch (spell.data.spellTarget)
            {
                case SpellTarget.EnemyHero:      return EvalDamagePlayerHero(spell);
                case SpellTarget.PlayerHero:     return EvalSelfEffect(spell);
                case SpellTarget.EnemyUnit:      return EvalDamagePlayerUnit(spell);
                case SpellTarget.AllyUnit:       return EvalBuffAIUnit(spell);
                case SpellTarget.AllEnemyUnits:  return EvalAoEPlayerUnits(spell);
                default: return null;
            }
        }

        // Dégâts sur le héros du joueur
        private AIAction EvalDamagePlayerHero(CardInstance spell)
        {
            int dmg = EffectValue(spell, EffectType.Damage);
            if (dmg <= 0) return null;
            var cm = CombatManager.Instance;
            // Plus urgent quand le joueur est bas en HP
            float pressure = cm != null
                ? Mathf.Lerp(1f, 2.5f, 1f - (float)cm.playerHP / Mathf.Max(1, cm.playerMaxHP))
                : 1f;
            return new CastSpellAction { spell = spell, targetPlayerHero = true, Score = dmg * 3f * pressure };
        }

        // Effets sur le héros / camp de l'IA (soin, pioche)
        private AIAction EvalSelfEffect(CardInstance spell)
        {
            int heal = EffectValue(spell, EffectType.Heal);
            int draw = EffectValue(spell, EffectType.DrawCard);
            float score = 0f;

            if (heal > 0)
            {
                float ratio = AverageEnemyBoardHPRatio();
                if (ratio > 0.8f) return null; // pas la peine de se soigner si presque plein
                score += heal * Mathf.Lerp(3f, 1f, ratio);
            }
            if (draw > 0)
            {
                // Utile surtout si la main est petite
                int handSize = enemyDeck.Hand.Count;
                score += draw * Mathf.Lerp(6f, 1f, handSize / 6f);
            }

            return score > 0f ? new CastSpellAction { spell = spell, targetEnemyHero = true, Score = score } : null;
        }

        // Dégâts sur une unité du joueur (vise la plus dangereuse)
        private AIAction EvalDamagePlayerUnit(CardInstance spell)
        {
            int dmg = EffectValue(spell, EffectType.Damage);
            if (dmg <= 0) return null;

            Lane bestTarget = null;
            float bestScore = -1f;

            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                foreach (var pLane in board.playerLanes)
                {
                    if (pLane == null || !pLane.IsOccupied) continue;
                    var unit = pLane.Occupant;
                    float s = dmg * 2f + unit.CurrentAttack;
                    if (dmg >= unit.currentHP) s += 20f; // kill bonus
                    if (s > bestScore) { bestScore = s; bestTarget = pLane; }
                }
            }

            if (bestTarget == null) return null;
            return new CastSpellAction { spell = spell, targetLane = bestTarget, Score = bestScore };
        }

        // Buff / soin / bouclier sur une unité de l'IA
        private AIAction EvalBuffAIUnit(CardInstance spell)
        {
            Lane bestTarget = null;
            float bestScore = -1f;

            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                foreach (var eLane in board.enemyLanes)
                {
                    if (eLane == null || !eLane.IsOccupied) continue;
                    var unit = eLane.Occupant;
                    float s = 0f;

                    int atkBuff = EffectValue(spell, EffectType.BuffAttack);
                    int shield  = EffectValue(spell, EffectType.Shield);
                    int heal    = EffectValue(spell, EffectType.Heal);

                    if (atkBuff > 0)
                        s += atkBuff * 3f + unit.CurrentAttack; // renforcer la plus forte
                    if (shield > 0)
                    {
                        // Protéger l'unité la plus exposée (face à un attaquant)
                        var opp = board.playerLanes[eLane.laneIndex];
                        float threat = (opp != null && opp.IsOccupied) ? opp.Occupant.CurrentAttack : 1f;
                        s += shield * 2f + threat;
                    }
                    if (heal > 0)
                    {
                        int missing = unit.data.maxHP - unit.currentHP;
                        s += Mathf.Min(heal, missing) * 2f;
                    }

                    if (s > bestScore) { bestScore = s; bestTarget = eLane; }
                }
            }

            return (bestTarget != null && bestScore > 0f)
                ? new CastSpellAction { spell = spell, targetLane = bestTarget, Score = bestScore }
                : null;
        }

        // Dégâts AoE sur toutes les unités du joueur (board actif)
        private AIAction EvalAoEPlayerUnits(CardInstance spell)
        {
            int dmg = EffectValue(spell, EffectType.Damage);
            if (dmg <= 0) return null;

            var active = boardManager.ActiveBoard;
            if (active == null || active.IsDefeated) return null;

            int targets = 0, kills = 0;
            foreach (var pLane in active.playerLanes)
            {
                if (pLane == null || !pLane.IsOccupied) continue;
                targets++;
                if (dmg >= pLane.Occupant.currentHP) kills++;
            }

            if (targets == 0) return null;
            float score = dmg * targets * 2f + kills * 15f;
            return new CastSpellAction { spell = spell, isAoE = true, Score = score };
        }

        // ── Exécution ─────────────────────────────────────────────────────────

        private void ExecuteAction(AIAction action)
        {
            if (action is PlaceUnitAction pu)
            {
                pu.lane.PlaceCard(pu.unit);
                enemyDeck.PlayCard(pu.unit);
                AudioManager.Instance.PlaySFX("sfx_card_place");
                Log($"> L'ennemi pose {pu.unit.data.cardName} ({pu.unit.CurrentAttack}/{pu.unit.currentHP})");
                var board = boardManager.boards[pu.boardIdx];
                CombatManager.Instance?.NotifyUnitPlaced(pu.unit, pu.lane, board);
            }
            else if (action is CastSpellAction cs)
            {
                manaManager.Spend(cs.spell.data.manaCost);
                ApplySpellEffects(cs);
                enemyDeck.PlayCard(cs.spell);
                AudioManager.Instance.PlaySFX(cs.isAoE ? "sfx_spell_aoe" : "sfx_spell_cast");
            }
        }

        private void ApplySpellEffects(CastSpellAction cs)
        {
            var cm = CombatManager.Instance;

            foreach (var effect in cs.spell.data.effects)
            {
                if (cs.targetPlayerHero)
                {
                    ApplyEffectToPlayerHero(effect, cs.spell.data.cardName, cm);
                }
                else if (cs.targetEnemyHero)
                {
                    ApplyEffectToAIHero(effect, cs.spell.data.cardName, cm);
                }
                else if (cs.isAoE)
                {
                    var active = boardManager.ActiveBoard;
                    if (active == null) continue;
                    int count = 0;
                    foreach (var pLane in active.playerLanes)
                    {
                        if (pLane == null || !pLane.IsOccupied) continue;
                        ApplyEffectToUnit(effect, pLane);
                        count++;
                    }
                    if (count > 0)
                        Log($"> L'ennemi lance {cs.spell.data.cardName} sur {count} unité(s) !");
                }
                else if (cs.targetLane != null)
                {
                    string targetName = cs.targetLane.IsOccupied ? cs.targetLane.Occupant.data.cardName : "cible";
                    ApplyEffectToUnit(effect, cs.targetLane);
                    Log($"> L'ennemi lance {cs.spell.data.cardName} sur {targetName}");
                }
            }
        }

        private void ApplyEffectToPlayerHero(CardEffect effect, string spellName, CombatManager cm)
        {
            if (cm == null) return;
            switch (effect.effectType)
            {
                case EffectType.Damage:
                    cm.playerHP = Mathf.Max(0, cm.playerHP - effect.value);
                    Log($"> L'ennemi lance {spellName} ! Vous perdez {effect.value} HP ({cm.playerHP}/{cm.playerMaxHP})");
                    break;
            }
        }

        private void ApplyEffectToAIHero(CardEffect effect, string spellName, CombatManager cm)
        {
            switch (effect.effectType)
            {
                case EffectType.Heal:
                    // Soigne le board actif en priorité, sinon le plus abîmé
                    var target = MostDamagedBoard();
                    if (target != null)
                    {
                        target.enemyCurrentHP = Mathf.Min(target.enemyMaxHP, target.enemyCurrentHP + effect.value);
                        Log($"> L'ennemi se soigne de {effect.value} HP !");
                    }
                    break;
                case EffectType.DrawCard:
                    enemyDeck.DrawCards(effect.value);
                    Log($"> L'ennemi pioche {effect.value} carte(s).");
                    break;
            }
        }

        private void ApplyEffectToUnit(CardEffect effect, Lane lane)
        {
            if (!lane.IsOccupied) return;
            var unit = lane.Occupant;

            switch (effect.effectType)
            {
                case EffectType.Damage:
                    int dmg = effect.value;
                    if (unit.shieldHP > 0)
                    {
                        int absorbed = Mathf.Min(unit.shieldHP, dmg);
                        unit.shieldHP -= absorbed;
                        dmg -= absorbed;
                    }
                    unit.currentHP -= dmg;
                    if (!unit.IsAlive) lane.ClearCard();
                    break;
                case EffectType.Heal:
                    unit.currentHP = Mathf.Min(unit.data.maxHP, unit.currentHP + effect.value);
                    break;
                case EffectType.BuffAttack:
                    unit.bonusAttack += effect.value;
                    break;
                case EffectType.Shield:
                    unit.shieldHP += effect.value;
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int EffectValue(CardInstance spell, EffectType type)
        {
            if (spell.data.effects == null) return 0;
            foreach (var e in spell.data.effects)
                if (e.effectType == type) return e.value;
            return 0;
        }

        private float AverageEnemyBoardHPRatio()
        {
            float total = 0f; int count = 0;
            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                total += (float)board.enemyCurrentHP / Mathf.Max(1, board.enemyMaxHP);
                count++;
            }
            return count > 0 ? total / count : 1f;
        }

        private Board MostDamagedBoard()
        {
            Board best = null; float bestRatio = 2f;
            foreach (var board in boardManager.boards)
            {
                if (!board.isActive || board.IsDefeated) continue;
                float r = (float)board.enemyCurrentHP / Mathf.Max(1, board.enemyMaxHP);
                if (r < bestRatio) { bestRatio = r; best = board; }
            }
            return best;
        }

        private void Log(string msg)
        {
            Debug.Log(msg);
            CombatManager.Instance?.combatLog?.AddEntry(msg);
        }
    }

    // ── Types d'actions (classes internes) ───────────────────────────────────

    internal abstract class AIAction { public float Score; }

    internal class PlaceUnitAction : AIAction
    {
        public CardInstance unit;
        public Lane         lane;
        public int          boardIdx;
    }

    internal class CastSpellAction : AIAction
    {
        public CardInstance spell;
        public Lane         targetLane;
        public bool         targetPlayerHero;
        public bool         targetEnemyHero;
        public bool         isAoE;
    }
}
