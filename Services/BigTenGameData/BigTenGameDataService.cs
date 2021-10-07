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

        public async Task<BigTenGameDataDto> GetGameData(int week)
        {
            var result = await _client.Request().SetQueryParams(new
            {
                groups = BigTenGroupNumber,
                week = week
            }).GetJsonAsync<BigTenGameDataDto>();

            return result;
        }

        public async Task<IEnumerable<BigTenCalendarEntryDto>> GetScheduleData()
        {
            var result = await _client.Request().SetQueryParams(new
            {
                groups = BigTenGroupNumber
            }).GetJsonAsync<BigTenGameDataDto>();
            return result
                .Leagues
                .First()
                .Calendar
                .First(c => c.Label == "Regular Season")
                .Entries;
        }
    }
}