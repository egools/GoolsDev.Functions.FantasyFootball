using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public interface IBigTenGameDataService
    {
        Task<BigTenGameDataDto> GetGameData(int week);

        Task<IEnumerable<BigTenCalendarEntryDto>> GetScheduleData();
    }
}