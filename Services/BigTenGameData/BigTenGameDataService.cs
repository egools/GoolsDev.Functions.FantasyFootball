using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenGameDataService : IBigTenGameDataService
    {
        private readonly string BigTenGroupNumber = "5";
        private IFlurlClient _client;

        public BigTenGameDataService(IOptions<BigTenGameDataServiceSettings> options, IFlurlClientFactory factory)
        {
            _client = factory.Get($"{options.Value.BaseScoreboardUrl}");
        }

        public async Task<IEnumerable<BigTenTeamData>> GetGameData(string week)
        {
            var result = await _client.Request().SetQueryParams(new
            {
                groups = BigTenGroupNumber,
                week = week
            }).GetJsonAsync<BigTenGameDataDto>();

            var teamData = new List<BigTenTeamData>();
            foreach (var game in result.Events)
            {
                var homeComptetitor = game.Competitions.First().Competitors.FirstOrDefault(c => c.HomeAway == "home");
                var awayComptetitor = game.Competitions.First().Competitors.FirstOrDefault(c => c.HomeAway == "away");

                if (homeComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(new BigTenTeamData(
                        homeComptetitor,
                        awayComptetitor,
                        result.Week.Number));
                }
                if (awayComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(new BigTenTeamData(
                        awayComptetitor,
                        homeComptetitor,
                        result.Week.Number));
                }
            }

            return teamData;
        }
    }
}