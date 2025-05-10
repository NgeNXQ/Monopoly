// #if UNITY_EDITOR

// using UnityEditor;
// using Monopoly.Client.Scriptable.Objects.Cards;

// namespace Monopoly.Client.Editor.Scriptable.Objects.Cards
// {
//     [CustomEditor(typeof(ChanceCardScriptableObject))]
//     internal sealed class ChanceCardScriptableObjectEditor : UnityEditor.Editor
//     {
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();

//             ChanceCardScriptableObject chanceCard = (ChanceCardScriptableObject)target;

//             EditorGUILayout.Space();
//             EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
//             EditorGUILayout.Space();

//             EditorGUILayout.Space();
//             EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
//             EditorGUILayout.Space();

//             EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

//             if (chanceCard.ChanceType == ChanceCardScriptableObject.Type.Reward || chanceCard.ChanceType == ChanceCardScriptableObject.Type.Penalty)
//             {
//                 EditorGUILayout.Space();
//                 EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
//                 EditorGUILayout.Space();
//             }

//             switch (chanceCard.ChanceType)
//             {
//                 case ChanceCardScriptableObject.Type.Reward:
//                     EditorGUILayout.PropertyField(serializedObject.FindProperty("reward"));
//                     break;
//                 case ChanceCardScriptableObject.Type.Penalty:
//                     EditorGUILayout.PropertyField(serializedObject.FindProperty("penalty"));
//                     break;
//             }

//             serializedObject.ApplyModifiedProperties();
//         }
//     }

// }

// #endif
