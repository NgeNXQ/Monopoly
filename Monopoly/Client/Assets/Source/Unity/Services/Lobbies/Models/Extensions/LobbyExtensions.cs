using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Monopoly.Unity.Services.Lobbies.Models.Extensions
{
    internal static class LobbyExtensions
    {
        internal static bool HasPlayerWithId(this Lobby lobby, string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException($"Invalid value of the {nameof(playerId)}.");

            return lobby.Players.Any(player => player.Id.Equals(playerId, StringComparison.Ordinal));
        }

        internal static bool ArePlayersSynchronized(this Lobby lobby, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Invalid value of the {nameof(value)}.");

            return lobby.Players.All(player => player.Data[key].Value.Equals(value, StringComparison.Ordinal));
        }

        internal static string GetData(this Lobby lobby, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            return lobby.Data[key].Value;
        }

        internal static async Task SetIsPrivate(this Lobby lobby, bool isPrivate)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                IsPrivate = isPrivate
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);
        }

        internal static async Task SetData(this Lobby lobby, string key, string value, DataObject.VisibilityOptions visibility, DataObject.IndexOptions index = 0)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Invalid value of the {nameof(key)}.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"Invalid value of the {nameof(value)}.");

            lobby.Data[key] = new DataObject(visibility, value, index);

            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = lobby.Data
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);
        }
    }
}
