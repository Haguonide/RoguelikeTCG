using UnityEditor;
using UnityEngine;
using RoguelikeTCG.UI;

[CustomEditor(typeof(HexBackgroundScroller))]
public class HexBackgroundScrollerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Effacer", GUILayout.Height(24)))
            ((HexBackgroundScroller)target).Clear();
    }
}
