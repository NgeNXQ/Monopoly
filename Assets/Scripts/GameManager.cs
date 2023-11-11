using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField]
    private MonopolyBoard monopolyBoard;

    [SerializeField]
    private List<Player> players = new List<Player>();

    [SerializeField]
    private int currentPlayer;

    [SerializeField]
    private int maxTurnsInJail = 3;

    [SerializeField]
    private int startBalance = 15000;

    [SerializeField]
    private int circleBonus = 2000;

    [SerializeField]
    private GameObject playerInfoPrefab;

    [SerializeField]
    private Transform playersPanel;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Add players to the game

        for (int i = 0; i < players.Count; ++i)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playersPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            players[i].Initialize(monopolyBoard.route[0], this.startBalance, info);
        }
    }
}
