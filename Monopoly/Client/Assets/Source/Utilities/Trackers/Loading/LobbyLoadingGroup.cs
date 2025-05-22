using Monopoly.Client.Runtime.UI.Managers.Lobby;
using Monopoly.Client.Utilities.Trackers.Loading;

namespace Monopoly.Client.Runtime.Core.Models
{
    internal sealed class LobbyLoadingGroup : LoadingGroup
    {
        internal LobbyLoadingGroup()
        {
            base.Objects.Add(typeof(UIManagerPrivateLobby), 1);
        }
    }
}
