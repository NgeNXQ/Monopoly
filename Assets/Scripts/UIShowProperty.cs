using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;

public sealed class UIShowProperty : MonoBehaviour
{
    Player playerReference;

    MonopolyCell nodeReference;

    [SerializeField] GameObject propertyUIPanel;

    [SerializeField] TMP_Text propertyNameText;

    [SerializeField] Image colorField;

    [SerializeField] TMP_Text rentPriceText;

    [SerializeField] TMP_Text oneHouseRentText;

    [SerializeField] TMP_Text twoHouseRentText;

    [SerializeField] TMP_Text threeHouseRentText;

    [SerializeField] TMP_Text fourHouseRentText;

    [SerializeField] TMP_Text hotelRentText;

    [SerializeField] TMP_Text housePriceText;

    [SerializeField] TMP_Text hotelPriceText;

    [SerializeField] Button buyPropertyButton;

    [SerializeField] TMP_Text propertyPriceText;

    [SerializeField] TMP_Text playerMoneyText;

    private void OnEnable()
    {
        MonopolyCell.OnShowPropertyBuyPanel += ShowBuyPropertyUI;
    }

    private void OnDisable()
    {
        MonopolyCell.OnShowPropertyBuyPanel -= ShowBuyPropertyUI;
    }

    private void Start()
    {
        propertyUIPanel.SetActive(false);
    }

    void ShowBuyPropertyUI(MonopolyCell node, Player player)
    {
        this.playerReference = player;
        nodeReference = node;
        propertyNameText.text = node.name;
        //colorField = node.cellMonopolyType.color;

        // fill other;

        if (player.CanAfford(node.BaseRentPrice))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            buyPropertyButton.interactable = false;
        }

        propertyUIPanel.SetActive(true);
    }

    public void BuyPropertyButton()
    {
        playerReference.BuyProperty(nodeReference);
    }

    public void ClosePropertyButton()
    {
        propertyUIPanel.SetActive(false);
        nodeReference = null;
        playerReference = null;
    }
}
