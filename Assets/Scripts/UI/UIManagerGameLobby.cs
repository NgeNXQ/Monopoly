using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class UIManagerGameLobby : MonoBehaviour
{
    #region Host Controls

    [Space]
    [Header("Host Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasHost;

    [Space]
    [SerializeField] private Button buttonStartGame;

    [Space]
    [SerializeField] private Button buttonDisconnect;

    #endregion

    #region Client Controls

    [Space]
    [Header("Client Controls")]
    [Space]

    [Space]
    [SerializeField] private Canvas canvasClient;

    //[Space]
    //[SerializeField] private Button buttonStartGame;

    //[Space]
    //[SerializeField] private Button buttonDisconnect;

    #endregion

    #region Shared Visuals

    [Space]
    [Header("Shared Visuals")]
    [Space]

    [Space]
    [SerializeField] private TMP_Text labelJoinCode;

    #endregion

    public static UIManagerGameLobby LocalInstance { get; private set; }

    public PanelMessageBoxUI PanelMessageBox { get => PanelMessageBoxUI.Instance; }

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
    }

    private void OnEnable()
    {
        this.buttonDisconnect.onClick.AddListener(this.HandleButtonCancelClicked);
    }

    private void OnDisable()
    {
        this.buttonDisconnect.onClick.RemoveListener(this.HandleButtonCancelClicked);
    }

    private void Start()
    {
        this.labelJoinCode.text = GameLobbyManager.LocalInstance.JoinCode;
    }

    private void HandleButtonCancelClicked()
    {
        GameCoordinator.LocalInstance.LoadScene(GameCoordinator.Scene.MainMenu);
    }
}
