using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public interface IGoogleSheetsService
    {
        Task<IEnumerable<IEnumerable<string>>> GetRows(string sheetName, string startCell, string endCell);
    }
}