using System;
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

    private List<MonopolyNode> nodes;

    public static MonopolyBoard Instance { get; private set; }

    public int NumberOfNodes { get => this.nodes.Count; }

    public MonopolyNode NodeJail { get => this.jail; }

    public MonopolyNode NodeStart { get => this.start; }

    public MonopolyNode NodeSendToJail { get => this.sendJail; }

    public MonopolyNode NodeFreeParking { get => this.freeParking; }

    private void Awake()
    {
        Instance = this;

        this.nodes = new List<MonopolyNode>();

        foreach (Transform child in this.transform)
        {
            if (child.TryGetComponent<MonopolyNode>(out MonopolyNode monopolyNode))
                this.nodes.Add(monopolyNode);
        }

        for (int i = 0; i < this.monopolies.Count; ++i)
        {
            for (int j = 0; j < this.monopolies[i].NodesInSet.Count; ++j)
                this.monopolies[i].NodesInSet[j].MonopolySetColor = this.monopolies[i].ColorOfSet;
        }
    }

    public MonopolyNode this[int index]
    {
        get
        {
            if (index < 0 || index >= this.nodes.Count)
                throw new System.IndexOutOfRangeException($"{nameof(index)} is out of range.");

            return this.nodes[index];
        }
    }

    public int this[MonopolyNode monopolyNode]
    {
        get
        {
            if (monopolyNode == null)
                throw new System.NullReferenceException($"{nameof(monopolyNode)} is null.");

            return this.nodes.IndexOf(monopolyNode);
        }
    }

    public int GetDistanceBetweenNodes(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int clockwiseDistance = (this[toNode] - this[fromNode] + MonopolyBoard.Instance.NumberOfNodes) % MonopolyBoard.Instance.NumberOfNodes;
        int counterclockwiseDistance = (this[fromNode] - this[toNode] + MonopolyBoard.Instance.NumberOfNodes) % MonopolyBoard.Instance.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }

    public int GetDistanceBetweenNodes(int fromNodeIndex, int toNodeIndex)
    {
        int clockwiseDistance = (toNodeIndex - fromNodeIndex + MonopolyBoard.Instance.NumberOfNodes) % MonopolyBoard.Instance.NumberOfNodes;
        int counterclockwiseDistance = (fromNodeIndex - toNodeIndex + MonopolyBoard.Instance.NumberOfNodes) % MonopolyBoard.Instance.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }
}
