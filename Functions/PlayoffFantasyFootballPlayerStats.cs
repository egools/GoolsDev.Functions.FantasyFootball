using EspnDataService;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball
{
    public class PlayoffFantasyFootbalPlayerStats
    {
        private readonly IEspnService _espnService;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _gamesContainer;
        private readonly Container _statsContainer;
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootbalPlayerStats(
            IEspnService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _cosmosClient = cosmosClient;
            _gamesContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _statsContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-player-stats");
            _mapper = new PlayoffFantasyFootballMapper();
        }

        [Function("PlayoffFantasyFootbalPlayerStats")]
        public async Task Run([TimerTrigger("%PlayoffFantasyFootbalPlayerStatsTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger(nameof(PlayoffFantasyFootbalPlayerStats));

            context.BindingContext.BindingData.TryGetValue("context", out var dateParam);
            var dateToUse = DateTime.TryParse(dateParam?.ToString(), out var date) ? date : DateTime.Now;
            var (week, year) = NflDateHelper.GetPostSeasonWeek(dateToUse);

            if (week < 0 || year < 0)
            {
                logger.LogInformation("Not an active playoff week.");
                return;
            }

            logger.LogInformation("Fetching playoff games for {Year} - Week {Week}", year, week);
            var gameResult = await _espnService.GetNflPostSeasonGames(year, week);
            if (!gameResult.Success)
            {
                logger.LogError("Games service call failed with status code {Code}", gameResult.Error.HttpStatusCode);
            }
            await Task.Delay(1000);

            var weekId = $"{year}.3.{week}";
            QueryDefinition query = new QueryDefinition(
                "select * from games g where g.weekId = @WeekId")
                .WithParameter("@WeekId", weekId);

            List<NflGameDocument> games = new List<NflGameDocument>();
            using (FeedIterator<NflGameDocument> resultSet = _gamesContainer.GetItemQueryIterator<NflGameDocument>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(weekId),
                    MaxItemCount = 16
                }))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<NflGameDocument> response = await resultSet.ReadNextAsync();
                    NflGameDocument oldGame = response.First();

                    if ((oldGame.HadStatsPulled && oldGame.Status.Complete) || oldGame.Status.StartDate > DateTime.Now)
                    {
                        logger.LogInformation("Game stats for {Year} - Week {Week}: {Away} @ {Home} have already been pulled.", year, week, oldGame.AwayTeam.ShortName, oldGame.HomeTeam.ShortName);
                        continue;
                    }

                    var gameData = gameResult.Data.FirstOrDefault(g => g.GameId == oldGame.Id);
                    var newGame = _mapper.Map(gameData, gameData.Status.Complete);
                    var batch = _gamesContainer.CreateTransactionalBatch(new PartitionKey(newGame.WeekId));
                    batch.UpsertItem(newGame);

                    logger.LogInformation("Fetching playoff game stats for {Year} - Week {Week}: {Away} @ {Home}", year, week, newGame.AwayTeam.ShortName, newGame.HomeTeam.ShortName);
                    var result = await _espnService.GetNflGamePlayerStats(newGame.Id);
                    if (!result.Success)
                    {
                        logger.LogError("Stats service call failed with status code {Code}", result.Error.HttpStatusCode);
                    }
                    await Task.Delay(1000);

                    var stats = result.Data.Teams.SelectMany(t => t.Players.Select(p => _mapper.Map(p, $"{newGame.WeekId}.{p.PlayerId}", newGame.Year, t.TeamId, t.ShortName)));
                    foreach (var statline in stats)
                    {
                        await _statsContainer.UpsertItemAsync(statline, new PartitionKey(statline.Year));
                    }

                    await batch.ExecuteAsync();
                }
            }

        }
    }
}
