using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RoguelikeTCG.UI
{
    public class CombatLog : MonoBehaviour
    {
        public TextMeshProUGUI logText;
        public int maxLines = 40;

        private List<string> lines = new();

        public void AddEntry(string message)
        {
            lines.Add(message);
            if (lines.Count > maxLines)
                lines.RemoveAt(0);
            if (logText != null)
                logText.text = string.Join("\n", lines);
        }
    }
}
