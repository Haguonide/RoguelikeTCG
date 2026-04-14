using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche la barre d'or + reliques actives en bas de l'écran.
    /// À attacher sur un GameObject enfant du Canvas principal (pas dans le ScrollRect).
    /// Se reconstruit automatiquement à chaque activation.
    /// </summary>
    public class RelicBarUI : MonoBehaviour
    {
        private GameObject _panel;

        private void OnEnable()  => Refresh();
        private void Start()     => Refresh();

        public void Refresh()
        {
            if (_panel != null) Destroy(_panel);

            var persistence = RunPersistence.Instance;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // ── Panneau principal ─────────────────────────────────────────────
            _panel = new GameObject("RelicBar", typeof(RectTransform));
            _panel.transform.SetParent(canvas.transform, false);

            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(1f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(0f, 52f);

            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.05f, 0.02f, 0.88f);
            bg.raycastTarget = false;

            // ── Or ────────────────────────────────────────────────────────────
            int gold = persistence?.PlayerGold ?? 0;
            MakeTMP(_panel, "Gold",
                new Vector2(8f, 0f), new Vector2(110f, 52f),
                $"💰 {gold} or", 16f, new Color(0.95f, 0.82f, 0.35f),
                TextAlignmentOptions.MidlineLeft);

            // ── Reliques ──────────────────────────────────────────────────────
            var relics = persistence?.PlayerRelics ?? new List<RelicData>();

            if (relics.Count == 0)
            {
                MakeTMP(_panel, "NoRelic",
                    new Vector2(120f, 0f), new Vector2(400f, 52f),
                    "Aucune relique", 13f, new Color(0.55f, 0.52f, 0.45f),
                    TextAlignmentOptions.MidlineLeft);
            }
            else
            {
                float x = 120f;
                for (int i = 0; i < relics.Count; i++)
                {
                    var relic = relics[i];
                    if (relic == null) continue;

                    float w = Mathf.Max(120f, relic.relicName.Length * 9f);

                    // Fond pastille
                    var pill = new GameObject($"Relic_{i}", typeof(RectTransform));
                    pill.transform.SetParent(_panel.transform, false);
                    var prt = pill.GetComponent<RectTransform>();
                    prt.anchorMin        = new Vector2(0f, 0f);
                    prt.anchorMax        = new Vector2(0f, 0f);
                    prt.pivot            = new Vector2(0f, 0f);
                    prt.anchoredPosition = new Vector2(x, 6f);
                    prt.sizeDelta        = new Vector2(w, 40f);
                    var pillBg = pill.AddComponent<Image>();
                    pillBg.color = new Color(0.30f, 0.18f, 0.04f, 0.90f);
                    pillBg.raycastTarget = false;

                    MakeTMP(pill, "Name",
                        new Vector2(6f, 0f), new Vector2(w - 6f, 40f),
                        relic.relicName, 13f, Color.white,
                        TextAlignmentOptions.Midline);

                    x += w + 8f;
                }
            }
        }

        private static void MakeTMP(GameObject parent, string name,
            Vector2 offsetMin, Vector2 offsetMax,
            string text, float size, Color color,
            TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.zero;
            rt.pivot            = Vector2.zero;
            rt.anchoredPosition = offsetMin;
            rt.sizeDelta        = offsetMax - offsetMin;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = size;
            tmp.color         = color;
            tmp.alignment     = align;
            tmp.raycastTarget = false;
        }
    }
}
