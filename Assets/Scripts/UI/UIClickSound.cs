using UnityEngine;
using UnityEngine.UI;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Attacher ce composant sur n'importe quel Button pour lui donner un son de clic.
    /// Aucune configuration requise — joue automatiquement "sfx_button_click".
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIClickSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            AudioManager.Instance.PlaySFX("sfx_button_click");
        }
    }
}
