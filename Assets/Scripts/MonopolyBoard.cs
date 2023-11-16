using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    [Space]
    [Header("Special nodes")]
    [Space]

    [SerializeField] public MonopolyNode NodeJail;

    [SerializeField] public MonopolyNode NodeStart;

    [SerializeField] public MonopolyNode NodeSendToJail;

    //[SerializeField] public MonopolyNode StartingNode;

    [Space]
    [Header("Monopolies")]
    [Space]

    [SerializeField] private List<MonopolySet> monopolySets = new List<MonopolySet>();

    public List<MonopolyNode> Nodes { get; private set; }

    public static MonopolyBoard Instance { get; private set; }

    public int NodeJailIndex { get => this.Nodes.IndexOf(this.NodeJail); }

    public int NodeStartIndex { get => this.Nodes.IndexOf(this.NodeStart); }

    public int NodeSendToJailIndex { get => this.Nodes.IndexOf(this.NodeSendToJail); }

    private void Awake()
    {
        Instance = this;

        this.Nodes = new List<MonopolyNode>();

        foreach (Transform node in this.transform.GetComponentInChildren<Transform>())
            this.Nodes.Add(node.GetComponent<MonopolyNode>());

        for (int i = 0; i < this.monopolySets.Count; ++i)
        {
            for (int j = 0; j < this.monopolySets[i].NodesInSet.Count; ++j)
                this.monopolySets[i].NodesInSet[j].ImageMonopolySetType.color = this.monopolySets[i].ColorOfSet;
        }
    }

    // Potential bugs!
    public int GetDistanceBetweenNodes(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int toNodeIndex = this.Nodes.IndexOf(toNode);
        int fromNodeIndex = this.Nodes.IndexOf(fromNode);

        return toNodeIndex - fromNodeIndex;
    }

    //public (List<MonopolyCell> list, bool AlSame) PlayerHasAllNodesOfSet(MonopolyCell cell)
    //{
    //    bool alSame = false;

    //    foreach (var nodeSet in nodeSetList)
    //    {
    //        if (nodeSet.nodesInSet.Contains(cell))
    //        {
    //            alSame = nodeSet.nodesInSet.All(node => node.Owner == cell.Owner);
    //            return (nodeSet.nodesInSet, alSame);
    //        }
    //    }

    //    return (null, false);
    //}
}
