using UnityEditor;
using UnityEngine;

namespace FuzzyTools
{
    [CustomEditor(typeof(MeshMerger))]
    public class MeshManagerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = target as MeshMerger;
            if (!script) return;
            EditorGUI.BeginDisabledGroup(script.skipLODObjects);
            script.selectedLODOption = GUILayout.Toolbar(script.selectedLODOption, script.LODOptions, "Radio");
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Find Mergeable Meshes"))
            {
                script.FindMeshes();
            }
        }
    }
}