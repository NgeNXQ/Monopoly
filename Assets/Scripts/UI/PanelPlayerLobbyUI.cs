using TMPro;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public sealed class PanelPlayerLobbyUI : MonoBehaviour
{
    #region Setup

    [Space]
    [Header("Setup")]

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

    private void HandleButtonKickPlayerClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (this.playerId == LobbyManager.Instance.LocalLobby.HostId)
            {
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Warning;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageCannotKickYourself;
                UIManagerGlobal.Instance.PanelMessageBox.Show(null);
            }
            else
            {
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Question;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OKCancel;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = $"{UIManagerLobby.Instance.MessageConfirmKickPlayer} {this.PlayerNickname}?";
                UIManagerGlobal.Instance.PanelMessageBox.Show(this.InvokeKickPlayerCallback);
            }
        }
        else
        {
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
            UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageCannotKickNotHost;
            UIManagerGlobal.Instance.PanelMessageBox.Show(null);
        }
    }

    private async void InvokeKickPlayerCallback()
    {
        if (UIManagerGlobal.Instance.PanelMessageBox.MessageBoxDialogResult == PanelMessageBoxUI.DialogResult.OK)
        {
            if (this == null || !(bool)LobbyManager.Instance?.LocalLobby.Players.Any(player => player.Id.Equals(this.playerId, System.StringComparison.Ordinal)))
            {
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxType = PanelMessageBoxUI.Type.OK;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxIcon = PanelMessageBoxUI.Icon.Error;
                UIManagerGlobal.Instance.PanelMessageBox.MessageBoxText = UIManagerLobby.Instance.MessageCannotKickPlayerAlreadyLeft;
                UIManagerGlobal.Instance.PanelMessageBox.Show(null);
            }
            else
            {
                await LobbyManager.Instance?.KickFromLobby(this.playerId);
            }
        }
    }
}
