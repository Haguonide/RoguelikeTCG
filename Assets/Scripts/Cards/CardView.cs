using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Cards
{
    public class CardView : MonoBehaviour
    {
        [Header("Identity")]
        public TMP_Text nameText;
        public TMP_Text costText;
        public TMP_Text descText;

        [Header("Artwork")]
        public Image artworkImage;
        public Image cardFrameImage;
        public Image elementIcon;

        [Header("Unit stats")]
        public GameObject atkHpBlock;
        public TMP_Text atkText;
        public TMP_Text hpText;

        [Header("Terrain")]
        public GameObject terrainMissionBlock;
        public TMP_Text missionText;
        public TMP_Text rewardText;

        [Header("Ephemeral")]
        public GameObject ephemeralBadge;

        private static readonly Color _degradedColor = Color.red;
        private static readonly Color _normalColor   = Color.white;

        public CardInstance BoundInstance { get; private set; }

        public void Bind(CardInstance instance)
        {
            BoundInstance = instance;
            var data = instance.Data;

            if (nameText  != null) nameText.text = data.cardName;
            if (costText  != null) costText.text = instance.CurrentCost.ToString();
            if (descText  != null) descText.text = data.description;
            if (artworkImage != null && data.artwork != null) artworkImage.sprite = data.artwork;

            bool isUnit    = data.cardType == CardType.Unit;
            bool isTerrain = data.cardType == CardType.Terrain;

            if (atkHpBlock != null) atkHpBlock.SetActive(isUnit);
            if (terrainMissionBlock != null) terrainMissionBlock.SetActive(isTerrain);
            if (ephemeralBadge != null) ephemeralBadge.SetActive(instance.IsEphemeral);

            if (isUnit)
            {
                if (atkText != null)
                {
                    atkText.text = instance.CurrentATK.ToString();
                    atkText.color = instance.CurrentATK < data.atk ? _degradedColor : _normalColor;
                }
                if (hpText != null)
                {
                    hpText.text = instance.CurrentHP.ToString();
                    hpText.color = instance.CurrentHP < data.maxHP ? _degradedColor : _normalColor;
                }
            }

            if (isTerrain)
            {
                if (missionText != null) missionText.text = data.missionText;
                if (rewardText  != null) rewardText.text  = data.rewardText;
            }
        }
    }
}
