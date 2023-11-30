﻿using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class MonopolyNode : NetworkBehaviour
{
    public enum MonopolyNodeType : byte
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

    #region In-editor setup (Visuals & Logic)

    [SerializeField] private MonopolyNodeType type;

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

    #endregion

    private int level;

    private Player player;

    public MonopolyNodeType Type { get => this.type; }

    public Player Owner 
    { 
        get => this.player; 
        set
        {
            this.level = 1;
            this.player = value; 
            this.imageOwner.gameObject.SetActive(true);
            this.imageOwner.color = this.player.PlayerColor;
        } 
    }

    public int Level { get => this.level; }

    //public int Price { get => this.pricing[this.level]; }

    public int Price { get => 0; }

    public bool IsMortgaged { get => this.level == 0; }

    public Sprite NodeSprite { get => this.spriteLogo; }

    public Color OwnerColor { set => this.imageOwner.color = value; }

    public Color MonopolyColor { set => this.imageMonopolyType.color = value; }

    private void Awake() => this.imageLogo.sprite = this.spriteLogo;

    public void Upgrade()
    {
        ++this.level;
        this.UpdateVisuals();
    }

    public void Downgrade()
    {
        --this.level;
        this.UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        switch (this.level)
        {
            case 0:
                {
                    this.imageLevel1.gameObject.SetActive(false);
                    this.imageMortgageStatus.gameObject.SetActive(true);
                }
                break;
            case 1:
                {
                    this.imageLevel1.gameObject.SetActive(true);
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



















    //public delegate void DrawCommunityCard(Player player);
    //public static DrawCommunityCard OnDrawCommunityCard;

    //public delegate void DrawChanceCard(Player player);
    //public static DrawChanceCard OnDrawChanceCard;

    //public int NumberOfHouses { get; set; }

    //[SerializeField] public GameObject[] houses;
    //[SerializeField] public GameObject hotel;

    //public delegate void ShowInputPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    //public static ShowInputPanel OnShowInputPanel;

    //public delegate void ShowPropertyBuyPanel(MonopolyCell node, Player player);
    //public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;

    //private void OnValidate()
    //{
    //    if ((this.Type == MonopolyCellType.Property || this.Type == MonopolyCellType.Transport) && this.textPrice == null)
    //        throw new System.NullReferenceException($"{nameof(this.textPrice)} is not set.");

    //    // Check if all rent prices are input

    //    //OnOwnerUpdated();
    //    //UnMortgageCell();
    //}

    //private void Awake()
    //{
    //    if (this.Type == MonopolyCellType.Property || this.Type == MonopolyCellType.Transport)
    //        this.textPrice.text = $"₴ {this.initialPrice}К";
    //}

    //internal int MortgageCell()
    //{
    //    //        //this.IsMortgaged = true;
    //    //
    //    //        //change visual
    //    //
    //    //        return this.CurrentMortgageValue;

    //    return 0;
    //}

    //internal void UnMortgageCell()
    //{
    //    this.IsMortgaged = false;

    //    //change visual
    //}

    //public void OnOwnerUpdated()
    //{
    //    //change visual

    //    //if (Owner.Name != "")
    //    //{
    //        //imageOwner. // FILL IMAGE
    //    //}
    //}

    //public void ChangeColorField(Color color)
    //{
    //    if (cellMonopolyType != null)
    //        cellMonopolyType.color = color;
    //}

    //public void PlayerLandedOnNode(Player player)
    //{
    //    bool continueTurn = true;

    //    switch (this.Type)
    //    {
    //        case MonopolyCellType.Property:
    //            {
    //                if (Owner != null && Owner != player && !IsMortgaged)
    //                {
    //                    int rentToPay = CalculatePropertyRent();
    //                    player.PayRent(rentToPay, this.Owner);
    //                }
    //                else if (Owner != null && player.CanAfford(this.initialPrice))
    //                {
    //                    OnShowPropertyBuyPanel.Invoke(this, player);
    //                    player.BuyProperty(this);
    //                    OnOwnerUpdated();
    //                }
    //                else
    //                {

    //                }
    //            }
    //        break;
    //        case MonopolyCellType.Gamble:
    //        {

    //                if (Owner != null && Owner != player && !IsMortgaged)
    //                {
    //                    int rentToPay = CalculateGambleRent();
    //                    player.PayRent(rentToPay, this.Owner);
    //                }
    //                else if (Owner != null && player.CanAfford(this.initialPrice))
    //                {
    //                    player.BuyProperty(this);
    //                    OnOwnerUpdated();
    //                }
    //                else
    //                {

    //                }
    //            }
    //        break;
    //        case MonopolyCellType.Transport:
    //            {

    //                if (Owner != null && Owner != player && !IsMortgaged)
    //                {
    //                    int rentToPay = CalculateTransportRent();
    //                    player.PayRent(rentToPay, this.Owner);
    //                }
    //                else if (Owner != null && player.CanAfford(this.initialPrice))
    //                {
    //                    player.BuyProperty(this);
    //                    OnOwnerUpdated();
    //                }
    //                else
    //                {

    //                }
    //            }
    //        break;
    //        case MonopolyCellType.Tax:
    //            {
    //                //GameManager.instance.AddTaxToPool(initialPrice);
    //                //player.PayTax(initialPrice);
    //                OnDrawCommunityCard.Invoke(player);
    //                continueTurn = false;
    //            }
    //        break;
    //        case MonopolyCellType.SendJail:
    //            {
    //                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(player.CurrentPosition);
    //                player.GoToJail(indexOnBoard);
    //                continueTurn = false;
    //            }
    //        break;
    //        case MonopolyCellType.Chance:
    //            {
    //                OnDrawChanceCard.Invoke(player);
    //                continueTurn = false;
    //            }
    //        break;
    //            //case MonopolyCellType.:
    //            //{

    //            //}
    //            //break;


    //    }

    //    if (!continueTurn)
    //        return;

    //    OnShowInputPanel.Invoke(true, GameManager.instance.RolledADounle, !GameManager.instance.RolledADounle);
    //}

    //public void ContinueGame()
    //{
    //    // double roll

    //    // or

    //    if (GameManager.instance.RolledADounle)
    //    {
    //        GameManager.instance.RollDice();
    //    }
    //    else
    //    {
    //        GameManager.instance.SwitchPlayer();
    //    }


    //}

    //public int CalculatePropertyRent()
    //{
    //    switch (numberOfHouses)
    //    {
    //        case 0:
    //            {
    //                var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

    //                if (allSame)
    //                {
    //                    CurrentRentPrice = BaseRentPrice * 2;
    //                }
    //                else
    //                {
    //                    CurrentRentPrice = BaseRentPrice;
    //                }
    //            }
    //        break;
    //        case 1:
    //            {
    //                CurrentRentPrice = rentValues[numberOfHouses - 1];
    //            }
    //        break;
    //        case 2:
    //            {
    //                CurrentRentPrice = rentValues[numberOfHouses - 1];
    //            }
    //        break;
    //        case 3:
    //            {
    //                CurrentRentPrice = rentValues[numberOfHouses - 1];
    //            }
    //        break;
    //        case 4:
    //            {
    //                CurrentRentPrice = rentValues[numberOfHouses - 1];
    //            }
    //        break;
    //        case 5:
    //            {
    //                CurrentRentPrice = rentValues[numberOfHouses - 1];
    //            }
    //        break;
    //    }

    //    return CurrentRentPrice;
    //}

    //public void SetOWner(Player player)
    //{
    //    Owner = player;
    //    OnOwnerUpdated();
    //}

    //public int CalculateGambleRent()
    //{
    //    int[] lastRolledDices = GameManager.instance.LastRolledDice();

    //    int result = 0;

    //    var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

    //    if (allSame)
    //    {
    //        result = (lastRolledDices[0] + lastRolledDices[0]) * 10;
    //    }
    //    else
    //    {
    //        result = (lastRolledDices[0] + lastRolledDices[0]) * 4;
    //    }

    //    return result;
    //}

    //public int CalculateTransportRent()
    //{
    //    int result = 0;

    //    var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

    //    int amount = 0;

    //    foreach (var item in list)
    //    {
    //        if (item.Owner == this.Owner)
    //            ++amount;
    //    }

    //    result = BaseRentPrice * (int)Mathf.Pow(2, amount - 1);

    //    return result;
    //}

    //void VisualizeHouses()
    //{
    //    switch (numberOfHouses) 
    //    {
    //        case 0:
    //            {
    //                houses[0].SetActive(false);
    //                houses[1].SetActive(false);
    //                houses[2].SetActive(false);
    //                houses[3].SetActive(false);
    //                hotel.SetActive(false);
    //            }
    //        break;
    //        case 1:
    //            {
    //                houses[0].SetActive(true);
    //                houses[1].SetActive(false);
    //                houses[2].SetActive(false);
    //                houses[3].SetActive(false);
    //                hotel.SetActive(false);
    //            }
    //        break;
    //        case 2:
    //            {
    //                houses[0].SetActive(true);
    //                houses[1].SetActive(true);
    //                houses[2].SetActive(false);
    //                houses[3].SetActive(false);
    //                hotel.SetActive(false);
    //            }
    //        break;
    //        case 3:
    //            {
    //                houses[0].SetActive(true);
    //                houses[1].SetActive(true);
    //                houses[2].SetActive(true);
    //                houses[3].SetActive(false);
    //                hotel.SetActive(false);
    //            }
    //        break;
    //        case 4:
    //            {
    //                houses[0].SetActive(true);
    //                houses[1].SetActive(true);
    //                houses[2].SetActive(true);
    //                houses[3].SetActive(true);
    //                hotel.SetActive(false);
    //            }
    //        break;
    //        case 5:
    //            {
    //                houses[0].SetActive(false);
    //                houses[1].SetActive(false);
    //                houses[2].SetActive(false);
    //                houses[3].SetActive(false);
    //                hotel.SetActive(true);
    //            }
    //        break;
    //    }
    //}

    //public void BuildHouseOrHotel()
    //{
    //    if (Type == MonopolyCellType.Property)
    //    {
    //        numberOfHouses++;
    //        VisualizeHouses();
    //    }
    //}

    //public int SellHouseOrHotel()
    //{
    //    if (Type == MonopolyCellType.Property)
    //    {
    //        numberOfHouses--;
    //        VisualizeHouses();
    //    }

    //    return HouseCost / 2;
    //}

    //public void ResetNode()
    //{
    //    if (IsMortgaged)
    //    {
    //        // Change visual
    //        IsMortgaged = false;
    //    }

    //    if (Type == MonopolyCellType.Property)
    //    {
    //        numberOfHouses = 0;
    //        VisualizeHouses();
    //    }

    //    Owner.RemoveProperty(this);

    //    // Change visual
    //}


}
