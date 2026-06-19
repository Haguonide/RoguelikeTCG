using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Cards;

namespace RoguelikeTCG.UI
{
    public class BoardSlotUI : MonoBehaviour
    {
        [Header("Display")]
        public GameObject emptySlotIndicator;
        public CardView cardViewPrefab;

        [Header("Slot Identity")]
        public bool isPlayerSlot;
        public bool isTerrain;
        public int slotIndex;

        public CardView CurrentView { get; private set; }

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

            // Détruire la vue précédente
            if (CurrentView != null)
            {
                Destroy(CurrentView.gameObject);
                CurrentView = null;
            }

            if (!occupied || cardViewPrefab == null) return;

            // Instancier la CardView et la binder
            CurrentView = Instantiate(cardViewPrefab, transform);
            CurrentView.Bind(unit);
        }
    }
}
