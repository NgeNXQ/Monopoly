using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Newtonsoft.Json;

namespace Monopoly.Backend.Gateway.Services.Account;

public sealed class AccountServiceEndpoint
{
    private const string KEY_PLAYER_NICKNAME = "nickname";
    private const string KEY_PLAYER_TROPHIES = "trophies";

    private readonly IGameApiClient _gameApiClient;

    public AccountServiceEndpoint(IGameApiClient gameApiClient)
    {
        _gameApiClient = gameApiClient ?? throw new ArgumentNullException(nameof(gameApiClient));
    }

    private string BuildResponse(string status, HttpStatusCode code, string message, object? data = null, object[]? errors = null)
    {
        var response = new
        {
            status,
            code = (int)code, // Cast to int to store the HTTP status code numerically
            message,
            data = data ?? new { },
            errors = errors ?? Array.Empty<object>()
        };

        return JsonConvert.SerializeObject(response);
    }

    [CloudCodeFunction("PostAccount")]
    public async Task<string> PostAccount(IExecutionContext context, string nickname, string trophies)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");
        if (string.IsNullOrEmpty(nickname))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Nickname cannot be null or empty.");
        if (string.IsNullOrEmpty(trophies))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Trophies cannot be null or empty.");

        try
        {
            List<SetItemBody> data = new List<SetItemBody>
            {
                new SetItemBody(KEY_PLAYER_NICKNAME, nickname),
                new SetItemBody(KEY_PLAYER_TROPHIES, trophies)
            };

            await _gameApiClient.CloudSaveData.SetItemBatchAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new SetItemBatchBody(data)
            );

            Dictionary<string, string> accountData = new Dictionary<string, string>
            {
                { KEY_PLAYER_NICKNAME, nickname },
                { KEY_PLAYER_TROPHIES, trophies }
            };

            return this.BuildResponse("success", HttpStatusCode.Created, "Account created successfully", accountData);
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }

    [CloudCodeFunction("GetAccount")]
    public async Task<string> GetAccount(IExecutionContext context)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");

        try
        {
            ApiResponse<GetItemsResponse> result = await _gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new List<string> { KEY_PLAYER_NICKNAME, KEY_PLAYER_TROPHIES }
            );

            List<Item> items = result.Data.Results;

            if (items == null || items.Count == 0)
                return this.BuildResponse("error", HttpStatusCode.NotFound, "No account data found.");

            Dictionary<string, string> accountData = new Dictionary<string, string>
            {
                { KEY_PLAYER_TROPHIES, items.Find(i => i.Key == KEY_PLAYER_TROPHIES)?.Value?.ToString() ?? "0" },
                { KEY_PLAYER_NICKNAME, items.Find(i => i.Key == KEY_PLAYER_NICKNAME)?.Value?.ToString() ?? "<blank>" },
            };

            return this.BuildResponse("success", HttpStatusCode.OK, "Account retrieved", accountData);
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }

    [CloudCodeFunction("GetAccountNickname")]
    public async Task<string> GetAccountNickname(IExecutionContext context)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");

        try
        {
            ApiResponse<GetItemsResponse> result = await _gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new List<string> { KEY_PLAYER_NICKNAME }
            );

            string? nickname = result.Data.Results.Find(i => i.Key == KEY_PLAYER_NICKNAME)?.Value?.ToString();

            if (nickname == null)
                return this.BuildResponse("error", HttpStatusCode.NotFound, "Nickname not found.");

            return this.BuildResponse("success", HttpStatusCode.OK, "Nickname retrieved", new { nickname });
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }

    [CloudCodeFunction("GetAccountTrophies")]
    public async Task<string> GetAccountTrophies(IExecutionContext context)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");

        try
        {
            ApiResponse<GetItemsResponse> result = await _gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new List<string> { KEY_PLAYER_TROPHIES }
            );

            string? trophies = result.Data.Results.Find(i => i.Key == KEY_PLAYER_TROPHIES)?.Value?.ToString();

            if (trophies == null)
                return this.BuildResponse("error", HttpStatusCode.NotFound, "Trophies not found.");

            return this.BuildResponse("success", HttpStatusCode.OK, "Trophies retrieved", new { trophies });
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }

    [CloudCodeFunction("PutAccountNickname")]
    public async Task<string> PutAccountNickname(IExecutionContext context, string nickname)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");
        if (string.IsNullOrEmpty(nickname))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Nickname cannot be null or empty.");

        try
        {
            await _gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new SetItemBody(KEY_PLAYER_NICKNAME, nickname)
            );

            return this.BuildResponse("success", HttpStatusCode.OK, "Nickname updated", new { nickname });
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }

    [CloudCodeFunction("PutAccountTrophies")]
    public async Task<string> PutAccountTrophies(IExecutionContext context, string trophies)
    {
        if (string.IsNullOrEmpty(context.PlayerId))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Player ID is required.");
        if (string.IsNullOrEmpty(trophies))
            return this.BuildResponse("error", HttpStatusCode.BadRequest, "Trophies cannot be null or empty.");

        try
        {
            await _gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId,
                new SetItemBody(KEY_PLAYER_TROPHIES, trophies)
            );

            return this.BuildResponse("success", HttpStatusCode.OK, "Trophies updated", new { trophies });
        }
        catch (ApiException exception)
        {
            return this.BuildResponse("error", HttpStatusCode.BadRequest, exception.Message);
        }
    }
}
