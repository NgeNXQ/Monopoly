using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MonopolyCell : MonoBehaviour
{
    internal enum MonopolyCellType : byte
    {
        Tax,
        Jail,
        Start,
        Casino,
        Chance,
        Vehicle,
        SendJail,
        Property,    
    }

    [SerializeField]
    internal MonopolyCellType Type;

    [SerializeField]
    private Image imageMortgagedCell;

    [SerializeField]
    private TextMeshProUGUI textPrice;

    #region Pricing

    [Header("Pricing")]

    [SerializeField]
    private int initialPrice;

    [SerializeField]
    internal int[] rentValues;

    [SerializeField]
    private int initialMortgageValue;

    #endregion

    internal int CurrentRentPrice { get; }

    internal int CurrentMortgageValue { get; }

    internal bool IsMortgaged { get; private set; }

    private void OnValidate()
    {
        if ((this.Type == MonopolyCellType.Property || this.Type == MonopolyCellType.Vehicle) && this.textPrice == null)
            throw new System.NullReferenceException($"{nameof(this.textPrice)} is not set.");

        // Check if all rent prices are input
    }

    private void Awake()
    {
        if (this.Type == MonopolyCellType.Property || this.Type == MonopolyCellType.Vehicle)
            this.textPrice.text = $"₴ {this.initialPrice}К";
    }

//   internal int MortgageCell()
//    {
//        //this.IsMortgaged = true;
//
//        //change visual
//
//        return this.CurrentMortgageValue;
//    }

//    internal int UnMortgageCell()
//    {
//        //this.isMortgaged = false;
//
//        //change visual
//
//        return this.CurrentMortgageValue;
//    }

//    public void OnOwnerUpdated()
//    {
//
//    }
}
