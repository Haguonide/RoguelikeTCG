using System;
using System.IO;
using UnityEngine;
using RoguelikeTCG.Core;

namespace RoguelikeTCG.SaveSystem
{
    [Serializable]
    public class AccountSaveData
    {
        public int accountLevel       = 1;
        public int currentXP          = 0;
        public int totalXPEarned      = 0;
        public int totalRunsCompleted = 0;
    }

    public static class AccountSave
    {
        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, "account_save.json");

        public static void Save(AccountData data)
        {
            var saveData = new AccountSaveData
            {
                accountLevel       = data.AccountLevel,
                currentXP          = data.CurrentXP,
                totalXPEarned      = data.TotalXPEarned,
                totalRunsCompleted = data.TotalRunsCompleted,
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(saveData, prettyPrint: true));
            Debug.Log($"[AccountSave] Sauvegardé → {SavePath}");
        }

        public static void LoadInto(AccountData data)
        {
            if (!File.Exists(SavePath)) return;

            AccountSaveData saveData;
            try
            {
                saveData = JsonUtility.FromJson<AccountSaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountSave] Impossible de charger : {e.Message}");
                return;
            }

            data.SetData(saveData.accountLevel, saveData.currentXP,
                         saveData.totalXPEarned, saveData.totalRunsCompleted);
            Debug.Log($"[AccountSave] Compte chargé — Niv.{saveData.accountLevel}, {saveData.currentXP} XP");
        }
    }
}
