#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(ChanceNodeSO))]
public sealed class ChanceNodeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ChanceNodeSO chanceNode = (ChanceNodeSO)target;

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        if (chanceNode.ChanceType == ChanceNodeSO.Type.Reward || chanceNode.ChanceType == ChanceNodeSO.Type.Penalty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        switch (chanceNode.ChanceType)
        {
            case ChanceNodeSO.Type.Reward:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reward"));
                break;
            case ChanceNodeSO.Type.Penalty:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("penalty"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif