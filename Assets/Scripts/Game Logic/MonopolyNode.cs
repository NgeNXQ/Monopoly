using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class MonopolyNode : MonoBehaviour
{
    public enum MonopolyNodeType
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

    [SerializeField] private MonopolyNodeType type;

    #region Visuals

    [SerializeField] private Image imageLogo;

    [SerializeField] private Sprite spriteLogo;

    [SerializeField] private Image imageOwner;

    [SerializeField] private Image imageMonopolyType;

    [SerializeField] private Image imageMortgageStatus;

    #endregion

    #region Pricing

    [SerializeField] private int taxAmount;

    [SerializeField] private int priceInitial;

    [SerializeField] private int priceMortgage;

    [SerializeField] private List<int> pricesRent = new List<int>();

    #endregion

    private int currentLevel;

    public Player Owner { get; set; }

    public bool IsMortgaged { get; set; }

    public int TaxAmount { get => this.taxAmount; }

    public MonopolyNodeType Type { get => this.type; }

    public Color OwnerColor { set => this.imageOwner.color = value; }

    public Color MonopolySetColor { set => this.imageMonopolyType.color = value; }

    public int Price
    {
        get
        {
            if (this.Owner == null)
                return this.priceInitial;
            else
                return this.pricesRent[this.currentLevel];
        }
    }

    private void Awake() => this.imageLogo.sprite = this.spriteLogo;






















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
