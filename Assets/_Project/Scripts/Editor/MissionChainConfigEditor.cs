using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionChainConfig))]
public class MissionChainConfigEditor : Editor
{
    private bool _showMissions = true;
    
    public override void OnInspectorGUI()
    {
        MissionChainConfig config = (MissionChainConfig)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mission Chain Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_chainName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_chainDescription"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Chain Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoStart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_initialDelay"));
        
        EditorGUILayout.Space();
        
        _showMissions = EditorGUILayout.Foldout(_showMissions, 
            $"Missions ({config.Missions?.Count ?? 0})", true);
        
        if (_showMissions)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_missions"), true);
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Validate Chain"))
        {
            bool isValid = config.Validate();
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", 
                    "Chain configuration is valid!", "OK");
            }
        }
        
        if (config.Missions != null && config.Missions.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chain Flow:", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < config.Missions.Count; i++)
            {
                var mission = config.Missions[i];
                if (mission != null)
                {
                    string delay = mission.StartDelay > 0 ? $" (delay: {mission.StartDelay}s)" : "";
                    EditorGUILayout.LabelField($"{i + 1}. {mission.MissionName}{delay}");
                    
                    if (i < config.Missions.Count - 1)
                    {
                        EditorGUILayout.LabelField("    ↓");
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}