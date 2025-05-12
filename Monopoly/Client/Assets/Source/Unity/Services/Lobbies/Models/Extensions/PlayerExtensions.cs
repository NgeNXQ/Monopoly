using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Monopoly.Unity.Services.Lobbies.Models.Extensions
{
    internal static class PlayerExtensions
    {
        internal static string GetData(this Player player, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            return player.Data[key].Value;
        }

        internal static void SetDataLocally(this Player player, string key, string value, PlayerDataObject.VisibilityOptions visibility)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Invalid value of the {nameof(value)}.");

            player.Data[key] = new PlayerDataObject(visibility, value);
        }

        internal static async Task SetDataRemotely(this Player player, string lobbyId, string key, string value, PlayerDataObject.VisibilityOptions visibility)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Invalid value of the {nameof(value)}.");

            if (string.IsNullOrEmpty(lobbyId))
                throw new ArgumentException($"Invalid value of the {nameof(lobbyId)}.");

            player.Data[key] = new PlayerDataObject(visibility, value);

            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = player.Data
            };

            await LobbyService.Instance.UpdatePlayerAsync(lobbyId, player.Id, updatePlayerOptions);
        }
    }
}