using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public sealed class Player
{
    public string Name;
    private int balance;
    private int turnsInJail;

    public MonopolyCell CurrentPosition { get; }

    [SerializeField]
    private GameObject Token { get; }

    public bool IsInJail { get; }



    [SerializeField]
    private List<MonopolyCell> playerCells = new List<MonopolyCell>();

    // PLAYER INFO
}
