using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    #region Setup

    #region Special nodes

    [Header("Special nodes")]

    [Space]
    [SerializeField] private MonopolyNode jail;

    [Space]
    [SerializeField] private MonopolyNode start;

    [Space]
    [SerializeField] private MonopolyNode sendJail;

    [Space]
    [SerializeField] private MonopolyNode freeParking;

    #endregion

    #region Monopolies

    [Space]
    [Header("Monopolies")]

    [Space]
    [SerializeField] private List<MonopolySet> monopolies = new List<MonopolySet>();

    #endregion

    #region Chance & Tax nodes

    [Space]
    [Header("Chance & Tax nodes")]

    [Space]
    [SerializeField] private List<ChanceNodeSO> taxNodes = new List<ChanceNodeSO>();

    [Space]
    [SerializeField] private List<ChanceNodeSO> chanceNodes = new List<ChanceNodeSO>();

    #endregion

    #endregion

    private List<MonopolyNode> nodes;

    public static MonopolyBoard Instance { get; private set; }

    public List<MonopolySet> Monopolies { get => this.monopolies; }

    public int NumberOfNodes { get => this.nodes.Count; }
    
    public MonopolyNode NodeJail { get => this.jail; }

    public MonopolyNode NodeStart { get => this.start; }

    public MonopolyNode NodeSendToJail { get => this.sendJail; }

    public MonopolyNode NodeFreeParking { get => this.freeParking; }

    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");
        }
        
        Instance = this;
    }

    private void Start()
    {
        this.nodes = new List<MonopolyNode>();

        foreach (Transform child in this.transform)
        {
            if (child.TryGetComponent(out MonopolyNode monopolyNode))
            {
                this.nodes.Add(monopolyNode);
            }
        }

        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
    }

    public MonopolyNode this[int index]
    {
        get
        {
            if (index < 0 || index >= this.nodes.Count)
            {
                throw new System.IndexOutOfRangeException($"{nameof(index)} is out of range.");
            }

            return this.nodes[index];
        }
    }

    public int this[MonopolyNode monopolyNode]
    {
        get
        {
            if (monopolyNode == null)
            {
                throw new System.NullReferenceException($"{nameof(monopolyNode)} is null.");
            } 

            return this.nodes.IndexOf(monopolyNode);
        }
    }

    public ChanceNodeSO GetTaxNode()
    {
        return this.taxNodes[UnityEngine.Random.Range(0, this.taxNodes.Count)];
    }

    public ChanceNodeSO GetChanceNode()
    {
        return this.chanceNodes[UnityEngine.Random.Range(0, this.chanceNodes.Count)];
    }

    public MonopolySet GetMonopolySet(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
        {
            throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");
        }

        foreach (MonopolySet monopolySet in this.monopolies)
        {
            if (monopolySet.Contains(monopolyNode))
            {
                return monopolySet;
            }
        }

        return null;
    }

    public int GetDistance(int fromNodeIndex, int toNodeIndex)
    {
        int clockwiseDistance = (toNodeIndex - fromNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (fromNodeIndex - toNodeIndex + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
    }

    public int GetDistance(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int clockwiseDistance = (this[toNode] - this[fromNode] + this.NumberOfNodes) % this.NumberOfNodes;
        int counterclockwiseDistance = (this[fromNode] - this[toNode] + this.NumberOfNodes) % this.NumberOfNodes;

        return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
    }
}
