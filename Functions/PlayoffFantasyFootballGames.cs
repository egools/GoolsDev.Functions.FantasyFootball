using EspnDataService;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball
{
    public class PlayoffFantasyFootballGames
    {
        private readonly IEspnService _espnService;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _gamesContainer;
        private MapperlyMapper _mapper;

        public PlayoffFantasyFootballGames(
            IEspnService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _cosmosClient = cosmosClient;
            _gamesContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _mapper = new MapperlyMapper();
        }

        [Function(nameof(PlayoffFantasyFootballGames))]
        public async Task Run([TimerTrigger("%PlayoffFantasyFootbalGamesTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger(nameof(PlayoffFantasyFootballGames));
            logger.LogInformation($"Playoff Fantasy Football Games Trigger executed at: {DateTime.Now}");

            var result = await _espnService.GetNflPostSeasonGames(2023, 3);
            if (result.Success)
            {
                foreach (var game in result.Data)
                {
                    var gameDoc = _mapper.Map(game);
                    await _gamesContainer.UpsertItemAsync(gameDoc, new PartitionKey(gameDoc.WeekId));
                }
            }
        }
    }
}
