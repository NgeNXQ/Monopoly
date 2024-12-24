using UnityEngine;
using UnityEngine.EventSystems;

internal sealed class UINodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShown = false;
    private MonopolyNode associatedNode;

    private void Awake()
    {
        this.associatedNode = this.GetComponent<MonopolyNode>();
    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        if (this.associatedNode.Owner == null)
            return;

        if (GameManager.Instance.CurrentPawn.NetworkIndex != PlayerPawnController.LocalInstance.NetworkIndex)
            return;

        if (PlayerPawnController.LocalInstance.TradeReceiver != null)
        {
            if (!this.associatedNode.IsTradable)
                return;

            if (this.associatedNode.Owner == PlayerPawnController.LocalInstance)
                UIManagerMonopolyGame.Instance.PanelSendTrade.SenderNode = this.associatedNode;
            else if (this.associatedNode.Owner == PlayerPawnController.LocalInstance.TradeReceiver)
                UIManagerMonopolyGame.Instance.PanelSendTrade.ReceiverNode = this.associatedNode;
        }
        else
        {
            if (this.isShown)
            {
                PlayerPawnController.LocalInstance.SelectedNode = null;
                UIManagerMonopolyGame.Instance.HidePanelNodeMenu();
            }
            else
            {
                PlayerPawnController.LocalInstance.SelectedNode = this.associatedNode;
                UIManagerMonopolyGame.Instance.ShowPanelNodeMenu(this.associatedNode, PlayerPawnController.LocalInstance.OnNodeMenuShown);
            }

            this.isShown = !this.isShown;
        }
    }
}
