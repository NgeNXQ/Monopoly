
#if UNITY_EDITOR

using UnityEditor;
using Monopoly.Client.Runtime.Game.Core;

namespace Monopoly.Client.Editor.Game.Core
{
    [CustomEditor(typeof(MonopolyTile))]
    internal sealed class MonopolyNodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MonopolyTile tile = (MonopolyTile)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonopolyTile \"Type\"", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonopolyTile \"Visuals\"", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("imageLogo"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteLogo"));
            EditorGUILayout.Space();

            if (tile.TileType == MonopolyTile.Type.Property)
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
                EditorGUILayout.LabelField("MonopolyTile \"Values\"", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricePurchase"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("priceUpgrade"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
            }

            if (tile.TileType == MonopolyTile.Type.Gambling)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
                EditorGUILayout.Space();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("MonopolyTile \"Values\"", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricePurchase"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
            }

            if (tile.TileType == MonopolyTile.Type.Transport)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageOwner"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMonopolyType"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("imageMortgageStatus"));
                EditorGUILayout.Space();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("MonopolyTile \"Values\"", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricePurchase"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pricesRent"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
