using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIMonopolyNodeTouchHandler : MonoBehaviour, IPointerClickHandler
{
    private bool isShown = false;

    private MonopolyNode monopolyNode;

    private void Awake() => this.monopolyNode = this.GetComponent<MonopolyNode>();

    public void OnPointerClick(PointerEventData eventData = null)
    {
        if (this.monopolyNode.Type == MonopolyNode.MonopolyNodeType.Property && this.monopolyNode.Owner == GameManager.Instance.CurrentPlayer)
        {
            if (!this.monopolyNode.Owner.HasBuilt && this.monopolyNode.Owner.HasFullMonopoly(this.monopolyNode))
            {
                this.isShown = !this.isShown;
                UIManager.Instance.SetControlVisibility(UIManager.UIControl.PanelMonopolyNode, this.isShown);
            }
        }
    }
}
