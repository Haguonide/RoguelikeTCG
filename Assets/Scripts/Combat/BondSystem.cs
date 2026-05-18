using System;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public enum BondType
    {
        None,
        Embrasement,    // Fire+Fire     : +1 ATK, splash 1 dmg colonnes c±1
        Blizzard,       // Ice+Ice       : Freeze la cible
        Surcharge,      // Lightning+Lightning : si cible a déjà subi des dégâts, +2 ATK
        Abime,          // Shadow+Shadow : drain 1 HP à l'attaquant
        ForetDense,     // Nature+Nature : +1 HP max aux deux unités (appliqué à la pose)
        EclairArdent,   // Lightning+Fire ou Fire+Lightning : attaque saute sur colonne c±1
        FoudreNoire,    // Lightning+Shadow ou Shadow+Lightning : ignore HP bonus cible
        GivreVivant,    // Ice+Nature ou Nature+Ice : Nature régénère 1 HP debut de tour joueur
        Cendres,        // Fire+Shadow ou Shadow+Fire : si cible meurt, Shadow adjacente +1 ATK
        Decomposition,  // Nature+Shadow ou Shadow+Nature : HP max ennemies en face -1
    }

    public enum BondTiming { None, OnAttack, Passive }

    public static class BondSystem
    {
        // ── Détermination du bond ─────────────────────────────────────────────

        public static BondType GetBond(Element a, Element b)
        {
            if (a == Element.None || b == Element.None) return BondType.None;

            if (a == b)
            {
                return a switch
                {
                    Element.Fire      => BondType.Embrasement,
                    Element.Ice       => BondType.Blizzard,
                    Element.Lightning => BondType.Surcharge,
                    Element.Shadow    => BondType.Abime,
                    Element.Nature    => BondType.ForetDense,
                    _                 => BondType.None,
                };
            }

            // Paires mixtes (commutatives)
            bool isLF = (a == Element.Lightning && b == Element.Fire)
                     || (a == Element.Fire      && b == Element.Lightning);
            if (isLF) return BondType.EclairArdent;

            bool isLS = (a == Element.Lightning && b == Element.Shadow)
                     || (a == Element.Shadow    && b == Element.Lightning);
            if (isLS) return BondType.FoudreNoire;

            bool isIN = (a == Element.Ice    && b == Element.Nature)
                     || (a == Element.Nature && b == Element.Ice);
            if (isIN) return BondType.GivreVivant;

            bool isFS = (a == Element.Fire   && b == Element.Shadow)
                     || (a == Element.Shadow && b == Element.Fire);
            if (isFS) return BondType.Cendres;

            bool isNS = (a == Element.Nature && b == Element.Shadow)
                     || (a == Element.Shadow && b == Element.Nature);
            if (isNS) return BondType.Decomposition;

            return BondType.None;
        }

        public static BondTiming GetTiming(BondType bond) => bond switch
        {
            BondType.ForetDense    => BondTiming.Passive,
            BondType.GivreVivant   => BondTiming.Passive,
            BondType.Decomposition => BondTiming.Passive,
            BondType.None          => BondTiming.None,
            _                      => BondTiming.OnAttack,
        };

        // ── Application des bonds d'attaque ──────────────────────────────────

        /// <summary>
        /// Applique l'effet d'un bond d'attaque.
        /// attacker est en [row, col], neighbor est son voisin sur la même ligne,
        /// target est l'unité ennemie en face (peut être null).
        /// Modifie ref dmg, peut appliquer des effets secondaires.
        /// </summary>
        public static void ApplyAttackBond(
            BondType bond,
            CardInstance attacker,
            CardInstance neighbor,
            CardInstance target,
            GridManager grid,
            ref int dmg,
            Action<string> log)
        {
            switch (bond)
            {
                case BondType.Embrasement:
                {
                    // +1 ATK
                    dmg += 1;
                    log?.Invoke($"  [Bond Embrasement] +1 ATK → {dmg} dmg");

                    // Splash 1 dmg sur les colonnes c±1 de la ligne ennemie
                    int row = IsPlayerRow(attacker) ? 0 : 1; // ligne ennemie
                    int col = attacker.col;
                    SplashDamage(grid, row, col - 1, 1, attacker.isPlayerCard, log);
                    SplashDamage(grid, row, col + 1, 1, attacker.isPlayerCard, log);
                    break;
                }

                case BondType.Blizzard:
                {
                    if (target != null)
                    {
                        target.isFrozen = true;
                        log?.Invoke($"  [Bond Blizzard] {target.data.cardName} gelé — passera son prochain tour d'attaque");
                    }
                    break;
                }

                case BondType.Surcharge:
                {
                    if (target != null && target.tookDamageThisTurn)
                    {
                        dmg += 2;
                        log?.Invoke($"  [Bond Surcharge] Cible déjà touchée ce tour → +2 ATK ({dmg} dmg)");
                    }
                    break;
                }

                case BondType.Abime:
                {
                    // Drain : soigne 1 HP à l'attaquant
                    if (attacker != null && target != null)
                    {
                        int healed = System.Math.Min(1, attacker.data.hp - attacker.currentHP);
                        if (healed > 0)
                        {
                            attacker.currentHP += healed;
                            log?.Invoke($"  [Bond Abîme] {attacker.data.cardName} récupère {healed} HP → {attacker.currentHP}/{attacker.data.hp}");
                        }
                    }
                    break;
                }

                case BondType.EclairArdent:
                {
                    // Après impact principal, attaque saute sur c±1 (1 dmg)
                    // La propagation est gérée dans CombatManager après l'attaque principale
                    log?.Invoke($"  [Bond Éclair Ardent] L'attaque va ricocher sur les colonnes adjacentes");
                    break;
                }

                case BondType.FoudreNoire:
                {
                    // Ignore les HP bonus : ramène currentHP à data.hp si supérieur
                    if (target != null && target.currentHP > target.data.hp)
                    {
                        int excess = target.currentHP - target.data.hp;
                        target.currentHP = target.data.hp;
                        log?.Invoke($"  [Bond Foudre Noire] HP bonus de {target.data.cardName} annulés (-{excess})");
                    }
                    break;
                }

                case BondType.Cendres:
                {
                    // Le kill est géré après — le flag est posé dans CombatManager
                    log?.Invoke($"  [Bond Cendres] Si la cible meurt, Shadow adjacente gagne +1 ATK");
                    break;
                }
            }
        }

        // ── Application des bonds passifs ─────────────────────────────────────

        /// <summary>
        /// Vérifie et applique les bonds passifs pour une ligne entière.
        /// Appeler au début du tour joueur (GivreVivant) et à la pose (ForetDense, Decomposition).
        /// </summary>
        public static void RefreshPassiveBonds(GridManager grid, Action<string> log)
        {
            // Parcourir la ligne joueur (row 1) pour GivreVivant et ForetDense
            for (int c = 0; c < GridManager.COLS; c++)
            {
                var unit = grid.GetUnit(1, c);
                if (unit == null) continue;

                // GivreVivant : Nature adjacente à Ice régénère 1 HP
                if (unit.data.element == Element.Nature)
                {
                    bool adjacentToIce = false;
                    if (c > 0)
                    {
                        var left = grid.GetUnit(1, c - 1);
                        if (left != null && left.data.element == Element.Ice) adjacentToIce = true;
                    }
                    if (!adjacentToIce && c < GridManager.COLS - 1)
                    {
                        var right = grid.GetUnit(1, c + 1);
                        if (right != null && right.data.element == Element.Ice) adjacentToIce = true;
                    }

                    if (adjacentToIce && unit.currentHP < unit.data.hp)
                    {
                        unit.currentHP++;
                        log?.Invoke($"  [Bond Givre Vivant] {unit.data.cardName} régénère 1 HP → {unit.currentHP}/{unit.data.hp}");
                    }
                }
            }
        }

        /// <summary>
        /// Applique les bonds passifs lors de la pose d'une unité (ForetDense, Decomposition).
        /// Appeler après PlaceUnit dans CombatManager.
        /// </summary>
        public static void ApplyOnPlaceBonds(CardInstance placed, GridManager grid, Action<string> log)
        {
            int row = placed.row;
            int col = placed.col;

            // Vérifie les voisins gauche/droite
            int[] neighborCols = new[] { col - 1, col + 1 };
            foreach (int nc in neighborCols)
            {
                if (!GridManager.InBounds(row, nc)) continue;
                var neighbor = grid.GetUnit(row, nc);
                if (neighbor == null) continue;
                if (neighbor.isPlayerCard != placed.isPlayerCard) continue; // bonds entre alliés seulement

                var bond = GetBond(placed.data.element, neighbor.data.element);

                switch (bond)
                {
                    case BondType.ForetDense:
                    {
                        // +1 HP max aux deux unités (appliqué une seule fois à la pose de la 2e)
                        placed.currentHP   = System.Math.Min(placed.data.hp + 1, placed.currentHP + 1);
                        neighbor.currentHP = System.Math.Min(neighbor.data.hp + 1, neighbor.currentHP + 1);
                        log?.Invoke($"  [Bond Forêt Dense] {placed.data.cardName} et {neighbor.data.cardName} +1 HP");
                        break;
                    }

                    case BondType.Decomposition:
                    {
                        // Réduit le HP max des unités ennemies en face des deux unités Nature+Shadow
                        int enemyRow = placed.isPlayerCard ? 0 : 1;
                        ApplyDecompositionToCol(grid, enemyRow, col, log);
                        ApplyDecompositionToCol(grid, enemyRow, nc, log);
                        break;
                    }
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool IsPlayerRow(CardInstance unit) => unit.isPlayerCard;

        private static void SplashDamage(GridManager grid, int row, int col, int dmg, bool killedByPlayer, Action<string> log)
        {
            if (!GridManager.InBounds(row, col)) return;
            var target = grid.GetUnit(row, col);
            if (target == null) return;
            target.TakeDamage(dmg);
            log?.Invoke($"  [Splash] {target.data.cardName} ({row},{col}) reçoit {dmg} dégât(s) → HP {target.currentHP}");
            if (target.currentHP <= 0)
            {
                grid.RemoveUnit(row, col);
                log?.Invoke($"  [Splash] {target.data.cardName} détruit par splash !");
            }
        }

        private static void ApplyDecompositionToCol(GridManager grid, int enemyRow, int col, Action<string> log)
        {
            if (!GridManager.InBounds(enemyRow, col)) return;
            var enemy = grid.GetUnit(enemyRow, col);
            if (enemy == null) return;
            // Réduire le HP max effectif (currentHP est plafonné à data.hp - 1)
            int cap = enemy.data.hp - 1;
            if (cap <= 0) cap = 1;
            if (enemy.currentHP > cap)
            {
                enemy.currentHP = cap;
                log?.Invoke($"  [Bond Décomposition] {enemy.data.cardName} HP max réduit → {enemy.currentHP}/{cap}");
            }
        }
    }
}
