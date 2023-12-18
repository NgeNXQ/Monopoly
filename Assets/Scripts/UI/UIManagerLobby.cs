using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonDisconnectClicked);

        //LobbyManager.LocalInstance.ClientConnected += this.ShowPlayerControls;
        //LobbyManager.LocalInstance.ClientConnected += this.UpdatePlayersList;
    }

    private void OnDisable()
    {
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonDisconnectClicked);

        //LobbyManager.LocalInstance.ClientConnected -= this.ShowPlayerControls;

        //LobbyManager.LocalInstance.ClientConnected -= this.UpdatePlayersList;
    }

    private void Start()
    {
        this.labelJoinCode.text = LobbyManager.LocalInstance.JoinCode;
    }

    public void UpdatePlayersList()
    {
        //string pla
        //this.panelPlayerLobby.PlayerNickname = LobbyManager.LocalInstance.LastConnectedPlayer.Data[LobbyManager.KEY_PLAYER_NICKNAME].Value;
        GameObject.Instantiate(this.panelPlayerLobby, this.canvaslPlayersList.transform);
    }

    private void HandleButtonDisconnectClicked()
    {
        this.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
        this.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
        this.PanelMessageBox.MessageText = this.messageConfirmDisconnect;
        this.PanelMessageBox.Show();
    }

    private void ShowPlayerControls()
    {
        if (LobbyManager.LocalInstance.LastConnectedPlayer.Id == LobbyManager.LocalInstance.CurrentLobby.HostId)
        {
            this.canvasHost.gameObject.SetActive(true);
        }
        else
        {
            this.canvasClient.gameObject.SetActive(true);
        }
    }

}
