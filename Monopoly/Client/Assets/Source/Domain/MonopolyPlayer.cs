// using System;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Unity.Services.Lobbies.Models;
// using Unity.Services.CloudSave;
// using Unity.Services.CloudSave.Models;

// namespace Monopoly.Client.Runtime.Core.Models
// {
//     internal sealed class MonopolyPlayer
//     {
//         private const string KEY_PLAYER_NICKNAME = "nickname";
//         private const string KEY_PLAYER_TROPHIES = "trophies";

//         internal MonopolyPlayer(Player player)
//         {
//             this.InternalPlayer = player ?? throw new ArgumentNullException(nameof(player));
//         }

//         internal Player InternalPlayer { get; private set; }

//         internal async Task<string> GetNickname()
//         {
//             Dictionary<string, Item> data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { KEY_PLAYER_NICKNAME });
//             return data.ContainsKey(KEY_PLAYER_NICKNAME) ? data[KEY_PLAYER_NICKNAME].Value.ToString() : "<blank>";
//         }

//         internal async Task<string> GetTrophies()
//         {
//             Dictionary<string, Item> data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { KEY_PLAYER_NICKNAME });
//             return data.ContainsKey(KEY_PLAYER_TROPHIES) ? data[KEY_PLAYER_TROPHIES].Value.ToString() : "0";
//         }



//         // internal async Task SavePlayerData(string nickname, string trophies)
//         // {
//         //     if (string.IsNullOrEmpty(nickname)) throw new ArgumentException("Nickname cannot be null or empty.", nameof(nickname));
//         //     if (string.IsNullOrEmpty(trophies)) throw new ArgumentException("Trophies cannot be null or empty.", nameof(trophies));

//         //     var data = new Dictionary<string, object>
//         //     {
//         //         { KEY_PLAYER_NICKNAME, nickname },
//         //         { KEY_PLAYER_TROPHIES, trophies }
//         //     };

//         //     try
//         //     {
//         //         await CloudSaveService.Instance.Data.Player.SaveAsync(data);
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         UnityEngine.Debug.LogError($"Failed to save player data: {ex.Message}");
//         //         throw;
//         //     }
//         // }
//     }
// }