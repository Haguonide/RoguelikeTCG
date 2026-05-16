using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    [ExecuteAlways]
    public class CircleGridScroller : MonoBehaviour
    {
        [Header("Grid")]
        public float circlesPerHeight = 6f;
        [Range(0.1f, 0.9f)] public float circleRadius = 0.54f;

        [Header("Colors")]
        public Color circleColor = new Color(0.18f, 0.48f, 0.52f, 1f);
        public Color bgColor     = new Color(0.10f, 0.32f, 0.36f, 1f);

        [Header("Scroll")]
        public float scrollSpeed = 0.5f;

        private Material _mat;
        private Vector2  _offset;

        void Awake() => SetupMaterial();

        void SetupMaterial()
        {
            var shader = Shader.Find("RoguelikeTCG/CircleGridBG");
            if (shader == null)
            {
                Debug.LogWarning("[CircleGridScroller] Shader 'RoguelikeTCG/CircleGridBG' introuvable.");
                return;
            }

            if (_mat != null) DestroyImmediate(_mat);
            _mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            var img = GetComponent<RawImage>();
            if (img == null) img = gameObject.AddComponent<RawImage>();
            img.material      = _mat;
            img.raycastTarget = false;
            img.color         = Color.white;

            ApplyProperties();
        }

        void ApplyProperties()
        {
            if (_mat == null) return;
            _mat.SetFloat("_Scale",  circlesPerHeight);
            _mat.SetFloat("_Radius", circleRadius);
            _mat.SetColor("_CircleColor", circleColor);
            _mat.SetColor("_BGColor",     bgColor);
        }

        void Update()
        {
            if (_mat == null) return;
            if (!Application.isPlaying) return;

            ApplyProperties();

            // Direction diagonale bas-gauche — même sens que HexBackgroundScroller
            _offset += new Vector2(-1f, -1f).normalized * scrollSpeed * Time.deltaTime;

            // Wrap pour éviter l'overflow float sur de longues sessions
            _offset.x = Mathf.Repeat(_offset.x, 1000f);
            _offset.y = Mathf.Repeat(_offset.y, 1000f);

            _mat.SetFloat("_OffsetX", _offset.x);
            _mat.SetFloat("_OffsetY", _offset.y);
        }

        void OnValidate()
        {
            if (_mat == null) SetupMaterial();
            else ApplyProperties();
        }

        void OnDestroy()
        {
            if (_mat != null) DestroyImmediate(_mat);
        }
    }
}
