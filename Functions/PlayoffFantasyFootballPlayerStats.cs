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
        private readonly IEspnNflService _espnService;
        private readonly Container _gamesContainer;
        private readonly Container _statsContainer;
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootbalPlayerStats(
            IEspnNflService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _gamesContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _statsContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-player-stats");
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
                return;
            }

            var weekId = Helpers.CreateWeekId(year, 3, week);
            var query = new QueryDefinition(
                "select * from games g where g.weekId = @WeekId")
                .WithParameter("@WeekId", weekId);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(weekId),
                MaxItemCount = 16
            };

            using var resultSet = _gamesContainer.GetItemQueryIterator<NflGameDocument>(query, requestOptions: requestOptions);
            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                foreach (var oldGame in response)
                {
                    if ((oldGame.HadStatsPulled && oldGame.Status.Complete) || oldGame.Status.StartDate > DateTime.Now)
                    {
                        logger.LogInformation("Game stats for {Year} - Week {Week}: {Away} @ {Home} have already been pulled.", year, week, oldGame.AwayTeam.ShortName, oldGame.HomeTeam.ShortName);
                        continue;
                    }

                    var gameData = gameResult.Data.FirstOrDefault(g => g.GameId == oldGame.Id);
                    var newGame = _mapper.Map(gameData, gameData.Status.Complete);
                    var gameBatch = _gamesContainer.CreateTransactionalBatch(new PartitionKey(newGame.WeekId));
                    gameBatch.UpsertItem(newGame);

                    logger.LogInformation("Fetching playoff game stats for {Year} - Week {Week}: {Away} @ {Home}", year, week, newGame.AwayTeam.ShortName, newGame.HomeTeam.ShortName);
                    var result = await _espnService.GetNflGamePlayerStats(newGame.Id);
                    if (!result.Success)
                    {
                        logger.LogError("Stats service call failed with status code {Code}", result.Error.HttpStatusCode);
                        continue;
                    }
                    await Task.Delay(500);

                    var statsBatch = _statsContainer.CreateTransactionalBatch(new PartitionKey(year));
                    var existingPlayers = await GetGamePlayerDocs(newGame.HomeTeam.TeamId, newGame.AwayTeam.TeamId, newGame.Year);
                    foreach (var team in result.Data.Teams)
                    {
                        foreach (var player in team.Players)
                        {
                            var playerDoc = existingPlayers.FirstOrDefault(p => p.PlayerId == player.PlayerId);
                            var stats = _mapper.Map(player.Statline, newGame.Week, (int)newGame.SeasonType);
                            if (playerDoc == null)
                            {
                                var playerResult = await _espnService.GetNflPlayer(player.PlayerId);
                                if (!playerResult.Success)
                                {
                                    logger.LogError("Error fetching player info for {FirstName} {LastName} ID:{PlayerId}", player.FirstName, player.LastName, player.PlayerId);
                                }

                                playerDoc = _mapper.Map(
                                    player: player,
                                    id: Helpers.CreatePlayerId(player.PlayerId, team.TeamId, newGame.Year),
                                    year: newGame.Year,
                                    teamId: team.TeamId,
                                    teamShortName: team.ShortName);
                                playerDoc.JerseyNumber = playerResult.Data?.JerseyNumber;
                                playerDoc.PositionId = playerResult.Data?.PositionId;
                                playerDoc.Position = playerResult.Data?.Position;
                                playerDoc.Statlines.Add(stats);
                                statsBatch.UpsertItem(playerDoc);
                            }
                            else
                            {
                                statsBatch.PatchItem(
                                    playerDoc.Id,
                                    patchOperations: [PatchOperation.Add($"/statlines/-", stats)]
                                );
                            }
                        }
                    }
                    var batchResult = await statsBatch.ExecuteAsync();
                    if (batchResult.IsSuccessStatusCode)
                    {
                        await gameBatch.ExecuteAsync();
                    }
                }
            }
        }

        private async Task<List<NflPlayerStatsDocument>> GetGamePlayerDocs(string teamId1, string teamId2, int year)
        {
            var query = new QueryDefinition("select * from games g where g.year = @Year and (g.teamId = @TeamId1 OR g.teamId = @TeamId2)")
                .WithParameter("@Year", year)
                .WithParameter("@TeamId1", teamId1)
                .WithParameter("@TeamId2", teamId2);
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
            return players;
        }
    }
}
