using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoguelikeTCG.RunMap
{
    /// <summary>
    /// Affiche une ligne en pointillés entre deux nœuds de la carte.
    /// Chaque tiret est un enfant Image indépendant.
    /// Expose Dashes et HalfLength pour permettre l'animation "dessin progressif".
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class EdgeView : MonoBehaviour
    {
        public static readonly Color ColEdge        = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        public static readonly Color ColEdgeVisited = new Color(0.18f, 0.60f, 0.18f, 0.9f);

        private const float DashLen = 10f;
        private const float GapLen  =  8f;
        private const float LineH   =  3f;

        /// <summary>RectTransforms des tirets, dans l'ordre de fromPos → toPos.</summary>
        public List<RectTransform> Dashes     { get; } = new List<RectTransform>();
        /// <summary>Images des tirets — même ordre que Dashes, pour animer la couleur.</summary>
        public List<Image>         DashImages { get; } = new List<Image>();

        /// <summary>Demi-longueur de la ligne en unités canvas (= dist/2).</summary>
        public float HalfLength { get; private set; }

        /// <param name="fromPos">Position anchorée du nœud parent (espace parent commun)</param>
        /// <param name="toPos">Position anchorée du nœud enfant</param>
        public void Setup(Vector2 fromPos, Vector2 toPos)
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot     = new Vector2(0.5f, 0.5f);

            Vector2 dir   = toPos - fromPos;
            float   dist  = dir.magnitude;
            float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rt.anchoredPosition = (fromPos + toPos) * 0.5f;
            rt.sizeDelta        = new Vector2(dist, LineH);
            rt.localRotation    = Quaternion.Euler(0f, 0f, angle);

            HalfLength = dist * 0.5f;

            // Créer les tirets en espace local (origine = centre de la ligne)
            float period = DashLen + GapLen;
            float halfW  = HalfLength;
            float x      = -halfW;

            Dashes.Clear();

            while (x < halfW)
            {
                float dashW = Mathf.Min(DashLen, halfW - x);
                if (dashW <= 0f) break;

                var dash = new GameObject("Dash", typeof(RectTransform), typeof(Image));
                dash.transform.SetParent(transform, false);
                dash.GetComponent<Image>().color         = ColEdge;
                dash.GetComponent<Image>().raycastTarget = false;

                var drt              = dash.GetComponent<RectTransform>();
                drt.anchorMin        = new Vector2(0.5f, 0.5f);
                drt.anchorMax        = new Vector2(0.5f, 0.5f);
                drt.pivot            = new Vector2(0f, 0.5f);   // pivot au bord gauche → scale X "grandit vers la droite"
                drt.sizeDelta        = new Vector2(dashW, LineH);
                drt.anchoredPosition = new Vector2(x, 0f);

                var img = dash.GetComponent<Image>();
                Dashes.Add(drt);
                DashImages.Add(img);
                x += period;
            }
        }
    }
}
