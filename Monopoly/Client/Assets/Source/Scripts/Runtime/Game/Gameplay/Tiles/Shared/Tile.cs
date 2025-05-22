using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Controllers.Pawns.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared
{
    internal abstract class Tile : NetworkBehaviour
    {
        [SerializeField]
        private Image imagePicture;

        [field: SerializeField]
        internal Sprite SpritePicture { get; private set; }

        internal abstract void HandleLanding(PawnController pawn);

        private void Awake()
        {
            this.imagePicture.sprite = this.SpritePicture;
        }
    }
}
