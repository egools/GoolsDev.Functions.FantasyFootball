using Newtonsoft.Json;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public partial class GoogleSheetsService
    {
        public class GoogleErrorResult
        {
            [JsonProperty("error")]
            public GoogleError Error { get; init; }
        }
    }
}