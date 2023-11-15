using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public sealed class MonopolySet
{
    [SerializeField] public Color ColorOfSet = Color.white;

    [SerializeField] public List<MonopolyNode> NodesInSet = new List<MonopolyNode>();
}
