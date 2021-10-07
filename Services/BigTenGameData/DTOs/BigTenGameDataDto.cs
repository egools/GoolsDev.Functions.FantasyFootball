using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenGameDataDto
    {
        public BigTenGameDataWeekDto Week { get; init; }
        public List<BigTenGameEventDto> Events { get; init; }
        public List<NcaaLeaguesDto> Leagues { get; init; }
    }
}