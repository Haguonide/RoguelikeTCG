using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoguelikeTCG.RunMap
{
    /// <summary>
    /// Construit et affiche la carte de run à l'intérieur d'un ScrollRect.
    /// Les nœuds sont disposés de bas (départ) en haut (boss).
    /// </summary>
    public class RunMapUI : MonoBehaviour
    {
        public static RunMapUI Instance { get; private set; }

        [Header("Références scène")]
        [Tooltip("RectTransform du Content du ScrollRect")]
        public RectTransform contentRT;

        [Header("Mise en page")]
        [Tooltip("Espacement vertical entre deux lignes (px)")]
        public float rowSpacing  = 500f;
        [Tooltip("Espacement horizontal entre nœuds d'une même ligne (px)")]
        public float nodeSpacing = 460f;
        [Tooltip("Diamètre du cercle d'un nœud (px)")]
        public float nodeSize    = 190f;
        [Tooltip("Largeur de référence du contenu — adapter si la fenêtre est différente")]
        public float contentWidth = 600f;

        [Header("Icônes nœuds (optionnel)")]
        public Sprite iconStart;
        public Sprite iconCombat;
        public Sprite iconElite;
        public Sprite iconBoss;
        public Sprite iconEvent;
        public Sprite iconShop;
        public Sprite iconForge;
        public Sprite iconRest;
        public Sprite iconMystery;

        // -------------------------------------------------------
        // État interne
        // -------------------------------------------------------
        private readonly Dictionary<RunNode, NodeView>                       _nodeViews   = new Dictionary<RunNode, NodeView>();
        private readonly Dictionary<RunNode, Vector2>                        _nodePos     = new Dictionary<RunNode, Vector2>();
        private readonly Dictionary<int, List<GameObject>>                  _rowNodeGOs  = new Dictionary<int, List<GameObject>>();
        private readonly Dictionary<int, List<GameObject>>                  _rowEdgeGOs  = new Dictionary<int, List<GameObject>>();
        // Clé : (fromRow, fromCol, toRow, toCol) — identifiant unique d'une arête
        private readonly Dictionary<(int, int, int, int), EdgeView>         _edgeViewMap = new Dictionary<(int, int, int, int), EdgeView>();
        private Sprite      _circleSprite;
        private ScrollRect  _scrollRect;   // mis en cache dans BuildMap

        // Métadonnées utilisées par l'animation d'intro et de navigation
        private float _introContentHeight;
        private float _introBottomPad;
        private int   _introTotalRows;

        public bool IsIntroPlaying { get; private set; }

        // -------------------------------------------------------
        // Lifecycle
        // -------------------------------------------------------
        private void Awake()
        {
            // Utiliser Destroy(this) et non Destroy(gameObject) :
            // détruire le gameObject détruirait tout le Canvas (Background, Title, etc.)
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            // S'assurer que RunPersistence existe (point d'entrée alternatif)
            if (RoguelikeTCG.Core.RunPersistence.Instance == null)
            {
                var go = new GameObject("RunPersistence");
                go.AddComponent<RoguelikeTCG.Core.RunPersistence>();
            }

            CacheOrRestoreSprites();
        }

        // -------------------------------------------------------
        // Cache / restauration des sprites via RunPersistence
        // -------------------------------------------------------
        private void CacheOrRestoreSprites()
        {
            var p = RoguelikeTCG.Core.RunPersistence.Instance;
            if (p == null) return;

            var bgImg = transform.Find("Background")?.GetComponent<UnityEngine.UI.Image>();

            if (iconStart != null)
            {
                p.CachedNodeIcons = new Sprite[]
                {
                    iconStart, iconCombat, iconElite, iconBoss,
                    iconEvent, iconShop,   iconForge, iconRest, iconMystery
                };

                if (bgImg != null)
                {
                    p.CachedBackground      = bgImg.sprite;
                    p.CachedBackgroundColor = bgImg.color;
                }
            }
            else if (p.CachedNodeIcons != null && p.CachedNodeIcons.Length >= 9)
            {
                iconStart   = p.CachedNodeIcons[0];
                iconCombat  = p.CachedNodeIcons[1];
                iconElite   = p.CachedNodeIcons[2];
                iconBoss    = p.CachedNodeIcons[3];
                iconEvent   = p.CachedNodeIcons[4];
                iconShop    = p.CachedNodeIcons[5];
                iconForge   = p.CachedNodeIcons[6];
                iconRest    = p.CachedNodeIcons[7];
                iconMystery = p.CachedNodeIcons[8];
                Debug.Log("[RunMapUI] Sprites restaurés depuis RunPersistence cache.");

                if (bgImg != null)
                {
                    if (p.CachedBackground != null) bgImg.sprite = p.CachedBackground;
                    bgImg.color = p.CachedBackgroundColor;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            var p = RoguelikeTCG.Core.RunPersistence.Instance;
            if (p != null && contentRT != null)
            {
                var sr = contentRT.GetComponentInParent<ScrollRect>();
                if (sr != null)
                    p.MapScrollPosition = sr.verticalNormalizedPosition;
            }
        }

        private void Start()
        {
            Debug.Log($"[RunMapUI] Start — RunMapManager.Instance={(RunMapManager.Instance != null ? "OK" : "NULL")}, Map={(RunMapManager.Instance?.Map != null ? RunMapManager.Instance.Map.Count + " rows" : "NULL")}");
            if (RunMapManager.Instance != null)
                StartCoroutine(BuildMapNextFrame(RunMapManager.Instance.Map));
            else
                Debug.LogError("[RunMapUI] RunMapManager.Instance est NULL dans Start() !");
        }

        private IEnumerator BuildMapNextFrame(List<List<RunNode>> map)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Debug.Log($"[RunMapUI] BuildMapNextFrame — map={map?.Count} rows");
            if (map != null) BuildMap(map);
            else { Debug.LogError("[RunMapUI] map est NULL — BuildMap non appelé !"); yield break; }

            var p = RoguelikeTCG.Core.RunPersistence.Instance;

            if (p != null && p.IsNewRun)
            {
                // Cacher tout immédiatement (même frame que BuildMap) pour éviter le flash
                HideAllNodesAndEdges();

                // Consommer le flag immédiatement pour éviter un double-déclenchement
                p.IsNewRun = false;

                // Laisser un frame supplémentaire pour que le ScrollRect connaisse sa hauteur
                yield return null;
                Canvas.ForceUpdateCanvases();
                yield return StartCoroutine(PlayIntroAnimation());
            }
            else if (p != null && p.MapScrollPosition >= 0f)
            {
                yield return null;
                Canvas.ForceUpdateCanvases();
                var sr = contentRT.GetComponentInParent<ScrollRect>();
                if (sr != null)
                    sr.verticalNormalizedPosition = Mathf.Clamp01(p.MapScrollPosition);
                p.MapScrollPosition = -1f;
            }
        }

        // -------------------------------------------------------
        // Construction de la carte
        // -------------------------------------------------------
        public void BuildMap(List<List<RunNode>> map)
        {
            Debug.Log($"[RunMapUI] BuildMap — {map?.Count} rows, iconCombat={(iconCombat != null ? "OK" : "NULL")}");
            if (contentRT == null) { Debug.LogError("[RunMapUI] contentRT non assigné !"); return; }

            _nodeViews.Clear();
            _nodePos.Clear();
            _rowNodeGOs.Clear();
            _rowEdgeGOs.Clear();
            _edgeViewMap.Clear();
            _scrollRect = contentRT.GetComponentInParent<ScrollRect>();

            foreach (Transform child in contentRT)
                Destroy(child.gameObject);

            int   totalRows   = map.Count;
            float bottomPad   = nodeSize * 0.5f + 20f;
            float topPad      = 40f;
            float totalHeight = bottomPad + (totalRows - 1) * rowSpacing + nodeSize + topPad;
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, totalHeight);

            // Stocker pour l'animation d'intro
            _introContentHeight = totalHeight;
            _introBottomPad     = bottomPad;
            _introTotalRows     = totalRows;

            var edgeParent = MakeContainer("Edges");
            var nodeParent = MakeContainer("Nodes");

            var scrollRectForWidth = contentRT.GetComponentInParent<ScrollRect>();
            float viewportW = (scrollRectForWidth != null && scrollRectForWidth.viewport != null)
                ? scrollRectForWidth.viewport.rect.width : 0f;
            float actualWidth = viewportW > 10f ? viewportW
                : (contentRT.rect.width > 10f ? contentRT.rect.width : contentWidth);
            float cx = actualWidth * 0.5f;

            Debug.Log($"[RunMapUI] BuildMap LAYOUT — Screen=({Screen.width}x{Screen.height}), " +
                      $"viewportW={viewportW:F1}, actualWidth={actualWidth:F1}, cx={cx:F1}");

            for (int r = 0; r < totalRows; r++)
            {
                var row = map[r];
                float y          = bottomPad + r * rowSpacing;
                float totalWidth = (row.Count - 1) * nodeSpacing;
                float startX     = cx - totalWidth * 0.5f;

                for (int c = 0; c < row.Count; c++)
                {
                    var   node = row[c];
                    float x    = startX + c * nodeSpacing;
                    var   pos  = new Vector2(x, y);
                    _nodePos[node] = pos;

                    CreateNodeGO(node, nodeParent, pos, r);
                }
            }

            for (int r = 0; r < totalRows - 1; r++)
            {
                foreach (var node in map[r])
                {
                    foreach (var child in node.children)
                    {
                        if (_nodePos.ContainsKey(node) && _nodePos.ContainsKey(child))
                            CreateEdgeGO(node, child, edgeParent, r);
                    }
                }
            }

            Canvas.ForceUpdateCanvases();
            var sr2 = contentRT.GetComponentInParent<ScrollRect>();
            if (sr2 != null) sr2.verticalNormalizedPosition = 0f;
        }

        // -------------------------------------------------------
        // Rafraîchir toutes les couleurs (après visite)
        // -------------------------------------------------------
        public void RefreshNodeStates()
        {
            foreach (var kv in _nodeViews)
                kv.Value.RefreshState();
        }

        // -------------------------------------------------------
        // Animation d'intro (nouvelle partie uniquement)
        // -------------------------------------------------------

        /// <summary>
        /// Révèle l'arbre rangée par rangée avec un effet de bump et
        /// fait défiler la caméra de bas en haut jusqu'au boss.
        /// </summary>
        private void HideAllNodesAndEdges()
        {
            foreach (var list in _rowNodeGOs.Values)
                foreach (var go in list)
                    if (go) go.transform.localScale = Vector3.zero;

            foreach (var list in _rowEdgeGOs.Values)
                foreach (var go in list)
                {
                    if (!go) continue;
                    var ev = go.GetComponent<EdgeView>();
                    if (ev == null) continue;
                    foreach (var dash in ev.Dashes)
                        if (dash) dash.localScale = Vector3.zero;
                }
        }

        private IEnumerator PlayIntroAnimation()
        {
            var sr = contentRT.GetComponentInParent<ScrollRect>();
            if (sr == null) yield break;

            IsIntroPlaying = true;

            // --- Cacher tout (au cas où HideAllNodesAndEdges n'aurait pas été appelé) ---
            HideAllNodesAndEdges();

            const float bumpDur = 1.90f;   // durée du bump d'un nœud
            const float fadeDur = 1.40f;   // durée du tracé d'une arête
            const float pause   = 0.30f;   // courte pause entre rangées

            // --- Rangée 0 (Start) : positionner caméra et attendre le bump complet ---
            sr.verticalNormalizedPosition = NormForRow(0);
            if (_rowNodeGOs.TryGetValue(0, out var row0))
                yield return StartCoroutine(BumpInObjects(row0, bumpDur));
            yield return new WaitForSeconds(pause);

            // --- Rangées 1 → n-1 ---
            for (int r = 1; r < _introTotalRows; r++)
            {
                float fromNorm = NormForRow(r - 1);
                float toNorm   = NormForRow(r);

                // Phase 1 — tracé du trait + scroll synchronisé (tous deux bloquants ensemble)
                // Le bump du nœud d'arrivée est lancé à 85% du tracé (non-bloquant)
                if (_rowEdgeGOs.TryGetValue(r - 1, out var edges))
                    StartCoroutine(DrawOnAllEdges(edges, fadeDur));

                if (_rowNodeGOs.TryGetValue(r, out var nodes))
                    StartCoroutine(DelayedBump(nodes, fadeDur * 0.85f, bumpDur));

                // La caméra coulisse de la rangée source vers la rangée cible
                // exactement pendant que le trait se dessine
                yield return StartCoroutine(SmoothScrollFromTo(sr, fromNorm, toNorm, fadeDur));

                // Phase 2 — caméra centrée sur le nœud pendant le reste du bump
                // (le bump a démarré à 85% du fadeDur, il lui reste bumpDur - fadeDur*0.15 secondes)
                float bumpRemaining = bumpDur - fadeDur * 0.15f;
                if (bumpRemaining > 0f)
                    yield return new WaitForSeconds(bumpRemaining);

                // Petite pause avant la prochaine rangée (sauf après le boss)
                if (r < _introTotalRows - 1)
                    yield return new WaitForSeconds(pause);
            }

            // --- Pause au boss, puis retour en bas ---
            yield return new WaitForSeconds(3.0f);
            yield return StartCoroutine(SmoothScrollTo(sr, 0f, 4.25f));

            IsIntroPlaying = false;
        }

        // -------------------------------------------------------
        // Animation de navigation (clic sur un nœud)
        // -------------------------------------------------------

        /// <summary>
        /// Déclenche l'animation de navigation : scroll camera + verdissement de l'arête,
        /// puis appelle <paramref name="onComplete"/> (charge la scène ou affiche l'événement).
        /// </summary>
        public void PlayNavigationAnimation(RunNode from, RunNode to, System.Action onComplete)
        {
            StartCoroutine(NavigationAnimCoroutine(from, to, onComplete));
        }

        private IEnumerator NavigationAnimCoroutine(RunNode from, RunNode to, System.Action onComplete)
        {
            if (_scrollRect == null) { onComplete?.Invoke(); yield break; }

            const float navDuration = 1.5f;

            float currentPos = _scrollRect.verticalNormalizedPosition;
            float fromNorm   = NormForRow(from.row);
            float toNorm     = NormForRow(to.row);

            // Phase 1 : recentrage fluide vers le nœud source (si la caméra n'y est pas déjà)
            float distPre = Mathf.Abs(currentPos - fromNorm);
            if (distPre > 0.001f)
            {
                float preDuration = Mathf.Clamp(distPre * 2.0f, 0.25f, 0.75f);
                yield return StartCoroutine(SmoothScrollFromTo(_scrollRect, currentPos, fromNorm, preDuration));
            }

            // Phase 2 : navigation vers le nœud cible + verdissement de l'arête
            _edgeViewMap.TryGetValue((from.row, from.col, to.row, to.col), out var edgeView);

            float t = 0f;
            while (t < navDuration)
            {
                t += Time.deltaTime;
                float n = EaseInOutQuad(Mathf.Clamp01(t / navDuration));

                _scrollRect.verticalNormalizedPosition = Mathf.Lerp(fromNorm, toNorm, n);

                if (edgeView != null)
                {
                    float halfW = edgeView.HalfLength;
                    float front = Mathf.Lerp(-halfW, halfW, n);

                    for (int i = 0; i < edgeView.Dashes.Count; i++)
                    {
                        var drt = edgeView.Dashes[i];
                        var img = edgeView.DashImages[i];
                        if (!drt || !img) continue;
                        float dashX = drt.anchoredPosition.x;
                        float dashW = drt.sizeDelta.x;
                        float fill  = Mathf.Clamp01((front - dashX) / dashW);
                        img.color   = Color.Lerp(EdgeView.ColEdge, EdgeView.ColEdgeVisited, fill);
                    }
                }

                yield return null;
            }

            // État final garanti
            _scrollRect.verticalNormalizedPosition = toNorm;
            if (edgeView != null)
                foreach (var img in edgeView.DashImages)
                    if (img) img.color = EdgeView.ColEdgeVisited;

            yield return new WaitForSeconds(0.35f);
            onComplete?.Invoke();
        }

        private IEnumerator DelayedBump(List<GameObject> gos, float delay, float duration)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(BumpInObjects(gos, duration));
        }

        private IEnumerator BumpInObjects(List<GameObject> gos, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float n     = Mathf.Clamp01(t / duration);
                float scale = EaseOutBack(n);
                foreach (var go in gos)
                    if (go) go.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            foreach (var go in gos)
                if (go) go.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Lance le dessin progressif de toutes les arêtes d'une rangée en parallèle.
        /// </summary>
        private IEnumerator DrawOnAllEdges(List<GameObject> edgeGOs, float duration)
        {
            foreach (var go in edgeGOs)
            {
                if (!go) continue;
                var ev = go.GetComponent<EdgeView>();
                if (ev != null) StartCoroutine(DrawOnSingleEdge(ev, duration));
            }
            yield return new WaitForSeconds(duration);
        }

        /// <summary>
        /// Révèle les tirets d'une arête un par un, de fromPos vers toPos,
        /// comme un trait qui se dessine progressivement.
        /// Le "front" de dessin avance en continu sur toute la longueur.
        /// </summary>
        private IEnumerator DrawOnSingleEdge(EdgeView ev, float duration)
        {
            var   dashes = ev.Dashes;
            float halfW  = ev.HalfLength;
            if (dashes == null || dashes.Count == 0 || halfW <= 0f) yield break;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                // "front" va de -halfW (fromPos) à +halfW (toPos)
                float front = Mathf.Lerp(-halfW, halfW, EaseInOutQuad(Mathf.Clamp01(t / duration)));

                foreach (var dash in dashes)
                {
                    if (!dash) continue;
                    float dashX = dash.anchoredPosition.x;   // bord gauche du tiret
                    float dashW = dash.sizeDelta.x;
                    // Portion du tiret déjà franchie par le front
                    float fill = Mathf.Clamp01((front - dashX) / dashW);
                    dash.localScale = new Vector3(fill, 1f, 1f);
                }

                yield return null;
            }

            // S'assurer que tout est bien à 1 en fin d'animation
            foreach (var dash in dashes)
                if (dash) dash.localScale = Vector3.one;
        }

        private IEnumerator SmoothScrollTo(ScrollRect sr, float target, float duration)
        {
            float start = sr.verticalNormalizedPosition;
            float t     = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                sr.verticalNormalizedPosition = Mathf.Lerp(start, target, EaseInOutQuad(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            sr.verticalNormalizedPosition = target;
        }

        /// <summary>Scroll avec positions de départ et d'arrivée explicites — évite tout glitch
        /// si la caméra n'est pas exactement à <paramref name="from"/> au moment du lancement.</summary>
        private IEnumerator SmoothScrollFromTo(ScrollRect sr, float from, float to, float duration)
        {
            sr.verticalNormalizedPosition = from;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                sr.verticalNormalizedPosition = Mathf.Lerp(from, to, EaseInOutQuad(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            sr.verticalNormalizedPosition = to;
        }

        // f(t) ∈ [0,1] — rebond légèrement au-delà de 1 avant de redescendre à 1
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseInOutQuad(float t)
            => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

        // -------------------------------------------------------
        // Helpers de construction
        // -------------------------------------------------------
        private RectTransform MakeContainer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(contentRT, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        private void CreateNodeGO(RunNode node, RectTransform parent, Vector2 pos, int row)
        {
            var go = new GameObject($"Node_{node.row}_{node.col}",
                typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt              = go.GetComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.zero;
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(nodeSize, nodeSize);

            var bg = go.GetComponent<Image>();
            bg.sprite = _circleSprite ??= CreateCircleSprite(64);
            bg.type   = Image.Type.Simple;

            var iconGO  = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(go.transform, false);
            var iconRT  = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin        = new Vector2(0.125f, 0.125f);
            iconRT.anchorMax        = new Vector2(0.875f, 0.875f);
            iconRT.sizeDelta        = Vector2.zero;
            iconRT.anchoredPosition = Vector2.zero;
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.raycastTarget  = false;
            iconImg.preserveAspect = true;

            var nv = go.AddComponent<NodeView>();
            nv.Setup(node, GetIcon(node.type));
            _nodeViews[node] = nv;

            // Enregistrer pour l'animation d'intro
            if (!_rowNodeGOs.TryGetValue(row, out var list))
                _rowNodeGOs[row] = list = new List<GameObject>();
            list.Add(go);
        }

        private void CreateEdgeGO(RunNode fromNode, RunNode toNode, RectTransform parent, int fromRow)
        {
            var go = new GameObject("Edge", typeof(RectTransform), typeof(EdgeView));
            go.transform.SetParent(parent, false);
            var ev = go.GetComponent<EdgeView>();
            ev.Setup(_nodePos[fromNode], _nodePos[toNode]);

            // Enregistrement pour l'animation d'intro (par rangée source)
            if (!_rowEdgeGOs.TryGetValue(fromRow, out var list))
                _rowEdgeGOs[fromRow] = list = new List<GameObject>();
            list.Add(go);

            // Enregistrement pour l'animation de navigation (par paire de nœuds)
            _edgeViewMap[(fromNode.row, fromNode.col, toNode.row, toNode.col)] = ev;
        }

        /// <summary>Position scroll normalisée pour centrer la rangée r dans le viewport.</summary>
        private float NormForRow(int r)
        {
            float viewportH = (_scrollRect != null && _scrollRect.viewport != null)
                ? _scrollRect.viewport.rect.height : 600f;
            float maxScroll = Mathf.Max(1f, _introContentHeight - viewportH);
            float rowY      = _introBottomPad + r * rowSpacing;
            float scrollY   = rowY - viewportH * 0.5f;
            return Mathf.Clamp01(scrollY / maxScroll);
        }

        /// <summary>Génère un sprite circulaire blanc par code (pas de dépendance externe).</summary>
        private static Sprite CreateCircleSprite(int radius)
        {
            int size   = radius * 2;
            var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];
            float center = radius - 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx   = x - center;
                    float dy   = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    byte  a    = (byte)(Mathf.Clamp01((radius - dist) + 0.75f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private Sprite GetIcon(NodeType type) => type switch
        {
            NodeType.Start   => iconStart,
            NodeType.Combat  => iconCombat,
            NodeType.Elite   => iconElite,
            NodeType.Boss    => iconBoss,
            NodeType.Event   => iconEvent,
            NodeType.Shop    => iconShop,
            NodeType.Forge   => iconForge,
            NodeType.Rest    => iconRest,
            NodeType.Mystery => iconMystery,
            _                => null
        };
    }
}
