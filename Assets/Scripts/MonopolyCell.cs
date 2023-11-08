using TMPro;
using UnityEditor;
using UnityEngine;

public sealed class MonopolyCell : MonoBehaviour
{
    public enum MonopolyCellType : byte
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

    #region Type

    [Header("Type")]

    [SerializeField]
    public MonopolyCellType Type;

    #endregion

    #region Pricing

    [Header("Pricing")]

    [SerializeField]
    private int initialPrice;

    [SerializeField]
    private int initialRent;

    [SerializeField]
    internal int mortgageValue;

    [SerializeField]
    internal int[] rentValues;

    public bool IsMortgaged { get; private set; }

    //public bool IsMortgaged { get; private set; }

    #endregion

    private TMP_Text costText;

    private void Awake()
    {
        //if (this.Type == MonopolyCellType.Property || this.Type == MonopolyCellType.Vehicle)
        //{
           // this.rentValues = new int[5];
            //this.GetComponentsInChildren<TMP_Text>()[0].text = $"₴ {this.initialPrice}";
        //}
    }

    private void OnValidate()
    {
        //if (this.costText != null)
        //{
            //this.rentValues = new int[5];
            //this.costText.text = $"₴ {this.initialPrice}";
        //}
    }

    internal int MortgageCell()
    {
        //this.IsMortgaged = true;

        //change visual

        return this.mortgageValue;
    }

    internal int UnMortgageCell()
    {
        //this.isMortgaged = false;

        //change visual

        return this.mortgageValue;
    }

    public void OnOwnerUpdated()
    {

    }
}
