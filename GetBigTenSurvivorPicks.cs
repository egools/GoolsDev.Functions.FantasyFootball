using System;
using System.Linq;
using System.Threading.Tasks;
using GoolsDev.Functions.FantasyFootball.Services;
using GoolsDev.Functions.FantasyFootball.Services.BigTenGameData;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoolsDev.Functions.FantasyFootball
{
    public class GetBigTenSurvivorPicks
    {
        private readonly IGoogleSheetsService _sheetsService;
        private readonly IBigTenGameDataService _gameDataService;

        public GetBigTenSurvivorPicks(
            IGoogleSheetsService sheetsService,
            IBigTenGameDataService gameDataService)
        {
            _sheetsService = sheetsService;
            _gameDataService = gameDataService;
        }

        [Function("GetBigTenSurvivorPicks")]
        public async Task Run([TimerTrigger("%GetSurvivorDataTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger("GetBigTenSurvivorPicks");

            //var rows = await _sheetsService.GetRows("Form Responses 1", "A2", "F35");
            //logger.LogInformation(rows.First().First());
            var gameData = await _gameDataService.GetGameData("4");
        }
    }
}