using EspnDataService;
using GoolsDev.Functions.FantasyFootball.Services;
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
        private readonly ICosmosGames _cosmosGames;
        private readonly ICosmosPlayerStats _cosmosPlayerStats;
        private readonly ICosmosFailedEspnCalls _failedCalls;
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootballGames(
            IEspnNflService espnService,
            ICosmosGames cosmosGames,
            ICosmosPlayerStats cosmosPlayerStats,
            ICosmosFailedEspnCalls failedCalls)
        {
            _espnService = espnService;
            _cosmosGames = cosmosGames;
            _cosmosPlayerStats = cosmosPlayerStats;
            _mapper = new PlayoffFantasyFootballMapper();
            _failedCalls = failedCalls;
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
                if (result.Error.ApiResponse != null)
                {
                    logger.LogError(result.Error.Exception, result.Error.Message);
                    await _failedCalls.LogFailedCall(new FailedEspnCallDocument(
                        Guid.NewGuid(),
                        $"{year}.3.{week}",
                        new { year, week },
                        result.Error.ApiResponse));
                }
                else
                {
                    logger.LogError("Games service call failed with status code {Code}", result.Error.HttpStatusCode);
                }
                return;
            }

            foreach (var game in result.Data)
            {
                var gameDoc = _mapper.Map(game, false);
                await _cosmosGames.UpsertGame(gameDoc);

                var existingPlayers = await _cosmosPlayerStats.GetGamePlayerDocs(gameDoc.HomeTeam.TeamId, gameDoc.AwayTeam.TeamId, year);
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

                await Task.WhenAll(missingPlayers.Select(p => _cosmosPlayerStats.UpsertPlayer(p)));
            }
        }
    }
}
