using Unity.Netcode;
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
        //Debug.Log($"Owner ID {this.monopolyNode.Owner.OwnerClientId}");

        //Debug.Log($"Current Player Owner ID {GameManager.Instance.CurrentPlayer.OwnerClientId}");

        //Debug.Log($"Current Player Index {GameManager.Instance.CurrentPlayerIndex}");

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
