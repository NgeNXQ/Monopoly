using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    #region Setup

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

    [Space]
    [Header("Chance nodes")]
    [Space]

    [Space]
    [SerializeField] private List<ChanceNodeSO> chanceNodes = new List<ChanceNodeSO>();

    [Space]
    [SerializeField] private List<ChanceNodeSO> taxNodes = new List<ChanceNodeSO>();

    #endregion

    private List<MonopolyNode> nodes;

    public static MonopolyBoard Instance { get; private set; }

    public int NumberOfNodes { get => this.nodes.Count; }

    public MonopolyNode NodeJail { get => this.jail; }

    public MonopolyNode NodeStart { get => this.start; }

    public MonopolyNode NodeSendToJail { get => this.sendJail; }

    public MonopolyNode NodeFreeParking { get => this.freeParking; }

    public List<MonopolySet> Monopolies { get => this.monopolies; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.nodes = new List<MonopolyNode>();

        foreach (Transform child in this.transform)
        {
            if (child.TryGetComponent(out MonopolyNode monopolyNode))
                this.nodes.Add(monopolyNode);
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

    public int GetDistance(int fromNodeIndex, int toNodeIndex)
    {
        int clockwiseDistance = (toNodeIndex - fromNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (fromNodeIndex - toNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }

    public int GetDistance(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int clockwiseDistance = (this[toNode] - this[fromNode] + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (this[fromNode] - this[toNode] + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance);
    }

    public MonopolySet GetMonopolySet(MonopolyNode monopolyNode)
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

    public ChanceNodeSO GetTaxNode() => this.taxNodes[UnityEngine.Random.Range(0, this.taxNodes.Count)];

    public ChanceNodeSO GetChanceNode() => this.chanceNodes[UnityEngine.Random.Range(0, this.chanceNodes.Count)];
}
