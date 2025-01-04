using EspnDataService;
using Riok.Mapperly.Abstractions;
using System;

namespace GoolsDev.Functions.FantasyFootball
{
    [Mapper]
    public partial class PlayoffFantasyFootballMapper
    {
        [MapProperty(nameof(NflGame.GameId), nameof(NflGameDocument.Id))]
        [MapPropertyFromSource(nameof(NflGameDocument.WeekId), Use = nameof(MapWeekId))]
        public partial NflGameDocument Map(NflGame nflGame, bool hadStatsPulled);
        private static string MapWeekId(NflGame nflGame) => $"{nflGame.Year}.{(int)nflGame.SeasonType}.{nflGame.Week}";

        public partial NflPlayerStatsDocument Map(NflBoxscorePlayer player, string id, int year, string teamId, string teamShortName, double fantasyPoints);
    }
}
