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
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootballGames(
            IEspnService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _cosmosClient = cosmosClient;
            _gamesContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _mapper = new PlayoffFantasyFootballMapper();
        }

        [Function(nameof(PlayoffFantasyFootballGames))]
        public async Task Run([TimerTrigger("%PlayoffFantasyFootbalGamesTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger(nameof(PlayoffFantasyFootballGames));
            logger.LogInformation($"Playoff Fantasy Football Games Trigger executed at: {DateTime.Now}");

            context.BindingContext.BindingData.TryGetValue("context", out var dateParam);
            var dateToUse = DateTime.TryParse(dateParam?.ToString(), out var date) ? date : DateTime.Now;
            var (week, year) = NflDateHelper.GetPostSeasonWeek(dateToUse);

            if (week < 0 || year < 0)
            {
                logger.LogInformation("Not an active playoff week.");
                return;
            }

            logger.LogInformation("Fetching playoff games for {Year} - Week {Week}", year, week);
            var result = await _espnService.GetNflPostSeasonGames(week, year);
            if (!result.Success)
            {
                logger.LogError("Games service call failed with status code {Code}", result.Error.HttpStatusCode);
            }

            foreach (var game in result.Data)
            {
                var gameDoc = _mapper.Map(game, false);
                await _gamesContainer.UpsertItemAsync(gameDoc, new PartitionKey(gameDoc.WeekId));
            }
        }
    }
}
