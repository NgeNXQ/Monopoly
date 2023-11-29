#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(SO_ChanceNode))]
public sealed class Editor_SO_ChanceNode : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SO_ChanceNode chanceNode = (SO_ChanceNode)target;

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        if (chanceNode.Type == SO_ChanceNode.ChanceNodeType.Reward || chanceNode.Type == SO_ChanceNode.ChanceNodeType.Penalty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        switch (chanceNode.Type)
        {
            case SO_ChanceNode.ChanceNodeType.Reward:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reward"));
                break;
            case SO_ChanceNode.ChanceNodeType.Penalty:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("penalty"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif