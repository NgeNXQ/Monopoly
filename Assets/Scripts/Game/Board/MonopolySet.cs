using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
internal sealed class MonopolySet
{
    [SerializeField]
    private Color colorOfSet;

    [SerializeField]
    private List<MonopolyNode> nodesInSet = new List<MonopolyNode>();

    internal Color ColorOfSet => this.colorOfSet;
    internal int NodesCount => this.nodesInSet.Count;
    internal IReadOnlyList<MonopolyNode> NodesInSet => this.nodesInSet;

    internal int GetLevel(PawnController pawn)
    {
        if (pawn == null)
            throw new System.ArgumentNullException($"{nameof(pawn)} is null.");

        return this.nodesInSet.Where(node => node.Owner == pawn).Select(node => node.Level).Max();
    }

    internal IEnumerable<MonopolyNode> GetNodesOwnedByPawn(PawnController pawn)
    {
        if (pawn == null)
            throw new System.ArgumentNullException($"{nameof(pawn)} is null.");

        return this.nodesInSet.Where(node => node.Owner == pawn);
    }

    internal bool Contains(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
            throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");

        return this.NodesInSet.Contains(monopolyNode);
    }
}
