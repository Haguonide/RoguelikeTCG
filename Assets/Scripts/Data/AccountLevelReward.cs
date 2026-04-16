using UnityEngine;

namespace RoguelikeTCG.Data
{
    /// <summary>
    /// Types de récompenses débloquables par le niveau de compte.
    /// </summary>
    public enum AccountRewardType
    {
        None,
        StartingCard,       // Ajoute une carte au deck de départ d'un personnage
        StartingRelic,      // Offre une relique au départ de chaque run
        UnlockCardInPool,   // Ajoute une carte épique au cardPool du personnage (récompenses de combat)
        UnlockCharacter,    // Débloque un personnage jouable
        Other,
    }

    /// <summary>
    /// ScriptableObject décrivant la récompense obtenue à un niveau de compte donné.
    /// Placer dans Assets/Resources/LevelRewards/ pour chargement automatique.
    /// </summary>
    [CreateAssetMenu(fileName = "AccountLevelReward", menuName = "RoguelikeTCG/Account Level Reward")]
    public class AccountLevelReward : ScriptableObject
    {
        [Header("Condition")]
        [Tooltip("Niveau de compte requis pour obtenir cette récompense.")]
        public int requiredLevel = 2;

        [Tooltip("Personnage concerné. Null = récompense universelle.")]
        public CharacterData characterFilter;

        [Header("Récompense")]
        public AccountRewardType rewardType = AccountRewardType.None;

        [Tooltip("Carte concernée (StartingCard ou UnlockCardInPool).")]
        public CardData cardReward;

        [Tooltip("Relique offerte au départ d'une run (StartingRelic).")]
        public RelicData relicReward;

        [Tooltip("Description affichée lors du déblocage.")]
        [TextArea(2, 4)]
        public string description;
    }
}
