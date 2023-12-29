using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public sealed class PanelPlayerLobbyUI : MonoBehaviour
{
    #region Setup

    #region Visuals

    [Header("Visuals")]

    [Space]
    [SerializeField] private Button buttonKickPlayer;

    #endregion

    #region Controls

    [Header("Controls")]

    [Space]
    [SerializeField] private TMP_Text textLabelPlayerNickname;

    #endregion

    #endregion

    private string playerId;

    public string PlayerNickname 
    {
        get => this.textLabelPlayerNickname.text;
        set => this.textLabelPlayerNickname.text = value;
    }

    private void Start()
    {
        this.playerId = this.gameObject.name;
    }

    private void OnEnable()
    {
        this.buttonKickPlayer.onClick.AddListener(this.HandleButtonKickPlayerClicked);
    }

    private void OnDisable()
    {
        this.buttonKickPlayer.onClick.RemoveListener(this.HandleButtonKickPlayerClicked);
    }

    private async void CallbackKickPlayer()
    {
        if (UIManagerGlobal.Instance.LastMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            if (this == null || !(bool)LobbyManager.Instance?.LocalLobby?.Players.Any(player => player.Id.Equals(this.playerId, System.StringComparison.Ordinal)))
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageCannotKickPlayerAlreadyLeft, PanelMessageBoxUI.Icon.Error);
            }
            else
            {
                await LobbyManager.Instance?.KickFromLobbyAsync(this.playerId);
            }
        }
    }

    private void HandleButtonKickPlayerClicked()
    {
        if (LobbyManager.Instance.IsHost)
        {
            if (this.playerId == LobbyManager.Instance.LocalLobby.HostId)
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageCannotKickYourself, PanelMessageBoxUI.Icon.Warning);
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OKCancel, $"{UIManagerGameLobby.Instance.MessageConfirmKickPlayer} {this.PlayerNickname}?", PanelMessageBoxUI.Icon.Question, this.CallbackKickPlayer);
            }
        }
        else
        {
            UIManagerGlobal.Instance.ShowMessageBox(PanelMessageBoxUI.Type.OK, UIManagerGameLobby.Instance.MessageCannotKickNotHost, PanelMessageBoxUI.Icon.Error);
        }
    }
}
