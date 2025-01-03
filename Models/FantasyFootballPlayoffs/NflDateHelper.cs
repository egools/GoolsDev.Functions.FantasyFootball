using System;

namespace GoolsDev.Functions.FantasyFootball
{
    public static class NflDateHelper
    {
        private const long wc2023Start = 638404344000000000;    //2024-01-10T03:00Z
        private const long wc2023End = 638410391400000000;      //2024-01-17T02:59Z
        private const long div2023Start = 638410392000000000;   //2024-01-17T03:00Z
        private const long div2023End = 638416439400000000;     //2024-01-24T02:59Z
        private const long conf2023Start = 638416440000000000;  //2024-01-24T03:00Z
        private const long conf2023End = 638422487400000000;    //2024-01-31T02:59Z
        private const long sb2023Start = 638428536000000000;    //2024-02-07T03:00Z
        private const long sb2023End = 638434583400000000;      //2024-02-14T02:59Z

        private const long wc2024Start = 638718840000000000;    //2025-01-08T03:00Z
        private const long wc2024End = 638724887400000000;      //2025-01-15T02:59Z
        private const long div2024Start = 638724888000000000;   //2025-01-15T03:00Z
        private const long div2024End = 638730935400000000;     //2025-01-22T02:59Z
        private const long conf2024Start = 638730936000000000;  //2025-01-22T03:00Z
        private const long conf2024End = 638736983400000000;    //2025-01-29T02:59Z
        private const long sb2024Start = 638743032000000000;    //2025-02-05T03:00Z
        private const long sb2024End = 638749079400000000;      //2025-02-12T02:59Z

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