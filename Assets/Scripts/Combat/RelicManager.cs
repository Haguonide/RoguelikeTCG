using System.Collections.Generic;
using UnityEngine;
using RoguelikeTCG.Core;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    /// <summary>
    /// Lit les reliques actives depuis RunPersistence et expose leurs effets cumulés.
    /// Placé sur le GameManager de la scène Combat.
    /// </summary>
    public class RelicManager : MonoBehaviour
    {
        public static RelicManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private List<RelicData> ActiveRelics =>
            RunPersistence.Instance?.PlayerRelics ?? new List<RelicData>();

        // ── Effets cumulés ────────────────────────────────────────────────────

        public int GetExtraDrawPerTurn()  => SumEffect(RelicEffect.DrawExtraCardPerTurn);
        public int GetBonusStartMana()    => SumEffect(RelicEffect.StartWithBonusMana);
        public int GetHealAfterCombat()   => SumEffect(RelicEffect.HealAfterCombat);

        private int SumEffect(RelicEffect effect)
        {
            int total = 0;
            foreach (var r in ActiveRelics)
                if (r != null && r.effect == effect)
                    total += r.effectValue;
            return total;
        }
    }
}
