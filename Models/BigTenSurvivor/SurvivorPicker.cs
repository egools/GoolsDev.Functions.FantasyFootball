using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class SurvivorPicker
    {
        private readonly Regex AliasRegex = new("[\\s'’\\-_,\"`]", RegexOptions.Compiled);
        public string Name { get; set; }
        public bool Eliminated { get; set; }
        public int? WeekEliminated { get; set; }
        public string EliminationReason { get; set; }
        public ICollection<SurvivorSelection> Picks { get; set; }

        [JsonProperty]
        private ICollection<string> AllNames { get; set; } = new List<string>();

        public bool HasAlias(string candidate)
        {
            var name = AliasRegex.Replace(candidate.ToLower(), "");
            return AllNames.Contains(name);
        }

        public void AddAliasName(string alias)
        {
            AllNames.Add(AliasRegex.Replace(alias.ToLower(), ""));
        }

        public void AddPick(SurvivorSelection selection)
        {
            var existingSelection = Picks.FirstOrDefault(p => p.Week == selection.Week);
            if (existingSelection is not null && selection.PickDateTime > existingSelection.PickDateTime)
            {
                Picks.Remove(existingSelection);
                Picks.Add(selection);
            }
            else
            {
                Picks.Add(selection);
            }
        }

        public void CheckPicks(int startWeek, int currentWeek)
        {
            for (int week = startWeek; week <= currentWeek; week++)
            {
                var pick = Picks.FirstOrDefault(p => p.Week == week);
                if (pick is null)
                {
                    Eliminated = true;
                    WeekEliminated = week;
                    EliminationReason = "No Pick";
                    return;
                }

                if (pick.Correct == false)
                {
                    Eliminated = true;
                    WeekEliminated = pick.Week;
                    EliminationReason = $"Incorrect Pick: {pick.Team}";
                    return;
                }

                if (Picks.Any(p => p.Team == pick.Team && p.Week != pick.Week && p.Week < week))
                {
                    Eliminated = true;
                    WeekEliminated = pick.Week;
                    EliminationReason = $"Repeat Pick: {pick.Team}";
                    return;
                }
            }
        }
    }
}