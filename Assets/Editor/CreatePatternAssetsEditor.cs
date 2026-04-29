using UnityEngine;
using UnityEditor;
using RoguelikeTCG.Data;

/// <summary>
/// Menu Editor pour générer les 27 ScriptableObjects PatternData dans Assets/Data/Patterns/.
/// Menu : RoguelikeTCG / Generate Pattern Assets
/// </summary>
public static class CreatePatternAssetsEditor
{
    private const string OutputFolder = "Assets/Data/Patterns";

    [MenuItem("RoguelikeTCG/Generate Pattern Assets")]
    public static void GenerateAllPatterns()
    {
        System.IO.Directory.CreateDirectory(
            System.IO.Path.Combine(Application.dataPath, "../" + OutputFolder));
        AssetDatabase.Refresh();

        var patterns = GetPatternDefinitions();
        int created  = 0;
        int skipped  = 0;

        foreach (var (name, indices) in patterns)
        {
            string path = $"{OutputFolder}/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<PatternData>(path) != null)
            {
                skipped++;
                continue;
            }

            var asset = ScriptableObject.CreateInstance<PatternData>();
            asset.patternName = name;
            asset.cellIndices = indices;
            AssetDatabase.CreateAsset(asset, path);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[PatternAssets] {created} asset(s) créé(s), {skipped} déjà existant(s). Dossier : {OutputFolder}");
    }

    private static (string name, int[] indices)[] GetPatternDefinitions()
    {
        return new (string, int[])[]
        {
            // ── 3 cases (4 pts) ───────────────────────────────────────────────
            // Lignes horizontales
            ("Ligne_0",    new[] { 0, 1, 2 }),
            ("Ligne_1",    new[] { 3, 4, 5 }),
            ("Ligne_2",    new[] { 6, 7, 8 }),
            // Colonnes
            ("Col_0",      new[] { 0, 3, 6 }),
            ("Col_1",      new[] { 1, 4, 7 }),
            ("Col_2",      new[] { 2, 5, 8 }),
            // Diagonales
            ("Diag_TL_BR", new[] { 0, 4, 8 }),
            ("Diag_TR_BL", new[] { 2, 4, 6 }),
            // Coins L
            ("Coin_TL",    new[] { 0, 1, 3 }),
            ("Coin_TR",    new[] { 1, 2, 5 }),
            ("Coin_BL",    new[] { 3, 6, 7 }),
            ("Coin_BR",    new[] { 5, 7, 8 }),

            // ── 4 cases (6 pts) ───────────────────────────────────────────────
            ("Carre_TL",   new[] { 0, 1, 3, 4 }),
            ("Carre_TR",   new[] { 1, 2, 4, 5 }),
            ("Carre_BL",   new[] { 3, 4, 6, 7 }),
            ("Carre_BR",   new[] { 4, 5, 7, 8 }),
            ("4_Coins",    new[] { 0, 2, 6, 8 }),
            ("T_Haut",     new[] { 1, 3, 4, 5 }),
            ("T_Bas",      new[] { 3, 4, 5, 7 }),
            ("T_Gauche",   new[] { 1, 3, 4, 7 }),
            ("T_Droite",   new[] { 1, 4, 5, 7 }),
            ("L_TL",       new[] { 0, 3, 6, 7 }),

            // ── 5 cases (9 pts) ───────────────────────────────────────────────
            ("Croix",      new[] { 1, 3, 4, 5, 7 }),
            ("X",          new[] { 0, 2, 4, 6, 8 }),
            ("U_Bas",      new[] { 3, 5, 6, 7, 8 }),
            ("U_Gauche",   new[] { 0, 1, 3, 6, 7 }),
            ("Z",          new[] { 0, 1, 4, 7, 8 }),
        };
    }
}
