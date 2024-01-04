using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class MonopolyNode : NetworkBehaviour
{
    #region Setup (Editor)

    [SerializeField] private Type type;

    [SerializeField] private Image imageLogo;

    [SerializeField] private Sprite spriteLogo;

    [SerializeField] private Image imageOwner;

    [SerializeField] private Image imageMonopolyType;

    [SerializeField] private Image imageMortgageStatus;

    [SerializeField] private Image imageLevel1;

    [SerializeField] private Image imageLevel2;

    [SerializeField] private Image imageLevel3;

    [SerializeField] private Image imageLevel4;

    [SerializeField] private Image imageLevel5;

    [SerializeField] private int pricePurchase;

    [SerializeField] private int priceUpgrade;

    [SerializeField] private List<int> pricesRent = new List<int>();

    #endregion

    public enum Type : byte
    {
        Tax,
        Jail,
        Start,
        Chance,
        SendJail,
        Property,
        Gambling,
        Transport,
        FreeParking
    }

    public const int PROPERTY_MAX_LEVEL = 6;

    public const int PROPERTY_MIN_LEVEL = 0;

    public Type NodeType 
    { 
        get => this.type;
    }
    
    public int PriceRent 
    { 
        get => this.pricesRent[this.Level]; 
    }

    public bool IsMortgaged 
    {
        get => this.Level == 0;
    }

    public int PriceUpgrade 
    {
        get
        {
            return this.type == MonopolyNode.Type.Transport || this.type == MonopolyNode.Type.Gambling ? this.pricePurchase : this.priceUpgrade;
        }
    }

    public int PricePurchase 
    {
        get => this.pricePurchase;
    }

    public Sprite NodeSprite 
    {
        get => this.spriteLogo;
    }

    public bool IsUpgradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level >= this.Level);
            return isEquallySpread && this.Level <= MonopolyNode.PROPERTY_MAX_LEVEL;
        }
    }

    public bool IsDowngradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level <= this.Level);
            return isEquallySpread && this.Level >= 0;
        }
    }

    public int Level { get; private set; } = 1;

    public MonopolyPlayer Owner { get; private set; }

    public MonopolySet AffiliatedMonopoly { get; private set; }
    
    private void Awake()
    {
        this.imageLogo.sprite = this.spriteLogo;

        switch (this.NodeType)
        {
            case MonopolyNode.Type.Property:
            case MonopolyNode.Type.Gambling:
            case MonopolyNode.Type.Transport:
                {
                    this.Level = 1;
                    this.AffiliatedMonopoly = MonopolyBoard.Instance.GetMonopolySet(this);
                    this.imageMonopolyType.color = this.AffiliatedMonopoly.ColorOfSet;
                }
                break;
        }
    }

    #region Ownership

    public void UpdateOwner()
    {
        this.UpdateOwnerLocally();
        this.UpdateOwnerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    public void ResetOwnership()
    {
        this.ResetOwnershipLocally();
        this.ResetOwnershipServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    private void UpdateOwnerLocally()
    {
        this.Level = 1;
        this.imageOwner.gameObject.SetActive(true);
        this.Owner = GameManager.Instance.CurrentPlayer;
        this.imageOwner.color = GameManager.Instance.CurrentPlayer.PlayerColor;

        if (this.NodeType != MonopolyNode.Type.Gambling || this.NodeType != MonopolyNode.Type.Transport)
        {
            return;
        }

        if (this.Owner.HasPartialMonopoly(this, out MonopolySet monopolySet))
        {
            foreach (MonopolyNode node in monopolySet.NodesInSet)
            {
                if (node.Owner == this.Owner)
                {
                    this.Upgrade();
                }
            }
        }
    }

    private void ResetOwnershipLocally()
    {
        this.Level = 1;
        this.Owner = null;
        this.imageOwner.color = Color.white;
        this.imageOwner?.gameObject?.SetActive(false);
        this.imageLevel1?.gameObject?.SetActive(false);
        this.imageLevel2?.gameObject?.SetActive(false);
        this.imageLevel3?.gameObject?.SetActive(false);
        this.imageLevel4?.gameObject?.SetActive(false);
        this.imageLevel5?.gameObject?.SetActive(false);
        this.imageMortgageStatus?.gameObject?.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateOwnerServerRpc(ServerRpcParams serverRpcParams)
    {
        this.UpdateOwnerClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void UpdateOwnerClientRpc(ClientRpcParams clientRpcParams)
    {
        this.UpdateOwnerLocally();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc(ServerRpcParams serverRpcParams)
    {
        this.ResetOwnershipClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void ResetOwnershipClientRpc(ClientRpcParams clientRpcParams)
    {
        this.ResetOwnershipLocally();
    }

    #endregion

    #region Upgrade/Downgrade

    public void Upgrade()
    {
        this.UpgradeLocally();
        this.UpgradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    public void Downgrade()
    {
        this.DowngradeLocally();
        this.DowngradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    private void UpdateVisualsSpecial()
    {
        switch (this.Level)
        {
            case 0:
                this.imageMortgageStatus.gameObject.SetActive(true);
                break;
            default:
                this.imageMortgageStatus.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateVisualsDefault()
    {
        switch (this.Level)
        {
            case 0:
                {
                    this.imageMortgageStatus.gameObject.SetActive(true);
                }
                break;
            case 1:
                {
                    this.imageLevel1.gameObject.SetActive(false);
                    this.imageMortgageStatus.gameObject.SetActive(false);
                }
                break;
            case 2:
                {
                    this.imageLevel1.gameObject.SetActive(true);
                    this.imageLevel2.gameObject.SetActive(false);
                }
                break;
            case 3:
                {
                    this.imageLevel2.gameObject.SetActive(true);
                    this.imageLevel3.gameObject.SetActive(false);
                }
                break;
            case 4:
                {
                    this.imageLevel3.gameObject.SetActive(true);
                    this.imageLevel4.gameObject.SetActive(false);
                }
                break;
            case 5:
                {
                    this.imageLevel4.gameObject.SetActive(true);
                    this.imageLevel5.gameObject.SetActive(false);
                }
                break;
            case 6:
                {
                    this.imageLevel5.gameObject.SetActive(true);
                    this.imageLevel1.gameObject.SetActive(false);
                    this.imageLevel2.gameObject.SetActive(false);
                    this.imageLevel3.gameObject.SetActive(false);
                    this.imageLevel4.gameObject.SetActive(false);
                }
                break;
        }
    }

    private void UpgradeLocally()
    {
        int previousLevel = this.Level;

        this.Level = ++this.Level % this.pricesRent.Count;

        if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
        {
            this.UpdateVisualsDefault();
        }
        else
        {
            if (!this.IsMortgaged)
            {
                this.UpdateVisualsSpecial();
            }
            else
            {
                this.Level = previousLevel;
            }
        }
    }

    private void DowngradeLocally()
    {
        int previousLevel = this.Level;

        this.Level = --this.Level % this.pricesRent.Count;

        if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
        {
            this.UpdateVisualsDefault();
        }
        else
        {
            if (!this.IsMortgaged)
            {
                this.UpdateVisualsSpecial();
            }
            else
            {
                this.Level = previousLevel;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpgradeServerRpc(ServerRpcParams serverRpcParams)
    {
        this.UpgradeClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void UpgradeClientRpc(ClientRpcParams clientRpcParams)
    {
        this.UpgradeLocally();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DowngradeServerRpc(ServerRpcParams serverRpcParams)
    {
        this.DowngradeClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void DowngradeClientRpc(ClientRpcParams clientRpcParams)
    {
        this.DowngradeLocally();
    }

    #endregion
}
