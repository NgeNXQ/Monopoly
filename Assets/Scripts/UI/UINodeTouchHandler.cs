using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UINodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShown = false;

    private MonopolyNode monopolyNode;

    private void Awake() => this.monopolyNode = this.GetComponent<MonopolyNode>();

    public void OnPointerClick(PointerEventData eventData = null)
    {
        //if (this.isShown)
        //{
        //    this.isShown = false;
        //    UIManager.Instance.PanelMonopolyNode.Hide();
        //    return;
        //}

        //if (this.monopolyNode.Owner != GameManager.Instance.CurrentPlayer)
        //    return;

        //if (this.monopolyNode.Owner.IsMoving)
        //    return;

        //if (this.monopolyNode.Owner.HasBuilt)
        //    return;

        //this.isShown = !this.isShown;
        //this.monopolyNode.Owner.SelectedNode = this.monopolyNode;

        //UIManager.Instance.PanelMonopolyNode.LogoSprite = this.monopolyNode.NodeSprite;
        //UIManager.Instance.PanelMonopolyNode.PriceText = this.monopolyNode.PriceUpgrade.ToString();
        //UIManager.Instance.PanelMonopolyNode.Show();
    }
}
