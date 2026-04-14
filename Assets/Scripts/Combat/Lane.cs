using UnityEngine;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.Combat
{
    public class Lane : MonoBehaviour
    {
        public bool isPlayerLane;
        public int laneIndex;

        private CardInstance occupant;

        public bool IsOccupied => occupant != null;
        public CardInstance Occupant => occupant;

        public void PlaceCard(CardInstance card)
        {
            occupant = card;
        }

        public void ClearCard()
        {
            occupant = null;
        }
    }
}
