using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class Player
{
    [SerializeField]
    private GameObject Token { get; }

    [SerializeField]
    public string Name;

    private int balance;
    private int turnsInJail;

    [HideInInspector]
    public MonopolyCell CurrentPosition { get; private set; }

    public bool IsInJail { get; }

    [SerializeField]
    private List<MonopolyCell> playerCells = new List<MonopolyCell>();

    // PLAYER INFO

    private PlayerInfo playerInfo;

    public Player(MonopolyCell cell, int balance, PlayerInfo playerInfo)
    {
        this.balance = balance;
        this.CurrentPosition = cell;
        this.playerInfo = playerInfo;

        this.playerInfo.SetPlayerName(this.Name);
        this.playerInfo.SetPlayerBalance(this.balance);
    }

    public void Initialize(MonopolyCell cell, int balance, PlayerInfo playerInfo)
    {
        this.balance = balance;
        this.CurrentPosition = cell;
        this.playerInfo = playerInfo;

        this.playerInfo.SetPlayerName(this.Name);
        this.playerInfo.SetPlayerBalance(this.balance);
    }
}
