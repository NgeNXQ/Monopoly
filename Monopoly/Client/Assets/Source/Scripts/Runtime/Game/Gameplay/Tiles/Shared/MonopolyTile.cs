using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Monopoly.Client.Runtime.Game.Controllers.Common;

namespace Monopoly.Client.Runtime.Game.Gameplay.Tiles.Shared
{
    internal abstract class MonopolyTile : NetworkBehaviour
    {
        [SerializeField, Header("Assets")]
        private Sprite spriteLogo;

        [SerializeField, Header("Editor")]
        private Image imageLogo;

        internal abstract void HandleLanding(PawnController pawn);

        private void Awake()
        {
            this.imageLogo.sprite = this.spriteLogo;
        }
    }
}
