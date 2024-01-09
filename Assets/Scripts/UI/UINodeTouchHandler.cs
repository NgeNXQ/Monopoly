using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public sealed class UINodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShown = false;

    private MonopolyNode monopolyNode;

    private void Start()
    {
        this.monopolyNode = this.GetComponent<MonopolyNode>();
    }

    public void OnPointerClick(PointerEventData eventData = null)
    {
        if (GameManager.Instance.CurrentPlayer.IsTrading)
        {
            if (!this.monopolyNode.IsTradable)
            {
                return;
            }

            if (this.monopolyNode.Owner == GameManager.Instance.CurrentPlayer)
            {
                UIManagerMonopolyGame.Instance.PanelTradeOffer.ThisSprite = this.monopolyNode.NodeSprite;
                UIManagerMonopolyGame.Instance.PanelTradeOffer.ThisNodeIndex = MonopolyBoard.Instance[this.monopolyNode];
            }
            else if (this.monopolyNode.Owner == GameManager.Instance.CurrentPlayer.PlayerTradingWith)
            {
                UIManagerMonopolyGame.Instance.PanelTradeOffer.OtherSprite = this.monopolyNode.NodeSprite;
                UIManagerMonopolyGame.Instance.PanelTradeOffer.OtherNodeIndex = MonopolyBoard.Instance[this.monopolyNode];
            }
        }
        else
        {
            if (this.monopolyNode.Owner == null)
            {
                return;
            }

            if (NetworkManager.Singleton.LocalClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
            {
                return;
            }

            if (this.monopolyNode.Owner.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            if (this.isShown)
            {
                this.isShown = false;
                UIManagerMonopolyGame.Instance.HideMonopolyNode();
                return;
            }

            this.isShown = !this.isShown;
            this.monopolyNode.Owner.SelectedNode = this.monopolyNode;

            UIManagerMonopolyGame.Instance.ShowMonopolyNode(this.monopolyNode.NodeSprite, this.monopolyNode.AffiliatedMonopoly.ColorOfSet, this.monopolyNode.Owner.CallbackMonopolyNode);
        }  
    }
}
