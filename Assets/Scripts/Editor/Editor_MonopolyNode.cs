#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MonopolyNode))]
public sealed class Editor_MonopolyNode : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyNode monopolyNode = (MonopolyNode)target;

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLogo"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteLogo"));
        EditorGUILayout.Space();

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Tax)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("taxAmount"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Property)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Gambling)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Transport)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
