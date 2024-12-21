using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public sealed class MonopolyNode : NetworkBehaviour
{
    [SerializeField]
    private Type type;

    [SerializeField]
    private Image imageLogo;

    [SerializeField]
    private Sprite spriteLogo;

    [SerializeField]
    private Image imageOwner;

    [SerializeField]
    private Image imageMonopolyType;

    [SerializeField]
    private Image imageMortgageStatus;

    [SerializeField]
    private Image imageLevel1;

    [SerializeField]
    private Image imageLevel2;

    [SerializeField]
    private Image imageLevel3;

    [SerializeField]
    private Image imageLevel4;

    [SerializeField]
    private Image imageLevel5;

    [SerializeField]
    private int pricePurchase;

    [SerializeField]
    private int priceUpgrade;

    [SerializeField]
    private List<int> pricesRent = new List<int>();

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
    internal const int PROPERTY_MIN_LEVEL = 0;
    internal const int PROPERTY_MAX_LEVEL = 6;

    public Type NodeType => this.type;
    internal Sprite NodeSprite => this.spriteLogo;
    internal MonopolySet AffiliatedMonopoly { get; private set; }

    internal int Level { get; private set; }
    internal bool IsMortgaged => this.Level == 0;
    internal int PricePurchase => this.pricePurchase;
    internal PawnController Owner { get; private set; }

    internal int WorthTotal
    {
        get
        {
            if (this.IsMortgaged)
                return 0;

            if (this.NodeType == Type.Property)
                return this.pricePurchase + this.priceUpgrade * (this.Level - 1);
            else
                return this.pricePurchase;
        }
    }

    internal int PriceRent
    {
        get
        {
            return this.pricesRent[this.Level] * (this.NodeType == Type.Gambling ? GameManager.Instance.TotalRollResult : 1);
        }
    }

    internal int PriceUpgrade
    {
        get
        {
            if (this.NodeType == Type.Property)
                return this.Level == MonopolyNode.LEVEL_MORTGAGE ? this.pricePurchase : this.priceUpgrade;
            else
                return this.pricePurchase;
        }
    }

    internal int PriceDowngrade
    {
        get
        {
            if (this.NodeType == Type.Property)
                return this.Level == MonopolyNode.LEVEL_OWNERSHIP ? this.pricePurchase : this.priceUpgrade;
            else
                return this.pricePurchase;
        }
    }

    internal bool IsTradable
    {
        get
        {
            if (this.NodeType == Type.Property)
            {
                if (this.Level != MonopolyNode.LEVEL_OWNERSHIP)
                    return false;

                foreach (MonopolyNode node in this.AffiliatedMonopoly.NodesInSet)
                {
                    if (node.Level > MonopolyNode.LEVEL_OWNERSHIP)
                        return false;
                }

                return true;
            }
            else
            {
                return this.Level > MonopolyNode.LEVEL_MORTGAGE;
            }
        }
    }

    internal bool IsUpgradable
    {
        get
        {
            if (this.NodeType == Type.Property)
            {
                bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level >= this.Level);
                return (isEquallySpread && this.Level < MonopolyNode.PROPERTY_MAX_LEVEL) || this.IsMortgaged;
            }
            else
            {
                return this.Level == MonopolyNode.LEVEL_MORTGAGE;
            }
        }
    }

    internal bool IsDowngradable
    {
        get
        {
            if (this.NodeType == Type.Property)
            {
                bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level <= this.Level);
                return isEquallySpread && this.Level > MonopolyNode.PROPERTY_MIN_LEVEL;
            }
            else
            {
                return this.Level > MonopolyNode.PROPERTY_MIN_LEVEL;
            }
        }
    }

    private void Awake()
    {
        this.imageLogo.sprite = this.spriteLogo;

        switch (this.NodeType)
        {
            case Type.Property:
            case Type.Gambling:
            case Type.Transport:
                {
                    this.Level = 1;
                    this.AffiliatedMonopoly = MonopolyBoard.Instance.GetMonopolySet(this);
                    this.imageMonopolyType.color = this.AffiliatedMonopoly.ColorOfSet;
                }
                break;
        }
    }

    private void UpdateVisualsSpecial()
    {
        if (this.Owner == null)
        {
            this.imageOwner.gameObject.SetActive(false);
            this.imageMortgageStatus.gameObject.SetActive(false);
        }
        else
        {
            if (this.Level == MonopolyNode.LEVEL_MORTGAGE)
            {
                this.imageMortgageStatus.gameObject.SetActive(true);
            }
            else
            {
                this.imageOwner.gameObject.SetActive(true);
                this.imageOwner.color = this.Owner.PawnColor;
                this.imageMortgageStatus.gameObject.SetActive(false);
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
            switch (this.Level)
            {
                case MonopolyNode.LEVEL_MORTGAGE:
                    {
                        this.imageMortgageStatus.gameObject.SetActive(true);
                    }
                    break;
                case MonopolyNode.LEVEL_OWNERSHIP:
                    {
                        this.imageOwner.gameObject.SetActive(true);
                        this.imageOwner.color = this.Owner.PawnColor;

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

    [ServerRpc(RequireOwnership = false)]
    internal void ResetOwnershipServerRpc(ServerRpcParams serverRpcParams)
    {
        this.ResetOwnershipClientRpc(GameManager.Instance.TargetAllClients);
    }

    [ClientRpc]
    private void ResetOwnershipClientRpc(ClientRpcParams clientRpcParams)
    {
        this.Level = MonopolyNode.LEVEL_OWNERSHIP;
        PawnController previousOwner = this.Owner;
        this.Owner.OwnedNodes.Remove(this);
        this.Owner = null;

        if (this.NodeType == Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
            this.UpdateSpecialNodesLevel(previousOwner); ;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void UpdateOwnershipServerRpc(int networkIndex, ServerRpcParams serverRpcParams)
    {
        this.UpdateOwnershipClientRpc(networkIndex, GameManager.Instance.TargetAllClients);
    }

    [ClientRpc]
    private void UpdateOwnershipClientRpc(int networkIndex, ClientRpcParams clientRpcParams)
    {
        this.Owner = GameManager.Instance.GetPawnController(networkIndex);
        this.Owner.OwnedNodes.Add(this);

        if (this.NodeType == Type.Property)
        {
            this.Level = MonopolyNode.LEVEL_OWNERSHIP;
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
            this.UpdateSpecialNodesLevel(this.Owner);
            this.Level = this.AffiliatedMonopoly.GetLevel(this.Owner);
        }
    }

    internal void Upgrade()
    {
        this.UpdateLevelServerRpc(this.Level + 1, GameManager.Instance.SenderLocalClient);
    }

    internal void Downgrade()
    {
        this.UpdateLevelServerRpc(this.Level - 1, GameManager.Instance.SenderLocalClient);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateLevelServerRpc(int level, ServerRpcParams serverRpcParams)
    {
        this.UpdateLevelClientRpc(level, GameManager.Instance.TargetAllClients);
    }

    [ClientRpc]
    private void UpdateLevelClientRpc(int level, ClientRpcParams clientRpcParams)
    {
        this.Level = level;

        if (this.NodeType == Type.Property)
        {
            this.UpdateVisualsProperty();
        }
        else
        {
            this.UpdateVisualsSpecial();
            this.UpdateSpecialNodesLevel(this.Owner);
        }
    }

    private void UpdateSpecialNodesLevel(PawnController owner)
    {
        if (owner == null)
            throw new System.ArgumentNullException($"{nameof(owner)} is null.");

        if (owner.HasPartialMonopoly(this.AffiliatedMonopoly))
        {
            int levelAdjustment = this.IsMortgaged ? -1 : 1;

            foreach (MonopolyNode node in this.AffiliatedMonopoly.GetNodesOwnedByPawn(owner))
            {
                if (!node.IsMortgaged)
                    node.Level += levelAdjustment;
            }
        }
    }
}
