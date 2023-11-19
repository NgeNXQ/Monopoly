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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageMonopolyNode"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SpriteMonopolyNode"));
        EditorGUILayout.Space();

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Property)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageMonopolySetType"));
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageMonopolySetType"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Gamble)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageMonopolySetType"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }
        
        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Tax)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("taxAmount"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
