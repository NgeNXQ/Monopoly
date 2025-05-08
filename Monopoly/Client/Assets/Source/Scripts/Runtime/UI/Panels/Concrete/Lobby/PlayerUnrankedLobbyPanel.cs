using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Client.Runtime.P2P;
using Monopoly.Client.Runtime.UI.Managers;
using Monopoly.Client.Runtime.UI.Panels.Concrete.Global;

namespace Monopoly.Client.Runtime.UI.Panels.Concrete.Lobby
{
    internal sealed class PlayerUnrankedLobbyPanel : MonoBehaviour
    {
        [Header("Visuals")]

        [Space]
        [SerializeField] private Button buttonKickPlayer;

        [Header("Controls")]

        [Space]
        [SerializeField] private TMP_Text textLabelPlayerNickname;

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

        private async void CallbackKickPlayer()
        {
            if (UIManagerGlobal.Instance.TopMessageBox.PanelDialogResult == MessageBoxPanel.DialogResult.OK)
            {
                if (this == null || !(bool)LobbyManager.Instance?.LocalLobby?.Players.Any(player => player.Id.Equals(this.playerId, System.StringComparison.Ordinal)))
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Error,
                        UIManagerUnrankedLobby.Instance.MessageCannotKickPlayerAlreadyLeft
                    );
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
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OK,
                        MessageBoxPanel.Icon.Warning,
                        UIManagerUnrankedLobby.Instance.MessageCannotKickYourself
                    );
                }
                else
                {
                    UIManagerGlobal.Instance.ShowMessageBox(
                        MessageBoxPanel.Type.OKCancel,
                        MessageBoxPanel.Icon.Question,
                        $"{UIManagerUnrankedLobby.Instance.MessageConfirmKickPlayer} {this.PlayerNickname}?",
                        this.CallbackKickPlayer
                    );
                }
            }
            else
            {
                UIManagerGlobal.Instance.ShowMessageBox(
                    MessageBoxPanel.Type.OK,
                    MessageBoxPanel.Icon.Error,
                    UIManagerUnrankedLobby.Instance.MessageCannotKickNotHost
                );
            }
        }
    }
}
