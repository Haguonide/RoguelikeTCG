using UnityEngine;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gestion minimale du tour dans le nouveau système 2x5.
    /// Pas de rounds/manches — le combat continue jusqu'à 0 HP.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public bool IsPlayerTurn { get; private set; }

        public void StartPlayerTurn()
        {
            IsPlayerTurn = true;
        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }
    }
}
