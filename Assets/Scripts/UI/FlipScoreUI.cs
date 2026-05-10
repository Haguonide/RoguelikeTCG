using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace RoguelikeTCG.UI
{
    /// <summary>
    /// Tableau de score flip-scoreboard (style gymnase/stade) — animation DOTween.
    /// Deux panneaux (joueur gauche, ennemi droite), construits procéduralement.
    /// </summary>
    public class FlipScoreUI : MonoBehaviour
    {
        [Header("Panneau joueur (gauche)")]
        [SerializeField] private RectTransform playerPanel;
        [Header("Panneau ennemi (droite)")]
        [SerializeField] private RectTransform enemyPanel;

        private static readonly Color BgFrame    = new Color(0.10f, 0.08f, 0.05f, 1f); // bois brun foncé
        private static readonly Color BgDigits   = new Color(0.04f, 0.04f, 0.04f, 1f); // ardoise
        private static readonly Color ColorTitle = new Color(0.85f, 0.70f, 0.20f, 1f); // or
        private static readonly Color ColorDigit = new Color(0.92f, 0.88f, 0.78f, 1f); // crème
        private static readonly Color ColorSep   = new Color(0.50f, 0.42f, 0.15f, 1f); // or foncé

        private RectTransform   _playerDigitRT,  _enemyDigitRT;
        private TextMeshProUGUI _playerDigitTMP, _enemyDigitTMP;
        private int             _playerScore = -1, _enemyScore = -1;
        private bool            _built;

        private void Awake()
        {
            if (playerPanel != null) BuildPanel(playerPanel, "VOUS", out _playerDigitRT, out _playerDigitTMP);
            if (enemyPanel  != null) BuildPanel(enemyPanel,  "ENN.", out _enemyDigitRT,  out _enemyDigitTMP);
            _built = true;
        }

        private void BuildPanel(RectTransform panel, string title,
            out RectTransform digitRT, out TextMeshProUGUI digitTMP)
        {
            // Nettoie les enfants stale sérialisés depuis une session Play mode précédente
            for (int i = panel.childCount - 1; i >= 0; i--)
                DestroyImmediate(panel.GetChild(i).gameObject);

            // Cadre (bois foncé)
            var frameImg = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            frameImg.color = BgFrame;

            // Titre
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(panel, false);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.05f, 0.56f);
            titleRT.anchorMax = new Vector2(0.95f, 1f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;
            var titleLabel = titleGO.AddComponent<TextMeshProUGUI>();
            titleLabel.text      = title;
            titleLabel.fontSize  = 8f;
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.alignment = TextAlignmentOptions.Center;
            titleLabel.color     = ColorTitle;
            titleLabel.raycastTarget = false;

            // Séparateur
            var sepGO = new GameObject("Sep", typeof(RectTransform));
            sepGO.transform.SetParent(panel, false);
            var sepRT = sepGO.GetComponent<RectTransform>();
            sepRT.anchorMin = new Vector2(0.05f, 0.53f);
            sepRT.anchorMax = new Vector2(0.95f, 0.56f);
            sepRT.offsetMin = sepRT.offsetMax = Vector2.zero;
            var sepImg = sepGO.AddComponent<Image>();
            sepImg.color = ColorSep;
            sepImg.raycastTarget = false;

            // Zone chiffres (c'est elle qui flip)
            var digitGO = new GameObject("DigitArea", typeof(RectTransform));
            digitGO.transform.SetParent(panel, false);
            digitRT = digitGO.GetComponent<RectTransform>();
            digitRT.anchorMin = new Vector2(0.05f, 0.05f);
            digitRT.anchorMax = new Vector2(0.95f, 0.53f);
            digitRT.offsetMin = digitRT.offsetMax = Vector2.zero;
            var digitBg = digitGO.AddComponent<Image>();
            digitBg.color = BgDigits;
            digitBg.raycastTarget = false;
            // DigitText : enfant séparé (Image + TMP ne coexistent pas sur le même GO)
            var digitTextGO = new GameObject("DigitText", typeof(RectTransform));
            digitTextGO.transform.SetParent(digitGO.transform, false);
            var digitTextRT = digitTextGO.GetComponent<RectTransform>();
            digitTextRT.anchorMin = Vector2.zero;
            digitTextRT.anchorMax = Vector2.one;
            digitTextRT.offsetMin = digitTextRT.offsetMax = Vector2.zero;
            digitTMP = digitTextGO.AddComponent<TextMeshProUGUI>();
            digitTMP.text      = "00";
            digitTMP.fontSize  = 20f;
            digitTMP.fontStyle = FontStyles.Bold;
            digitTMP.alignment = TextAlignmentOptions.Center;
            digitTMP.color     = ColorDigit;
            digitTMP.raycastTarget = false;
        }

        // ── API publique ──────────────────────────────────────────────────────

        public void SetScores(int playerScore, int enemyScore)
        {
            if (!_built) return;
            AnimateScore(playerScore, ref _playerScore, _playerDigitRT, _playerDigitTMP);
            AnimateScore(enemyScore,  ref _enemyScore,  _enemyDigitRT,  _enemyDigitTMP);
        }

        public void ResetDisplay()
        {
            _playerScore = _enemyScore = -1;
            if (_playerDigitTMP != null) _playerDigitTMP.text = "00";
            if (_enemyDigitTMP  != null) _enemyDigitTMP.text  = "00";
            if (_playerDigitRT  != null) { _playerDigitRT.DOKill(); _playerDigitRT.localScale = Vector3.one; }
            if (_enemyDigitRT   != null) { _enemyDigitRT.DOKill();  _enemyDigitRT.localScale  = Vector3.one; }
        }

        // ── Interne ───────────────────────────────────────────────────────────

        private void AnimateScore(int newScore, ref int current, RectTransform digitRT, TextMeshProUGUI digitTMP)
        {
            if (digitRT == null || digitTMP == null || newScore == current) return;

            int prev = current;
            current  = newScore;
            string formatted = newScore.ToString("D2");

            if (prev == -1)
            {
                digitTMP.text = formatted;
                return;
            }

            digitRT.DOKill();
            digitRT.DOScaleY(0f, 0.07f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                digitTMP.text = formatted;
                digitRT.DOScaleY(1f, 0.07f).SetEase(Ease.OutQuad);
            });
        }

        private void OnDestroy()
        {
            _playerDigitRT?.DOKill();
            _enemyDigitRT?.DOKill();
        }
    }
}
