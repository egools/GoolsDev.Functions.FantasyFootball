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
                //TODO: create pickers
            }

            foreach (var week in survivorData.Schedule)
            {
                if (DateTime.UtcNow < week.EndDate)
                {
                    var gamesDto = await _gameDataService.GetGameData(1);
                    var games = GameDataMapper.MapDto(gamesDto);
                    survivorData[1].Games = games;
                    //TODO: map games into picks
                    //TODO: handle unmapped picks
                }
            }

            //await _commitHandler.CommitSurvivorData(new BigTenSurvivorData(), string.Empty);
        }
    }
}