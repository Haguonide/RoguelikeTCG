using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.RunMap
{
    /// <summary>
    /// Gère les nœuds non-combat (Rest, Forge, Shop, Event, Mystery)
    /// sous forme d'overlays affichés par-dessus la RunMap.
    /// </summary>
    public class NodeEventManager : MonoBehaviour
    {
        public static NodeEventManager Instance { get; private set; }

        private GameObject _activeOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        // ── Point d'entrée ────────────────────────────────────────────────────

        private static void RefreshBar()
        {
            RoguelikeTCG.UI.RelicBarUI.Instance?.Refresh();
            // Re-bring the active overlay to front so RelicBar stays behind it
            if (Instance != null && Instance._activeOverlay != null)
                Instance._activeOverlay.transform.SetAsLastSibling();
        }

        public void ShowNode(NodeType type)
        {
            switch (type)
            {
                case NodeType.Rest:    ShowRest();    break;
                case NodeType.Forge:   ShowForge();   break;
                case NodeType.Shop:    ShowShop();    break;
                case NodeType.Event:   ShowEvent();   break;
                case NodeType.Mystery: ShowMystery(); break;
            }
        }

        // ── REST ──────────────────────────────────────────────────────────────

        private void ShowRest()
        {
            var overlay = MakeOverlay("RestOverlay");
            MakeTitle(overlay, "Taverne — Repos");
            MakeSubtitle(overlay, "Vous vous installez dans une taverne. Que souhaitez-vous faire ?");

            var healBtn = MakeButton(overlay, "BtnHeal", 0.30f, 0.52f, 0.70f, 0.63f,
                new Color(0.12f, 0.28f, 0.18f), "Se soigner (+20 HP)");
            healBtn.onClick.AddListener(() =>
            {
                var p = RunPersistence.Instance;
                if (p != null)
                {
                    p.PlayerHP = Mathf.Min(p.PlayerMaxHP, p.PlayerHP + 20);
                    p.SaveToDisk();
                }
                Destroy(overlay);
            });

            var playerDeck = RunPersistence.Instance?.PlayerDeck;
            bool deckAtMin = playerDeck == null || playerDeck.Count <= 20;

            var removeBtn = MakeButton(overlay, "BtnRemove", 0.30f, 0.38f, 0.70f, 0.49f,
                deckAtMin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.28f, 0.15f, 0.10f),
                deckAtMin ? "Supprimer une carte (minimum 20 cartes atteint)" : "Supprimer une carte du deck");
            if (deckAtMin)
            {
                removeBtn.interactable = false;
                var lbl = removeBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl) lbl.color = new Color(0.55f, 0.55f, 0.55f);
            }
            else
            {
                removeBtn.onClick.AddListener(() =>
                {
                    Destroy(overlay);
                    ShowCardRemoval();
                });
            }

            var skipBtn = MakeButton(overlay, "BtnSkip", 0.38f, 0.22f, 0.62f, 0.32f,
                new Color(0.22f, 0.22f, 0.26f), "Passer");
            skipBtn.onClick.AddListener(() => { RefreshBar(); Destroy(overlay); });
        }

        private void ShowCardRemoval()
        {
            var deck = RunPersistence.Instance?.PlayerDeck;
            if (deck == null || deck.Count == 0) return;

            var overlay = MakeOverlay("CardRemovalOverlay");
            MakeTitle(overlay, "Supprimer une carte");
            MakeSubtitle(overlay, "Choisissez une carte à retirer définitivement de votre deck.");

            ShowCardGrid(overlay, deck, card =>
            {
                var p = RunPersistence.Instance;
                p?.PlayerDeck.Remove(card);
                p?.SaveToDisk();
                Destroy(overlay);
            });

            var cancelBtn = MakeButton(overlay, "BtnCancel", 0.38f, 0.05f, 0.62f, 0.14f,
                new Color(0.22f, 0.22f, 0.26f), "Annuler");
            cancelBtn.onClick.AddListener(() => { RefreshBar(); Destroy(overlay); });
        }

        // ── FORGE ─────────────────────────────────────────────────────────────

        private void ShowForge()
        {
            var p    = RunPersistence.Instance;
            var deck = p?.PlayerDeck;

            var overlay = MakeOverlay("ForgeOverlay");
            MakeTitle(overlay, "Forge");

            // Compter les copies de chaque carte
            var counts = new Dictionary<CardData, int>();
            if (deck != null)
                foreach (var c in deck)
                    if (c != null && c.upgradedVersion != null)
                        counts[c] = counts.ContainsKey(c) ? counts[c] + 1 : 1;

            // Cartes éligibles : ≥ 3 copies
            var eligible = new List<CardData>();
            foreach (var kv in counts)
                if (kv.Value >= 3) eligible.Add(kv.Key);

            bool deckTooSmall = deck == null || deck.Count < 22; // 3→1 = −2, plancher 20

            if (eligible.Count == 0 || deckTooSmall)
            {
                string msg = deckTooSmall
                    ? "Votre deck est trop petit pour fusionner (minimum 22 cartes requis)."
                    : "Vous n'avez pas 3 copies identiques d'une même carte.";
                MakeSubtitle(overlay, msg);
                var ok = MakeButton(overlay, "BtnOk", 0.38f, 0.35f, 0.62f, 0.45f,
                    new Color(0.22f, 0.22f, 0.26f), "Continuer");
                ok.onClick.AddListener(() => Destroy(overlay));
                return;
            }

            MakeSubtitle(overlay, "Fusionnez 3 copies identiques → 1 carte améliorée (+). Le deck perd 2 cartes.");

            // Grille : une entrée par carte unique éligible (max 5)
            int count = Mathf.Min(eligible.Count, 5);
            float totalW = count * 0.16f + (count - 1) * 0.02f;
            float startX = 0.5f - totalW / 2f;

            for (int i = 0; i < count; i++)
            {
                var card = eligible[i];
                int copies = counts[card];
                float x0 = startX + i * 0.18f;
                float x1 = x0 + 0.16f;

                var cardGO = new GameObject($"ForgeCard_{i}", typeof(RectTransform));
                cardGO.transform.SetParent(overlay.transform, false);
                SetAnchors(cardGO, x0, 0.22f, x1, 0.72f);
                cardGO.AddComponent<Image>();
                var btn = cardGO.AddComponent<Button>();
                CardUIBuilder.ApplyTemplate(card, cardGO);

                // Label "×N / 3 requis"
                var labelGO = new GameObject($"CopiesLabel_{i}", typeof(RectTransform));
                labelGO.transform.SetParent(overlay.transform, false);
                SetAnchors(labelGO, x0, 0.14f, x1, 0.22f);
                var lbl = labelGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = $"×{copies} (3 requis)";
                lbl.fontSize  = 14f;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color     = new Color(1f, 0.85f, 0.1f);

                var capturedCard = card;
                var capturedOverlay = overlay;
                btn.onClick.AddListener(() =>
                {
                    var run = RunPersistence.Instance;
                    if (run?.PlayerDeck == null) return;

                    // Retirer 3 copies
                    int removed = 0;
                    for (int j = run.PlayerDeck.Count - 1; j >= 0 && removed < 3; j--)
                        if (run.PlayerDeck[j] == capturedCard) { run.PlayerDeck.RemoveAt(j); removed++; }

                    // Ajouter 1 version upgradée
                    run.PlayerDeck.Add(capturedCard.upgradedVersion);
                    run.SaveToDisk();

                    Destroy(capturedOverlay);
                });
            }

            var skipBtn = MakeButton(overlay, "BtnSkip", 0.38f, 0.05f, 0.62f, 0.14f,
                new Color(0.22f, 0.22f, 0.26f), "Passer");
            skipBtn.onClick.AddListener(() => { RefreshBar(); Destroy(overlay); });
        }

        // ── SHOP ──────────────────────────────────────────────────────────────

        private void ShowShop()
        {
            var registry = Resources.Load<CardRegistry>("CardRegistry");
            var overlay  = MakeOverlay("ShopOverlay");
            var p        = RunPersistence.Instance;

            MakeTitle(overlay, "Marchand");

            // Or du joueur
            var goldGO = new GameObject("Gold", typeof(RectTransform));
            goldGO.transform.SetParent(overlay.transform, false);
            SetAnchors(goldGO, 0.10f, 0.74f, 0.90f, 0.83f);
            var goldTMP = goldGO.AddComponent<TextMeshProUGUI>();
            goldTMP.text      = $"Votre or : {p?.PlayerGold ?? 0}";
            goldTMP.fontSize  = 18f;
            goldTMP.alignment = TextAlignmentOptions.Center;
            goldTMP.color     = new Color(0.95f, 0.82f, 0.35f);

            // Utiliser le pool effectif de la run (cardPool + épiques débloquées via leveling)
            var sourcePool = (p?.EffectiveCardPool != null && p.EffectiveCardPool.Count > 0)
                ? p.EffectiveCardPool
                : (registry != null ? registry.allCards : null);

            if (sourcePool == null || sourcePool.Count == 0)
            {
                MakeSubtitle(overlay, "Le marchand n'a rien à vendre aujourd'hui.");
                var ok = MakeButton(overlay, "BtnOk", 0.38f, 0.35f, 0.62f, 0.45f,
                    new Color(0.22f, 0.22f, 0.26f), "Partir");
                ok.onClick.AddListener(() => Destroy(overlay));
                return;
            }

            MakeSubtitle(overlay, "Achetez une carte ou vendez une carte de votre deck.");

            // Tirer 3 cartes aléatoires parmi les cartes de base uniquement (jamais les upgradées)
            var pool = sourcePool.FindAll(c => c != null && !c.cardName.EndsWith("+"));
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            var options = pool.GetRange(0, Mathf.Min(3, pool.Count));

            // Grille avec prix
            ShowShopCardGrid(overlay, options, goldTMP);

            // Bouton vendre
            var sellBtn = MakeButton(overlay, "BtnSell", 0.22f, 0.05f, 0.48f, 0.14f,
                new Color(0.28f, 0.18f, 0.10f), "Vendre une carte");
            sellBtn.onClick.AddListener(() =>
            {
                Destroy(overlay);
                ShowCardSell();
            });

            var skipBtn = MakeButton(overlay, "BtnSkip", 0.52f, 0.05f, 0.78f, 0.14f,
                new Color(0.22f, 0.22f, 0.26f), "Partir sans rien faire");
            skipBtn.onClick.AddListener(() => { RefreshBar(); Destroy(overlay); });
        }

        private void ShowShopCardGrid(GameObject parent, List<CardData> cards, TextMeshProUGUI goldLabel)
        {
            int   count  = Mathf.Min(cards.Count, 3);
            float cardW  = 0.18f, gap = 0.04f;
            float total  = count * cardW + (count - 1) * gap;
            float startX = 0.5f - total / 2f;

            var p = RunPersistence.Instance;

            for (int i = 0; i < count; i++)
            {
                var   card  = cards[i];
                int   price = GetCardPrice(card.rarity);
                bool  canAfford = p != null && p.PlayerGold >= price;
                float x0    = startX + i * (cardW + gap);
                float x1    = x0 + cardW;

                var cardGO = new GameObject($"ShopCard_{i}", typeof(RectTransform));
                cardGO.transform.SetParent(parent.transform, false);
                SetAnchors(cardGO, x0, 0.18f, x1, 0.72f);
                cardGO.AddComponent<Image>();
                var btn = cardGO.AddComponent<Button>();
                btn.interactable = canAfford;
                CardUIBuilder.ApplyTemplate(card, cardGO);

                // Prix — label sous la carte
                var priceGO = new GameObject($"Price_{i}", typeof(RectTransform));
                priceGO.transform.SetParent(parent.transform, false);
                SetAnchors(priceGO, x0, 0.09f, x1, 0.18f);
                var priceTMP = priceGO.AddComponent<TextMeshProUGUI>();
                priceTMP.text      = $"{price} or";
                priceTMP.fontSize  = 15f;
                priceTMP.fontStyle = FontStyles.Bold;
                priceTMP.alignment = TextAlignmentOptions.Center;
                priceTMP.color     = canAfford
                    ? new Color(0.95f, 0.82f, 0.35f)
                    : new Color(0.55f, 0.45f, 0.25f);
                priceTMP.raycastTarget = false;

                var capturedCard  = card;
                var capturedPrice = price;
                var capturedLabel = goldLabel;
                btn.onClick.AddListener(() =>
                {
                    p?.SpendGold(capturedPrice);
                    p?.AddCardToDeck(capturedCard);
                    if (capturedLabel != null)
                        capturedLabel.text = $"Votre or : {p?.PlayerGold ?? 0}";
                    RefreshBar();
                    Destroy(cardGO);
                    btn.interactable = false;
                });
            }
        }

        private void ShowCardSell()
        {
            var deck = RunPersistence.Instance?.PlayerDeck;
            if (deck == null || deck.Count == 0) return;

            var overlay = MakeOverlay("SellOverlay");
            MakeTitle(overlay, "Vendre une carte");
            MakeSubtitle(overlay, "Choisissez une carte à vendre. Vous recevez la moitié de sa valeur.");

            ShowCardGrid(overlay, deck, card =>
            {
                int sellValue = GetCardPrice(card.rarity) / 2;
                var p = RunPersistence.Instance;
                p?.PlayerDeck.Remove(card);
                p?.AddGold(sellValue);
                RefreshBar();
                Destroy(overlay);
            }, showSellPrice: true);

            var cancelBtn = MakeButton(overlay, "BtnCancel", 0.38f, 0.05f, 0.62f, 0.14f,
                new Color(0.22f, 0.22f, 0.26f), "Annuler");
            cancelBtn.onClick.AddListener(() => { RefreshBar(); Destroy(overlay); });
        }

        private static int GetCardPrice(CardRarity rarity) => rarity switch
        {
            CardRarity.Common    => 50,
            CardRarity.Uncommon  => 75,
            CardRarity.Rare      => 100,
            CardRarity.Epic      => 150,
            CardRarity.Legendary => 200,
            _                    => 50,
        };

        // ── EVENT ─────────────────────────────────────────────────────────────

        private struct EventData
        {
            public string title, text, btn1Label, btn2Label;
            public System.Action action1, action2;
        }

        private void ShowEvent()
        {
            var events = BuildEvents();
            var ev     = events[Random.Range(0, events.Count)];

            var overlay = MakeOverlay("EventOverlay");
            MakeTitle(overlay, ev.title);

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(overlay.transform, false);
            SetAnchors(textGO, 0.12f, 0.44f, 0.88f, 0.74f);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = ev.text;
            tmp.fontSize  = 16f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(0.85f, 0.82f, 0.75f);

            var capturedOverlay = overlay;
            var btn1 = MakeButton(overlay, "Btn1", 0.18f, 0.28f, 0.46f, 0.39f,
                new Color(0.15f, 0.22f, 0.32f), ev.btn1Label);
            btn1.onClick.AddListener(() => { ev.action1?.Invoke(); RefreshBar(); Destroy(capturedOverlay); });

            var btn2 = MakeButton(overlay, "Btn2", 0.54f, 0.28f, 0.82f, 0.39f,
                new Color(0.28f, 0.15f, 0.15f), ev.btn2Label);
            btn2.onClick.AddListener(() => { ev.action2?.Invoke(); RefreshBar(); Destroy(capturedOverlay); });
        }

        private List<EventData> BuildEvents()
        {
            var p = RunPersistence.Instance;
            return new List<EventData>
            {
                // ── Événements existants ──────────────────────────────────────────
                new EventData
                {
                    title     = "Un mendiant vous aborde",
                    text      = "\"Seigneur, pitié ! Je n'ai pas mangé depuis trois jours.\"\n\nVous pouvez lui donner vos soins ou l'ignorer.",
                    btn1Label = "Lui donner des soins (-10 HP)",
                    btn2Label = "L'ignorer",
                    action1   = () => { if (p != null) { p.PlayerHP = Mathf.Max(1, p.PlayerHP - 10); p.SaveToDisk(); } },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Une vieille apothicaire",
                    text      = "Une femme vous propose un élixir mystérieux.\n\n\"Il guérit ou il tue — à vous de choisir !\"",
                    btn1Label = "Boire l'élixir",
                    btn2Label = "Refuser poliment",
                    action1   = () =>
                    {
                        if (p == null) return;
                        if (Random.value < 0.6f) p.PlayerHP = Mathf.Min(p.PlayerMaxHP, p.PlayerHP + 15);
                        else                     p.PlayerHP = Mathf.Max(1, p.PlayerHP - 15);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Défi de bras de fer",
                    text      = "Un colosse vous défie au bras de fer.\n\n\"Si tu gagnes, je te donne ma récompense !\"",
                    btn1Label = "Accepter le défi",
                    btn2Label = "Décliner",
                    action1   = () =>
                    {
                        if (p == null) return;
                        if (Random.value < 0.5f) p.PlayerHP = Mathf.Min(p.PlayerMaxHP, p.PlayerHP + 10);
                        else                     p.PlayerHP = Mathf.Max(1, p.PlayerHP - 5);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Bibliothèque abandonnée",
                    text      = "Vous trouvez une bibliothèque poussiéreuse.\nDes grimoires anciens pourraient renforcer votre deck...\nou l'alourdir.",
                    btn1Label = "Étudier les grimoires (+1 carte)",
                    btn2Label = "Ignorer et partir",
                    action1   = () =>
                    {
                        var registry = Resources.Load<CardRegistry>("CardRegistry");
                        if (registry != null && registry.allCards?.Count > 0)
                        {
                            var card = registry.allCards[Random.Range(0, registry.allCards.Count)];
                            p?.AddCardToDeck(card);
                        }
                    },
                    action2   = () => { }
                },

                // ── Nouveaux événements ───────────────────────────────────────────

                new EventData
                {
                    title     = "Le parieur de taverne",
                    text      = "Un homme à l'air louche vous propose un pari aux dés.\n\n\"Double ou rien, l'ami. La fortune sourit aux audacieux !\"",
                    btn1Label = "Parier 30 or",
                    btn2Label = "Refuser sagement",
                    action1   = () =>
                    {
                        if (p == null) return;
                        if (p.PlayerGold < 30) return;
                        if (Random.value < 0.5f) p.AddGold(50);
                        else                     p.SpendGold(30);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "L'alchimiste ambulant",
                    text      = "Un alchimiste aux doigts tachés d'encre vous propose ses services.\n\n\"Pour 40 pièces, je renforce votre constitution durablement.\"",
                    btn1Label = "Payer 40 or (+10 HP max)",
                    btn2Label = "Décliner",
                    action1   = () =>
                    {
                        if (p == null || p.PlayerGold < 40) return;
                        p.SpendGold(40);
                        p.PlayerMaxHP += 10;
                        p.PlayerHP     = Mathf.Min(p.PlayerHP + 10, p.PlayerMaxHP);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "La bourse mystérieuse",
                    text      = "Vous apercevez une bourse abandonnée au bord du chemin.\nElle semble bien garnie... mais à qui appartient-elle ?",
                    btn1Label = "Prendre la bourse",
                    btn2Label = "L'ignorer",
                    action1   = () =>
                    {
                        if (p == null) return;
                        if (Random.value < 0.70f) p.AddGold(30);
                        else                     { p.PlayerHP = Mathf.Max(1, p.PlayerHP - 15); p.SaveToDisk(); }
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Le vieux maître",
                    text      = "Un vieux guerrier vous propose de vous enseigner une technique rare.\nIl exige en échange que vous oubliiez une de vos habitudes les plus basiques.",
                    btn1Label = "Accepter l'enseignement",
                    btn2Label = "Décliner",
                    action1   = () =>
                    {
                        if (p == null) return;
                        var registry = Resources.Load<CardRegistry>("CardRegistry");
                        if (registry == null) return;

                        // Retirer une carte Commune du deck si possible
                        var common = p.PlayerDeck?.Find(c => c != null && c.rarity == CardRarity.Common);
                        if (common != null) p.PlayerDeck.Remove(common);

                        // Ajouter une carte Uncommon aléatoire du registre
                        var uncommons = registry.allCards?.FindAll(c => c != null && c.rarity == CardRarity.Uncommon);
                        if (uncommons != null && uncommons.Count > 0)
                            p.AddCardToDeck(uncommons[Random.Range(0, uncommons.Count)]);

                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Le voleur embusqué",
                    text      = "Un brigand surgit de l'ombre et exige votre bourse !\n\nVous pouvez vous battre ou fuir. Dans les deux cas, vous en sortirez marqué.",
                    btn1Label = "Se battre (-5 HP, +25 or)",
                    btn2Label = "Fuir (-10 HP)",
                    action1   = () =>
                    {
                        if (p == null) return;
                        p.PlayerHP = Mathf.Max(1, p.PlayerHP - 5);
                        p.AddGold(25);
                        p.SaveToDisk();
                    },
                    action2   = () =>
                    {
                        if (p == null) return;
                        p.PlayerHP = Mathf.Max(1, p.PlayerHP - 10);
                        p.SaveToDisk();
                    }
                },
                new EventData
                {
                    title     = "La fontaine de jouvence",
                    text      = "Une fontaine de pierre ornée de lierre vous apparaît dans la clairière.\nSon eau est d'une clarté exceptionnelle.",
                    btn1Label = "Boire l'eau (+20 HP)",
                    btn2Label = "Ne pas y toucher",
                    action1   = () =>
                    {
                        if (p == null) return;
                        p.PlayerHP = Mathf.Min(p.PlayerMaxHP, p.PlayerHP + 20);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "L'hommage à César",
                    text      = "Vous croisez une statue de Jules César dressée en plein chemin.\nDes pèlerins y déposent des offrandes.",
                    btn1Label = "Dérober les offrandes (+35 or, risqué)",
                    btn2Label = "Passer votre chemin",
                    action1   = () =>
                    {
                        if (p == null) return;
                        if (Random.value < 0.65f) p.AddGold(35);
                        else                     { p.PlayerHP = Mathf.Max(1, p.PlayerHP - 20); p.SaveToDisk(); }
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
                new EventData
                {
                    title     = "Le messager royal",
                    text      = "Un messager vous remet un pli scellé.\n\"Mission urgente pour Léonard de Vinci. Récompense à la clé — mais le danger est réel.\"",
                    btn1Label = "Accepter la mission (+30 or, -10 HP)",
                    btn2Label = "Décliner",
                    action1   = () =>
                    {
                        if (p == null) return;
                        p.AddGold(30);
                        p.PlayerHP = Mathf.Max(1, p.PlayerHP - 10);
                        p.SaveToDisk();
                    },
                    action2   = () => { }
                },
            };
        }

        // ── MYSTERY ───────────────────────────────────────────────────────────

        private void ShowMystery()
        {
            float roll = Random.value;
            if      (roll < 0.40f) ShowRest();
            else if (roll < 0.75f) ShowEvent();
            else                   ShowShop();
        }

        // ── Helpers UI ────────────────────────────────────────────────────────

        private GameObject MakeOverlay(string name)
        {
            var canvas = FindObjectOfType<Canvas>();
            var go     = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            SetAnchors(go, 0, 0, 1, 1);
            go.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.96f);
            go.transform.SetAsLastSibling();
            _activeOverlay = go;
            return go;
        }

        private void MakeTitle(GameObject parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, 0.10f, 0.83f, 0.90f, 0.96f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 38f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(0.95f, 0.82f, 0.35f);
        }

        private void MakeSubtitle(GameObject parent, string text)
        {
            var go = new GameObject("Subtitle", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, 0.10f, 0.74f, 0.90f, 0.83f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 16f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(0.80f, 0.78f, 0.72f);
        }

        private Button MakeButton(GameObject parent, string name,
            float x0, float y0, float x1, float y1, Color bg, string label)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, x0, y0, x1, y1);
            go.AddComponent<Image>().color = bg;
            var btn = go.AddComponent<Button>();

            var lbl = new GameObject("Label", typeof(RectTransform));
            lbl.transform.SetParent(go.transform, false);
            SetAnchors(lbl, 0, 0, 1, 1);
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 17f;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.color         = Color.white;
            tmp.raycastTarget = false;
            return btn;
        }

        private void ShowCardGrid(GameObject parent, List<CardData> cards, System.Action<CardData> onPick, bool showSellPrice = false)
        {
            int   count  = Mathf.Min(cards.Count, 5);
            float cardW  = 0.14f, gap = 0.03f;
            float total  = count * cardW + (count - 1) * gap;
            float startX = 0.5f - total / 2f;

            for (int i = 0; i < count; i++)
            {
                var   card = cards[i];
                float x0   = startX + i * (cardW + gap);
                float x1   = x0 + cardW;

                var cardGO = new GameObject($"Card_{i}", typeof(RectTransform));
                cardGO.transform.SetParent(parent.transform, false);
                SetAnchors(cardGO, x0, 0.18f, x1, 0.72f);
                cardGO.AddComponent<Image>();
                var btn = cardGO.AddComponent<Button>();
                CardUIBuilder.ApplyTemplate(card, cardGO);

                // Prix de vente — label sous la carte
                if (showSellPrice)
                {
                    var priceGO = new GameObject($"SellPrice_{i}", typeof(RectTransform));
                    priceGO.transform.SetParent(parent.transform, false);
                    SetAnchors(priceGO, x0, 0.09f, x1, 0.18f);
                    var priceTMP = priceGO.AddComponent<TextMeshProUGUI>();
                    priceTMP.text      = $"Vendre : {GetCardPrice(card.rarity) / 2} or";
                    priceTMP.fontSize  = 13f;
                    priceTMP.fontStyle = FontStyles.Bold;
                    priceTMP.alignment = TextAlignmentOptions.Center;
                    priceTMP.color     = new Color(0.95f, 0.82f, 0.35f);
                    priceTMP.raycastTarget = false;
                }

                var captured = card;
                btn.onClick.AddListener(() => onPick(captured));
            }
        }

        private void AddLabel(GameObject parent, string name,
            float x0, float y0, float x1, float y1,
            string text, float size, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            SetAnchors(go, x0, y0, x1, y1);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = size;
            tmp.fontStyle     = style;
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.color         = color;
            tmp.raycastTarget = false;
        }

        private static void SetAnchors(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rt       = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
