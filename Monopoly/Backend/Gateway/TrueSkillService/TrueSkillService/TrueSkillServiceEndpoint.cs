using System;
using System.Linq;
// using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudSave.Model;
using Moserware.Skills;

namespace Monopoly.Gateway;

public sealed class TrueSkillServiceEndpoint
{
    // private readonly IGameApiClient gameApiClient;

    // public RatingServiceEndpoint(IGameApiClient gameApiClient)
    // {
    //     this.gameApiClient = this.gameApiClient ?? throw new ArgumentNullException(nameof(gameApiClient));
    // }

    [CloudCodeFunction("GetELO")]
    public int GetELO(IExecutionContext context)
    {
        try
        {
            var player1 = new Player(1);
            var player2 = new Player(2);

            var gameInfo = GameInfo.DefaultGameInfo;

            var team1 = new Team(player1, gameInfo.DefaultRating);
            var team2 = new Team(player2, gameInfo.DefaultRating);

            var teams = Teams.Concat(team1, team2);

            var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            return (int)newRatings[player1].Mean * 50;
        }
        catch (Exception ex)
        {
            return -1;
            // throw new Exception($"Failed to retrieve MMR for player {context.PlayerId}. Error: {ex.Message}");
        }
    }
}
