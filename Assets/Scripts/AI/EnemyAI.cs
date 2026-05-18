using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Combat;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.AI
{
    /// <summary>
    /// IA ennemie pour la grille 2x5.
    /// Pose 1 unité par tour sur Row 0.
    /// Stratégie par défaut : Aggressive (priorité cases face aux unités joueur).
    /// Lit EnemyBehaviorData si assigné.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("Comportement optionnel (si null : comportement par défaut Aggressive)")]
        public EnemyBehaviorData behaviorData;

        private DeckManager _enemyDeck;
        private GridManager _grid;
        private ManaManager _mana;

        public void Initialize(DeckManager deck, GridManager grid, ManaManager mana)
        {
            _enemyDeck = deck;
            _grid      = grid;
            _mana      = mana;
        }

        public void PlayTurn()
        {
            if (_enemyDeck == null || _grid == null) return;

            // L'ennemi pioche 1 carte
            _enemyDeck.DrawCards(1);

            // 1. Poser 1 unité sur Row 0
            var unitCard = ChooseUnitToPlay();
            if (unitCard != null)
            {
                int col = ChooseBestColumn(unitCard);
                if (col >= 0)
                    CombatManager.Instance?.EnemyPlaceUnit(unitCard, 0, col);
            }

            // 2. Jouer des sorts si mana disponible (l'ennemi n'a pas de mana dans le système actuel)
            // Réservé pour une implémentation future
        }

        // ── Sélection d'unité ─────────────────────────────────────────────────

        private CardInstance ChooseUnitToPlay()
        {
            // Choisit la première unité disponible en main
            foreach (var card in new List<CardInstance>(_enemyDeck.Hand))
            {
                if (card.IsUnit) return card;
            }
            return null;
        }

        private int ChooseBestColumn(CardInstance unit)
        {
            var strategy = behaviorData?.strategy ?? EnemyBehaviorData.Strategy.Aggressive;

            switch (strategy)
            {
                case EnemyBehaviorData.Strategy.Aggressive:
                    return ChooseAggressiveColumn(unit);

                case EnemyBehaviorData.Strategy.Defensive:
                    return ChooseDefensiveColumn();

                case EnemyBehaviorData.Strategy.SynergySeeker:
                    return ChooseSynergyColumn(unit);

                default:
                    return ChooseAggressiveColumn(unit);
            }
        }

        /// <summary>
        /// Aggressive : priorité face aux unités joueur (Row 1).
        /// Secondaire : n'importe quelle case vide Row 0.
        /// </summary>
        private int ChooseAggressiveColumn(CardInstance unit)
        {
            // Cherche d'abord une case en face d'une unité joueur
            var bestCol = -1;
            float bestScore = float.MinValue;

            for (int c = 0; c < GridManager.COLS; c++)
            {
                if (!_grid.IsEmpty(0, c)) continue;

                float score = 0f;

                // Bonus si une unité joueur est en face
                var playerUnit = _grid.GetUnit(1, c);
                if (playerUnit != null) score += 10f;

                // Bonus synergies bonds (SynergySeeker aussi utilisé en secondaire)
                score += ScoreBondSynergy(unit, 0, c);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol   = c;
                }
            }

            return bestCol;
        }

        /// <summary>
        /// Defensive : priorité colonnes vides côté joueur (pour bloquer les lane leaks).
        /// </summary>
        private int ChooseDefensiveColumn()
        {
            // Priorité : colonnes où le joueur n'a pas d'unité (empêche les lane leaks ennemis)
            for (int c = 0; c < GridManager.COLS; c++)
            {
                if (!_grid.IsEmpty(0, c)) continue;
                var playerUnit = _grid.GetUnit(1, c);
                if (playerUnit == null) return c; // case vide en face → pas de lane leak ennemi possible
            }

            // Fallback : n'importe quelle case vide
            for (int c = 0; c < GridManager.COLS; c++)
                if (_grid.IsEmpty(0, c)) return c;

            return -1;
        }

        /// <summary>
        /// SynergySeeker : privilégie les colonnes qui créent des bonds avec les voisins.
        /// </summary>
        private int ChooseSynergyColumn(CardInstance unit)
        {
            int bestCol   = -1;
            float bestScore = float.MinValue;

            for (int c = 0; c < GridManager.COLS; c++)
            {
                if (!_grid.IsEmpty(0, c)) continue;

                float score = ScoreBondSynergy(unit, 0, c);
                // Bonus léger pour faire face à une unité joueur
                if (_grid.GetUnit(1, c) != null) score += 2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol   = c;
                }
            }

            return bestCol >= 0 ? bestCol : FallbackEmptyCol();
        }

        private float ScoreBondSynergy(CardInstance unit, int row, int col)
        {
            float score = 0f;
            if (unit.data.element == Element.None) return 0f;

            var leftNeighbor  = col > 0 ? _grid.GetUnit(row, col - 1) : null;
            var rightNeighbor = col < GridManager.COLS - 1 ? _grid.GetUnit(row, col + 1) : null;

            if (leftNeighbor != null && leftNeighbor.isPlayerCard == unit.isPlayerCard)
            {
                var bond = BondSystem.GetBond(unit.data.element, leftNeighbor.data.element);
                if (bond != BondType.None) score += 5f;
            }
            if (rightNeighbor != null && rightNeighbor.isPlayerCard == unit.isPlayerCard)
            {
                var bond = BondSystem.GetBond(unit.data.element, rightNeighbor.data.element);
                if (bond != BondType.None) score += 5f;
            }

            return score;
        }

        private int FallbackEmptyCol()
        {
            for (int c = 0; c < GridManager.COLS; c++)
                if (_grid.IsEmpty(0, c)) return c;
            return -1;
        }
    }
}
