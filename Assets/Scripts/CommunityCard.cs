using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class CommunityCard : MonoBehaviour
{
    //[SerializeField] List<CommunnityCardSO> cards = new List<CommunnityCardSO>();
    //[SerializeField] TMP_Text descreption;
    //[SerializeField] GameObject cardHolderBackground;
    //[SerializeField] float showTime = 3;
    //[SerializeField] float moveDelay = 0.5f;
    //[SerializeField] Button buttonClose;

    //List<CommunnityCardSO> cardPool = new List<CommunnityCardSO>();

    //Player player;
    //CommunnityCardSO pickedCard;

    //public delegate void ShowInputPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    //public static ShowInputPanel OnShowInputPanel;

    //private void OnEnable()
    //{
    //    MonopolyCell.OnDrawCommunityCard += DrawCard;
    //}

    //private void OnDisable()
    //{
    //    MonopolyCell.OnDrawCommunityCard -= DrawCard;
    //}

    //private void Start()
    //{
    //    cardHolderBackground.SetActive(false);
    //    cardPool.AddRange(cards);
    //    ShuffleCards();
    //}

    //void ShuffleCards()
    //{
    //    for (int i = 0; i < cardPool.Count; i++)
    //    {
    //        int index = Random.Range(0, cardPool.Count);
    //        CommunnityCardSO tempCard = cardPool[index];
    //        cardPool[index] = cardPool[i];
    //        cardPool[i] = tempCard;
    //    }
    //}

    //private void DrawCard(Player player)
    //{
    //    pickedCard = cardPool[0];
    //    this.player = player;
    //    cardHolderBackground.SetActive(true);
    //    descreption.text = pickedCard.Description;
    //    buttonClose.interactable = true;
    //}

    //public void ApplyCardEffect()
    //{
    //    bool isMoving = false;

    //    if (pickedCard.Reward != 0 && !pickedCard.CollectFromPlayer)
    //    {
    //        player.CollectMoney(pickedCard.Reward);
    //    }
    //    else if (pickedCard.Penality != 0)
    //    {
    //        player.PayTax(pickedCard.Penality);
    //    }
    //    else if (pickedCard.moveToBoardIndex != -1)
    //    {
    //        isMoving = true;

    //        int currentIndex = MonopolyBoard.instance.route.IndexOf(player.CurrentPosition);
    //        int lengthOfBoard = MonopolyBoard.instance.route.Count;
    //        int stepsToMove = 0;

    //        if (currentIndex < pickedCard.moveToBoardIndex)
    //        {
    //            stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
    //        }
    //        else if (currentIndex > pickedCard.moveToBoardIndex)
    //        {
    //            stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
    //        }

    //        MonopolyBoard.instance.MovePlayerToken(player, stepsToMove);
    //    }
    //    else if (pickedCard.CollectFromPlayer)
    //    {
    //        int totalCollected = 0;
    //        List<Player> players = GameManager.instance.players;

    //        foreach (Player player in players)
    //        {
    //            if (this.player != player)
    //            {
    //                int amount = Mathf.Min(player.ReadMoney(), pickedCard.Reward);
    //                player.PayTax(amount);
    //                totalCollected += amount;
    //            }
    //        }

    //        player.CollectMoney(totalCollected);
    //    }
    //    else if (pickedCard.StreetRepairs)
    //    {
    //        int[] allBuilding = player.CountHousesAndHotels();
    //        int totalCosts = pickedCard.streetRepairHousePrice * allBuilding[0] + pickedCard.streetRepairHotelPrice * allBuilding[1];
    //        player.PayTax(totalCosts);
    //    }
    //    else if (pickedCard.GoToJail)
    //    {
    //        isMoving = false;
    //        player.GoToJail(MonopolyBoard.instance.route.IndexOf(player.CurrentPosition));
    //    }
    //    else if (pickedCard.JailFree)
    //    {

    //    }

    //    ContinueGame(isMoving);
    //}

    //public void ContinueGame(bool isMoving)
    //{
    //    if (!isMoving && GameManager.instance.RolledADounle)
    //    {
    //        GameManager.instance.RollDice();
    //    }
    //    else if (!isMoving && GameManager.instance.RolledADounle)
    //    {
    //        GameManager.instance.SwitchPlayer();
    //    }

    //    if (!isMoving)
    //        OnShowInputPanel.Invoke(true, GameManager.instance.RolledADounle, !GameManager.instance.RolledADounle);
    //}
}
