using UnityEngine;
using System.Collections.Generic;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Scriptable.Objects.Cards;

namespace Monopoly.Client.Runtime.Game.Core
{
    internal sealed class MonopolyBoard : MonoBehaviour
    {
        [Header("Special nodes")]

        [Space]
        [SerializeField]
        private MonopolyTile jail;

        [Space]
        [SerializeField]
        private MonopolyTile start;

        [Space]
        [SerializeField]
        private MonopolyTile sendJail;

        [Space]
        [SerializeField]
        private MonopolyTile freeParking;

        [Space]
        [Header("Monopolies")]

        [Space]
        [SerializeField]
        private List<MonopolySet> monopolies = new List<MonopolySet>();

        [Space]
        [Header("Chance & Tax nodes")]

        [Space]
        [SerializeField]
        private List<ChanceCardScriptableObject> taxNodes = new List<ChanceCardScriptableObject>();

        [Space]
        [SerializeField]
        private List<ChanceCardScriptableObject> chanceNodes = new List<ChanceCardScriptableObject>();

        internal static MonopolyBoard Instance { get; private set; }

        private List<MonopolyTile> nodes;

        internal int NodesCount { get => this.nodes.Count; }
        internal MonopolyTile NodeJail { get => this.jail; }
        internal MonopolyTile NodeStart { get => this.start; }
        internal MonopolyTile NodeSendToJail { get => this.sendJail; }
        internal List<MonopolySet> Monopolies { get => this.monopolies; }
        internal MonopolyTile NodeFreeParking { get => this.freeParking; }

        private void Awake()
        {
            if (MonopolyBoard.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            MonopolyBoard.Instance = this;
        }

        private void Start()
        {
            this.nodes = new List<MonopolyTile>();

            foreach (Transform child in this.transform)
            {
                if (child.TryGetComponent(out MonopolyTile monopolyNode))
                    this.nodes.Add(monopolyNode);
            }

            GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
        }

        internal int GetIndexOfNode(MonopolyTile monopolyNode)
        {
            if (monopolyNode == null)
                throw new System.NullReferenceException($"{nameof(monopolyNode)} is null.");

            return this.nodes.IndexOf(monopolyNode);
        }

        internal MonopolyTile GetNodeByIndex(int index)
        {
            if (index < 0 || index >= this.nodes.Count)
                throw new System.IndexOutOfRangeException($"{nameof(index)} is out of range.");

            return this.nodes[index];
        }

        internal ChanceCardScriptableObject GetTaxNode()
        {
            return this.taxNodes[UnityEngine.Random.Range(0, this.taxNodes.Count)];
        }

        internal ChanceCardScriptableObject GetChanceNode()
        {
            return this.chanceNodes[UnityEngine.Random.Range(0, this.chanceNodes.Count)];
        }

        internal MonopolySet GetMonopolySet(MonopolyTile monopolyNode)
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

        internal int GetDistance(MonopolyTile fromNode, MonopolyTile toNode)
        {
            int clockwiseDistance = (this.GetIndexOfNode(toNode) - this.GetIndexOfNode(fromNode) + this.NodesCount) % this.NodesCount;
            int counterclockwiseDistance = (this.GetIndexOfNode(fromNode) - this.GetIndexOfNode(toNode) + this.NodesCount) % this.NodesCount;
            return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
        }
    }
}
