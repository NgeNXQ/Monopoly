using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ChanceField : MonoBehaviour
{
    [SerializeField] List<ChanceCardSO> cards = new List<ChanceCardSO>();
    [SerializeField] TMP_Text descreption;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3;
    [SerializeField] float moveDelay = 0.5f;
    [SerializeField] Button buttonClose;

    List<ChanceCardSO> cardPool = new List<ChanceCardSO>();

    Player player;
    ChanceCardSO pickedCard;

    private void OnEnable()
    {
        MonopolyCell.OnDrawChanceCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyCell.OnDrawChanceCard -= DrawCard;
    }

    private void Start()
    {
        cardHolderBackground.SetActive(false);
        cardPool.AddRange(cards);
        ShuffleCards();
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count);
            ChanceCardSO tempCard = cardPool[index];
            cardPool[index] = cardPool[i];
            cardPool[i] = tempCard;
        }
    }

    private void DrawCard(Player player)
    {
        pickedCard = cardPool[0];
        this.player = player;
        cardHolderBackground.SetActive(true);
        descreption.text = pickedCard.Description;
        buttonClose.interactable = true;
    }

    public void ApplyCardEffect()
    {
        bool isMoving = false;

        if (pickedCard.Reward != 0)
        {
            player.CollectMoney(pickedCard.Reward);
        }
        else if (pickedCard.Penality != 0 && !pickedCard.payToPlayer)
        {
            player.PayTax(pickedCard.Penality);
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;

            int currentIndex = MonopolyBoard.instance.route.IndexOf(player.CurrentPosition);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;
            int stepsToMove = 0;

            if (currentIndex < pickedCard.moveToBoardIndex)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }

            MonopolyBoard.instance.MovePlayerToken(player, stepsToMove);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> players = GameManager.instance.players;

            foreach (Player player in players)
            {
                if (this.player != player)
                {
                    int amount = Mathf.Min(player.ReadMoney(), pickedCard.Penality);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }

            player.PayTax(totalCollected);
        }
        else if (pickedCard.StreetRepairs)
        {
            int[] allBuilding = player.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairHousePrice * allBuilding[0] + pickedCard.streetRepairHotelPrice * allBuilding[1];
            player.PayTax(totalCosts);
        }
        else if (pickedCard.GoToJail)
        {
            isMoving = false;
            player.GoToJail(MonopolyBoard.instance.route.IndexOf(player.CurrentPosition));
        }
        else if (pickedCard.JailFree)
        {

        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = Mathf.Abs(pickedCard.moveStepsBackwards);
            MonopolyBoard.instance.MovePlayerToken(player, - steps);
            isMoving = true;
        }
        else if (pickedCard.NextRailRoad)
        {
            //MonopolyBoard.instance.MovePlayerToken(MonopolyCell.MonopolyCellType.Transport, player);
            isMoving = true;
        }
        else if (pickedCard.NextUtilityNode)
        {
            //MonopolyBoard.instance.MovePlayerToken(MonopolyCell.MonopolyCellType., player);
            isMoving = true;
        }









            ContinueGame(isMoving);
    }

    public void ContinueGame(bool isMoving)
    {
        if (!isMoving && GameManager.instance.RolledADounle)
        {
            GameManager.instance.RollDice();
        }
        else if (!isMoving && GameManager.instance.RolledADounle)
        {
            GameManager.instance.SwitchPlayer();
        }
    }
}
