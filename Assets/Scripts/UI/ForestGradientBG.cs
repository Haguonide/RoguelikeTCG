using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class ForestGradientBG : MonoBehaviour
    {
        [SerializeField] private int textureWidth = 4;
        [SerializeField] private int textureHeight = 256;

        private Gradient _gradient;
        private Texture2D _tex;

        private void Awake()
        {
            BuildGradient();
            Apply();
        }

        private void BuildGradient()
        {
            _gradient = new Gradient();

            var colors = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.04f, 0.13f, 0.07f), 0.00f), // bas : sous-bois très sombre
                new GradientColorKey(new Color(0.07f, 0.24f, 0.12f), 0.22f), // sol forêt profonde
                new GradientColorKey(new Color(0.13f, 0.38f, 0.20f), 0.45f), // vert forêt riche
                new GradientColorKey(new Color(0.22f, 0.54f, 0.32f), 0.68f), // vert feuillage
                new GradientColorKey(new Color(0.42f, 0.72f, 0.50f), 0.85f), // canopée lumineuse
                new GradientColorKey(new Color(0.62f, 0.86f, 0.70f), 1.00f), // ciel filtré vert clair
            };

            var alphas = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            };

            _gradient.SetKeys(colors, alphas);
        }

        private void Apply()
        {
            _tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            for (int y = 0; y < textureHeight; y++)
            {
                float t = y / (float)(textureHeight - 1);
                Color c = _gradient.Evaluate(t);
                for (int x = 0; x < textureWidth; x++)
                    _tex.SetPixel(x, y, c);
            }

            _tex.Apply();

            var raw = GetComponent<RawImage>();
            raw.texture = _tex;
            raw.color = Color.white;
        }

        private void OnDestroy()
        {
            if (_tex != null) Destroy(_tex);
        }
    }
}
