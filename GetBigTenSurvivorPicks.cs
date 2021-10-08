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
                if (survivorData.Schedule is null)
                {
                    var scheduleData = await _gameDataService.GetScheduleData();
                    survivorData.Schedule = GameDataMapper.MapSchedule(scheduleData);
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
                    if (DateTime.UtcNow > week.EndDate && !week.Games.Any())
                    {
                        var gamesDto = await _gameDataService.GetGameData(week.WeekNum);
                        var games = GameDataMapper.MapDto(gamesDto);
                        week.Games = games;

                        foreach (var selection in picks.Where(p => p.Week == week.WeekNum))
                        {
                            var selectedTeam = games.FirstOrDefault(team => team.Location == selection.Team);
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
                        changesMade = true;
                        logger.LogInformation($"Finished mapping for week {week.WeekNum}");
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