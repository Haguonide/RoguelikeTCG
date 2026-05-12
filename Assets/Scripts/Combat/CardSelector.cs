using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;
using RoguelikeTCG.UI;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Gère la sélection des cartes en main et le ciblage sur la grille 3×3.
    /// Remplace l'ancien système basé sur LaneSlotUI.
    /// </summary>
    public class CardSelector : MonoBehaviour
    {
        public static CardSelector Instance { get; private set; }

        private enum Mode
        {
            None,
            AwaitingGridCell,           // pose d'une unité
            AwaitingHero,               // sort ciblant un héros
            AwaitingAllyUnit,           // sort ciblant une unité alliée
            AwaitingEnemyUnit,          // sort ciblant une unité ennemie
            AwaitingAoEConfirm,         // sort AoE sur toutes unités ennemies (confirme en cliquant n'importe où)
            AwaitingAllAllyUnits,       // sort AoE sur toutes unités alliées
            AwaitingBricolage,          // obsolète
            AwaitingUtilityAllyUnit,    // Carte Déplacement — sélection de l'unité alliée à déplacer
            AwaitingUtilityTargetCell,  // Carte Déplacement — sélection de la case adjacente vide de destination
        }

        private CardView    _selectedView;
        private Mode        _mode = Mode.None;
        private GridCellUI  _utilitySourceCell; // cellule source pour la Carte Déplacement

        private static readonly Color HighlightCard  = new Color(1f, 0.85f, 0.1f, 1f);
        private static readonly Color HighlightSlot  = new Color(0.2f, 1f, 0.35f, 0.7f);
        private static readonly Color HighlightEnemy = new Color(1f, 0.3f, 0.15f, 0.7f);
        private static readonly Color HighlightAny   = new Color(1f, 0.9f, 0.4f, 0.7f);

        private readonly List<GridCellUI> _highlightedCells = new();

        [SerializeField] private SpellArrowUI spellArrow;
        [SerializeField] public  Sprite       targetSprite;
        public Sprite TargetSprite => targetSprite;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (spellArrow != null && IsSpellMode(_mode))
                spellArrow.UpdateArrow(Input.mousePosition);
        }

        public bool HasSelection => _selectedView != null || _mode == Mode.AwaitingBricolage;

        // ── Sélection depuis la main ──────────────────────────────────────────

        public void SelectCard(CardView view)
        {
            if (_selectedView == view) { Deselect(); return; }
            AudioManager.Instance.PlaySFX("sfx_card_select");

            Deselect();
            _selectedView = view;
            _selectedView.CancelHover();
            ApplyCardHighlight(true);

            var card = _selectedView?.CardInstance;
            if (card == null) { Deselect(); return; }

            if (card.IsUnit)
            {
                _mode = Mode.AwaitingGridCell;
            }
            else if (card.data.cardType == CardType.Utility)
            {
                _mode = Mode.AwaitingUtilityAllyUnit;
            }
            else
            {
                _mode = card.data.spellTarget switch
                {
                    SpellTarget.PlayerHero    => Mode.AwaitingHero,
                    SpellTarget.EnemyHero     => Mode.AwaitingHero,
                    SpellTarget.AllyUnit      => Mode.AwaitingAllyUnit,
                    SpellTarget.EnemyUnit     => Mode.AwaitingEnemyUnit,
                    SpellTarget.AllEnemyUnits => Mode.AwaitingAoEConfirm,
                    SpellTarget.AllAllyUnits  => Mode.AwaitingAllAllyUnits,
                    _                         => Mode.None,
                };
            }

            if (_mode == Mode.AwaitingHero) RefreshHighlights();
            ShowSpellArrow();
        }

        public void SelectBricolage() { } // obsolète

        public void Deselect()
        {
            ApplyCardHighlight(false);
            ClearHighlights();
            _selectedView      = null;
            _mode              = Mode.None;
            _utilitySourceCell = null;
        }

        // ── Clic sur une case de la grille (depuis GridCellUI) ────────────────

        public void OnGridCellClicked(GridCellUI cell)
        {
            if (cell == null) return;


            if (_selectedView == null) return;
            var card = _selectedView.CardInstance;
            if (card == null) return;

            switch (_mode)
            {
                case Mode.AwaitingGridCell:
                    if (cell.IsOccupied) return;
                    if (CombatManager.Instance.TryPlayUnit(card, cell.row, cell.col))
                        Deselect();
                    break;

                case Mode.AwaitingAllyUnit:
                    if (!cell.HasPlayerUnit) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, cell.row, cell.col))
                        Deselect();
                    break;

                case Mode.AwaitingEnemyUnit:
                    if (!cell.HasEnemyUnit) return;
                    if (CombatManager.Instance.TryPlaySpellOnUnit(card, cell.row, cell.col))
                        Deselect();
                    break;

                case Mode.AwaitingAoEConfirm:
                    if (!cell.HasEnemyUnit) return;
                    if (CombatManager.Instance.TryPlayAoESpell(card))
                        Deselect();
                    break;

                case Mode.AwaitingAllAllyUnits:
                    if (!cell.HasPlayerUnit) return;
                    if (CombatManager.Instance.TryPlayAoESpell(card))
                        Deselect();
                    break;

                case Mode.AwaitingUtilityAllyUnit:
                    if (!cell.HasPlayerUnit) return;
                    _utilitySourceCell = cell;
                    _mode = Mode.AwaitingUtilityTargetCell;
                    RefreshHighlights();
                    break;

                case Mode.AwaitingUtilityTargetCell:
                    if (cell.IsOccupied) return;
                    if (_utilitySourceCell == null) { Deselect(); return; }
                    if (!IsOrthogonallyAdjacent(_utilitySourceCell, cell)) return;
                    if (CombatManager.Instance.TryPlayUtility(
                            card,
                            _utilitySourceCell.row, _utilitySourceCell.col,
                            cell.row, cell.col))
                        Deselect();
                    break;
            }
        }

        // ── Clic sur un portrait héros (depuis HeroPortraitUI) ────────────────

        public void OnHeroClicked(bool isPlayerPortrait)
        {
            if (_selectedView == null || _mode != Mode.AwaitingHero) return;
            var card = _selectedView.CardInstance;
            if (card == null) return;

            bool wantsPlayer = card.data.spellTarget == SpellTarget.PlayerHero;
            if (isPlayerPortrait != wantsPlayer) return;

            if (CombatManager.Instance.TryPlaySpellOnHero(card, isPlayerPortrait))
                Deselect();
        }

        // ── Ciblage visuel ────────────────────────────────────────────────────

        public bool IsCellTargetable(GridCellUI cell) => _mode switch
        {
            Mode.AwaitingGridCell          => !cell.IsOccupied,
            Mode.AwaitingAllyUnit          => cell.HasPlayerUnit,
            Mode.AwaitingEnemyUnit         => cell.HasEnemyUnit,
            Mode.AwaitingAoEConfirm        => cell.HasEnemyUnit,
            Mode.AwaitingAllAllyUnits      => cell.HasPlayerUnit,
            Mode.AwaitingUtilityAllyUnit   => cell.HasPlayerUnit,
            Mode.AwaitingUtilityTargetCell => !cell.IsOccupied && _utilitySourceCell != null
                                              && IsOrthogonallyAdjacent(_utilitySourceCell, cell),
            _ => false,
        };

        // ── Hover sur case ────────────────────────────────────────────────────

        public void OnGridCellHoverEnter(GridCellUI cell)
        {
            foreach (var c in _highlightedCells) c.StartShake();

            if (_mode == Mode.None || _selectedView == null) return;

            if (_mode == Mode.AwaitingAoEConfirm && cell.HasEnemyUnit)
            {
                foreach (var c in FindObjectsByType<GridCellUI>(FindObjectsInactive.Include))
                    if (IsCellTargetable(c)) c.ShowTarget();
            }
            else if (_mode == Mode.AwaitingAllAllyUnits && cell.HasPlayerUnit)
            {
                foreach (var c in FindObjectsByType<GridCellUI>(FindObjectsInactive.Include))
                    if (IsCellTargetable(c)) c.ShowTarget();
            }
            else if (IsCellTargetable(cell))
            {
                cell.ShowTarget();
            }
        }

        public void OnGridCellHoverExit(GridCellUI cell)
        {
            foreach (var c in _highlightedCells) c.StopShake();

            if ((_mode == Mode.AwaitingAoEConfirm    && cell.HasEnemyUnit) ||
                (_mode == Mode.AwaitingAllAllyUnits && cell.HasPlayerUnit))
            {
                foreach (var c in FindObjectsByType<GridCellUI>(FindObjectsInactive.Include))
                    c.HideTarget();
            }
        }

        // ── Compatibilité LaneSlotUI (ancien système — no-op dans le nouveau) ──

        public void OnCellClicked(LaneSlotUI slot) { }
        public void OnSlotHoverEnter(LaneSlotUI slot) { }
        public void OnSlotHoverExit(LaneSlotUI slot) { }

        // ── Highlights ────────────────────────────────────────────────────────

        private void RefreshHighlights()
        {
            ClearHighlights();
            var allCells  = FindObjectsByType<GridCellUI>(FindObjectsInactive.Include);
            var allHeroes = FindObjectsByType<HeroPortraitUI>(FindObjectsInactive.Include);

            switch (_mode)
            {
                case Mode.AwaitingGridCell:
                    foreach (var c in allCells)
                        if (!c.IsOccupied)
                            Highlight(c, HighlightSlot);
                    break;

                case Mode.AwaitingBricolage:
                    foreach (var c in allCells)
                        if (!c.IsOccupied)
                            Highlight(c, HighlightSlot);
                    break;

                case Mode.AwaitingAllyUnit:
                    foreach (var c in allCells)
                        if (c.HasPlayerUnit)
                            Highlight(c, HighlightSlot);
                    break;

                case Mode.AwaitingEnemyUnit:
                    foreach (var c in allCells)
                        if (c.HasEnemyUnit)
                            Highlight(c, HighlightEnemy);
                    break;

                case Mode.AwaitingAoEConfirm:
                    foreach (var c in allCells)
                        if (c.HasEnemyUnit)
                            Highlight(c, HighlightEnemy);
                    break;

                case Mode.AwaitingAllAllyUnits:
                    foreach (var c in allCells)
                        if (c.HasPlayerUnit)
                            Highlight(c, HighlightSlot);
                    break;

                case Mode.AwaitingHero:
                    if (_selectedView?.CardInstance == null) break;
                    bool wantsPlayer = _selectedView.CardInstance.data.spellTarget == SpellTarget.PlayerHero;
                    foreach (var h in allHeroes)
                        if (h.isPlayerPortrait == wantsPlayer) h.SetHighlight(true);
                    break;

                case Mode.AwaitingUtilityAllyUnit:
                    foreach (var c in allCells)
                        if (c.HasPlayerUnit)
                            Highlight(c, HighlightSlot);
                    break;

                case Mode.AwaitingUtilityTargetCell:
                    if (_utilitySourceCell == null) break;
                    foreach (var c in allCells)
                    {
                        if (c.IsOccupied) continue;
                        if (IsOrthogonallyAdjacent(_utilitySourceCell, c))
                            Highlight(c, HighlightSlot);
                    }
                    break;
            }
        }

        private void Highlight(GridCellUI cell, Color color)
        {
            cell.SetHighlight(true, color);
            _highlightedCells.Add(cell);
        }

        private void ClearHighlights()
        {
            foreach (var c in _highlightedCells)
                c.SetHighlight(false, Color.clear);
            _highlightedCells.Clear();
            foreach (var h in FindObjectsByType<HeroPortraitUI>(FindObjectsInactive.Include))
                h.SetHighlight(false);
            foreach (var c in FindObjectsByType<GridCellUI>(FindObjectsInactive.Include))
                c.HideTarget();
            spellArrow?.Hide();
        }

        // ── Animation vol carte → case grille ────────────────────────────────

        private void FlyCardToCell(CardView cv, GridCellUI targetCell)
        {
            cv.SetOnGrid(true);

            // Reparenter au Canvas racine pour le z-order pendant le vol
            var canvas = cv.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                while (!canvas.isRootCanvas && canvas.transform.parent != null)
                {
                    var parent = canvas.transform.parent.GetComponentInParent<Canvas>();
                    if (parent == null) break;
                    canvas = parent;
                }
                cv.transform.SetParent(canvas.transform, true);
                cv.transform.SetAsLastSibling();
            }

            var rt         = cv.GetComponent<RectTransform>();
            Vector3 target = targetCell.transform.position;

            // Upright pendant le vol
            rt.DOLocalRotate(Vector3.zero, 0.08f);

            rt.DOMove(target, 0.22f)
              .SetEase(Ease.InCubic)
              .SetLink(cv.gameObject)
              .OnComplete(() =>
              {
                  if (cv == null) return;
                  targetCell.SetPlacedCard(cv);
                  CombatManager.Instance?.RefreshAllUI();
              });
        }

        // ── Flèche sorts ──────────────────────────────────────────────────────

        private static bool IsSpellMode(Mode m) =>
            m != Mode.None && m != Mode.AwaitingBricolage;

        /// <summary>
        /// Retourne true si les deux cellules sont orthogonalement adjacentes (pas de diagonale).
        /// Distance Manhattan == 1, et même ligne ou même colonne.
        /// </summary>
        private static bool IsOrthogonallyAdjacent(GridCellUI a, GridCellUI b)
        {
            int dr = Mathf.Abs(a.row - b.row);
            int dc = Mathf.Abs(a.col - b.col);
            return (dr == 1 && dc == 0) || (dr == 0 && dc == 1);
        }

        private void ShowSpellArrow()
        {
            if (spellArrow == null || _selectedView == null) return;

            Color color = _mode switch
            {
                Mode.AwaitingEnemyUnit        => new Color(1f, 0.5f, 0.2f, 0.85f),
                Mode.AwaitingAoEConfirm       => new Color(1f, 0.5f, 0.2f, 0.85f),
                Mode.AwaitingAllyUnit         => new Color(0.3f, 1f, 0.4f, 0.85f),
                Mode.AwaitingAllAllyUnits     => new Color(0.3f, 1f, 0.4f, 0.85f),
                Mode.AwaitingGridCell         => new Color(0.3f, 0.8f, 1f, 0.85f),  // bleu clair — pose unité
                Mode.AwaitingUtilityAllyUnit  => new Color(0.9f, 0.7f, 0.2f, 0.85f),
                Mode.AwaitingUtilityTargetCell=> new Color(0.9f, 0.7f, 0.2f, 0.85f),
                _                             => new Color(1f, 0.9f, 0.4f, 0.85f),
            };

            spellArrow.SetArrowColor(color);
            spellArrow.Show(_selectedView.GetComponent<RectTransform>());
        }

        // ── Highlight carte sélectionnée ──────────────────────────────────────

        private void ApplyCardHighlight(bool on)
        {
            if (_selectedView == null) return;
            var outline = _selectedView.GetComponent<Outline>();
            if (outline == null && on) outline = _selectedView.gameObject.AddComponent<Outline>();
            if (outline == null) return;
            outline.effectColor    = HighlightCard;
            outline.effectDistance = new Vector2(4, -4);
            outline.enabled        = on;
        }
    }
}
