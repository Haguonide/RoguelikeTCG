using UnityEngine;
using RoguelikeTCG.Combat;

namespace RoguelikeTCG.UI
{
    public class BugReportButton : MonoBehaviour
    {
        public void OnClick()
        {
            CombatManager.Instance?.SaveBugReport();
            Debug.Log("[BugReport] Rapport sauvegardé !");
        }
    }
}
