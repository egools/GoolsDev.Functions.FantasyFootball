using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public partial class GoogleSheetsService : IGoogleSheetsService
    {
        private string _apiKey;
        private IFlurlClient _client;

        public GoogleSheetsService(
            IOptions<GoogleSheetsServiceSettings> options,
            IFlurlClientFactory factory)
        {
            _apiKey = options.Value.ApiKey;
            _client = factory
                .Get($"{options.Value.BaseUrl}/{options.Value.SpreadsheetId}")
                .OnError(OnRequestFailure);
        }

        public async Task<IEnumerable<IEnumerable<string>>> GetRows(string sheetName, string startCell, string endCell)
        {
            var range = $"{sheetName}!{startCell}:{endCell}";
            var result = await _client
                .Request("values", range)
                .SetQueryParam("key", _apiKey)
                .GetJsonAsync<GoogleSheetsResult>();

            return result.Values;
        }

        private void OnRequestFailure(FlurlCall call)
        {
            var error = call.Response.GetJsonAsync<GoogleError>();
        }
    }
}