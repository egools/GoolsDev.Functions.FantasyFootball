using EspnDataService;
using GoolsDev.Functions.FantasyFootball.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball
{
    public class PlayoffFantasyFootbalPlayerStats
    {
        private readonly IEspnNflService _espnService;
        private readonly ICosmosGames _cosmosGames;
        private readonly ICosmosPlayerStats _cosmosPlayerStats;
        private readonly ICosmosFailedEspnCalls _failedCalls;
        private PlayoffFantasyFootballMapper _mapper;

        public PlayoffFantasyFootbalPlayerStats(
            IEspnNflService espnService,
            ICosmosGames cosmosGames,
            ICosmosPlayerStats cosmosPlayerStats,
            ICosmosFailedEspnCalls failedCalls)
        {
            _espnService = espnService;
            _cosmosGames = cosmosGames;
            _cosmosPlayerStats = cosmosPlayerStats;
            _failedCalls = failedCalls;
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
                if (gameResult.Error.ApiResponse != null)
                {
                    logger.LogError(gameResult.Error.Exception, gameResult.Error.Message);
                    await _failedCalls.LogFailedCall(new FailedEspnCallDocument(
                        Guid.NewGuid(),
                        $"{year}.3.{week}",
                        new { year, week },
                        gameResult.Error.ApiResponse));
                }
                else
                {
                    logger.LogError("Games service call failed with status code {Code}", gameResult.Error.HttpStatusCode);
                }
                return;
            }

            var oldGames = await _cosmosGames.GetGames(year, 3, week);
            foreach (var oldGame in oldGames)
            {
                if (oldGame.HadStatsPulled && oldGame.Status.Complete)
                {
                    logger.LogInformation("Game stats for {Year} - Week {Week}: {Away} @ {Home} have already been pulled.", year, week, oldGame.AwayTeam.ShortName, oldGame.HomeTeam.ShortName);
                    continue;
                }
                else if (oldGame.Status.StartDate > DateTime.Now)
                {
                    logger.LogInformation("Game: {Year} - Week {Week}: {Away} @ {Home} has not yet started.", year, week, oldGame.AwayTeam.ShortName, oldGame.HomeTeam.ShortName);
                    continue;
                }

                var gameData = gameResult.Data.FirstOrDefault(g => g.GameId == oldGame.Id);
                var newGame = _mapper.Map(gameData, gameData.Status.Complete);

                logger.LogInformation("Fetching playoff game stats for {Year} - Week {Week}: {Away} @ {Home}", year, week, newGame.AwayTeam.ShortName, newGame.HomeTeam.ShortName);
                var result = await _espnService.GetNflGamePlayerStats(newGame.Id);
                if (!result.Success)
                {
                    if (result.Error.ApiResponse != null)
                    {
                        logger.LogError(result.Error.Exception, result.Error.Message);
                        await _failedCalls.LogFailedCall(new FailedEspnCallDocument(
                            Guid.NewGuid(),
                            newGame.Id,
                            new { year, week, gameId = newGame.Id },
                            result.Error.ApiResponse));
                    }
                    else
                    {
                        logger.LogError("Stats service call failed with status code {Code}", result.Error.HttpStatusCode);
                    }
                    continue;
                }
                await Task.Delay(500);

                _cosmosPlayerStats.CreateBatch(year);
                var existingPlayers = await _cosmosPlayerStats.GetGamePlayerDocs(newGame.HomeTeam.TeamId, newGame.AwayTeam.TeamId, newGame.Year);
                foreach (var team in result.Data.Teams)
                {
                    foreach (var player in team.Players)
                    {
                        var newStats = _mapper.Map(player.Statline, newGame.Week, (int)newGame.SeasonType);
                        var playerDoc = existingPlayers.FirstOrDefault(p => p.PlayerId == player.PlayerId);
                        if (playerDoc == null)
                        {
                            var playerResult = await _espnService.GetNflPlayer(player.PlayerId);
                            if (!playerResult.Success)
                            {
                                if (playerResult.Error.ApiResponse != null)
                                {
                                    logger.LogError(playerResult.Error.Exception, playerResult.Error.Message);
                                    await _failedCalls.LogFailedCall(new FailedEspnCallDocument(
                                        Guid.NewGuid(),
                                        newGame.Id,
                                        new { year, week, gameId = newGame.Id, playerId = player.PlayerId },
                                        playerResult.Error.ApiResponse));
                                }
                                else
                                {
                                    logger.LogError("Error fetching player info for {FirstName} {LastName} ID:{PlayerId}", player.FirstName, player.LastName, player.PlayerId);
                                }
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
                            playerDoc.Statlines.Add(newStats);
                            _cosmosPlayerStats.AddPlayerToBatch(playerDoc);
                        }
                        else
                        {
                            var oldStats = playerDoc.Statlines.FirstOrDefault(s => s.Week == week && s.SeasonType == (int)newGame.SeasonType);
                            if (oldStats is not null)
                            {
                                playerDoc.Statlines.Remove(oldStats);
                            }
                            playerDoc.Statlines.Add(newStats);
                            _cosmosPlayerStats.PatchStatlinesInBatch(playerDoc.Id, playerDoc.Statlines);
                        }
                    }
                }
                var batchSuccessful = await _cosmosPlayerStats.ExecuteBatch();
                if (batchSuccessful)
                {
                    await _cosmosGames.UpsertGame(newGame);
                }
                else
                {
                    logger.LogError("Player transaction batch for {Away} @ {Home} failed.", newGame.AwayTeam.ShortName, newGame.HomeTeam.ShortName);
                }
            }
        }
    }
}
