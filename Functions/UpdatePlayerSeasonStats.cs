using EspnDataService;
using GoolsDev.Functions.FantasyFootball.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Linq;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Functions
{
    public class UpdatePlayerSeasonStats(
        IEspnNflService espnService,
        ICosmosPlayerStats cosmosPlayerStats)
    {
        private readonly IEspnNflService _espnService = espnService;
        private readonly ICosmosPlayerStats _cosmosPlayerStats = cosmosPlayerStats;
        private readonly PlayoffFantasyFootballMapper _mapper = new();

        [Function("UpdatePlayerSeasonStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "UpdatePlayerSeasonStats/{year:int}")]
            HttpRequest req, int year, FunctionContext context)
        {
            var logger = context.GetLogger(nameof(UpdatePlayerSeasonStats));

            var players = await _cosmosPlayerStats.GetAllPlayers(year);
            foreach (var playerDoc in players)
            {
                if (playerDoc.Statlines.Any(s => s.Week == 99))
                {
                    continue;
                }

                var statsResult = await _espnService.GetPlayerSeasonStats(year, 2, playerDoc.PlayerId);
                if (statsResult.Success)
                {
                    var statline = _mapper.Map(statsResult.Data, 99, 2);
                    playerDoc.Statlines.Add(statline);
                    await _cosmosPlayerStats.PatchStatlines(playerDoc.Id, year, playerDoc.Statlines);
                }
                await Task.Delay(200);
            }

            return new JsonResult(players);
        }
    }
}
