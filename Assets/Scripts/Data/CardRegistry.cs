using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Registre de toutes les cartes du jeu.
    /// Placé dans Assets/Resources/ pour être chargeable via Resources.Load en build.
    /// </summary>
    [CreateAssetMenu(fileName = "CardRegistry", menuName = "RoguelikeTCG/CardRegistry")]
    public class CardRegistry : ScriptableObject
    {
        public List<CardData> allCards = new List<CardData>();

        public CardData FindByName(string cardName)
        {
            foreach (var card in allCards)
                if (card != null && card.cardName == cardName)
                    return card;
            return null;
        }
    }
}
