using System;
using EspnDataService;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoolsDev.Functions.FantasyFootball
{
    public class PlayoffFantasyFootbalScores
    {
        private readonly IEspnService _espnService; 
        private readonly CosmosClient _cosmosClient;
        private readonly Container _gamesContainer;
        private readonly Container _scoresContainer;
        private MapperlyMapper _mapper;

        public PlayoffFantasyFootbalScores(
            IEspnService espnService,
            CosmosClient cosmosClient)
        {
            _espnService = espnService;
            _cosmosClient = cosmosClient;
            _gamesContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
            _scoresContainer = _cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "player-scores");
            _mapper = new MapperlyMapper();
        }

        [Function("PlayoffFantasyFootbalScores")]
        public void Run([TimerTrigger("%PlayoffFantasyFootbalScoresTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger(nameof(PlayoffFantasyFootballGames));
        }
    }
}
