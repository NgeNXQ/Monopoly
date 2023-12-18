using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

internal sealed class UIManagerLobby : MonoBehaviour
{
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
    [SerializeField] private string messageConfirmDisconnect;

    #endregion

    public static UIManagerLobby Instance { get; private set; }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (Instance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        Instance = this;
    }

    private void OnEnable()
    {
        this.buttonStartGame.onClick.AddListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClicked);

        PanelMessageBox.ButtonConfirmPanelOKCancelClicked += this.OKPanelMessageBoxClicked;
        PanelMessageBox.ButtonCancelPanelOKCancelClicked += this.CancelPanelMessageBoxClicked;

    }

    private void OnDisable()
    {
        this.buttonStartGame.onClick.RemoveListener(this.HandleButtonStartGame);
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClicked);

        PanelMessageBox.ButtonConfirmPanelOKCancelClicked -= this.OKPanelMessageBoxClicked;
        PanelMessageBox.ButtonCancelPanelOKCancelClicked -= this.CancelPanelMessageBoxClicked;
    }

    private void Start()
    {
        this.labelJoinCode.text = LobbyManager.LocalInstance.JoinCode;
    }

    public void ShowPlayerControls(Player player)
    {
        if (player.Id == LobbyManager.LocalInstance.CurrentLobby.HostId)
        {
            this.canvasHost.gameObject.SetActive(true);
        }
        else
        {
            this.canvasClient.gameObject.SetActive(true);
        }
    }

    private async void OKPanelMessageBoxClicked()
    {
        await LobbyManager.LocalInstance.DisconnectLobby();
    }

    private void HandleButtonStartGame()
    {
        LobbyManager.LocalInstance.StartGame();
    }

    private void CancelPanelMessageBoxClicked()
    {
        this.PanelMessageBox.Hide();
    }

    private void HandleButtonDisconnectClicked()
    {
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        this.PanelMessageBox.MessageText = this.messageConfirmDisconnect;
        this.PanelMessageBox.Show();
    }

    public void UpdatePlayersList(Player player)
    {
        this.panelPlayerLobby.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
        GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
    }

    public void FillPlayersList(List<Player> players)
    {
        foreach (Player player in players)
        {
            this.panelPlayerLobby.PlayerNickname = player.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
            GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
        }
    }
}
