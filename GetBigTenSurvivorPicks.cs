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

            var survivorData = await _commitHandler.GetSurvivorDataFromRepo();
            if (survivorData.Schedule is null)
            {
                var scheduleData = await _gameDataService.GetScheduleData();
                survivorData.Schedule = GameDataMapper.MapSchedule(scheduleData);
            }

            var rows = await _sheetsService.GetRows("Form Responses 1", "A2", "O1000");
            var picks = PlayerPickMapper.MapRows(rows);

            if (survivorData.Pickers is null)
            {
                survivorData.Pickers = new List<SurvivorPicker>();
                foreach (var selection in picks.Where(p => p.Week == 1))
                {
                    survivorData.Pickers.Add(PlayerPickMapper.MapSelectionToNewPicker(selection));
                }
            }

            foreach (var week in survivorData.Schedule)
            {
                if (DateTime.UtcNow < week.EndDate && !week.Games.Any())
                {
                    var gamesDto = await _gameDataService.GetGameData(week.WeekNum);
                    var games = GameDataMapper.MapDto(gamesDto);
                    week.Games = games;

                    foreach (var selection in picks.Where(p => p.Week == week.WeekNum))
                    {
                        var picker = survivorData[selection.Name];
                        if (picker is null)
                        {
                            //TODO: handle unmapped picks
                        }
                        else
                        {
                            var selectedTeam = games.FirstOrDefault(team => team.Location == selection.Team);
                            if (!selectedTeam.Winner)
                            {
                                selection.Correct = false;
                                picker.Eliminated = true;
                                picker.WeekEliminated = week.WeekNum;
                            }
                            else
                            {
                                selection.Correct = true;
                            }
                            picker.Picks.Add(selection);
                        }
                    }
                }
            }

            //await _commitHandler.CommitSurvivorData(new BigTenSurvivorData(), string.Empty);
        }
    }
}