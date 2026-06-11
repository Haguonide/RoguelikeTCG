using UnityEngine;

namespace RoguelikeTCG.Data
{
    public enum EventEffectType
    {
        Nothing,
        GainGold,
        LoseGold,
        GainHP,
        LoseHP,
        GainCard,
        LoseCard,    // retirer une carte du deck (comme Repos)
        GainRelic,
        GainRunes,
    }

    [System.Serializable]
    public class EventChoice
    {
        [TextArea(1, 2)]
        public string choiceText;
        [TextArea(1, 2)]
        public string resultText;
        public EventEffectType effectType;
        public int effectValue;
        public CardData cardReward;      // utilisé si effectType == GainCard
        public RelicData relicReward;    // utilisé si effectType == GainRelic
    }

    [CreateAssetMenu(fileName = "NewEvent", menuName = "RoguelikeTCG/Event")]
    public class EventData : ScriptableObject
    {
        [Header("Narrative")]
        public string eventTitle;
        [TextArea(3, 6)]
        public string eventDescription;
        public Sprite illustration;

        [Header("Choices")]
        public EventChoice[] choices;
    }
}
