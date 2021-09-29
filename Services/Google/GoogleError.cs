using Newtonsoft.Json;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public partial class GoogleSheetsService
    {
        public class GoogleError
        {
            [JsonProperty("code")]
            public int Code { get; init; }

            [JsonProperty("message")]
            public string Message { get; init; }

            [JsonProperty("status")]
            public string Status { get; init; }
        }
    }
}