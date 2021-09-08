using System;
using System.Collections.Generic;
using System.Linq;
using YahooFantasyService;

namespace FantasyComponents.Factories
{
    public static class SeasonFactory
    {
        public static Season FromYahooDto(YahooLeague yLeague)
        {
            if (yLeague.Settings is null)
            {
                throw new ArgumentException("League initialization requires Settings to not be null.");
            }
            var season = new Season(
                short.Parse(yLeague.Season),
                yLeague.LeagueKey,
                yLeague.Name,
                new Settings
                {
                    Divisions = CreateDivisions(yLeague.Settings.Divisions),
                    StartWeek = int.Parse(yLeague.StartWeek),
                    EndWeek = int.Parse(yLeague.EndWeek),
                    StartDate = DateTime.Parse(yLeague.StartDate),
                    EndDate = DateTime.Parse(yLeague.EndDate),
                    HasConsolations = yLeague.Settings.HasPlayoffConsolationGames,
                    ConsolationTeams = yLeague.Settings.NumPlayoffConsolationTeams,
                    PlayoffTeams = int.Parse(yLeague.Settings.NumPlayoffTeams),
                    PlayoffStartWeek = int.Parse(yLeague.Settings.PlayoffStartWeek),
                    UsePlayoffReseeding = yLeague.Settings.UsesPlayoffReseeding == 1,
                    UseFractionalPoints = yLeague.Settings.UsesFractionalPoints == "1",
                    UseNegativePoints = yLeague.Settings.UsesNegativePoints == "1",
                    UsesFaab = yLeague.Settings.UsesFaab == "1",
                    WaiverRule = yLeague.Settings.WaiverRule,
                    WaiverType = yLeague.Settings.WaiverType,
                    RosterPositions = CreateRosterPositions(yLeague.Settings.RosterPositions),
                    StatModifiers = CreateModifiers(yLeague.Settings.StatCategories, yLeague.Settings.StatModifiers)
                });

            return season;
        }

        private static List<Division> CreateDivisions(List<SettingsDivision> divisions) =>
            divisions.Select(d => new Division
            {
                Name = d.Name,
                DivisionId = d.DivisionId
            }).ToList();

        private static List<RosterPosition> CreateRosterPositions(List<SettingsRosterPosition> rp) =>
            rp.Select(p => new RosterPosition
            {
                PositionCount = p.Count,
                Position = PositionUtilities.ParseFantasyPosition(p.Position),
                PositionType = PositionUtilities.ParsePositionType(p.PositionType)
            }).ToList();

        private static Dictionary<int, StatModifier> CreateModifiers(List<SettingsStatCategory> cats, List<SettingsStatModifier> mods)
        {
            var modifiers = new Dictionary<int, StatModifier>();
            foreach (var cat in cats)
            {
                var mod = mods.FirstOrDefault(m => m.StatId == cat.StatId);
                float.TryParse(mod?.Value, out float value);

                modifiers.TryAdd(
                    cat.StatId,
                    new StatModifier
                    {
                        StatId = cat.StatId,
                        Value = value,
                        StatName = cat.Name,
                        StatDisplayName = cat.DisplayName,
                        Enabled = cat.Enabled == "1",
                        IsDisplayOnly = cat.IsOnlyDisplayStat == "1",
                        PositionTypes = cat.StatPositionTypes.Select(type => new StatPositionType
                        {
                            PositionType = PositionUtilities.ParsePositionType(type.PositionType),
                            IsDisplayOnly = type.IsOnlyDisplayStat == "1"
                        }).ToList(),
                        Bonuses = mod?.Bonuses?.Select(b => new StatBonus
                        {
                            BonusAmount = b.Points,
                            BonusThreshold = b.Target
                        }).ToList()
                    });
            }

            return modifiers;
        }
    }
}