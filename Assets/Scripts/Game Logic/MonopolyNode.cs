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

    private const int LEVEL_MORTGAGE = 0;

    private const int LEVEL_OWNERSHIP = 1;

    public const int PROPERTY_MIN_LEVEL = 0;

    public const int PROPERTY_MAX_LEVEL = 6;

    public NetworkVariable<int> Level { get; private set; }

    public Type NodeType 
    { 
        get => this.type;
    }
    
    public int PriceRent 
    {
        get
        {
            return this.pricesRent[this.LocalLevel] * (this.NodeType == MonopolyNode.Type.Gambling ? GameManager.Instance.TotalRollResult : 1);
        }
    }

    public bool IsMortgaged 
    {
        get => this.LocalLevel == 0;
    }

    public int PriceUpgrade 
    {
        get
        {
            return this.LocalLevel == 0 ? this.pricePurchase : this.priceUpgrade;
        }
    }

    public int PricePurchase 
    {
        get => this.pricePurchase;
    }

    public int PriceDowngrade 
    {
        get
        {
            return this.LocalLevel == 1 ? this.pricePurchase : this.priceUpgrade;
        }
    }
    
    public Sprite NodeSprite 
    {
        get => this.spriteLogo;
    }

    public bool IsTradable 
    {
        get => this.LocalLevel == 1;
    }

    public bool IsUpgradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.LocalLevel >= this.LocalLevel);
            return (isEquallySpread && this.LocalLevel < MonopolyNode.PROPERTY_MAX_LEVEL) || this.IsMortgaged;
        }
    }

    public bool IsDowngradable 
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.LocalLevel <= this.LocalLevel);
            return isEquallySpread && this.LocalLevel > MonopolyNode.PROPERTY_MIN_LEVEL;
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
        this.Level.OnValueChanged += this.HandleLevelChanged;
    }

    private void OnDisable()
    {
        this.Level.OnValueChanged -= this.HandleLevelChanged;
    }

    #region Visuals

    private void UpdateVisualsSpecial()
    {
        if (this.Owner == null)
        {
            this.imageOwner.gameObject.SetActive(false);
            this.imageMortgageStatus.gameObject.SetActive(false);
        }
        else
        {
            if (this.LocalLevel == MonopolyNode.LEVEL_MORTGAGE)
            {
                this.imageMortgageStatus.gameObject.SetActive(true);
            }
            else
            {
                this.imageOwner.gameObject.SetActive(true);
                this.imageOwner.color = this.Owner.PlayerColor;
            }
        }
    }

    private void UpdateVisualsProperty()
    {
        if (this.Owner == null)
        {
            this.imageOwner.gameObject.SetActive(false);
            this.imageLevel1.gameObject.SetActive(false);
            this.imageLevel2.gameObject.SetActive(false);
            this.imageLevel3.gameObject.SetActive(false);
            this.imageLevel4.gameObject.SetActive(false);
            this.imageLevel5.gameObject.SetActive(false);
            this.imageMortgageStatus.gameObject.SetActive(false);
        }
        else
        {
            switch (this.LocalLevel)
            {
                case MonopolyNode.LEVEL_MORTGAGE:
                    {
                        this.imageMortgageStatus.gameObject.SetActive(true);
                    }
                    break;
                case MonopolyNode.LEVEL_OWNERSHIP:
                    {
                        this.imageOwner.gameObject.SetActive(true);
                        this.imageOwner.color = this.Owner.PlayerColor;

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
    }

    #endregion

    #region Ownership

    public void ResetOwnership()
    {
        this.Owner = null;
        this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;

        if (this.NodeType == MonopolyNode.Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
        }

        this.ResetOwnershipServerRpc(GameManager.Instance.ServerParamsCurrentClient);
    }

    public void UpdateOwnership(ulong ownerId)
    {
        this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;
        this.Owner = GameManager.Instance.GetPlayerById(ownerId);

        if (this.Owner == null)
        {
            return;
        }

        if (this.NodeType == MonopolyNode.Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
        }

        this.UpdateOwnershipServerRpc(ownerId, GameManager.Instance.ServerParamsCurrentClient);

        if (this.NodeType != MonopolyNode.Type.Transport && this.NodeType != MonopolyNode.Type.Gambling)
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
                while (node.LocalLevel < this.AffiliatedMonopoly.OwnedByPlayerCount)
                {
                    node.Upgrade();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc(ServerRpcParams serverRpcParams)
    {
        this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;
        this.Level.Value = MonopolyNode.LEVEL_OWNERSHIP;

        if (this.Owner != null)
        {
            this.Owner = null;

            if (this.NodeType == MonopolyNode.Type.Property)
            {
                this.UpdateVisualsProperty();
            }
            else
            {
                this.UpdateVisualsSpecial();
            }
        }

        this.ResetOwnershipClientRpc(GameManager.Instance.ClientParamsClientOtherClients);
    }

    [ClientRpc]
    private void ResetOwnershipClientRpc(ClientRpcParams clientRpcParams)
    {
        if (this.Owner != null)
        {
            this.Owner = null;
            this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;

            if (this.NodeType == MonopolyNode.Type.Property)
            {
                this.UpdateVisualsProperty();
            }
            else
            {
                this.UpdateVisualsSpecial();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateOwnershipServerRpc(ulong ownerId, ServerRpcParams serverRpcParams)
    {
        this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;
        this.Level.Value = MonopolyNode.LEVEL_OWNERSHIP;

        MonopolyPlayer playerOwner = GameManager.Instance.GetPlayerById(ownerId);

        if (this.Owner != playerOwner)
        {
            this.Owner = playerOwner;

            if (this.NodeType == MonopolyNode.Type.Property)
            {
                this.UpdateVisualsProperty();
            }
            else
            {
                this.UpdateVisualsSpecial();
            }
        }

        this.UpdateOwnershipClientRpc(ownerId, GameManager.Instance.ClientParamsClientOtherClients);
    }

    [ClientRpc]
    private void UpdateOwnershipClientRpc(ulong ownerId, ClientRpcParams clientRpcParams)
    {
        MonopolyPlayer playerOwner = GameManager.Instance.GetPlayerById(ownerId);

        if (this.Owner != playerOwner)
        {
            this.Owner = playerOwner;
            this.LocalLevel = MonopolyNode.LEVEL_OWNERSHIP;

            if (this.NodeType == MonopolyNode.Type.Property)
            {
                this.UpdateVisualsProperty();
            }
            else
            {
                this.UpdateVisualsSpecial();
            }
        }
    }
    
    #endregion

    #region Upgrade/Downgrade

    public void Upgrade()
    {
        ++this.LocalLevel;

        if (this.NodeType == MonopolyNode.Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
        }

        this.ChangeLevelServerRpc(this.LocalLevel, GameManager.Instance.ServerParamsCurrentClient);
    }

    public void Downgrade()
    {
        --this.LocalLevel;

        if (this.NodeType == MonopolyNode.Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
        }

        this.ChangeLevelServerRpc(this.LocalLevel, GameManager.Instance.ServerParamsCurrentClient);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeLevelServerRpc(int level, ServerRpcParams serverRpcParams)
    {
        this.Level.Value = level;
    }

    #endregion

    private void HandleLevelChanged(int previousValue, int newValue)
    {
        this.LocalLevel = newValue;

        if (this.NodeType == MonopolyNode.Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
        }
    }
}
