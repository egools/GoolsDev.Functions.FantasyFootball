using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball
{
    public static class NflDateHelper
    {
        private const long wc2023Start = 638405388000000000;
        private const long wc2023End = 638410571400000000;
        private const long div2023Start = 638410572000000000;
        private const long div2023End = 638416619400000000;
        private const long conf2023Start = 638416620000000000;
        private const long conf2023End = 638422667400000000;
        private const long sb2023Start = 638428716000000000;
        private const long sb2023End = 638435627400000000;

        private const long wc2024Start = 638719020000000000;
        private const long wc2024End = 638726795400000000;
        private const long div2024Start = 638726796000000000;
        private const long div2024End = 638732843400000000;
        private const long conf2024Start = 638732844000000000;
        private const long conf2024End = 638738891400000000;
        private const long sb2024Start = 638744940000000000;
        private const long sb2024End = 638751851400000000;

        public static (int week, int year) GetPostSeasonWeek(DateTime date) => date.Ticks switch
        {
            >= wc2023Start and <= wc2023End => (1, 2023),
            >= div2023Start and <= div2023End => (2, 2023),
            >= conf2023Start and <= conf2023End => (3, 2023),
            >= sb2023Start and <= sb2023End => (5, 2023),
            >= wc2024Start and <= wc2024End => (1, 2024),
            >= div2024Start and <= div2024End => (2, 2024),
            >= conf2024Start and <= conf2024End => (3, 2024),
            >= sb2024Start and <= sb2024End => (5, 2024),
            _ => (-1, -1)
        };
    }
}
