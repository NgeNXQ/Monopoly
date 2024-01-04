using UnityEngine;
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
        if (this.monopolyNode.Owner != GameManager.Instance.CurrentPlayer)
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

        UIManagerMonopolyGame.Instance.ShowMonopolyNode(this.monopolyNode.NodeSprite, this.monopolyNode.AffiliatedMonopoly.ColorOfSet, this.monopolyNode.PriceUpgrade, this.monopolyNode.Owner.CallbackMonopolyNode);
    }
}
