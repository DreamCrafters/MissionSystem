using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionConfig))]
public class MissionConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MissionConfig config = (MissionConfig)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mission Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (config.MissionPrefab != null)
        {
            var missionComponent = config.MissionPrefab.GetComponent<IMission>();
            if (missionComponent == null)
            {
                EditorGUILayout.HelpBox("Warning: Mission prefab doesn't have IMission component!",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ Valid mission prefab", MessageType.Info);
            }
        }

        if (GUILayout.Button("Create Test Instance"))
        {
            if (config.MissionPrefab != null)
            {
                var instance = config.CreateMissionInstance();
                if (instance != null)
                {
                    Debug.Log($"Test instance created: {config.MissionName}");
                }
            }
        }
    }
}