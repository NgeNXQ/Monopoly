using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    #region Editor setup

    [Space]
    [Header("Special nodes")]
    [Space]

    [SerializeField] private MonopolyNode jail;

    [SerializeField] private MonopolyNode start;

    [SerializeField] private MonopolyNode sendJail;

    [SerializeField] private MonopolyNode freeParking;

    [Space]
    [Header("Monopolies")]
    [Space]

    [SerializeField] private List<MonopolySet> monopolies = new List<MonopolySet>();

    #endregion

    public static MonopolyBoard Instance { get; private set; }

    public List<MonopolyNode> Nodes { get; private set; }

    public MonopolyNode NodeJail { get => this.jail; }

    public MonopolyNode NodeStart { get => this.start; }

    public MonopolyNode NodeSendToJail { get => this.sendJail; }

    public MonopolyNode NodeFreeParking { get => this.freeParking; }

    public int NodeJailIndex { get => this.Nodes.IndexOf(this.jail); }

    public int NodeStartIndex { get => this.Nodes.IndexOf(this.start); }

    private void Awake()
    {
        Instance = this;

        this.Nodes = new List<MonopolyNode>();

        foreach (Transform node in this.transform.GetComponentInChildren<Transform>())
            this.Nodes.Add(node.GetComponent<MonopolyNode>());

        for (int i = 0; i < this.monopolies.Count; ++i)
        {
            for (int j = 0; j < this.monopolies[i].NodesInSet.Count; ++j)
                this.monopolies[i].NodesInSet[j].MonopolySetColor = this.monopolies[i].ColorOfSet;
        }
    }

    // Potential bugs!
    public int GetDistanceBetweenNodes(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int toNodeIndex = this.Nodes.IndexOf(toNode);
        int fromNodeIndex = this.Nodes.IndexOf(fromNode);

        return toNodeIndex - fromNodeIndex;
    }
}
