using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.Core.P2P;
using Monopoly.Client.Runtime.UI.Managers.Lobby;
using Monopoly.Client.Runtime.UI.Managers.Global;
using Monopoly.Client.Runtime.UI.Views.Concrete.Global;
using Monopoly.Unity.Services.Lobbies.Models.Extensions;

namespace Monopoly.Client.Runtime.UI.Views.Concrete.Lobby
{
    internal sealed class PlayerUnrankedLobbyView : MonoBehaviour
    {
        [SerializeField]
        private Button buttonKickPlayer;

        [SerializeField]
        private TMP_Text textLabelPlayerNickname;

        private string playerId;

        internal string PlayerNickname
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
            if (LobbyManager.Instance.IsHost)
            {
                if (this.playerId == LobbyManager.Instance.LocalLobby.HostId)
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Warning,
                        UIManagerPrivateLobby.Instance.MessageCannotKickYourself
                    );
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OKCancel,
                        MessageBoxView.Icon.Question,
                        $"{UIManagerPrivateLobby.Instance.MessageConfirmKickPlayer} {this.PlayerNickname}",
                        this.CallbackKickPlayer
                    );
                }
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxView.Type.OK,
                    MessageBoxView.Icon.Error,
                    UIManagerPrivateLobby.Instance.MessageCannotKickNotHost
                );
            }
        }

        private async void CallbackKickPlayer()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxView.DialogResult.OK)
            {
                if (!LobbyManager.Instance.LocalLobby.HasPlayerWithId(this.playerId))
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxView.Type.OK,
                        MessageBoxView.Icon.Error,
                        UIManagerPrivateLobby.Instance.MessageCannotKickPlayerAlreadyLeft
                    );
                }
                else
                {
                    await LobbyManager.Instance?.KickFromLobbyAsync(this.playerId);
                }
            }
        }
    }
}
