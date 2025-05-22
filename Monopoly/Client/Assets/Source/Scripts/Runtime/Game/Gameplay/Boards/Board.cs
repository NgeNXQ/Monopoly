using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Monopoly.Client.Runtime.Game.Gameplay.Groups;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared;
using Monopoly.Client.Runtime.Game.Gameplay.Tiles.Properties.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Boards
{
    internal sealed class Board : MonoBehaviour
    {
        [SerializeField]
        private Group[] monopolies;

        [field: SerializeField]
        internal Tile TileJail { get; private set; }

        [field: SerializeField]
        internal Tile TileStart { get; private set; }

        [field: SerializeField]
        internal Tile TileParking { get; private set; }

        [field: SerializeField]
        internal Tile TileJailTrigger { get; private set; }

        private List<Tile> tiles;

        internal static Board Instance { get; private set; }

        internal int TilesCount => this.tiles.Count;

        private void Awake()
        {
            if (Board.Instance != null)
                throw new InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

            Board.Instance = this;
        }

        private void Start()
        {
            this.tiles = new List<Tile>();

            foreach (Transform child in this.transform)
            {
                if (child.TryGetComponent(out Tile tile))
                    this.tiles.Add(tile);
            }
        }

        internal int GetIndexOfTile(Tile tile)
        {
            if (tile == null)
                throw new NullReferenceException($"{nameof(tile)} is null.");

            return this.tiles.IndexOf(tile);
        }

        internal Tile GetTileByIndex(int index)
        {
            if (index < 0 || index >= this.tiles.Count)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of range.");

            return this.tiles[index];
        }

        internal Group GetMonopolySet(PropertyTile tile)
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

        internal int GetDistance(Tile tileFrom, Tile tileTo)
        {
            int clockwiseDistance = (this.GetIndexOfTile(tileTo) - this.GetIndexOfTile(tileFrom) + this.TilesCount) % this.TilesCount;
            int counterclockwiseDistance = (this.GetIndexOfTile(tileFrom) - this.GetIndexOfTile(tileTo) + this.TilesCount) % this.TilesCount;
            return Mathf.Min(clockwiseDistance, counterclockwiseDistance) == counterclockwiseDistance ? -counterclockwiseDistance : clockwiseDistance;
        }
    }
}
