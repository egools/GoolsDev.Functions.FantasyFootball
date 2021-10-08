using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class SurvivorPicker
    {
        private readonly Regex AliasRegex = new("[\\s']", RegexOptions.Compiled);
        public string Name { get; set; }
        public bool Eliminated { get; set; }
        public int? WeekEliminated { get; set; }
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
    }
}