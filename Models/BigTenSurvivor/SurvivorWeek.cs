using System;
using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class SurvivorWeek
    {
        public SurvivorWeek(int week, DateTime startDate, DateTime endDate)
        {
            WeekNum = week;
            StartDate = startDate;
            EndDate = endDate;
            Games = new List<BigTenTeamGame>();
        }

        public int WeekNum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<BigTenTeamGame> Games { get; set; }
    }
}