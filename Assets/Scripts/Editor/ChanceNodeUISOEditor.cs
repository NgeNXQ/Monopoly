#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(ChanceNodeUISO))]
public class ChanceNodeUISOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ChanceNodeUISO chanceNode = (ChanceNodeUISO)target;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));

        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("MonopolyNodeImage"));

        if (chanceNode.Type == ChanceNodeUISO.ChanceNodeType.Reward || chanceNode.Type == ChanceNodeUISO.ChanceNodeType.Penality)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        switch (chanceNode.Type)
        {
            case ChanceNodeUISO.ChanceNodeType.Reward:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Reward"));
                break;
            case ChanceNodeUISO.ChanceNodeType.Penality:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Penalty"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif