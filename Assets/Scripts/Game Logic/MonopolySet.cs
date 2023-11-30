using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public sealed class MonopolySet
{
    [SerializeField] public Color ColorOfSet;

    [SerializeField] public List<MonopolyNode> NodesInSet = new List<MonopolyNode>();

    //public Color ColorOfSet { get => this.colorOfSet; }

    //public List<MonopolyNode> NodesInSet { get => this.nodesInSet; }

    public bool Contains(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
            throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");

        return this.NodesInSet.Contains(monopolyNode);
    }
}
