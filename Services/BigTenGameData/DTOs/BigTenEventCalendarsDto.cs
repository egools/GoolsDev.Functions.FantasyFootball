using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenEventCalendarsDto
    {
        public string Label { get; set; }
        public List<BigTenCalendarEntryDto> Entries { get; init; }
    }
}