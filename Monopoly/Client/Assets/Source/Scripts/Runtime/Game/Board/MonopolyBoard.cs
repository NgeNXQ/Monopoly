using UnityEngine;
using System.Collections.Generic;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.Game.Tiles.Common;
using Monopoly.Client.Runtime.Game.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Board
{
    internal sealed class MonopolyBoard : MonoBehaviour
    {
        [field: Space, SerializeField]
        internal MonopolyTile TileStart {get; private set; }

        [field: Space, SerializeField]
        internal MonopolyTile TileParking {get; private set; }

        [field:Space, SerializeField]
        internal MonopolyTile TileJailVisit {get; private set; }

        [field:Space, SerializeField]
        internal MonopolyTile TileJailTrigger {get; private set; }

        [field: Space, SerializeField]
        internal List<MonopolySet> Monopolies { get; private set; } = new List<MonopolySet>();

        private List<MonopolyTile> tiles;

        internal static MonopolyBoard Instance { get; private set; }

        internal int TilesCount => this.tiles.Count;

        private void Awake()
        {
            if (MonopolyBoard.Instance != null)
                throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            MonopolyBoard.Instance = this;
        }

        private void Start()
        {
            this.tiles = new List<MonopolyTile>();

            foreach (Transform child in this.transform)
            {
                if (child.TryGetComponent(out MonopolyTile tile))
                    this.tiles.Add(tile);
            }

            // GameCoordinator.Instance?.UpdateInitializedObjects(this.GetType());
        }

        internal int GetIndexOfTile(MonopolyTile tile)
        {
            if (tile == null)
                throw new System.NullReferenceException($"{nameof(tile)} is null.");

            return this.tiles.IndexOf(tile);
        }

        internal MonopolyTile GetTileByIndex(int index)
        {
            if (index < 0 || index >= this.tiles.Count)
                throw new System.IndexOutOfRangeException($"{nameof(index)} is out of range.");

            return this.tiles[index];
        }

        internal MonopolySet GetMonopolySet(PropertyMonopolyTile tile)
        {
            if (tile == null)
                throw new System.ArgumentNullException($"{nameof(tile)} is null.");

            foreach (MonopolySet monopoly in this.Monopolies)
            {
                if (monopoly.Contains(tile))
                    return monopoly;
            }

            return null;
        }

        internal int GetDistance(int tileIndexFrom, int tileIndexTo)
        {
            int clockwiseDistance = (tileIndexTo - tileIndexFrom + this.TilesCount) % this.TilesCount;
            int counterclockwiseDistance = (tileIndexFrom - tileIndexTo + this.TilesCount) % this.TilesCount;
            return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
        }

        internal int GetDistance(MonopolyTile tileFrom, MonopolyTile tileTo)
        {
            int clockwiseDistance = (this.GetIndexOfTile(tileTo) - this.GetIndexOfTile(tileFrom) + this.TilesCount) % this.TilesCount;
            int counterclockwiseDistance = (this.GetIndexOfTile(tileFrom) - this.GetIndexOfTile(tileTo) + this.TilesCount) % this.TilesCount;
            return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
        }
    }
}
