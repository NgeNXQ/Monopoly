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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("MonopolyNodeImage"));

        if (chanceNode.Type == ChanceNodeSO.ChanceNodeType.Reward || chanceNode.Type == ChanceNodeSO.ChanceNodeType.Penalty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        switch (chanceNode.Type)
        {
            case ChanceNodeSO.ChanceNodeType.Reward:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Reward"));
                break;
            case ChanceNodeSO.ChanceNodeType.Penalty:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Penalty"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif