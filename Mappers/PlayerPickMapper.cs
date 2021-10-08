using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Mappers
{
    public static class PlayerPickMapper
    {
        private static readonly Regex WeekRegex = new("Week (?<WeekNum>\\d+).*", RegexOptions.Compiled);
        private static readonly Regex TeamRegex = new("(?<TeamLocation>.*) \\(.*\\)", RegexOptions.Compiled);

        public static IEnumerable<SurvivorSelection> MapRows(IEnumerable<IEnumerable<string>> rows)
        {
            return rows
                .Select(row => row.Where(cell => !string.IsNullOrEmpty(cell)))
                .Where(row => row.Any())
                .Select(RowToSelection);
        }

        public static SurvivorPicker MapSelectionToNewPicker(SurvivorSelection selection)
        {
            var picker = new SurvivorPicker
            {
                Name = selection.Name,
                Eliminated = false,
                Picks = new List<SurvivorSelection>()
            };
            picker.AddAliasName(selection.Name);
            return picker;
        }

        private static SurvivorSelection RowToSelection(IEnumerable<string> row)
        {
            if (row.Count() != 4)
                throw new FormatException("Row does not contain correctly formatted data.");
            return new SurvivorSelection
            {
                PickDateTime = GetDateFromRowData(row.ElementAt(0)),
                Name = row.ElementAt(1),
                Week = GetWeekFromRowData(row.ElementAt(2)),
                Team = GetTeamFromRowData(row.ElementAt(3))
            };
        }

        private static DateTime GetDateFromRowData(string rowData)
        {
            if (!DateTime.TryParse(rowData, out var date))
                throw new FormatException("Date data does not contain correctly formatted data.");
            return date;
        }

        private static int GetWeekFromRowData(string rowData)
        {
            var matches = WeekRegex.Match(rowData);
            if (!matches.Success)
                throw new FormatException("Week data does not contain correctly formated data.");
            return int.Parse(matches.Groups["WeekNum"].Value);
        }

        private static string GetTeamFromRowData(string rowData)
        {
            var matches = TeamRegex.Match(rowData);
            if (!matches.Success)
                throw new FormatException("Team data does not contain correctly formated data.");
            return matches.Groups["TeamLocation"].Value;
        }
    }
}