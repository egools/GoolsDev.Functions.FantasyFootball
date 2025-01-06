using EspnDataService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Functions
{
    public class UpdatePlayerSeasonStats
    {
        private readonly IEspnNflService _espnService;
        private readonly Container _statsContainer;
        private PlayoffFantasyFootballMapper _mapper;

        public UpdatePlayerSeasonStats(
            IEspnNflService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _statsContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-player-stats");
            _mapper = new PlayoffFantasyFootballMapper();
        }

        [Function("UpdatePlayerSeasonStats")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "UpdatePlayerSeasonStats/{year:int}")]
            HttpRequest req, int year, FunctionContext context)
        {
            var logger = context.GetLogger(nameof(UpdatePlayerSeasonStats));

            var query = new QueryDefinition("select * from games g where g.year = @Year")
                .WithParameter("@Year", year);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(year),
                MaxItemCount = 16
            };

            using var resultSet = _statsContainer.GetItemQueryIterator<NflPlayerStatsDocument>(query, requestOptions: requestOptions);
            var players = new List<NflPlayerStatsDocument>();
            while (resultSet.HasMoreResults)
            {
                FeedResponse<NflPlayerStatsDocument> response = await resultSet.ReadNextAsync();
                players.AddRange(response.ToList());
            }

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
                    playerDoc.Statlines.Clear();
                    playerDoc.Statlines.Add(statline);
                    await _statsContainer.PatchItemAsync<NflPlayerStatsDocument>(
                        playerDoc.Id,
                        new PartitionKey(year),
                        patchOperations: [PatchOperation.Set($"/statlines", playerDoc.Statlines)]);
                }
                await Task.Delay(200);
            }

            return new JsonResult(players);
        }
    }
}
