using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gère les 3 motifs actifs tirés au début d'un combat.
    /// Scoring à la pose : premier arrivé premier servi.
    /// Un motif fermé se réouvre à la manche suivante.
    ///
    /// N'est PAS un singleton — composant attaché en scène, référencé par GridManager.
    /// </summary>
    public class PatternManager : MonoBehaviour
    {
        [Header("Banque complète des motifs")]
        [Tooltip("Assigner tous les PatternData depuis Assets/Data/Patterns/")]
        public List<PatternData> allPatterns;

        // ── Motifs actifs du combat (tirés une seule fois) ────────────────────
        private PatternData[] _activePatterns = new PatternData[3];

        // ── État par manche : quel motif est fermé et par qui ─────────────────
        // -1 = ouvert | 0 = fermé par joueur | 1 = fermé par ennemi
        private int[] _closedBy = new int[3] { -1, -1, -1 };

        // ── Accesseurs ────────────────────────────────────────────────────────

        /// <summary>Les 3 motifs actifs pour ce combat (lecture seule).</summary>
        public PatternData[] ActivePatterns => _activePatterns;

        /// <summary>
        /// Retourne l'état d'un motif : -1=ouvert, 0=fermé joueur, 1=fermé ennemi.
        /// </summary>
        public int GetClosedBy(int patternIndex) => _closedBy[patternIndex];

        // ── Initialisation ────────────────────────────────────────────────────

        /// <summary>
        /// Tire 3 motifs aléatoires depuis la banque.
        /// Règle : au maximum 2 des 3 motifs peuvent contenir la case centrale (index 4).
        /// Appelé une seule fois au début du combat par CombatManager.
        /// </summary>
        public void DrawPatterns()
        {
            if (allPatterns == null || allPatterns.Count < 3)
            {
                Debug.LogError("[PatternManager] Banque de motifs insuffisante (< 3 entrées).");
                return;
            }

            var pool = new List<PatternData>(allPatterns);
            Shuffle(pool);

            _activePatterns = new PatternData[3];
            int centreCount = 0;
            int filled = 0;

            foreach (var p in pool)
            {
                if (filled >= 3) break;
                bool hasCentre = p.ContainsCentre;
                if (hasCentre && centreCount >= 2) continue; // règle max 2 avec centre
                _activePatterns[filled] = p;
                if (hasCentre) centreCount++;
                filled++;
            }

            if (filled < 3)
            {
                // Fallback : relâche la contrainte si la banque ne permet pas de l'honorer
                Debug.LogWarning("[PatternManager] Impossible de respecter la contrainte centre avec la banque actuelle — contrainte relâchée.");
                pool = new List<PatternData>(allPatterns);
                Shuffle(pool);
                _activePatterns = new PatternData[3];
                for (int i = 0; i < 3 && i < pool.Count; i++)
                    _activePatterns[i] = pool[i];
            }

            ResetRound();
            Debug.Log($"[PatternManager] Motifs tirés : {_activePatterns[0]?.patternName} | {_activePatterns[1]?.patternName} | {_activePatterns[2]?.patternName}");
        }

        // ── Par manche ────────────────────────────────────────────────────────

        /// <summary>Réouvre tous les motifs pour la nouvelle manche.</summary>
        public void ResetRound()
        {
            _closedBy[0] = -1;
            _closedBy[1] = -1;
            _closedBy[2] = -1;
        }

        // ── Scoring à la pose ─────────────────────────────────────────────────

        /// <summary>
        /// Appelée après chaque placement d'unité.
        /// Vérifie si le placement (et les unités déjà présentes) complète un motif ouvert.
        /// Retourne les points à encaisser et ferme les motifs complétés.
        ///
        /// <param name="cellIndex">Case placée (0-8).</param>
        /// <param name="isPlayer">True = joueur, False = ennemi.</param>
        /// <param name="getOwnerAt">Fonction retournant -1 (vide), 0 (joueur), 1 (ennemi) pour un index de case.</param>
        /// </summary>
        public int CheckAndScore(int cellIndex, bool isPlayer,
                                 System.Func<int, int> getOwnerAt)
        {
            int camp = isPlayer ? 0 : 1;
            int points = 0;

            for (int i = 0; i < 3; i++)
            {
                var pattern = _activePatterns[i];
                if (pattern == null) continue;
                if (_closedBy[i] != -1) continue;          // déjà fermé cette manche

                // Ce motif doit contenir la case qui vient d'être posée
                bool containsCell = false;
                foreach (int idx in pattern.cellIndices)
                    if (idx == cellIndex) { containsCell = true; break; }
                if (!containsCell) continue;

                // Vérifie que toutes les cases du motif appartiennent au même camp
                bool complete = true;
                foreach (int idx in pattern.cellIndices)
                {
                    if (getOwnerAt(idx) != camp) { complete = false; break; }
                }

                if (complete)
                {
                    points += pattern.Points;
                    _closedBy[i] = camp;
                    Debug.Log($"[PatternManager] Motif '{pattern.patternName}' complété par {(isPlayer ? "joueur" : "ennemi")} (+{pattern.Points} pts)");
                }
            }

            return points;
        }
        // ── Helpers ──────────────────────────────────────────────────────────

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
