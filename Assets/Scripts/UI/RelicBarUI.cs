using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Affiche l'or et les reliques en colonne verticale dans le coin haut-gauche de l'écran.
    /// L'or : icône + montant. Les reliques : icône seule (pas de nom).
    /// Les sprites iconGold et iconRelique sont assignés dans l'Inspector.
    /// </summary>
    public class RelicBarUI : MonoBehaviour
    {
        [Header("Icônes")]
        public Sprite iconGold;
        public Sprite iconRelique;

        private GameObject _panel;

        private void OnEnable() => Refresh();
        private void Start()    => Refresh();

        public void Refresh()
        {
            if (_panel != null) Destroy(_panel);

            var persistence = RunPersistence.Instance;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            int gold   = persistence?.PlayerGold ?? 0;
            var relics = persistence?.PlayerRelics ?? new List<RelicData>();

            const float iconSz   = 128f;  // icône carrée bien visible
            const float rowGap   = 14f;
            const float padX     = 12f;
            const float padY     = 12f;
            const float goldTextH = 40f;  // hauteur du label or (sous l'icône or)
            const float goldFontSz = 36f;

            // Rangée or : icône + texte côte à côte sur une seule ligne plus haute
            const float goldRowH = iconSz;
            const float panelW   = iconSz + 80f + padX * 2f; // icône + texte or + marges

            // Le panneau contient : 1 rangée or + N icônes reliques
            int relicCount = relics.Count;
            int totalRows  = 1 + (relicCount == 0 ? 1 : relicCount);
            float panelH   = padY + totalRows * iconSz + (totalRows - 1) * rowGap + padY;

            // ── Panneau principal ─────────────────────────────────────────────
            _panel = new GameObject("RelicBarPanel", typeof(RectTransform));
            _panel.transform.SetParent(canvas.transform, false);

            var rt = _panel.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(padX, -padY);
            rt.sizeDelta        = new Vector2(panelW, panelH);

            var bg = _panel.AddComponent<Image>();
            bg.color         = Color.clear;
            bg.raycastTarget = false;

            // ── Rangée Or : icône à gauche + montant à droite ─────────────────
            float goldY = -(padY);

            // Icône or
            AddIcon("GoldIcon", iconGold, padX, goldY, iconSz);

            // Texte montant
            var textGO = new GameObject("GoldLabel", typeof(RectTransform));
            textGO.transform.SetParent(_panel.transform, false);
            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin        = new Vector2(0f, 1f);
            trt.anchorMax        = new Vector2(0f, 1f);
            trt.pivot            = new Vector2(0f, 1f);
            trt.anchoredPosition = new Vector2(padX + iconSz + 8f, goldY);
            trt.sizeDelta        = new Vector2(panelW - padX - iconSz - 8f - padX, goldRowH);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text          = gold.ToString();
            tmp.fontSize      = goldFontSz;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.color         = new Color(0.95f, 0.82f, 0.35f);
            tmp.alignment     = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            // ── Lignes Reliques : icône seule ─────────────────────────────────
            if (relicCount == 0)
            {
                // Icône relique grisée si aucune relique
                float y = -(padY + iconSz + rowGap);
                AddIcon("RelicIcon_empty", iconRelique, padX, y, iconSz, new Color(1f, 1f, 1f, 0.30f));
            }
            else
            {
                for (int i = 0; i < relicCount; i++)
                {
                    if (relics[i] == null) continue;
                    float y = -(padY + (i + 1) * (iconSz + rowGap));
                    AddIcon($"RelicIcon_{i}", iconRelique, padX, y, iconSz);
                }
            }
        }

        private void AddIcon(string goName, Sprite sprite, float x, float y, float size,
            Color? tint = null)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_panel.transform, false);
            var irt = go.GetComponent<RectTransform>();
            irt.anchorMin        = new Vector2(0f, 1f);
            irt.anchorMax        = new Vector2(0f, 1f);
            irt.pivot            = new Vector2(0f, 1f);
            irt.anchoredPosition = new Vector2(x, y);
            irt.sizeDelta        = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            img.sprite         = sprite;
            img.preserveAspect = true;
            img.color          = tint ?? Color.white;
            img.raycastTarget  = false;
        }
    }
}
