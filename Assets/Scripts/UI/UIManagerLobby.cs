using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

internal sealed class UIManagerLobby : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]
    [Space]

    #region Host Controls

    [Space]
    [Header("Host Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasHost;

    [Space]
    [SerializeField] private Button buttonStartGame;

    #endregion

    #region Client Controls

    [Space]
    [Header("Client Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasClient;

    [Space]
    [SerializeField] private Button buttonReady;

    #endregion

    #region Shared Visuals

    [Space]
    [Header("Shared Visuals")]
    [Space]

    [Space]
    [SerializeField] private TMP_Text labelJoinCode;

    [Space]
    [SerializeField] private Canvas canvaslPlayersList;

    [Space]
    [SerializeField] private PanelPlayerLobbyUI panelPlayerLobby;

    #endregion

    #region Shared Controls

    [Space]
    [Header("Shared Controls")]
    [Space]

    [Space]
    [SerializeField] private Button buttonDisconnect;

    #endregion

    #region Messages

    [Space]
    [Header("Messages")]
    [Space]

    [Space]
    [SerializeField] private string messageLoadingGame;

    [Space]
    [SerializeField] private string messageDisconnecting;

    [Space]
    [SerializeField] private string messageConfirmStartGame;

    [Space]
    [SerializeField] private string messageHostDisconnected;

    [Space]
    [SerializeField] private string messageConfirmDisconnect;

    #endregion

    #endregion

    public static UIManagerLobby Instance { get; private set; }

    public string MessageHostDisconnected 
    { 
        get => this.messageHostDisconnected; 
    }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void Start()
    {
        this.labelJoinCode.text = LobbyManager.Instance.JoinCode;
    }

    private void OnEnable()
    {
        this.buttonStartGame.onClick.AddListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClicked);
    }

    private void OnDisable()
    {
        this.buttonStartGame.onClick.RemoveListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClicked);
    }

    public void ShowHostControls()
    {
        this.canvasHost.gameObject.SetActive(true);
    }

    public void ShowClientControls()
    {
        this.canvasClient.gameObject.SetActive(true);
    }

    public void AddPlayerToList(Player player)
    {
        PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
        newPanelPlayer.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
        newPanelPlayer.name = player.Id;
    }

    public void RemovePlayerFromList(int playerIndex)
    {
        GameObject.Destroy(this.canvaslPlayersList.transform.GetChild(playerIndex).gameObject);
    }

    public void InitializePlayersList(List<Player> players)
    {
        foreach (Player player in players)
        {
            this.panelPlayerLobby.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;

            PanelPlayerLobbyUI newPanelPlayer = GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
            newPanelPlayer.name = player.Id;
        }
    }

    private void HandleButtonStartGame()
    {
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        this.PanelMessageBox.MessageText = this.messageConfirmStartGame;
        this.PanelMessageBox.Show(this.InvokeButtonStartGameCallback);
    }

    private void InvokeButtonStartGameCallback()
    {
        if (this.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK) 
        {
            this.PanelMessageBox.MessageText = this.messageLoadingGame;
            this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            this.PanelMessageBox.Show(null);

            LobbyManager.Instance.StartGame();
        }
    }

    private void HandleButtonDisconnectClicked()
    {
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        this.PanelMessageBox.MessageText = this.messageConfirmDisconnect;
        this.PanelMessageBox.Show(this.InvokeButtonDisconnectCallback);
    }

    private async void InvokeButtonDisconnectCallback()
    {
        if (this.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            this.PanelMessageBox.MessageText = this.messageDisconnecting;
            this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.None;
            this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Loading;
            this.PanelMessageBox.Show(null);

            await LobbyManager.Instance.DisconnectLobby();
        }
    }
}
