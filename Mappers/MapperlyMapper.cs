using EspnDataService;
using Riok.Mapperly.Abstractions;

namespace GoolsDev.Functions.FantasyFootball
{
    [Mapper]
    public partial class MapperlyMapper
    {
        [MapProperty(nameof(NflGame.GameId), nameof(NflGameDocument.Id))]
        [MapPropertyFromSource(nameof(NflGameDocument.WeekId), Use = nameof(MapWeekId))]
        public partial NflGameDocument Map(NflGame nflGame);

        private string MapWeekId(NflGame nflGame) => $"{nflGame.Year}.{(int)nflGame.SeasonType}.{nflGame.Week}";
    }
}
