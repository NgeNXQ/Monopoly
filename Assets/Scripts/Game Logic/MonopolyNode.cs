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

    public const int PROPERTY_MIN_LEVEL = 0;

    public const int PROPERTY_MAX_LEVEL = 6;

    public NetworkVariable<int> Level;

    public Type NodeType 
    { 
        get => this.type;
    }
    
    public int PriceRent 
    {
        get
        {
            return this.pricesRent[this.Level.Value] * (this.NodeType == MonopolyNode.Type.Gambling ? GameManager.Instance.TotalRollResult : 1);
        }
    }

    public bool IsMortgaged 
    {
        get => this.Level.Value == 0;
    }

    public int PriceUpgrade 
    {
        get
        {
            return this.Level.Value == 0 ? this.pricePurchase : this.priceUpgrade;
        }
    }

    public int PriceDowngrade
    {
        get
        {
            return this.Level.Value == 1 ? this.pricePurchase : this.priceUpgrade;
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

    public bool IsTradable 
    {
        get => this.Level.Value == 1 || this.Level.Value == 0;
    }

    public bool IsUpgradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level.Value >= this.Level.Value);
            return (isEquallySpread && this.Level.Value <= MonopolyNode.PROPERTY_MAX_LEVEL) || this.IsMortgaged;
        }
    }

    public bool IsDowngradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level.Value <= this.Level.Value);
            return isEquallySpread && this.Level.Value >= MonopolyNode.PROPERTY_MIN_LEVEL;
        }
    }

    public int LocalLevel { get; private set; }

    public MonopolyPlayer Owner { get; private set; }

    public MonopolySet AffiliatedMonopoly { get; private set; }
    
    private void Awake()
    {
        this.imageLogo.sprite = this.spriteLogo;

        this.Level = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        switch (this.NodeType)
        {
            case MonopolyNode.Type.Property:
            case MonopolyNode.Type.Gambling:
            case MonopolyNode.Type.Transport:
                {
                    this.LocalLevel = 1;
                    this.AffiliatedMonopoly = MonopolyBoard.Instance.GetMonopolySet(this);
                    this.imageMonopolyType.color = this.AffiliatedMonopoly.ColorOfSet;
                }
                break;
        }
    }

    private void OnEnable()
    {
        this.Level.OnValueChanged += this.HandleValueChanged;
    }

    private void OnDisable()
    {
        this.Level.OnValueChanged -= this.HandleValueChanged;
    }

    private void Update()
    {
        if (this.NodeType == Type.Gambling || this.NodeType == Type.Transport) 
            Debug.Log($"{this.name}:{this.Level.Value}");
    }

    #region Ownership

    //[ServerRpc]
    //private void UpgradeSpecialNodeServerRpc(int desiredLevel, ServerRpcParams serverRpcParams)
    //{
    //    while (this.Level < desiredLevel)
    //    {
    //        this.UpgradeLocally();
    //    }
    //}



    private void HandleValueChanged(int previousValue, int newValue)
    {
        this.LocalLevel = newValue;

        this.UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (this.NodeType == Type.Property)
        {
            switch (this.Level.Value)
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
                        this.imageLevel1.gameObject.SetActive(true);
                        this.imageLevel2.gameObject.SetActive(true);
                        this.imageLevel3.gameObject.SetActive(true);
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
        else
        {
            switch (this.Level.Value)
            {
                case 0:
                    this.imageMortgageStatus.gameObject.SetActive(true);
                    break;
                default:
                    this.imageMortgageStatus.gameObject.SetActive(false);
                    break;
            }
        }
    }

    public void UpdateOwnership()
    {


        this.UpdateOwnerServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    public void ResetOwnership()
    {


        this.ResetOwnershipServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateOwnerServerRpc(ServerRpcParams serverRpcParams)
    {
        this.Level.Value = 1;
        this.UpdateVisuals();
        this.UpdateOwnerClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void UpdateOwnerClientRpc(ClientRpcParams clientRpcParams)
    {
        this.imageOwner.gameObject.SetActive(true);
        this.Owner = GameManager.Instance.CurrentPlayer;
        this.imageOwner.color = GameManager.Instance.CurrentPlayer.PlayerColor;

        if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
        {
            return;
        }

        if (this.Owner?.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (!this.Owner.HasPartialMonopoly(this, out _))
        {
            return;
        }

        foreach (MonopolyNode node in this.AffiliatedMonopoly.OwnedByPlayerNodes)
        {
            if (!node.IsMortgaged)
            {
                node.UpgradeServerRpc(this.AffiliatedMonopoly.OwnedByPlayerCount, GameManager.Instance.ServerParamsCurrentClient);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc(ServerRpcParams serverRpcParams)
    {
        this.Level.Value = 1;
        this.UpdateVisuals();
        this.ResetOwnershipClientRpc(GameManager.Instance.ClientParamsHostOtherClients);
    }

    [ClientRpc]
    private void ResetOwnershipClientRpc(ClientRpcParams clientRpcParams)
    {
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

    #endregion

    #region Upgrade/Downgrade

    public void Upgrade()
    {
        this.UpgradeServerRpc(this.Level.Value + 1, GameManager.Instance.ServerParamsCurrentClient);
    }

    public void Downgrade()
    {
        this.DowngradeServerRpc(this.Level.Value + 1, GameManager.Instance.ServerParamsCurrentClient);
    }

    //private void UpgradeLocally(int level)
    //{
    //    this.LocalLevel = level;
    //    this.LocalLevel = this.Level.Value;

    //    if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
    //    {
    //        this.UpdateVisuals();
    //    }
    //    else
    //    {
    //        if (this.LocalLevel == 1)
    //        {
    //            this.imageMortgageStatus.gameObject.SetActive(false);
    //        }

    //        //if (this.Owner?.OwnerClientId != NetworkManager.Singleton.LocalClientId)
    //        //{
    //        //    return;
    //        //}

    //        //if (!this.Owner.HasPartialMonopoly(this, out _))
    //        //{
    //        //    return;
    //        //}

    //        //while (this.Level < this.AffiliatedMonopoly.Level)
    //        //{
    //        //    ++this.Level;
    //        //    this.UpgradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    //        //}
    //    }
    //}

    //private void DowngradeLocally(int level)
    //{
    //    this.LocalLevel = level;
    //    this.LocalLevel = this.Level.Value;

    //    //if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
    //    //{
    //    //    this.UpdateVisuals();
    //    //}
    //    //else
    //    //{
    //    //    if (this.Level == 0)
    //    //    {
    //    //        this.imageMortgageStatus.gameObject.SetActive(true);
    //    //    }

    //    //    if (NetworkManager.Singleton.LocalClientId != GameManager.Instance.CurrentPlayer.OwnerClientId)
    //    //    {
    //    //        return;
    //    //    }

    //    //    if (this.Owner.OwnerClientId != NetworkManager.Singleton.LocalClientId)
    //    //    {
    //    //        return;
    //    //    }

    //    //    if (this.Owner.HasPartialMonopoly(this, out _))
    //    //    {
    //    //        while (this.Level > this.AffiliatedMonopoly.Level)
    //    //        {
    //    //            ++this.Level;
    //    //            this.UpgradeServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    //    //        }
    //    //    }
    //    //}
    //}

    //private void UpdateVisuals()
    //{
        
    //}

    [ServerRpc(RequireOwnership = false)]
    public void UpgradeServerRpc(int level, ServerRpcParams serverRpcParams)
    {
        this.Level.Value = level;
        this.UpgradeClientRpc(GameManager.Instance.ClientParamsClientOtherClients);
    }

    [ClientRpc]
    private void UpgradeClientRpc(ClientRpcParams clientRpcParams)
    {
        //this.UpgradeLocally();

        //this.LocalLevel = this.Level.Value;

        //if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
        //{
        //    this.UpdateVisuals();
        //}
        //else
        //{

        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void DowngradeServerRpc(int level, ServerRpcParams serverRpcParams)
    {
        this.Level.Value = level;
        this.DowngradeClientRpc(GameManager.Instance.ClientParamsClientOtherClients);
    }

    [ClientRpc]
    private void DowngradeClientRpc(ClientRpcParams clientRpcParams)
    {
        //this.DowngradeLocally(level);

        //this.LocalLevel = this.Level.Value;

        //if (this.NodeType != MonopolyNode.Type.Gambling && this.NodeType != MonopolyNode.Type.Transport)
        //{
        //    this.UpdateVisuals();
        //}
        //else
        //{

        //}
    }

    #endregion
}
