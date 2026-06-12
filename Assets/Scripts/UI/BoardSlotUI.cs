using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.UI
{
    public class BoardSlotUI : MonoBehaviour
    {
        [Header("Display")]
        public TMP_Text unitNameText;
        public TMP_Text unitAtkText;
        public TMP_Text unitHpText;
        public Image unitArtwork;
        public GameObject emptySlotIndicator;
        public GameObject unitPanel;

        [Header("Slot Identity")]
        public bool isPlayerSlot;
        public bool isTerrain;
        public int slotIndex;

        private static readonly Color _degradedColor = Color.red;
        private static readonly Color _normalColor   = Color.white;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnSlotClicked);
        }

        public void OnSlotClicked()
        {
            var mgr = CardSelectionManager.Instance;
            if (mgr == null) return;

            if (isPlayerSlot)
            {
                if (isTerrain) mgr.OnPlayerTerrainSlotClicked();
                else           mgr.OnPlayerUnitSlotClicked(slotIndex);
            }
            else
            {
                mgr.OnEnemyZoneClicked();
            }
        }

        public void Refresh(CardInstance unit)
        {
            bool occupied = unit != null;

            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(!occupied);
            if (unitPanel          != null) unitPanel.SetActive(occupied);

            if (!occupied) return;

            var data = unit.Data;

            if (unitNameText != null) unitNameText.text = data.cardName;

            if (unitAtkText != null)
            {
                unitAtkText.text  = unit.CurrentATK.ToString();
                unitAtkText.color = unit.CurrentATK < data.atk ? _degradedColor : _normalColor;
            }

            if (unitHpText != null)
            {
                unitHpText.text  = unit.CurrentHP.ToString();
                unitHpText.color = unit.CurrentHP < data.maxHP ? _degradedColor : _normalColor;
            }

            if (unitArtwork != null && data.artwork != null)
                unitArtwork.sprite = data.artwork;
        }
    }
}
