using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIHandler : MonoBehaviour
{
    [SerializeField] GameObject inputPanel;
    [SerializeField] Button buttonRollDice;
    [SerializeField] Button buttonEndTurn;

    private void OnEnable()
    {
        GameManager.OnShowInputPanel += ShowPanel;
        MonopolyCell.OnShowInputPanel += ShowPanel;
        CommunityCard.OnShowInputPanel += ShowPanel;
        ChanceField.OnShowInputPanel += ShowPanel;
        Player.OnShowInputPanel += ShowPanel;
    }

    private void OnDisable()
    {
        GameManager.OnShowInputPanel -= ShowPanel;
        MonopolyCell.OnShowInputPanel -= ShowPanel;
        CommunityCard.OnShowInputPanel -= ShowPanel;
        ChanceField.OnShowInputPanel += ShowPanel;
        Player.OnShowInputPanel -= ShowPanel;
    }

    private void Start()
    {
        
    }

    void ShowPanel(bool showPanl, bool enableRollDice, bool enableEndTurn)
    {
        inputPanel.SetActive(showPanl);
        buttonRollDice.interactable = enableRollDice;
        buttonEndTurn.interactable = enableEndTurn;

        
    }


}
