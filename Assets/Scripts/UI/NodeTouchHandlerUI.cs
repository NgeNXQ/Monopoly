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
            UIManagerMonopolyGame.Instance.PanelMonopolyNode.Hide();
            return;
        }

        this.isShown = !this.isShown;
        this.monopolyNode.Owner.SelectedNode = this.monopolyNode;

        UIManagerMonopolyGame.Instance.PanelMonopolyNode.PictureSprite = this.monopolyNode.NodeSprite;
        UIManagerMonopolyGame.Instance.PanelMonopolyNode.MonopolyTypeColor = this.monopolyNode.AffiliatedMonopoly.ColorOfSet;
        UIManagerMonopolyGame.Instance.PanelMonopolyNode.Show();
    }
}
