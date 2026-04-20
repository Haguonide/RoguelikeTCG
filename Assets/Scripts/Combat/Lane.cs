// Legacy stub — the active lane system is CombatLane.cs
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    [System.Obsolete("Use CombatLane instead")]
    public class Lane : UnityEngine.MonoBehaviour
    {
        public bool isPlayerLane;
        public int  laneIndex;

        public bool         IsOccupied => false;
        public CardInstance Occupant   => null;
        public void PlaceCard(CardInstance c) { }
        public void ClearCard()               { }
    }
}
