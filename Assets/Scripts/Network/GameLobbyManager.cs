using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using System.Collections;

#if UNITY_EDITOR
using ParrelSync;
#endif

internal sealed class GameLobbyManager : MonoBehaviour
{
    private const string LOCAL_LOBBY_NAME = "LOCAL_LOBBY_NAME";

    private Lobby lobby;

    private string joinCode;

    public static GameLobbyManager LocalInstance { get; private set; }

    public string JoinCode { get => this.joinCode; }

    public Action HostDisconnected;

    public Action ClientConnected;

    public Action ClientDisconnected;

    private void Awake()
    {
        if (LocalInstance != null)
            throw new System.InvalidOperationException($"Singleton {this.GetType().FullName} has already been initialized.");

        LocalInstance = this;
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
    }

    public void InitializeGameLobby(string joinCode)
    {
        this.joinCode = joinCode;
        UIManagerGameLobby.LocalInstance.LabelJoinCode = this.JoinCode;
        GameCoordinator.LocalInstance.LoadScene(GameCoordinator.Scene.GameLobby);
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(waitTimeSeconds);

        while (true) 
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return waitForSeconds;
        }
    }

    private void StartGame()
    {
        GameCoordinator.LocalInstance.LoadSceneNetwork(GameCoordinator.Scene.MonopolyGame);
    }

    private async void Kick()
    {
        await LobbyService.Instance.RemovePlayerAsync(this.lobby.Id, AuthenticationService.Instance.PlayerId);
    }

    private async void Disconnect()
    {
        await LobbyService.Instance.RemovePlayerAsync(this.lobby.Id, AuthenticationService.Instance.PlayerId);
    }

    //private async void Update()
    //{
    //    //HandleLobbyHearBeat();
    //}

    //private async void HandleLobbyHearBeat()
    //{
    //    if (this.lobby != null) 
    //    {
    //        heartbeatTimer -= Time.deltaTime;

    //        if (heartbeatTimer < 0)
    //        {
    //            float heartBitTimerMax = 15.0f;
    //            heartbeatTimer = heartBitTimerMax;
    //            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
    //        }
    //    }
    //}

    //public void CreateLobby() => this.CreateLobbyAsync();

    //private static async Task Aunteficate()
    //{
    //    await UnityServices.InitializeAsync();
    //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //}

    //private async void _CreateLobby()
    //{

    //}

    //    private async void _JoinLobby()
    //    {
    //        //joinInput.text
    //        //JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(this.sessionCode);

    //        //this.transportLayer.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);
    //        //NetworkManager.Singleton.StartHost();

    //        if (UnityServices.State == ServicesInitializationState.Uninitialized)
    //        {
    //            var options = new InitializationOptions();


    //#if UNITY_EDITOR
    //            // Remove this if you don't have ParrelSync installed. 
    //            // It's used to differentiate the clients, otherwise lobby will count them as the same
    //            if (ClonesManager.IsClone()) options.SetProfile(ClonesManager.GetArgument());
    //            else options.SetProfile("Primary");
    //#endif

    //            await UnityServices.InitializeAsync(options);
    //        }

    //        if (!AuthenticationService.Instance.IsSignedIn)
    //        {
    //            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //            PlayerId = AuthenticationService.Instance.PlayerId;
    //        }
    //    }

    //    private async void CreateLobbyAsync()
    //    {

    //    }

    //    private void JoinLobby()
    //    {
    //        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
    //        {
    //            Player = GetPlayer()
    //        };

    //        //Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
    //    }

    //    private Unity.Services.Lobbies.Models.Player GetPlayer()
    //    {
    //        Unity.Services.Lobbies.Models.Player player = new Unity.Services.Lobbies.Models.Player
    //        {
    //            Data = new Dictionary<string, PlayerDataObject>
    //            {
    //                { "Nickname", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "TestPlayer") }
    //            }
    //        };

    //        return player;
    //    }
}
