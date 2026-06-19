using System;
using RoguelikeTCG.Cards;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public class BoardManager
    {
        public const int SlotCount = 5;

        public CardInstance[] PlayerUnits { get; } = new CardInstance[SlotCount];
        public CardInstance[] EnemyUnits { get; } = new CardInstance[SlotCount];
        public CardInstance PlayerTerrain { get; private set; }
        public CardInstance EnemyTerrain { get; private set; }

        // (carte, camp propriétaire)
        public event Action<CardInstance, TurnSide> OnUnitDied;
        // (ancien terrain, camp propriétaire) — déclenché quand un terrain est remplacé
        public event Action<CardInstance, TurnSide> OnTerrainDiscarded;
        // (dégâts, camp du héros qui les reçoit)
        public event Action<int, TurnSide> OnDirectDamageToHero;
        // (dégâts, camp attaquant) — tout dégât infligé, unité ou direct
        public event Action<int, TurnSide> OnDamageDealt;
        // (carte unité, camp propriétaire) — déclenché juste avant l'attaque de Ruée
        public event Action<CardInstance, TurnSide> OnChargeAttack;
        // déclenché après tout changement d'état du board (pose unité, pose terrain)
        public event Action OnBoardChanged;
        // (index slot, camp propriétaire) — déclenché à chaque pose d'unité
        public event Action<int, TurnSide> OnUnitPlaced;

        public struct SlotAttackPreview
        {
            public bool DefenderDies;
            public bool AttackerDies;
            public bool IsDirect;
        }

        // ── Placement ────────────────────────────────────────────────────────

        public bool PlaceUnit(CardInstance unit, int slot, TurnSide side)
        {
            if (slot < 0 || slot >= SlotCount) return false;

            var slots = GetUnits(side);
            if (slots[slot] != null) return false;

            slots[slot] = unit;
            OnBoardChanged?.Invoke();
            OnUnitPlaced?.Invoke(slot, side);

            if (unit.HasKeyword(KeywordType.Charge))
            {
                OnChargeAttack?.Invoke(unit, side);
                ResolveSlotAttack(slot, side);
            }

            return true;
        }

        public void PlaceTerrain(CardInstance terrain, TurnSide side)
        {
            if (side == TurnSide.Player)
            {
                if (PlayerTerrain != null)
                    OnTerrainDiscarded?.Invoke(PlayerTerrain, TurnSide.Player);
                PlayerTerrain = terrain;
            }
            else
            {
                if (EnemyTerrain != null)
                    OnTerrainDiscarded?.Invoke(EnemyTerrain, TurnSide.Enemy);
                EnemyTerrain = terrain;
            }
            OnBoardChanged?.Invoke();
        }

        public void ClearTerrain(TurnSide side)
        {
            if (side == TurnSide.Player) PlayerTerrain = null;
            else EnemyTerrain = null;
        }

        // ── Résolution des attaques ───────────────────────────────────────────

        public void ResolveAttacks(TurnSide attackingSide)
        {
            for (int i = 0; i < SlotCount; i++)
                ResolveSlotAttack(i, attackingSide);
        }

        public SlotAttackPreview PreviewSlotAttack(int slot, TurnSide attackingSide)
        {
            var attackers = GetUnits(attackingSide);
            var defenders = GetUnits(Opposite(attackingSide));

            CardInstance attacker = attackers[slot];
            if (attacker == null) return new SlotAttackPreview();

            CardInstance defender = defenders[slot];

            if (defender == null)
                return new SlotAttackPreview { IsDirect = true };

            bool defenderDies = defender.CurrentHP - attacker.CurrentATK <= 0;
            bool attackerDies = false;
            if (!defenderDies)
                attackerDies = attacker.CurrentHP - defender.CurrentATK <= 0;

            return new SlotAttackPreview
            {
                DefenderDies = defenderDies,
                AttackerDies = attackerDies,
                IsDirect = false
            };
        }

        public void ResolveSlotAttack(int slot, TurnSide attackingSide)
        {
            var attackers = GetUnits(attackingSide);
            var defenders = GetUnits(Opposite(attackingSide));
            TurnSide defendingSide = Opposite(attackingSide);

            CardInstance attacker = attackers[slot];
            if (attacker == null) return;

            CardInstance defender = defenders[slot];

            if (defender != null)
            {
                defender.TakeDamage(attacker.CurrentATK);
                OnDamageDealt?.Invoke(attacker.CurrentATK, attackingSide);
                if (!defender.IsAlive)
                {
                    defenders[slot] = null;
                    OnUnitDied?.Invoke(defender, defendingSide);
                }
                else
                {
                    // Contre-attaque
                    attacker.TakeDamage(defender.CurrentATK);
                    if (!attacker.IsAlive)
                    {
                        attackers[slot] = null;
                        OnUnitDied?.Invoke(attacker, attackingSide);
                    }
                }
            }
            else
            {
                // Case vide → attaque directe au héros ennemi
                OnDirectDamageToHero?.Invoke(attacker.CurrentATK, defendingSide);
                OnDamageDealt?.Invoke(attacker.CurrentATK, attackingSide);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        public CardInstance[] GetUnits(TurnSide side) =>
            side == TurnSide.Player ? PlayerUnits : EnemyUnits;

        public int GetUnitSlot(CardInstance unit, TurnSide side)
        {
            var slots = GetUnits(side);
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] == unit) return i;
            return -1;
        }

        public int GetFirstEmptySlot(TurnSide side)
        {
            var slots = GetUnits(side);
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] == null) return i;
            return -1;
        }

        public static TurnSide Opposite(TurnSide side) =>
            side == TurnSide.Player ? TurnSide.Enemy : TurnSide.Player;
    }
}
