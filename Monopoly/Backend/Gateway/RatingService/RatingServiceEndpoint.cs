using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudSave.Model;

namespace Monopoly.Gateway;

public sealed class RatingServiceEndpoint
{
    private readonly IGameApiClient gameApiClient;

    public RatingServiceEndpoint(IGameApiClient gameApiClient)
    {
        this.gameApiClient = this.gameApiClient ?? throw new ArgumentNullException(nameof(gameApiClient));
    }

    [CloudCodeFunction("GetMMR")]
    public async Task<int> GetMMR(IExecutionContext context)
    {
        try
        {
            var response = await this.gameApiClient.CloudSaveData.GetItemsAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new List<string> { "mmr" }
            );

            if (response.Data.Results.Count > 0 && response.Data.Results[0].Value != null)
            {
                return Convert.ToInt32(response.Data.Results[0].Value);
            }

            return 1500;
        }
        catch (Exception ex)
        {
            return -1;
            // throw new Exception($"Failed to retrieve MMR for player {context.PlayerId}. Error: {ex.Message}");
        }
    }

    [CloudCodeFunction("UpdateMMR")]
    public async Task<int> UpdateMMR(IExecutionContext context, bool didWin)
    {
        try
        {
            int currentMMR = await GetMMR(context);

            int mmrChange = didWin ? 25 : -25;
            int newMMR = Math.Max(0, currentMMR + mmrChange);

            await this.gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new SetItemBody
                {
                    Key = "mmr",
                    Value = newMMR
                }
            );

            return newMMR;
        }
        catch (Exception ex)
        {
            return -1;
            // throw new Exception($"Failed to update MMR for player {context.PlayerId}. Error: {ex.Message}");
        }
    }

    [CloudCodeFunction("ResetMMR")]
    public async Task<int> ResetMMR(IExecutionContext context)
    {
        try
        {
            const int defaultMMR = 1500;

            await this.gameApiClient.CloudSaveData.SetItemAsync(
                context,
                context.AccessToken,
                context.ProjectId,
                context.PlayerId!,
                new SetItemBody
                {
                    Key = "mmr",
                    Value = defaultMMR
                }
            );

            return defaultMMR;
        }
        catch (Exception ex)
        {
            return -1;
            // throw new Exception($"Failed to reset MMR for player {context.PlayerId}. Error: {ex.Message}");
        }
    }
}
