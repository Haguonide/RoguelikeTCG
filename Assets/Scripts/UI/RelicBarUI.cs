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

        public static RelicBarUI Instance { get; private set; }

        private GameObject _panel;
        private GameObject _hpPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void OnEnable() => Refresh();
        private void Start()    => Refresh();

        public void Refresh()
        {
            if (_panel   != null) Destroy(_panel);
            if (_hpPanel != null) Destroy(_hpPanel);

            var persistence = RunPersistence.Instance;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            int gold   = persistence?.PlayerGold ?? 0;
            var relics = persistence?.PlayerRelics ?? new List<RelicData>();

            // HP display — top-right
            int rawHP = persistence?.PlayerHP    ?? 0;
            int maxHP = persistence?.PlayerMaxHP ?? 80;
            int hp    = Mathf.Clamp(rawHP, 0, maxHP);

            _hpPanel = new GameObject("HPDisplay", typeof(RectTransform));
            _hpPanel.transform.SetParent(canvas.transform, false);
            var hpRT = _hpPanel.GetComponent<RectTransform>();
            hpRT.anchorMin        = new Vector2(1f, 1f);
            hpRT.anchorMax        = new Vector2(1f, 1f);
            hpRT.pivot            = new Vector2(1f, 1f);
            hpRT.anchoredPosition = new Vector2(-12f, -12f);
            hpRT.sizeDelta        = new Vector2(220f, 60f);
            var hpTMP = _hpPanel.AddComponent<TextMeshProUGUI>();
            hpTMP.text          = $"HP {hp} / {maxHP}";
            hpTMP.fontSize      = 36f;
            hpTMP.fontStyle     = FontStyles.Bold;
            hpTMP.color         = new Color(0.95f, 0.25f, 0.25f);
            hpTMP.alignment     = TextAlignmentOptions.MidlineRight;
            hpTMP.raycastTarget = false;

            const float iconSz   = 128f;  // icône carrée bien visible
            const float rowGap   = 14f;
            const float padX     = 12f;
            const float padY     = 12f;

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
                    var iconGO = AddIcon($"RelicIcon_{i}", iconRelique, padX, y, iconSz, hoverable: true);
                    var hover  = iconGO.AddComponent<RelicIconHover>();
                    hover.relic = relics[i];
                }
            }
        }

        private GameObject AddIcon(string goName, Sprite sprite, float x, float y, float size,
            Color? tint = null, bool hoverable = false)
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
            img.raycastTarget  = hoverable; // true uniquement pour les reliques interactives
            return go;
        }
    }
}
