using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class BigTenSurvivorData
    {
        public ICollection<SurvivorPicker> Pickers { get; set; }

        public ICollection<SurvivorWeek> Schedule { get; set; }

        public SurvivorPicker this[string name]
        {
            get => Pickers.FirstOrDefault(p => p.HasAlias(name));
        }

        public SurvivorWeek this[int weekNum]
        {
            get => Schedule.FirstOrDefault(w => w.WeekNum == weekNum);
        }
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
        private readonly Regex AliasRegex = new("[\\s']", RegexOptions.Compiled);
        public string Name { get; set; }
        public bool Eliminated { get; set; }
        public int? WeekEliminated { get; set; }
        public ICollection<SurvivorSelection> Picks { get; set; }
        private ICollection<string> AllNames { get; set; }

        public bool HasAlias(string candidate)
        {
            var name = AliasRegex.Replace(candidate.ToLower(), "");
            return AllNames.Contains(name);
        }

        public void AddAliasName(string alias)
        {
            AllNames.Add(AliasRegex.Replace(alias.ToLower(), ""));
        }
    }

    public class SurvivorSelection
    {
        public DateTime PickDateTime { get; set; }
        public string Name { get; set; }
        public int Week { get; set; }
        public string Team { get; set; }
        public bool? Correct { get; set; }
    }
}