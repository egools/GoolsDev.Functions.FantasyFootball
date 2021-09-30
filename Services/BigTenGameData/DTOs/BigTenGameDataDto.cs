using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenGameDataDto
    {
        public BigTenGameDataWeek Week { get; init; }
        public List<BigTenGameEventDto> Events { get; init; }
    }
}