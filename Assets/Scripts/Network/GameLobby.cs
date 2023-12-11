using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Runtime.InteropServices.WindowsRuntime;

public sealed class GameLobby : MonoBehaviour
{
    private Lobby hostedLobby;
    private float heartbeatTimer;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        CreateLobby();
    }

    private void Update()
    {
        HandleLobbyHearBeat();
    }

    private async void HandleLobbyHearBeat()
    {
        if (this.hostedLobby != null) 
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer < 0)
            {
                float heartBitTimerMax = 15.0f;
                heartbeatTimer = heartBitTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostedLobby.Id);
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 5;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsPrivate = true;
            createLobbyOptions.Player = GetPlayer();

            this.hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created a lobby!" + hostedLobby.Name + " " + hostedLobby.MaxPlayers + " " + hostedLobby.LobbyCode);
        }
        catch (LobbyServiceException lobbyServiceException)
        {
            Debug.Log(lobbyServiceException.Message);
        }
    }

    private void JoinLobby()
    {
        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()
        };

        //Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        Unity.Services.Lobbies.Models.Player player = new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "TestPlayer") }
            }
        };

        return player;
    }

    private async void LeaveLobby()
    {
        await LobbyService.Instance.RemovePlayerAsync(hostedLobby.Id, AuthenticationService.Instance.PlayerId);
    }

    private async void KickPlayer()
    {
        await LobbyService.Instance.RemovePlayerAsync(hostedLobby.Id, AuthenticationService.Instance.PlayerId);
    }
}
