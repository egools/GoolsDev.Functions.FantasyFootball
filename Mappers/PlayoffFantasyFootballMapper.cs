using EspnDataService;
using Riok.Mapperly.Abstractions;

namespace GoolsDev.Functions.FantasyFootball
{
    [Mapper]
    public partial class PlayoffFantasyFootballMapper
    {
        [MapProperty(nameof(NflGame.GameId), nameof(NflGameDocument.Id))]
        [MapPropertyFromSource(nameof(NflGameDocument.WeekId), Use = nameof(CreateWeekIdFromGame))]
        public partial NflGameDocument Map(NflGame nflGame, bool hadStatsPulled);
        public string CreateWeekIdFromGame(NflGame nflGame) => Helpers.CreateWeekId(nflGame.Year, (int)nflGame.SeasonType, nflGame.Week);

        [MapperIgnoreTarget(nameof(NflPlayerStatsDocument.JerseyNumber))]
        [MapperIgnoreTarget(nameof(NflPlayerStatsDocument.PositionId))]
        [MapperIgnoreTarget(nameof(NflPlayerStatsDocument.Position))]
        [MapperIgnoreTarget(nameof(NflPlayerStatsDocument.Statlines))]
        [MapperIgnoreSource(nameof(NflBoxscorePlayer.Statline))]
        public partial NflPlayerStatsDocument Map(NflBoxscorePlayer player, string id, int year, string teamId, string teamShortName);

        [MapperIgnoreTarget(nameof(NflPlayerStatsDocument.Statlines))]
        public partial NflPlayerStatsDocument Map(NflRosterPlayer player, string id, int year, string teamId, string teamShortName);

        [MapPropertyFromSource(nameof(NflPlayerStatline.FantasyPoints), Use = nameof(CalculateFantasyPoints))]
        public partial NflPlayerStatline Map(Statline stats, int week, int seasonType);

        public double CalculateFantasyPoints(Statline stats) => Helpers.CalculateFantasyPoints(stats);

    }
}
