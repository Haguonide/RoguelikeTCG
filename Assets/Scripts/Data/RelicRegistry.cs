using System.Collections.Generic;
using UnityEngine;

namespace RoguelikeTCG.Data
{
    [CreateAssetMenu(fileName = "RelicRegistry", menuName = "RoguelikeTCG/RelicRegistry")]
    public class RelicRegistry : ScriptableObject
    {
        public List<RelicData> allRelics = new List<RelicData>();

        public RelicData FindByName(string relicName)
        {
            foreach (var r in allRelics)
                if (r != null && r.relicName == relicName)
                    return r;
            return null;
        }
    }
}
