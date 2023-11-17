#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(MonopolyNode))]
public class MonopolyNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyNode monopolyNode = (MonopolyNode)target;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Property
            || monopolyNode.Type == MonopolyNode.MonopolyNodeType.Transport
            || monopolyNode.Type == MonopolyNode.MonopolyNodeType.Gamble
            || monopolyNode.Type == MonopolyNode.MonopolyNodeType.Tax) 
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ImageMonopolySetType"));
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Property)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Transport)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }

        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Gamble)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceInitial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priceMortgage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
        }
        
        if (monopolyNode.Type == MonopolyNode.MonopolyNodeType.Tax)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("taxAmount"));

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
