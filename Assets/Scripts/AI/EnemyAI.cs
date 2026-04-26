using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.AI
{
    /// <summary>
    /// IA ennemie pour la grille 4×4.
    /// Joue 1 unité par tour (si possible) + sorts si mana disponible.
    /// Priorité : clusters Légion en early, complète ses propres lignes/diagonales en mid.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        private DeckManager  _enemyDeck;
        private GridManager  _grid;
        private ManaManager  _mana;

        public void Initialize(DeckManager deck, GridManager grid, ManaManager mana)
        {
            _enemyDeck = deck;
            _grid      = grid;
            _mana      = mana;
        }

        public void PlayTurn()
        {
            if (_enemyDeck == null || _grid == null || _mana == null) return;

            _enemyDeck.DrawCards(_enemyDeck.drawPerTurn);

            // Limite de sécurité
            const int safetyLimit = 10;
            int actions = 0;

            // 1. Poser 1 unité
            for (int i = 0; i < safetyLimit; i++)
            {
                var unitAction = FindBestUnitAction();
                if (unitAction == null) break;
                ExecuteUnit(unitAction);
                actions++;
                break; // 1 unité max par tour
            }

            // 2. Jouer des sorts tant que mana disponible
            for (int i = 0; i < safetyLimit; i++)
            {
                var spellAction = FindBestSpellAction();
                if (spellAction == null) break;
                ExecuteSpell(spellAction);
                actions++;
            }
        }

        // ── Sélection d'unité ─────────────────────────────────────────────────

        private GridPlaceAction FindBestUnitAction()
        {
            GridPlaceAction best = null;

            foreach (var card in new List<CardInstance>(_enemyDeck.Hand))
            {
                if (!card.IsUnit) continue;
                if (!_mana.CanAfford(card.data.manaCost)) continue;

                var placement = ChooseBestPlacement(card);
                if (placement == null) continue;

                float score = ScorePlacement(card, placement.Value.r, placement.Value.c);
                if (best == null || score > best.Score)
                    best = new GridPlaceAction { card = card, row = placement.Value.r, col = placement.Value.c, Score = score };
            }

            return best;
        }

        private (int r, int c)? ChooseBestPlacement(CardInstance unit)
        {
            // Cherche la case vide avec le meilleur score
            (int r, int c)? best = null;
            float bestScore = float.MinValue;

            for (int r = 0; r < GridManager.GRID_SIZE; r++)
            for (int c = 0; c < GridManager.GRID_SIZE; c++)
            {
                if (!_grid.IsEmpty(r, c)) continue;
                float s = ScorePlacement(unit, r, c);
                if (s > bestScore) { bestScore = s; best = (r, c); }
            }

            return best;
        }

        private float ScorePlacement(CardInstance unit, int r, int c)
        {
            float score = 1f;

            // Favoriser les cases qui complètent une ligne/diagonale ennemie
            score += CountAlignedEnemies(r, c) * 3f;

            // Légion : bonus si unité alliée adjacente
            if (unit.data.keyword == UnitKeyword.Légion)
            {
                var neighbors = ScoringSystem.GetOrthogonalNeighbors(r, c);
                foreach (var (nr, nc) in neighbors)
                {
                    var adj = _grid.GetUnit(nr, nc);
                    if (adj != null && !adj.isPlayerCard) { score += 4f; break; }
                }
            }

            // Éviter les cases déjà couvertes par des unités joueur (risque de mort rapide)
            score -= CountAdjacentPlayerUnits(r, c) * 1.5f;

            return score;
        }

        private int CountAlignedEnemies(int r, int c)
        {
            // Compte les unités ennemies déjà alignées sur la ligne H, V, ou diagonale de cette case
            int max = 0;
            max = Mathf.Max(max, CountInRow(r, isPlayer: false));
            max = Mathf.Max(max, CountInCol(c, isPlayer: false));
            return max;
        }

        private int CountInRow(int r, bool isPlayer)
        {
            int count = 0;
            for (int c = 0; c < GridManager.GRID_SIZE; c++)
            {
                var u = _grid.GetUnit(r, c);
                if (u != null && u.isPlayerCard == isPlayer) count++;
            }
            return count;
        }

        private int CountInCol(int c, bool isPlayer)
        {
            int count = 0;
            for (int r = 0; r < GridManager.GRID_SIZE; r++)
            {
                var u = _grid.GetUnit(r, c);
                if (u != null && u.isPlayerCard == isPlayer) count++;
            }
            return count;
        }

        private int CountAdjacentPlayerUnits(int r, int c)
        {
            int count = 0;
            var targets = _grid.GetAttackTargets(r, c, AttackDirection.Up | AttackDirection.Down | AttackDirection.Left | AttackDirection.Right);
            foreach (var (nr, nc) in targets)
            {
                var u = _grid.GetUnit(nr, nc);
                if (u != null && u.isPlayerCard) count++;
            }
            return count;
        }

        // ── Sélection de sort ─────────────────────────────────────────────────

        private GridSpellAction FindBestSpellAction()
        {
            GridSpellAction best = null;

            foreach (var card in new List<CardInstance>(_enemyDeck.Hand))
            {
                if (card.IsUnit) continue;
                if (!_mana.CanAfford(card.data.manaCost)) continue;

                var action = EvaluateSpell(card);
                if (action != null && (best == null || action.Score > best.Score))
                    best = action;
            }

            return best != null && best.Score > 0f ? best : null;
        }

        private GridSpellAction EvaluateSpell(CardInstance card)
        {
            float score = 0f;

            switch (card.data.spellTarget)
            {
                case SpellTarget.EnemyHero:
                    // Dégâts directs au joueur — toujours bon
                    foreach (var eff in card.data.effects)
                        if (eff.effectType == EffectType.Damage) score += eff.value * 1.5f;
                    if (score > 0f) return new GridSpellAction { card = card, row = -1, col = -1, Score = score };
                    break;

                case SpellTarget.PlayerHero:
                    // Soin héros ennemi
                    foreach (var eff in card.data.effects)
                        if (eff.effectType == EffectType.Heal) score += eff.value;
                    if (score > 0f) return new GridSpellAction { card = card, row = -1, col = -1, Score = score * 0.8f };
                    break;

                case SpellTarget.AllyUnit:
                {
                    // Buff une unité alliée
                    var allies = _grid.GetAllUnits(isPlayer: false);
                    if (allies.Count == 0) break;
                    var target = allies[0];
                    foreach (var eff in card.data.effects)
                    {
                        if (eff.effectType == EffectType.BuffAttack && eff.value > 0) score += eff.value * 2f;
                        if (eff.effectType == EffectType.BuffHP     && eff.value > 0) score += eff.value;
                        if (eff.effectType == EffectType.Heal                        ) score += eff.value;
                    }
                    if (score > 0f) return new GridSpellAction { card = card, row = target.gridRow, col = target.gridCol, Score = score };
                    break;
                }

                case SpellTarget.EnemyUnit:
                {
                    // Debuff / dégât à une unité joueur
                    var players = _grid.GetAllUnits(isPlayer: true);
                    if (players.Count == 0) break;
                    CardInstance bestTarget = null;
                    float bestS = 0f;
                    foreach (var t in players)
                    {
                        float s = 0f;
                        foreach (var eff in card.data.effects)
                        {
                            if (eff.effectType == EffectType.Damage    ) s += eff.value * (t.currentHP <= eff.value ? 3f : 1.5f);
                            if (eff.effectType == EffectType.BuffAttack && eff.value < 0) s += -eff.value * 2f;
                        }
                        if (s > bestS) { bestS = s; bestTarget = t; }
                    }
                    if (bestTarget != null && bestS > 0f)
                        return new GridSpellAction { card = card, row = bestTarget.gridRow, col = bestTarget.gridCol, Score = bestS };
                    break;
                }

                case SpellTarget.AllEnemyUnits:
                {
                    int count = _grid.GetAllUnits(isPlayer: true).Count;
                    if (count > 0)
                    {
                        foreach (var eff in card.data.effects)
                            if (eff.effectType == EffectType.Damage) score += eff.value * count * 1.2f;
                    }
                    if (score > 0f) return new GridSpellAction { card = card, row = -1, col = -1, Score = score };
                    break;
                }

                case SpellTarget.AllAllyUnits:
                {
                    int count = _grid.GetAllUnits(isPlayer: false).Count;
                    if (count > 1)
                    {
                        foreach (var eff in card.data.effects)
                            if (eff.effectType == EffectType.BuffAttack && eff.value > 0) score += eff.value * count * 1.5f;
                    }
                    if (score > 0f) return new GridSpellAction { card = card, row = -1, col = -1, Score = score };
                    break;
                }
            }

            return null;
        }

        // ── Exécution ─────────────────────────────────────────────────────────

        private void ExecuteUnit(GridPlaceAction action)
        {
            CombatManager.Instance?.EnemyPlaceUnit(action.card, action.row, action.col);
        }

        private void ExecuteSpell(GridSpellAction action)
        {
            var cm = CombatManager.Instance;
            if (cm == null) return;

            switch (action.card.data.spellTarget)
            {
                case SpellTarget.EnemyHero:
                case SpellTarget.PlayerHero:
                case SpellTarget.AllEnemyUnits:
                case SpellTarget.AllAllyUnits:
                    cm.EnemyCastSpell(action.card);
                    break;

                case SpellTarget.AllyUnit:
                case SpellTarget.EnemyUnit:
                    if (action.row >= 0)
                        cm.EnemyCastSpellOnUnit(action.card, action.row, action.col);
                    else
                        cm.EnemyCastSpell(action.card);
                    break;
            }
        }
    }

    // ── Types d'actions ───────────────────────────────────────────────────────

    internal class GridPlaceAction
    {
        public CardInstance card;
        public int          row;
        public int          col;
        public float        Score;
    }

    internal class GridSpellAction
    {
        public CardInstance card;
        public int          row;
        public int          col;
        public float        Score;
    }
}
