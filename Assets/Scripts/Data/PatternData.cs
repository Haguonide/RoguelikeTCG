using UnityEngine;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// ScriptableObject représentant un motif de scoring sur la grille 3x3.
    /// Les cases sont indexées 0-8 en row-major (0=TL, 4=centre, 8=BR).
    /// Les points sont calculés automatiquement depuis le nombre de cases :
    ///   3 cases → 4 pts | 4 cases → 6 pts | 5 cases → 9 pts
    /// </summary>
    [CreateAssetMenu(fileName = "NewPattern", menuName = "RoguelikeTCG/Pattern")]
    public class PatternData : ScriptableObject
    {
        [Header("Identifiant")]
        public string patternName;

        [Header("Cases du motif (indices 0-8, row-major)")]
        [Tooltip("0=TL 1=TM 2=TR / 3=ML 4=Centre 5=MR / 6=BL 7=BM 8=BR")]
        public int[] cellIndices;

        /// <summary>Points calculés automatiquement depuis le nombre de cases.</summary>
        public int Points => cellIndices == null ? 0 : cellIndices.Length switch
        {
            3 => 4,
            4 => 6,
            5 => 9,
            _ => 0,
        };

        /// <summary>Vrai si ce motif contient la case centrale (index 4).</summary>
        public bool ContainsCentre
        {
            get
            {
                if (cellIndices == null) return false;
                foreach (int idx in cellIndices)
                    if (idx == 4) return true;
                return false;
            }
        }
    }
}
