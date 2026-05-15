using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.UI
{
    public class BackgroundScatterer : MonoBehaviour
    {
        public Sprite[] sprites;
        public int count = 50;
        public float minScale = 0.8f;
        public float maxScale = 2.0f;
        [Range(0f, 1f)] public float opacity = 0.12f;
        public float minDistance = 90f;
        public Vector2 iconSize = new Vector2(96f, 96f);

        public void Generate()
        {
            Clear();

            var rt = GetComponent<RectTransform>();
            float w = rt.rect.width;
            float h = rt.rect.height;

            var placed = new List<Vector2>();
            int spawned = 0;
            int attempts = 0;
            int maxAttempts = count * 30;

            while (spawned < count && attempts < maxAttempts)
            {
                attempts++;

                var pos = new Vector2(
                    Random.Range(-w / 2f, w / 2f),
                    Random.Range(-h / 2f, h / 2f)
                );

                bool tooClose = false;
                foreach (var p in placed)
                {
                    if (Vector2.Distance(pos, p) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                placed.Add(pos);
                spawned++;

                var go = new GameObject("BG_" + spawned);
                go.transform.SetParent(transform, false);

                var childRt = go.AddComponent<RectTransform>();
                childRt.anchoredPosition = pos;
                childRt.sizeDelta = iconSize;

                var img = go.AddComponent<Image>();
                img.sprite = sprites[Random.Range(0, sprites.Length)];
                img.raycastTarget = false;
                img.color = new Color(1f, 1f, 1f, opacity);

                float scale = Random.Range(minScale, maxScale);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }

            Debug.Log($"[BackgroundScatterer] {spawned} icônes placées ({attempts} tentatives)");
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
