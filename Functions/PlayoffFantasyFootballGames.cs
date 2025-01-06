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
    public class PlayoffFantasyFootballGames
    {
        private readonly IEspnNflService _espnService;
        private readonly Container _gamesContainer;
        private readonly Container _statsContainer;
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootballGames(
            IEspnNflService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _gamesContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _statsContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-player-stats");
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
            var result = await _espnService.GetNflPostSeasonGames(year, week);
            if (!result.Success)
            {
                logger.LogError("Games service call failed with status code {Code}", result.Error.HttpStatusCode);
                return;
            }

            foreach (var game in result.Data)
            {
                var gameDoc = _mapper.Map(game, false);
                await _gamesContainer.UpsertItemAsync(gameDoc, new PartitionKey(gameDoc.WeekId));

                var existingPlayers = await GetGamePlayerDocs(gameDoc.HomeTeam.TeamId, gameDoc.AwayTeam.TeamId, year);
                var missingPlayers = new List<NflPlayerStatsDocument>();
                async Task GetMissingPlayers(NflGameTeam team)
                {
                    if (!team.IsActive) return;

                    var rosterResult = await _espnService.GetNflRoster(team.TeamId);
                    var roster = rosterResult.Data;
                    foreach (var player in roster.Players)
                    {
                        if (!player.IsOffensivePlayer() || existingPlayers.Any(p => p.PlayerId == player.PlayerId)) continue;

                        missingPlayers.Add(_mapper.Map(
                            player: player,
                            id: Helpers.CreatePlayerId(player.PlayerId, roster.Team.TeamId, year),
                            year: year,
                            teamId: roster.Team.TeamId,
                            teamShortName: roster.Team.ShortName));
                    }
                }

                await Task.WhenAll([
                    GetMissingPlayers(gameDoc.HomeTeam),
                    GetMissingPlayers(gameDoc.AwayTeam)
                ]);

                await Task.WhenAll(missingPlayers.Select(p => _statsContainer.UpsertItemAsync(p)));
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
