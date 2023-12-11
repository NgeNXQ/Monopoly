using UnityEngine;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public sealed class MonopolyNode : NetworkBehaviour
{
    #region In-editor setup (Visuals & Logic)

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

    [SerializeField] private List<int> pricing = new List<int>();

    [SerializeField] private string description;

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

    public const int PROPERTY_MAX_LEVEL = 7;

    public const int PROPERTY_MIN_LEVEL = 0;

    public Type NodeType { get => this.type; }

    public Player Owner { get; private set; }

    public int Level { get; private set; }

    public bool IsMortgaged { get => this.Level == 0; }

    public Sprite NodeSprite { get => this.spriteLogo; }

    public int PriceRent { get => this.pricing[this.Level]; }

    public int PriceUpgrade { get => this.pricing[this.Level + 1]; }

    public MonopolySet AffiliatedMonopoly { get; private set; }

    public bool IsDowngradable
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level <= this.Level);
            return isEquallySpread && this.Level >= 0;
        }
    }

    public bool IsUpgradable
    {
        get
        {
            bool isEquallySpread = this.AffiliatedMonopoly.NodesInSet.All(node => node.Level >= this.Level);
            return isEquallySpread && this.Level <= MonopolyNode.PROPERTY_MAX_LEVEL;
        }
    }

    private void Awake()
    {
        this.imageLogo.sprite = this.spriteLogo;
        
        if (this.NodeType == MonopolyNode.Type.Property || this.NodeType == MonopolyNode.Type.Transport || this.NodeType == MonopolyNode.Type.Gambling)
        {
            this.AffiliatedMonopoly = MonopolyBoard.Instance.GetMonopolySet(this);
            this.imageMonopolyType.color = this.AffiliatedMonopoly.ColorOfSet;
        }
    }

    #region UpdateOwner

    public void UpdateOwner(Player owner)
    {
        this.UpdateOwner();
        this.SyncUpdateOwnerServerRpc();
    }

    private void UpdateOwner()
    {
        this.Level = 1;
        this.imageOwner.gameObject.SetActive(true);
        this.Owner = GameManager.Instance.CurrentPlayer;
        this.imageOwner.color = GameManager.Instance.CurrentPlayer.PlayerColor;

        //if (this.NodeType == MonopolyNode.Type.Gambling || this.NodeType == MonopolyNode.Type.Transport)
        //{
        //    if (this.Owner.HasPartialMonopoly(this, out MonopolySet monopolySet))
        //    {
        //        foreach (MonopolyNode node in monopolySet.NodesInSet)
        //        {
        //            if (node.Owner == this.Owner)
        //                this.Upgrade();
        //        }
        //    }
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncUpdateOwnerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.SyncUpdateOwnerClientRpc();
    }

    [ClientRpc]
    private void SyncUpdateOwnerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        this.UpdateOwner();
    }

    #endregion

    #region Upgrade/Downgrade

    public void Upgrade()
    {
        this.Level = ++this.Level % this.pricing.Count;

        UpdateVisuals();
        UpdateVisualsServerRpc();
    }

    public void Downgrade()
    {
        this.Level = --this.Level % this.pricing.Count;

        UpdateVisuals();
        UpdateVisualsServerRpc();
    }

    private void UpdateVisuals()
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

    [ServerRpc(RequireOwnership = false)]
    private void UpdateVisualsServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.UpdateVisualsClientRpc();
    }

    [ClientRpc]
    private void UpdateVisualsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        this.UpdateVisuals();
    }

    #endregion
}
