using System;
using System.IO;
using UnityEngine;

namespace RoguelikeTCG.Core
{
    public class SessionLogger : MonoBehaviour
    {
        public static SessionLogger Instance { get; private set; }

        private StreamWriter _writer;
        private string _filePath;
        private bool _isSaved;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartSession()
        {
            string dir = Path.Combine(Application.persistentDataPath, "logs");
            Directory.CreateDirectory(dir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _filePath = Path.Combine(dir, $"session_{timestamp}.txt");

            _writer = new StreamWriter(_filePath, append: false, System.Text.Encoding.UTF8);
            _isSaved = false;

            _writer.WriteLine($"=== SESSION DE COMBAT — {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            _writer.WriteLine($"Dossier : {_filePath}");
            _writer.WriteLine();
            _writer.Flush();

            Debug.Log($"[SessionLogger] Logs enregistrés dans : {_filePath}");
        }

        public void Write(string entry)
        {
            if (_writer == null) return;
            _writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {entry}");
            _writer.Flush();
        }

        // Appelé normalement à la fin du combat (victoire/défaite)
        public void EndSession(string outcome)
        {
            if (_writer == null || _isSaved) return;
            _writer.WriteLine();
            _writer.WriteLine($"=== FIN : {outcome} — {DateTime.Now:HH:mm:ss} ===");
            Close();
            Debug.Log($"[SessionLogger] Session terminée ({outcome}) — {_filePath}");
        }

        // Appelé par le bouton "Bug" : marque explicitement le rapport comme bug
        public void SaveAsBugReport()
        {
            if (_writer == null || _isSaved) return;
            _writer.WriteLine();
            _writer.WriteLine($"=== ⚠ BUG REPORT — arrêt manuel à {DateTime.Now:HH:mm:ss} ===");
            Close();

            // Renomme le fichier pour le distinguer facilement
            string bugPath = _filePath.Replace("session_", "BUG_");
            try { File.Move(_filePath, bugPath); _filePath = bugPath; }
            catch { /* si le fichier existe déjà, garde le nom original */ }

            Debug.Log($"[SessionLogger] ⚠ Bug report sauvegardé : {_filePath}");
        }

        public string FilePath => _filePath;

        private void Close()
        {
            _isSaved = true;
            _writer?.Close();
            _writer = null;
        }

        private void OnDestroy()
        {
            if (_writer != null && !_isSaved)
                EndSession("session interrompue");
        }
    }
}
