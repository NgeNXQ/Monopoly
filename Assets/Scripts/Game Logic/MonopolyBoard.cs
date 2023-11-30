using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    #region In-editor Setup (Logic)

    [Space]
    [Header("Special nodes")]
    [Space]

    [Space]
    [SerializeField] private MonopolyNode jail;

    [Space]
    [SerializeField] private MonopolyNode start;

    [Space]
    [SerializeField] private MonopolyNode sendJail;

    [Space]
    [SerializeField] private MonopolyNode freeParking;

    [Space]
    [Header("Monopolies")]
    [Space]

    [Space]
    [SerializeField] private List<MonopolySet> monopolies = new List<MonopolySet>();

    #endregion

    private List<MonopolyNode> nodes;

    public static MonopolyBoard Instance { get; private set; }

    public int NumberOfNodes { get => this.nodes.Count; }

    public List<MonopolySet> Monopolies { get => this.monopolies; }

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
            if (child.TryGetComponent(out MonopolyNode monopolyNode))
                this.nodes.Add(monopolyNode);
        }

        for (int i = 0; i < this.monopolies.Count; ++i)
        {
            for (int j = 0; j < this.monopolies[i].NodesInSet.Count; ++j)
                this.monopolies[i].NodesInSet[j].MonopolyColor = this.monopolies[i].ColorOfSet;
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

    public int GetDistance(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int clockwiseDistance = (this[toNode] - this[fromNode] + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (this[fromNode] - this[toNode] + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }

    public int GetDistance(int fromNodeIndex, int toNodeIndex)
    {
        int clockwiseDistance = (toNodeIndex - fromNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (fromNodeIndex - toNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }

    public MonopolySet GetMonopolySetOfNode(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
            throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");

        foreach (MonopolySet monopolySet in this.monopolies)
        {
            if (monopolySet.Contains(monopolyNode))
                return monopolySet;
        }

        return null;
    }
}
