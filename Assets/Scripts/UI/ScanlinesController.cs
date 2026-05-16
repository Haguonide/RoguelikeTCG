using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    [ExecuteAlways]
    public sealed class ScanlinesController : MonoBehaviour
    {
        [Header("Parameters")]
        [Range(0f, 1f)]
        public float intensity = 0.15f;

        [Range(10f, 500f)]
        public float lineFrequency = 120f;

        static readonly int s_Intensity  = Shader.PropertyToID("_Intensity");
        static readonly int s_LineCount  = Shader.PropertyToID("_LineCount");

        Material _mat;

        void OnEnable() => FindMaterial();

        void OnValidate() => ApplyToMaterial();

        void Update()
        {
            if (_mat == null) FindMaterial();
            ApplyToMaterial();
        }

        void FindMaterial()
        {
            var overlay = GameObject.Find("ScanlinesOverlay");
            if (overlay == null) return;
            var img = overlay.GetComponent<RawImage>();
            if (img != null) _mat = img.material;
        }

        void ApplyToMaterial()
        {
            if (_mat == null) return;
            _mat.SetFloat(s_Intensity, intensity);
            _mat.SetFloat(s_LineCount, Mathf.Max(1f, lineFrequency));
        }

        public void SetIntensity(float value)     => intensity     = Mathf.Clamp01(value);
        public void SetLineFrequency(float value) => lineFrequency = Mathf.Max(1f, value);
    }
}
