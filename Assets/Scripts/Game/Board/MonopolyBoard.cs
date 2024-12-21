using UnityEngine;
using System.Collections.Generic;

internal sealed class MonopolyBoard : MonoBehaviour
{
    [Header("Special nodes")]

    [Space]
    [SerializeField]
    private MonopolyNode jail;

    [Space]
    [SerializeField]
    private MonopolyNode start;

    [Space]
    [SerializeField]
    private MonopolyNode sendJail;

    [Space]
    [SerializeField]
    private MonopolyNode freeParking;

    [Space]
    [Header("Monopolies")]

    [Space]
    [SerializeField]
    private List<MonopolySet> monopolies = new List<MonopolySet>();

    [Space]
    [Header("Chance & Tax nodes")]

    [Space]
    [SerializeField]
    private List<ChanceNodeSO> taxNodes = new List<ChanceNodeSO>();

    [Space]
    [SerializeField]
    private List<ChanceNodeSO> chanceNodes = new List<ChanceNodeSO>();

    internal static MonopolyBoard Instance { get; private set; }

    private List<MonopolyNode> nodes;

    internal int NodesCount { get => this.nodes.Count; }
    internal MonopolyNode NodeJail { get => this.jail; }
    internal MonopolyNode NodeStart { get => this.start; }
    internal MonopolyNode NodeSendToJail { get => this.sendJail; }
    internal List<MonopolySet> Monopolies { get => this.monopolies; }
    internal MonopolyNode NodeFreeParking { get => this.freeParking; }

    private void Awake()
    {
        if (MonopolyBoard.Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        MonopolyBoard.Instance = this;
    }

    private void Start()
    {
        this.nodes = new List<MonopolyNode>();

        foreach (Transform child in this.transform)
        {
            if (child.TryGetComponent(out MonopolyNode monopolyNode))
                this.nodes.Add(monopolyNode);
        }

        GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
    }

    internal int GetIndexOfNode(MonopolyNode monopolyNode)
    {
        if (monopolyNode == null)
            throw new System.NullReferenceException($"{nameof(monopolyNode)} is null.");

        return this.nodes.IndexOf(monopolyNode);
    }

    internal MonopolyNode GetNodeByIndex(int index)
    {
        if (index < 0 || index >= this.nodes.Count)
            throw new System.IndexOutOfRangeException($"{nameof(index)} is out of range.");

        return this.nodes[index];
    }

    internal ChanceNodeSO GetTaxNode()
    {
        return this.taxNodes[UnityEngine.Random.Range(0, this.taxNodes.Count)];
    }

    internal ChanceNodeSO GetChanceNode()
    {
        return this.chanceNodes[UnityEngine.Random.Range(0, this.chanceNodes.Count)];
    }

    internal MonopolySet GetMonopolySet(MonopolyNode monopolyNode)
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

    internal int GetDistance(int fromNodeIndex, int toNodeIndex)
    {
        int clockwiseDistance = (toNodeIndex - fromNodeIndex + this.NodesCount) % this.NodesCount;
        int counterclockwiseDistance = (fromNodeIndex - toNodeIndex + this.NodesCount) % this.NodesCount;
        return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
    }

    internal int GetDistance(MonopolyNode fromNode, MonopolyNode toNode)
    {
        int clockwiseDistance = (this.GetIndexOfNode(toNode) - this.GetIndexOfNode(fromNode) + this.NodesCount) % this.NodesCount;
        int counterclockwiseDistance = (this.GetIndexOfNode(fromNode) - this.GetIndexOfNode(toNode) + this.NodesCount) % this.NodesCount;
        return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
    }
}
