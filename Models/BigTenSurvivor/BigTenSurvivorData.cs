using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class BigTenSurvivorData
    {
        public ICollection<SurvivorPicker> Pickers { get; set; }

        public ICollection<SurvivorWeek> Schedule { get; set; }
    }

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

    public class SurvivorPicker
    {
        public string Name { get; set; }
        public ICollection<string> AlternateNames { get; set; }
        public bool Eliminated { get; set; }
        public int? WeekEliminated { get; set; }
        public ICollection<SurvivorPick> Picks { get; set; }
    }

    public class SurvivorPick
    {
        public DateTime PickDateTime { get; set; }
        public string Name { get; set; }
        public int Week { get; set; }
        public string Team { get; set; }
        public bool? Correct { get; set; }
    }
}