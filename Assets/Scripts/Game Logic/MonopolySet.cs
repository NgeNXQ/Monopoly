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
        get => this.nodesInSet.Where(node => node.Owner == GameManager.Instance.CurrentPlayer).Select(node => node.Level.Value).Max();
    }

    public Color ColorOfSet
    {
        get => this.colorOfSet;
    }

    public int OwnedByPlayerCount 
    {
        get => this.nodesInSet.Where(node => node.Owner == GameManager.Instance.CurrentPlayer).Count();
    }

    public IReadOnlyList<MonopolyNode> NodesInSet 
    {
        get => this.nodesInSet;
    }

    public IEnumerable<MonopolyNode> OwnedByPlayerNodes 
    {
        get => this.nodesInSet.Where(node => node.Owner == GameManager.Instance.CurrentPlayer);
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
