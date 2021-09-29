using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public class GoogleSheetsResult
    {
        [JsonProperty("range")]
        public string Range { get; init; }

        [JsonProperty("majorDimension")]
        public string MajorDimension { get; init; }

        [JsonProperty("values")]
        public IEnumerable<IEnumerable<string>> Values { get; init; }
    }
}