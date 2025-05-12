using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Layout
{
    internal sealed class MonopolyBoard : MonoBehaviour
    {
        [field: SerializeField]
        internal MonopolyTile TileJail { get; private set; }

        [field: SerializeField]
        internal MonopolyTile TileStart { get; private set; }

        [field: SerializeField]
        internal MonopolyTile TileParking { get; private set; }

        [field: SerializeField]
        internal MonopolyTile TileJailTrigger { get; private set; }

        [SerializeField]
        internal MonopolySet[] monopolies;

        private List<MonopolyTile> tiles;

        internal static MonopolyBoard Instance { get; private set; }

        internal int TilesCount => this.tiles.Count;

        private void Awake()
        {
            if (MonopolyBoard.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

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
                throw new NullReferenceException($"{nameof(tile)} is null.");

            return this.tiles.IndexOf(tile);
        }

        internal MonopolyTile GetTileByIndex(int index)
        {
            if (index < 0 || index >= this.tiles.Count)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of range.");

            return this.tiles[index];
        }

        internal MonopolySet GetMonopolySet(PropertyMonopolyTile tile)
        {
            if (tile == null)
                throw new System.ArgumentNullException($"{nameof(tile)} is null.");

            return this.monopolies.FirstOrDefault(monopoly => monopoly.Contains(tile));
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
