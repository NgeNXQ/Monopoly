using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(MonopolyBoard))]
public sealed class CellEditor : Editor
{
    SerializedProperty nodeSetListProperty;

    private void OnEnable()
    {
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyBoard monopolyBoard = (MonopolyBoard)target;
        EditorGUILayout.PropertyField(nodeSetListProperty, true);

        if (GUILayout.Button("Change Image Colors"))
        {
            Undo.RecordObject(monopolyBoard, "Change Image Colors");

            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];


                for (int j = 0; j < nodeSet.nodesInSet.Count; j++)
                {
                    MonopolyCell cell = nodeSet.nodesInSet[j];
                    Image image = cell.cellMonopolyType;

                    if (image != null)
                    {
                        Undo.RecordObject(image, "Change Image Color");
                        image.color = nodeSet.setColor;
                    }

                    //nodeSetList[i].nodesInSet[j].ChangeColorField(nodeSetList[i].setColor);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
