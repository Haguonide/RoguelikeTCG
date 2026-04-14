using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoguelikeTCG.RunMap
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class NodeView : MonoBehaviour
    {
        public RunNode Node { get; private set; }

        private Button             _button;
        private Image              _bg;
        private Image              _icon;
        private TextMeshProUGUI    _label;

        // -------------------------------------------------------
        // Couleurs selon état
        // -------------------------------------------------------
        private static readonly Color ColLocked    = new Color(0.30f, 0.30f, 0.30f, 1f);
        private static readonly Color ColVisited   = new Color(0.12f, 0.40f, 0.12f, 1f);
        private static readonly Color ColAvailable = new Color(0.28f, 0.80f, 0.28f, 1f);

        // -------------------------------------------------------
        // Initialisation
        // -------------------------------------------------------
        public void Setup(RunNode node, Sprite iconSprite)
        {
            Node    = node;
            _button = GetComponent<Button>();
            _bg     = GetComponent<Image>();

            var iconT  = transform.Find("Icon");
            var labelT = transform.Find("Label");
            if (iconT)  _icon  = iconT.GetComponent<Image>();
            if (labelT) _label = labelT.GetComponent<TextMeshProUGUI>();

            if (_icon != null && iconSprite != null) _icon.sprite = iconSprite;

            if (_label != null) _label.text = NodeLabel(node.type);

            _button.onClick.AddListener(OnClick);
            RefreshState();
        }

        // -------------------------------------------------------
        // Rafraîchir la couleur selon l'état courant
        // -------------------------------------------------------
        public void RefreshState()
        {
            if (_bg == null) return;

            switch (Node.state)
            {
                case NodeState.Locked:
                    _bg.color          = ColLocked;
                    _button.interactable = false;
                    break;
                case NodeState.Visited:
                    _bg.color          = ColVisited;
                    _button.interactable = false;
                    break;
                case NodeState.Available:
                    _bg.color          = ColAvailable;
                    _button.interactable = true;
                    break;
            }
        }

        // -------------------------------------------------------
        // Clic
        // -------------------------------------------------------
        private void OnClick() => RunMapManager.Instance?.VisitNode(Node);

        // -------------------------------------------------------
        // Libellé textuel du type de nœud
        // -------------------------------------------------------
        private static string NodeLabel(NodeType type) => type switch
        {
            NodeType.Start   => "DÉPART",
            NodeType.Combat  => "COMBAT",
            NodeType.Elite   => "ELITE",
            NodeType.Boss    => "BOSS",
            NodeType.Event   => "EVENT",
            NodeType.Shop    => "SHOP",
            NodeType.Forge   => "FORGE",
            NodeType.Rest    => "REPOS",
            NodeType.Mystery => "???",
            _                => "?"
        };
    }
}
