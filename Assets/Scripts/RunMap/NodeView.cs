using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RoguelikeTCG.RunMap
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class NodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RunNode Node { get; private set; }

        private Button          _button;
        private Image           _bg;
        private Image           _icon;
        private Outline         _iconOutline;

        private static readonly Color OutlineColor = new Color(1f, 0.85f, 0.1f, 1f);

        // -------------------------------------------------------
        // Initialisation
        // -------------------------------------------------------
        public void Setup(RunNode node, Sprite iconSprite)
        {
            Node    = node;
            _button = GetComponent<Button>();
            _bg     = GetComponent<Image>();

            var iconT = transform.Find("Icon");
            if (iconT) _icon = iconT.GetComponent<Image>();

            if (_icon != null && iconSprite != null) _icon.sprite = iconSprite;

            // Outline sur l'icône, masqué par défaut
            if (_icon != null)
            {
                _iconOutline = _icon.GetComponent<Outline>() ?? _icon.gameObject.AddComponent<Outline>();
                _iconOutline.effectColor    = OutlineColor;
                _iconOutline.effectDistance = new Vector2(3f, -3f);
                _iconOutline.enabled        = false;
            }

            _button.onClick.AddListener(OnClick);
            RefreshState();
        }

        // -------------------------------------------------------
        // Rafraîchir l'état (interactivité uniquement — bg toujours transparent)
        // -------------------------------------------------------
        public void RefreshState()
        {
            if (_bg == null) return;

            // Cercle invisible — l'icône seule porte l'information visuelle
            _bg.color = Color.clear;

            switch (Node.state)
            {
                case NodeState.Locked:
                case NodeState.Visited:
                    _button.interactable = false;
                    if (_iconOutline != null) _iconOutline.enabled = false;
                    break;
                case NodeState.Available:
                    _button.interactable = true;
                    break;
            }
        }

        // -------------------------------------------------------
        // Hover
        // -------------------------------------------------------
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_iconOutline != null && Node?.state == NodeState.Available)
                _iconOutline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_iconOutline != null)
                _iconOutline.enabled = false;
        }

        // -------------------------------------------------------
        // Clic
        // -------------------------------------------------------
        private void OnClick() => RunMapManager.Instance?.VisitNode(Node);

    }
}
