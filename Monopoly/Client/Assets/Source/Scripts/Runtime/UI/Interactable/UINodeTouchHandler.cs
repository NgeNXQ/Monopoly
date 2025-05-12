using UnityEngine;
using UnityEngine.EventSystems;
using Monopoly.Client.Runtime.UI.Managers;
// using Monopoly.Client.Runtime.Game.;
using Monopoly.Client.Runtime.Game.Gameplay.Layout;
using Monopoly.Client.Runtime.Game.Controllers.Common;
using Monopoly.Client.Runtime.Game.Controllers.Concrete;

internal sealed class UINodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    // private bool isShown = false;
    // private MonopolyTile associatedTile;

    private void Awake()
    {
        // this.associatedTile = this.GetComponent<MonopolyTile>();
    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        // if (this.associatedTile.Owner == null)
        //     return;

        // if (GameManager.Instance.CurrentPawn.NetworkIndex != PlayerPawnController.LocalInstance.NetworkIndex)
        //     return;

        // if (PlayerPawnController.LocalInstance.TradeReceiver != null)
        // {
        //     if (!this.associatedTile.IsTradable)
        //         return;

        //     if (this.associatedTile.Owner == PlayerPawnController.LocalInstance)
        //         UIManagerGame.Instance.PanelTradeSender.SenderNode = this.associatedTile;
        //     else if (this.associatedTile.Owner == PlayerPawnController.LocalInstance.TradeReceiver)
        //         UIManagerGame.Instance.PanelTradeSender.ReceiverNode = this.associatedTile;
        // }
        // else
        // {
        //     if (PlayerPawnController.LocalInstance != this.associatedTile.Owner)
        //         return;

        //     if (this.isShown)
        //     {
        //         PlayerPawnController.LocalInstance.SelectedTile = null;
        //         UIManagerGame.Instance.HidePanelTileManagement();
        //     }
        //     else
        //     {
        //         PlayerPawnController.LocalInstance.SelectedTile = this.associatedTile;
        //         UIManagerGame.Instance.ShowPanelTileManagement(this.associatedTile, PlayerPawnController.LocalInstance.OnTileManagementShown);
        //     }

        //     this.isShown = !this.isShown;
        // }
    }
}
