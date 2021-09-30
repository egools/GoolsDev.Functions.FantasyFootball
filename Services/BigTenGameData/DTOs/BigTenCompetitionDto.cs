using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenCompetitionDto
    {
        public string Id { get; init; }
        public string Date { get; init; }

        public List<BigTenCompetitorDto> Competitors { get; init; }
    }
}