using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionSystemConfig))]
public class MissionSystemConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MissionSystemConfig config = (MissionSystemConfig)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mission System Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_systemName"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("System Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoStartOnLoad"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_allowSimultaneousChains"));
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField($"Parallel Chains ({config.ParallelChains?.Count ?? 0})", 
            EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_parallelChains"), true);
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Validate System"))
        {
            bool isValid = config.Validate();
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", 
                    "System configuration is valid!", "OK");
            }
        }
        
        if (GUILayout.Button("Create Test Manager"))
        {
            GameObject managerGO = new GameObject("MissionManager_Test");
            var manager = managerGO.AddComponent<MissionManager>();
            
            var configField = typeof(MissionManager).GetField("_systemConfig", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(manager, config);
            
            Selection.activeGameObject = managerGO;
            EditorGUIUtility.PingObject(managerGO);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (config.ParallelChains != null && config.ParallelChains.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("System Structure:", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Parallel Execution:");
            
            foreach (var chain in config.ParallelChains)
            {
                if (chain != null)
                {
                    EditorGUILayout.LabelField($"• {chain.ChainName} ({chain.Missions?.Count ?? 0} missions)");
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}