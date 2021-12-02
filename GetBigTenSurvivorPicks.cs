using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoolsDev.Functions.FantasyFootball.Mappers;
using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using GoolsDev.Functions.FantasyFootball.Services;
using GoolsDev.Functions.FantasyFootball.Services.BigTenGameData;
using GoolsDev.Functions.FantasyFootball.Services.GitHub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoolsDev.Functions.FantasyFootball
{
    public class GetBigTenSurvivorPicks
    {
        private readonly IGoogleSheetsService _sheetsService;
        private readonly IBigTenGameDataService _gameDataService;
        private readonly IGitHubCommitHandler _commitHandler;

        public GetBigTenSurvivorPicks(
            IGoogleSheetsService sheetsService,
            IBigTenGameDataService gameDataService,
            IGitHubCommitHandler gitHubCommitHandler)
        {
            _sheetsService = sheetsService;
            _gameDataService = gameDataService;
            _commitHandler = gitHubCommitHandler;
        }

        [Function("GetBigTenSurvivorPicks")]
        public async Task Run([TimerTrigger("%GetSurvivorDataTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger("GetBigTenSurvivorPicks");
            try
            {
                var changesMade = false;
                var survivorData = await _commitHandler.GetSurvivorDataFromRepo();

                if (survivorData.HasWinner)
                {
                    logger.LogInformation("Winner already found. No data to be checked.");
                    return;
                }

                if (survivorData.Schedule is null)
                {
                    var scheduleData = await _gameDataService.GetScheduleData();
                    survivorData.Schedule = GameDataMapper.MapSchedule(scheduleData, 2, 13);
                    changesMade = true;
                }

                var rows = await _sheetsService.GetRows("Form Responses 1", "A2", "O1000");
                var picks = PlayerPickMapper.MapRows(rows);

                if (survivorData.Pickers is null)
                {
                    survivorData.Pickers = new List<SurvivorPicker>();
                    var firstWeek = picks.Min(p => p.Week);
                    foreach (var selection in picks.Where(p => p.Week == firstWeek))
                    {
                        survivorData.Pickers.Add(PlayerPickMapper.MapSelectionToNewPicker(selection));
                    }
                    changesMade = true;
                }

                for (int i = survivorData.UnmappedSelections.Count - 1; i >= 0; i--)
                {
                    var selection = survivorData.UnmappedSelections.ElementAt(i);
                    var picker = survivorData[selection.Name];
                    if (picker is not null)
                    {
                        picker.AddPick(selection);
                        picker.CheckPicks(2, selection.Week);
                        survivorData.UnmappedSelections.Remove(selection);
                        changesMade = true;
                    }
                }

                foreach (var week in survivorData.Schedule)
                {
                    if (!week.Games.Any() || (DateTime.UtcNow > week.StartDate && week.Games.Any(g => !g.IsCompleted)))
                    {
                        var gamesDto = await _gameDataService.GetGameData(week.WeekNum);
                        var games = GameDataMapper.MapDto(gamesDto);
                        week.Games = games;
                        changesMade = true;
                        logger.LogInformation($"Game data updated for week {week.WeekNum}");
                    }
                    if (week.Games.All(g => g.IsCompleted) && !survivorData.AllWeekPicksCompleted(week.WeekNum))
                    {
                        foreach (var selection in picks.Where(p => p.Week == week.WeekNum))
                        {
                            var selectedTeam = week.Games.FirstOrDefault(team => team.Location == selection.Team);
                            selection.Correct = selectedTeam.Winner;
                            var picker = survivorData[selection.Name];
                            if (picker is null)
                            {
                                survivorData.UnmappedSelections.Add(selection);
                            }
                            else
                            {
                                picker.AddPick(selection);
                            }
                        }
                        foreach (var picker in survivorData.Pickers)
                        {
                            picker.CheckPicks(2, week.WeekNum);
                        }

                        if (survivorData.Pickers.All(p => p.Eliminated))
                        {
                            logger.LogInformation("Winner determined. All players eliminated.");
                            survivorData.HasWinner = true;
                            survivorData.WinnerName = "All players eliminated. Tie breaker needed.";
                        }
                        else if (survivorData.Pickers.Count(p => !p.Eliminated) == 1)
                        {
                            logger.LogInformation("Winner found.");
                            survivorData.HasWinner = true;
                            survivorData.WinnerName = survivorData.Pickers.First(p => !p.Eliminated).Name;
                        }

                        changesMade = true;
                        logger.LogInformation($"Mapped picks for week {week.WeekNum}.");
                    }
                }

                if (changesMade)
                {
                    await _commitHandler.CommitSurvivorData(survivorData);
                    logger.LogInformation($"Committed updated survivor data.");
                }
                else
                {
                    logger.LogInformation($"No new data to be committed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}