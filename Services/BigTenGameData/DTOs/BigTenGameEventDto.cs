using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenGameEventDto
    {
        public string Name { get; init; }
        public string ShortName { get; init; }
        public List<BigTenCompetitionDto> Competitions { get; init; }
    }
}