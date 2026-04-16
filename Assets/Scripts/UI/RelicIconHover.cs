using UnityEngine;
using UnityEngine.EventSystems;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Attaché à chaque icône de relique dans la RelicBarUI.
    /// Déclenche l'affichage / masquage du tooltip via RelicTooltipUI.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RelicIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RelicData relic;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (RelicTooltipUI.Instance == null) return;
            RelicTooltipUI.Instance.Show(relic, GetComponent<RectTransform>());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (RelicTooltipUI.Instance == null) return;
            RelicTooltipUI.Instance.Hide();
        }

        private void OnDisable()
        {
            // Masquer la bulle si l'icône est désactivée (refresh du bar)
            if (RelicTooltipUI.Instance != null)
                RelicTooltipUI.Instance.Hide();
        }
    }
}
