using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.Combat
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class GridLinesDrawer : MaskableGraphic
    {
        [Header("Structure de la grille")]
        [SerializeField] public int rows = 4;
        [SerializeField] public int cols = 4;

        [Header("Apparence des lignes")]
        [SerializeField] public Sprite lineSprite    = null;   // sprite "Line" importé
        [SerializeField] public float  lineWidth     = 2.5f;
        [SerializeField] public Color  lineColor     = new Color(0f, 0f, 0f, 1f);
        [SerializeField] public bool   drawOuterBorder = false;
        [SerializeField] public float  textureTiling = 0.02f; // répétitions de la texture par pixel

        public override Texture mainTexture =>
            lineSprite != null ? lineSprite.texture : base.mainTexture;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (lineSprite != null)
                lineSprite.texture.wrapMode = TextureWrapMode.Repeat;

            Rect rect = rectTransform.rect;

            // Calcule le rect UV du sprite dans l'atlas
            Rect uvRect = lineSprite != null
                ? new Rect(
                    lineSprite.textureRect.x      / lineSprite.texture.width,
                    lineSprite.textureRect.y      / lineSprite.texture.height,
                    lineSprite.textureRect.width  / lineSprite.texture.width,
                    lineSprite.textureRect.height / lineSprite.texture.height)
                : new Rect(0, 0, 1, 1);

            int vStart = drawOuterBorder ? 0 : 1;
            int vEnd   = drawOuterBorder ? cols : cols - 1;

            for (int i = vStart; i <= vEnd; i++)
            {
                float x = rect.x + i * (rect.width / cols);
                DrawLine(vh,
                    new Vector2(x, rect.y),
                    new Vector2(x, rect.yMax),
                    uvRect);
            }

            int hStart = drawOuterBorder ? 0 : 1;
            int hEnd   = drawOuterBorder ? rows : rows - 1;

            for (int j = hStart; j <= hEnd; j++)
            {
                float y = rect.y + j * (rect.height / rows);
                DrawLine(vh,
                    new Vector2(rect.x,    y),
                    new Vector2(rect.xMax, y),
                    uvRect);
            }
        }

        private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Rect uvRect)
        {
            Vector2 dir    = end - start;
            float   length = dir.magnitude;
            if (length < 0.001f) return;

            Vector2 unit = dir / length;
            Vector2 perp = new Vector2(-unit.y, unit.x) * (lineWidth * 0.5f);

            // U tile le long du trait, V couvre la hauteur du sprite
            float uStart = uvRect.xMin;
            float uEnd   = uvRect.xMin + uvRect.width * (length * textureTiling);
            float vBot   = uvRect.yMin;
            float vTop   = uvRect.yMax;

            int baseIndex = vh.currentVertCount;

            AddVert(vh, start - perp, lineColor, new Vector2(uStart, vBot));
            AddVert(vh, start + perp, lineColor, new Vector2(uStart, vTop));
            AddVert(vh, end   + perp, lineColor, new Vector2(uEnd,   vTop));
            AddVert(vh, end   - perp, lineColor, new Vector2(uEnd,   vBot));

            vh.AddTriangle(baseIndex + 0, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex + 0, baseIndex + 2, baseIndex + 3);
        }

        private static void AddVert(VertexHelper vh, Vector2 pos, Color color, Vector2 uv)
        {
            var vert = UIVertex.simpleVert;
            vert.position = new Vector3(pos.x, pos.y, 0f);
            vert.color    = color;
            vert.uv0      = uv;
            vh.AddVert(vert);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            rows         = Mathf.Max(1, rows);
            cols         = Mathf.Max(1, cols);
            lineWidth    = Mathf.Max(0.5f, lineWidth);
            textureTiling = Mathf.Max(0.001f, textureTiling);
            SetVerticesDirty();
        }
#endif
    }
}
