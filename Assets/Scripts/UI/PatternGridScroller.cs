using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    public class PatternGridScroller : MonoBehaviour
    {
        [Header("Sprite")]
        public Sprite tileSprite;
        [Tooltip("Si rempli, chaque tuile reçoit un sprite déterministe parmi ceux-ci.")]
        public Sprite[] randomSprites;

        [Header("Grid")]
        public float tileSize = 80f;
        public float tileGap = 4f;

        [Header("Visual")]
        [Range(0f, 1f)] public float opacity = 0.12f;
        public Color tintColor = Color.white;

        [Header("Scroll")]
        public float scrollSpeed = 40f;
        [Tooltip("Décale les lignes impaires de tileSize*0.5 pour un aspect brique.")]
        public bool alternateRowOffset = false;

        float _cellSize;
        float _screenW, _screenH;
        int _colCount, _rowCount;

        // (RectTransform de la colonne, son index logique pour les sprites)
        readonly List<(RectTransform rt, int colIndex)> _cols = new List<(RectTransform, int)>();
        int _nextColIndex;

        void Start()
        {
            _cellSize = tileSize + tileGap;

            var rt = GetComponent<RectTransform>();
            _screenW = rt.rect.width  > 0f ? rt.rect.width  : Screen.width;
            _screenH = rt.rect.height > 0f ? rt.rect.height : Screen.height;

            _rowCount = Mathf.CeilToInt(_screenH / _cellSize) + 2;
            // +3 : 1 colonne de buffer à gauche, 2 à droite pour éviter tout vide
            _colCount = Mathf.CeilToInt(_screenW / _cellSize) + 3;

            for (int col = 0; col < _colCount; col++)
            {
                // La première colonne démarre 1 cellSize hors écran à gauche
                float x = -_screenW * 0.5f - _cellSize + col * _cellSize;
                _cols.Add(SpawnColumn(_nextColIndex++, x));
            }
        }

        (RectTransform, int) SpawnColumn(int colIndex, float xPos)
        {
            var go = new GameObject($"GridCol_{colIndex}");
            go.transform.SetParent(transform, false);

            var colRt = go.AddComponent<RectTransform>();
            colRt.anchorMin = new Vector2(0.5f, 0.5f);
            colRt.anchorMax = new Vector2(0.5f, 0.5f);
            colRt.pivot     = new Vector2(0.5f, 0.5f);
            colRt.anchoredPosition = new Vector2(xPos, 0f);
            colRt.sizeDelta = Vector2.zero;

            for (int row = 0; row < _rowCount; row++)
            {
                float brickX = (alternateRowOffset && row % 2 == 1) ? _cellSize * 0.5f : 0f;
                float y = -_screenH * 0.5f + row * _cellSize;
                SpawnTile(colRt, colIndex, row, brickX, y);
            }

            return (colRt, colIndex);
        }

        void SpawnTile(RectTransform colRt, int colIndex, int rowIndex, float localX, float localY)
        {
            var go = new GameObject($"Tile_{rowIndex}");
            go.transform.SetParent(colRt, false);

            var tileRt = go.AddComponent<RectTransform>();
            tileRt.anchorMin = new Vector2(0.5f, 0.5f);
            tileRt.anchorMax = new Vector2(0.5f, 0.5f);
            tileRt.pivot     = new Vector2(0.5f, 0.5f);
            tileRt.anchoredPosition = new Vector2(localX, localY);
            tileRt.sizeDelta = new Vector2(tileSize, tileSize);

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(tintColor.r, tintColor.g, tintColor.b, opacity);
            img.preserveAspect = false;

            if (randomSprites != null && randomSprites.Length > 0)
                img.sprite = randomSprites[Mathf.Abs(rowIndex * 1000 + colIndex) % randomSprites.Length];
            else
                img.sprite = tileSprite;
        }

        void Update()
        {
            if (_cols.Count == 0) return;

            float delta     = scrollSpeed * Time.deltaTime;
            float rightEdge = _screenW * 0.5f + _cellSize;

            // Trouver la colonne la plus à gauche AVANT de déplacer (pour le recyclage)
            float minX = float.MaxValue;
            foreach (var (rt, _) in _cols)
                if (rt != null && rt.anchoredPosition.x < minX)
                    minX = rt.anchoredPosition.x;

            bool recycled = false;
            for (int i = 0; i < _cols.Count; i++)
            {
                var (colRt, colIndex) = _cols[i];
                if (colRt == null) continue;

                var pos = colRt.anchoredPosition;
                pos.x += delta;

                // Recycle : la colonne sort à droite → on la remet juste à gauche de la plus à gauche
                if (pos.x > rightEdge)
                {
                    pos.x = minX - _cellSize;
                    minX  = pos.x; // la nouvelle min est celle qu'on vient de placer
                    recycled = true;
                }

                colRt.anchoredPosition = pos;
            }

            _ = recycled; // supprime le warning "unused"
        }

        public void Clear()
        {
            foreach (var (rt, _) in _cols)
                if (rt != null) Destroy(rt.gameObject);
            _cols.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
