using EspnDataService;
using GoolsDev.Functions.FantasyFootball.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Functions
{
    public class UpdateCurrentPlayoffPlayers(
        IEspnNflService espnService,
        ICosmosPlayerStats cosmosPlayerStats)
    {
        private readonly IEspnNflService _espnService = espnService;
        private readonly ICosmosPlayerStats _cosmosPlayerStats = cosmosPlayerStats;
        private readonly PlayoffFantasyFootballMapper _mapper = new();

        [Function("UpdateCurrentPlayoffPlayers")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Admin, "post")]
            HttpRequest req,
            FunctionContext context)
        {
            var logger = context.GetLogger(nameof(UpdateCurrentPlayoffPlayers));
            var year = 2025;
            var existingPlayers = await _cosmosPlayerStats.GetAllPlayers(year);

            var result = await _espnService.GetCurrentNflStandings();
            if (!result.Success)
            {
                return new BadRequestObjectResult("ESPN call failed.");
            }

            var playoffTeams = result.Data.Where(t => !t.IsEliminated);

            _cosmosPlayerStats.CreateBatch(year);
            foreach (var team in playoffTeams)
            {
                var rosterResult = await _espnService.GetNflRoster(team.TeamId);
                var roster = rosterResult.Data;
                foreach (var player in roster.Players)
                {
                    if (!player.IsOffensivePlayer() || existingPlayers.Any(p => p.PlayerId == player.PlayerId)) continue;

                    var playerDoc = _mapper.Map(
                        player: player,
                        id: Helpers.CreatePlayerId(player.PlayerId, roster.Team.TeamId, year),
                        year: year,
                        teamId: roster.Team.TeamId,
                        teamShortName: roster.Team.ShortName);

                    var statsResult = await _espnService.GetPlayerSeasonStats(year, 2, playerDoc.PlayerId);
                    if (statsResult.Success)
                    {
                        var statline = _mapper.Map(statsResult.Data, 99, 2);
                        playerDoc.Statlines.Add(statline);
                    }

                    var upsertResult = await _cosmosPlayerStats.UpsertPlayer(playerDoc);
                    if (upsertResult.StatusCode == HttpStatusCode.Created)
                    {
                        logger.LogInformation("{year} {FirstName} {LastName} added.", year, playerDoc.FirstName, playerDoc.LastName);
                    }
                    else
                    {
                        logger.LogError("{year} {FirstName} {LastName} failed.", year, playerDoc.FirstName, playerDoc.LastName);
                    }

                    await Task.Delay(200);
                }
            }

            return new JsonResult(playoffTeams);
        }
    }
}
