using System.Collections.Generic;
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
        // Rafraîchir l'état (interactivité + couleur icône)
        // -------------------------------------------------------
        public void RefreshState()
        {
            if (_bg == null) return;

            _bg.color = Color.clear;
            if (_iconOutline != null) _iconOutline.enabled = false;

            switch (Node.state)
            {
                case NodeState.Locked:
                    _button.interactable = false;
                    if (_icon != null)
                        _icon.color = IsPermanentlyInaccessible()
                            ? new Color(0.35f, 0.35f, 0.35f, 0.4f)
                            : Color.white;
                    break;
                case NodeState.Visited:
                    _button.interactable = false;
                    if (_icon != null) _icon.color = Color.white;
                    break;
                case NodeState.Available:
                    _button.interactable = true;
                    if (_icon != null) _icon.color = Color.white;
                    break;
            }
        }

        private bool IsPermanentlyInaccessible()
        {
            if (Node.state != NodeState.Locked) return false;
            if (Node.parents.Count == 0) return false;

            // BFS vers le haut : le nœud est accessible si AU MOINS UN ancêtre est Available.
            // On s'arrête sur les nœuds Visited (chemin fermé) et on ne traverse pas à travers eux.
            var seen  = new HashSet<RunNode>();
            var queue = new Queue<RunNode>(Node.parents);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!seen.Add(current)) continue;
                if (current.state == NodeState.Available) return false;
                if (current.state == NodeState.Locked)
                    foreach (var p in current.parents)
                        queue.Enqueue(p);
                // Visited = chemin choisi et fermé, on ne remonte pas plus loin
            }
            return true;
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
