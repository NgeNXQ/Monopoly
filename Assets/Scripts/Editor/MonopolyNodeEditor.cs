#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MonopolyNode))]
public sealed class MonopolyNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyNode monopolyNode = (MonopolyNode)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("MonopolyNode \"Type\"", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("MonopolyNode \"Visuals\"", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLogo"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteLogo"));
        EditorGUILayout.Space();

        if (monopolyNode.NodeType == MonopolyNode.Type.Property)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLevel1"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLevel2"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLevel3"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLevel4"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLevel5"));
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonopolyNode \"Values\"", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricing"));
        }

        if (monopolyNode.NodeType == MonopolyNode.Type.Gambling)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonopolyNode \"Values\"", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricing"));
        }

        if (monopolyNode.NodeType == MonopolyNode.Type.Transport)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonopolyNode \"Values\"", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricing"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
