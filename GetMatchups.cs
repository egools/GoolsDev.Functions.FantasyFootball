using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using YahooFantasyService;

namespace GoolsDev.Functions.FantasyFootball
{
    public class GetMatchups
    {
        private IYahooService _yahoo;

        public GetMatchups(IYahooService yahooService)
        {
            _yahoo = yahooService;
        }

        [Function("GetMatchups")]
        public void Run([TimerTrigger("%GetMatchupsTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger("GetMatchups");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}