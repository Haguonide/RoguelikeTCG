using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.AI
{
    public class EnemyAI : MonoBehaviour
    {
        private DeckManager  enemyDeck;
        private CombatLane[] lanes;
        private ManaManager  manaManager;

        public void Initialize(DeckManager deck, CombatLane[] combatLanes, ManaManager mana)
        {
            enemyDeck    = deck;
            lanes        = combatLanes;
            manaManager  = mana;
        }

        // ── Public entry point ────────────────────────────────────────────────

        public void PlayTurn()
        {
            enemyDeck.DrawCards(enemyDeck.drawPerTurn);

            // Play until no affordable action remains (mana is the only limiter)
            const int safetyLimit = 20;
            for (int i = 0; i < safetyLimit; i++)
            {
                var action = FindBestAction();
                if (action == null) break;
                ExecuteAction(action);
            }
        }

        // ── Action selection ──────────────────────────────────────────────────

        private AIAction FindBestAction()
        {
            AIAction best = null;

            foreach (var card in new List<CardInstance>(enemyDeck.Hand))
            {
                if (!manaManager.CanAfford(card.data.manaCost)) continue;

                var candidate = card.IsUnit ? EvaluateUnit(card) : EvaluateSpell(card);
                if (candidate != null && (best == null || candidate.Score > best.Score))
                    best = candidate;
            }

            return best != null && best.Score > 0f ? best : null;
        }

        // ── Unit evaluation ───────────────────────────────────────────────────

        private AIAction EvaluateUnit(CardInstance unit)
        {
            PlaceUnitAction best = null;

            foreach (var lane in lanes)
            {
                // Only deploy on the backmost enemy cell
                int cell = CombatLane.ENEMY_DEPLOY_CELL;
                if (lane.IsOccupied(cell)) continue;

                float score = ScorePlacement(unit, lane, cell);
                if (best == null || score > best.Score)
                    best = new PlaceUnitAction { card = unit, lane = lane, cell = cell, Score = score };
            }

            return best;
        }

        private float ScorePlacement(CardInstance unit, CombatLane lane, int cell)
        {
            float score = 5f;

            // Check if there are advancing player units in this lane
            var playerUnits = lane.GetPlayerUnits();
            if (playerUnits.Count > 0)
            {
                // Priority: block the closest player unit
                int closestPlayerCell = -1;
                int highestCell = -1;
                foreach (var (c, u) in playerUnits)
                    if (c > highestCell) { highestCell = c; closestPlayerCell = c; }

                // Cells 3-5 are our deploy zone; closer to cell 3 = more aggressive/blocking
                float proximity = (CombatLane.LANE_LENGTH - 1) - highestCell;
                score += 10f + unit.CurrentAttack + (6f - proximity);
            }
            else
            {
                // Empty lane — an enemy unit can traverse and deal direct damage
                score += 6f + unit.CurrentAttack;
            }

            // Prefer lanes where enemy already has support (Légion bonus)
            if (HasPassive(unit, UnitPassiveType.LegionBonusATK))
            {
                foreach (var (c, u) in lane.GetEnemyUnits())
                    if (c != cell) { score += 4f; break; }
            }

            return score;
        }

        // ── Spell evaluation ──────────────────────────────────────────────────

        private AIAction EvaluateSpell(CardInstance card)
        {
            float best = 0f;
            int   bestLane = 0;
            int   bestCell = -1;

            switch (card.data.spellTarget)
            {
                case SpellTarget.EnemyHero:
                    // Deals damage to player HP — always good
                    float heroScore = 0f;
                    foreach (var eff in card.data.effects)
                        if (eff.effectType == EffectType.Damage) heroScore += eff.value * 1.5f;
                    if (heroScore > 0f)
                        return new CastSpellAction { card = card, Score = heroScore };
                    break;

                case SpellTarget.PlayerHero:
                    // Heals enemy hero
                    float healScore = 0f;
                    foreach (var eff in card.data.effects)
                        if (eff.effectType == EffectType.Heal) healScore += eff.value;
                    if (healScore > 0f)
                        return new CastSpellAction { card = card, Score = healScore * 0.8f };
                    break;

                case SpellTarget.AllyUnit:
                    // Buff own unit
                    foreach (var lane in lanes)
                        foreach (var (c, u) in lane.GetEnemyUnits())
                        {
                            float s = ScoreBuffUnit(card, u);
                            if (s > best) { best = s; bestLane = System.Array.IndexOf(lanes, lane); bestCell = c; }
                        }
                    if (best > 0f && bestCell >= 0)
                        return new CastSpellOnUnitAction { card = card, lane = lanes[bestLane], cell = bestCell, Score = best };
                    break;

                case SpellTarget.EnemyUnit:
                    // Debuff / damage player unit
                    foreach (var lane in lanes)
                        foreach (var (c, u) in lane.GetPlayerUnits())
                        {
                            float s = ScoreTargetPlayerUnit(card, u);
                            if (s > best) { best = s; bestLane = System.Array.IndexOf(lanes, lane); bestCell = c; }
                        }
                    if (best > 0f && bestCell >= 0)
                        return new CastSpellOnUnitAction { card = card, lane = lanes[bestLane], cell = bestCell, Score = best };
                    break;

                case SpellTarget.AllEnemyUnits:
                    // AoE on all player units
                    int playerCount = 0;
                    foreach (var lane in lanes) playerCount += lane.GetPlayerUnits().Count;
                    if (playerCount > 0)
                        return new CastSpellAction { card = card, Score = playerCount * 5f };
                    break;

                case SpellTarget.AllAllyUnits:
                    int enemyCount = 0;
                    foreach (var lane in lanes) enemyCount += lane.GetEnemyUnits().Count;
                    if (enemyCount > 1)
                        return new CastSpellAction { card = card, Score = enemyCount * 3f };
                    break;
            }

            return null;
        }

        private static float ScoreBuffUnit(CardInstance card, CardInstance target)
        {
            float score = 0f;
            foreach (var eff in card.data.effects)
            {
                if (eff.effectType == EffectType.BuffAttack && eff.value > 0) score += eff.value * 2f;
                if (eff.effectType == EffectType.BuffHP     && eff.value > 0) score += eff.value * 1f;
                if (eff.effectType == EffectType.Heal                        ) score += eff.value * 1.2f;
            }
            return score;
        }

        private static float ScoreTargetPlayerUnit(CardInstance card, CardInstance target)
        {
            float score = 0f;
            foreach (var eff in card.data.effects)
            {
                if (eff.effectType == EffectType.Damage     ) score += eff.value * (target.currentHP <= eff.value ? 3f : 1.5f);
                if (eff.effectType == EffectType.BuffAttack && eff.value < 0) score += -eff.value * 2f;
                if (eff.effectType == EffectType.SlowUnit   ) score += 4f;
            }
            return score;
        }

        // ── Execute ───────────────────────────────────────────────────────────

        private void ExecuteAction(AIAction action)
        {
            var cm = CombatManager.Instance;
            if (cm == null) return;

            switch (action)
            {
                case PlaceUnitAction pu:
                    cm.EnemyPlaceUnit(pu.card, pu.lane, pu.cell);
                    break;

                case CastSpellOnUnitAction su:
                    cm.EnemyCastSpellOnUnit(su.card, su.lane, su.cell);
                    break;

                case CastSpellAction s:
                    cm.EnemyCastSpell(s.card);
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool HasPassive(CardInstance unit, UnitPassiveType type)
        {
            if (unit?.data?.unitPassives == null) return false;
            foreach (var p in unit.data.unitPassives)
                if (p.passiveType == type) return true;
            return false;
        }
    }

    // ── Action types ──────────────────────────────────────────────────────────

    internal abstract class AIAction
    {
        public float Score;
    }

    internal class PlaceUnitAction : AIAction
    {
        public CardInstance card;
        public CombatLane   lane;
        public int          cell;
    }

    internal class CastSpellOnUnitAction : AIAction
    {
        public CardInstance card;
        public CombatLane   lane;
        public int          cell;
    }

    internal class CastSpellAction : AIAction
    {
        public CardInstance card;
    }
}
