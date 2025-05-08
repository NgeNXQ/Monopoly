using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Runtime.Game.Core
{
    [System.Serializable]
    internal sealed class MonopolySet
    {
        [SerializeField]
        private Color colorOfSet;

        [SerializeField]
        private List<MonopolyTile> nodesInSet = new List<MonopolyTile>();

        internal Color ColorOfSet => this.colorOfSet;
        internal int NodesCount => this.nodesInSet.Count;
        internal IReadOnlyList<MonopolyTile> NodesInSet => this.nodesInSet;

        internal int GetLevel(PawnController pawn)
        {
            if (pawn == null)
                throw new System.ArgumentNullException($"{nameof(pawn)} is null.");

            return this.nodesInSet.Where(node => node.Owner == pawn).Select(node => node.Level).Max();
        }

        internal IEnumerable<MonopolyTile> GetNodesOwnedByPawn(PawnController pawn)
        {
            if (pawn == null)
                throw new System.ArgumentNullException($"{nameof(pawn)} is null.");

            return this.nodesInSet.Where(node => node.Owner == pawn);
        }

        internal bool Contains(MonopolyTile monopolyNode)
        {
            if (monopolyNode == null)
                throw new System.ArgumentNullException($"{nameof(monopolyNode)} is null.");

            return this.NodesInSet.Contains(monopolyNode);
        }
    }
}
