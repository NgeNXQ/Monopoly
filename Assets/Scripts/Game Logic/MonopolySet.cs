using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public sealed class MonopolySet
{
    #region Setup

    [SerializeField] private Color colorOfSet;

    [SerializeField] private List<MonopolyNode> nodesInSet = new List<MonopolyNode>();

    #endregion

    public int Level 
    { 
        get => this.nodesInSet.Where(node => node.Owner == GameManager.Instance.CurrentPlayer).Max(node => node.Level); 
    }

    public Color ColorOfSet 
    { 
        get => this.colorOfSet; 
    }

    public IReadOnlyList<MonopolyNode> NodesInSet 
    { 
        get => this.nodesInSet; 
    }

    public bool Contains(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
        {
            throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");
        }

        return this.NodesInSet.Contains(monopolyNode);
    }
}
