using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    public class HexBackgroundScroller : MonoBehaviour
    {
        [Header("Sprites")]
        public Sprite hexSprite;
        public Sprite[] patternSprites;

        [Header("Grid")]
        public float hexSize = 64f;

        [Header("Visual")]
        [Range(0f, 1f)] public float hexOpacity = 0.08f;
        [Range(0f, 1f)] public float patternOpacity = 0.18f;
        public Color tintColor = Color.white;

        [Header("Scroll")]
        public float scrollSpeed = 50f;

        private float _hexW;
        private float _rowH;
        private Vector2 _velocity;
        private readonly List<RectTransform> _rows = new List<RectTransform>();
        private int _topRowIndex;
        private float _screenW;
        private float _screenH;
        private int _colsHalf;

        void Start()
        {
            _hexW = hexSize * Mathf.Sqrt(3f);
            _rowH = hexSize * 1.5f;

            float periodX = 2f * _hexW;
            float periodY = 2f * _rowH;
            _velocity = new Vector2(-periodX, -periodY).normalized * scrollSpeed;

            var rt = GetComponent<RectTransform>();
            _screenW = rt.rect.width > 0f ? rt.rect.width : Screen.width;
            _screenH = rt.rect.height > 0f ? rt.rect.height : Screen.height;

            // Chaque ligne doit être assez large pour couvrir l'écran
            // même après tout le déplacement horizontal accumulé pendant sa traversée
            float diagonalTravel = Mathf.Abs(_velocity.x / _velocity.y) * (_screenH + _rowH * 4f);
            _colsHalf = Mathf.CeilToInt((_screenW * 0.5f + diagonalTravel) / _hexW) + 2;

            int rowCount = Mathf.CeilToInt(_screenH / _rowH) + 4;
            for (int i = 0; i < rowCount; i++)
            {
                float y = -_screenH * 0.5f - _rowH + i * _rowH;
                _rows.Add(SpawnRow(i, y));
            }
            _topRowIndex = rowCount - 1;
        }

        RectTransform SpawnRow(int rowIndex, float yPos)
        {
            // Hérite du décalage X courant pour s'aligner avec les lignes existantes
            float currentX = _rows.Count > 0 ? _rows[0].anchoredPosition.x : 0f;

            var go = new GameObject($"HexRow_{rowIndex}");
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(currentX, yPos);
            rt.sizeDelta = Vector2.zero;

            bool isOddRow = ((rowIndex % 2) + 2) % 2 == 1;
            float xOffset = isOddRow ? _hexW * 0.5f : 0f;

            for (int col = -_colsHalf; col <= _colsHalf; col++)
                SpawnHex(rt, rowIndex, col, xOffset);

            return rt;
        }

        void SpawnHex(RectTransform rowRt, int rowIndex, int col, float xOffset)
        {
            var go = new GameObject($"Hex_{col}");
            go.transform.SetParent(rowRt, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(col * _hexW + xOffset, 0f);
            rt.sizeDelta = new Vector2(_hexW, hexSize * 2f);

            var img = go.AddComponent<Image>();
            img.sprite = hexSprite;
            img.raycastTarget = false;
            img.color = new Color(tintColor.r, tintColor.g, tintColor.b, hexOpacity);
            img.preserveAspect = false;

            bool hasPattern = (rowIndex + col) % 2 == 0
                && patternSprites != null && patternSprites.Length > 0;

            if (!hasPattern) return;

            int rMod = ((rowIndex % 2) + 2) % 2;
            int cMod = ((col % 2) + 2) % 2;
            int spriteIdx = (rMod * 2 + cMod) % patternSprites.Length;

            var patGO = new GameObject("Pat");
            patGO.transform.SetParent(go.transform, false);

            var patRt = patGO.AddComponent<RectTransform>();
            patRt.anchorMin = new Vector2(0.15f, 0.15f);
            patRt.anchorMax = new Vector2(0.85f, 0.85f);
            patRt.offsetMin = Vector2.zero;
            patRt.offsetMax = Vector2.zero;

            var patImg = patGO.AddComponent<Image>();
            patImg.sprite = patternSprites[spriteIdx];
            patImg.raycastTarget = false;
            patImg.color = new Color(tintColor.r, tintColor.g, tintColor.b, patternOpacity);
            patImg.preserveAspect = true;
        }

        void Update()
        {
            if (_rows.Count == 0) return;

            Vector2 delta = _velocity * Time.deltaTime;
            foreach (var row in _rows)
                row.anchoredPosition += delta;

            // Détruire les lignes sorties par le bas (hors écran, invisibles)
            while (_rows.Count > 0 && _rows[0].anchoredPosition.y < -_screenH * 0.5f - _rowH * 2f)
            {
                Destroy(_rows[0].gameObject);
                _rows.RemoveAt(0);
            }

            // Ajouter de nouvelles lignes en haut dès qu'il y a un manque
            // Elles sont créées hors écran — jamais visibles au moment du spawn
            while (_rows.Count > 0 && _rows[_rows.Count - 1].anchoredPosition.y < _screenH * 0.5f + _rowH * 2f)
            {
                _topRowIndex++;
                float newY = _rows[_rows.Count - 1].anchoredPosition.y + _rowH;
                _rows.Add(SpawnRow(_topRowIndex, newY));
            }
        }

        public void Clear()
        {
            foreach (var row in _rows)
                if (row) Destroy(row.gameObject);
            _rows.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
