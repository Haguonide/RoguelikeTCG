using System;
using System.Collections.Generic;
using RoguelikeTCG.Data;

namespace RoguelikeTCG.Combat
{
    public class RuneSystem
    {
        private readonly Dictionary<RuneType, int> _runes = new();

        // (type, nouveau total)
        public event Action<RuneType, int> OnRunesChanged;

        public int Get(RuneType type) =>
            _runes.TryGetValue(type, out int v) ? v : 0;

        public void Add(RuneType type, int amount = 1)
        {
            _runes.TryGetValue(type, out int current);
            _runes[type] = current + amount;
            OnRunesChanged?.Invoke(type, _runes[type]);
        }

        public bool Spend(RuneType type, int amount)
        {
            if (Get(type) < amount) return false;
            _runes[type] -= amount;
            OnRunesChanged?.Invoke(type, _runes[type]);
            return true;
        }

        // Sérialisation pour RunPersistence
        public Dictionary<RuneType, int> GetAll() => new(_runes);

        public void LoadFrom(Dictionary<RuneType, int> saved)
        {
            _runes.Clear();
            foreach (var kv in saved)
                _runes[kv.Key] = kv.Value;
        }
    }
}
