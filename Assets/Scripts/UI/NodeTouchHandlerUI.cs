using UnityEngine;
using UnityEngine.EventSystems;

public sealed class NodeTouchHandlerUI : MonoBehaviour, IPointerClickHandler
{
    private bool isShown = false;

    private MonopolyNode monopolyNode;

    private void Awake() => this.monopolyNode = this.GetComponent<MonopolyNode>();

    public void OnPointerClick(PointerEventData eventData = null)
    {
        if (this.monopolyNode.Owner != GameManager.Instance.CurrentPlayer)
            return;

        if (this.isShown)
        {
            this.isShown = false;
            UIManagerMonopoly.Instance.PanelMonopolyNode.Hide();
            return;
        }

        this.isShown = !this.isShown;
        this.monopolyNode.Owner.SelectedNode = this.monopolyNode;

        UIManagerMonopoly.Instance.PanelMonopolyNode.PictureSprite = this.monopolyNode.NodeSprite;
        UIManagerMonopoly.Instance.PanelMonopolyNode.MonopolyTypeColor = this.monopolyNode.AffiliatedMonopoly.ColorOfSet;
        UIManagerMonopoly.Instance.PanelMonopolyNode.Show();
    }
}
