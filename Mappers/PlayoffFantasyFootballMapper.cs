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

        private string MapWeekId(NflGame nflGame) => $"{nflGame.Year}.{(int)nflGame.SeasonType}.{nflGame.Week}";

        [MapPropertyFromSource(nameof(NflPlayerStatsDocument.FantasyPoints), Use = nameof(MapFantasyPoints))]
        public partial NflPlayerStatsDocument Map(NflBoxscorePlayer player, string id, int year, string teamId, string teamShortName);

        private double MapFantasyPoints(NflBoxscorePlayer player) => 
            Math.Round(
                (player.PassingYards * 0.04) +
                (player.PassingTouchdowns * 4) +
                (player.Interceptions * -2) +
                (player.RushingYards * 0.1) +
                (player.RushingTouchdowns * 6) +
                (player.Receptions * 0.5) +
                (player.ReceivingYards * 0.1) +
                (player.ReceivingTouchdowns * 6) +
                (player.FumblesLost * -2) +
                (player.ReturnTouchdowns * 6), 
                digits: 2);
    }
}
