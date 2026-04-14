using UnityEngine;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Types de récompenses débloquables par le niveau de compte.
    /// </summary>
    public enum AccountRewardType
    {
        None,
        StartingCard,   // Ajoute une carte au deck de départ d'un personnage
        StartingRelic,  // Offre une relique supplémentaire au départ d'une run
        UnlockCharacter,// Débloque un personnage jouable
        Other,
    }

    /// <summary>
    /// ScriptableObject décrivant la récompense obtenue à un niveau de compte donné.
    /// Créer via : Assets > Create > RoguelikeTCG > Account Level Reward
    /// </summary>
    [CreateAssetMenu(fileName = "AccountLevelReward", menuName = "RoguelikeTCG/Account Level Reward")]
    public class AccountLevelReward : ScriptableObject
    {
        [Header("Condition")]
        [Tooltip("Niveau de compte requis pour obtenir cette récompense.")]
        public int requiredLevel = 2;

        [Header("Récompense")]
        public AccountRewardType rewardType = AccountRewardType.None;

        [Tooltip("Carte ajoutée au deck de départ (si rewardType = StartingCard).")]
        public CardData cardReward;

        [Tooltip("Relique offerte au départ d'une run (si rewardType = StartingRelic).")]
        public RelicData relicReward;

        [Tooltip("Description affichée lors du déblocage.")]
        [TextArea(2, 4)]
        public string description;
    }
}
