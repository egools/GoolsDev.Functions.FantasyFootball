using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using GoolsDev.Functions.FantasyFootball.Services.BigTenGameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Mappers
{
    public static class GameDataMapper
    {
        private const string BigTenGroupNumber = "5";

        public static ICollection<SurvivorWeek> MapSchedule(IEnumerable<BigTenCalendarEntryDto> scheduleData)
        {
            var schedule = new List<SurvivorWeek>();
            foreach (var entry in scheduleData)
            {
                schedule.Add(new SurvivorWeek(
                    int.Parse(entry.Value),
                    DateTime.Parse(entry.StartDate),
                    DateTime.Parse(entry.EndDate)));
            }
            return schedule;
        }

        public static ICollection<BigTenTeamGame> MapDto(BigTenGameDataDto gameData)
        {
            var teamData = new List<BigTenTeamGame>();
            foreach (var game in gameData.Events)
            {
                var homeComptetitor = game.Competitions.First().Competitors.FirstOrDefault(c => c.HomeAway == "home");
                var awayComptetitor = game.Competitions.First().Competitors.FirstOrDefault(c => c.HomeAway == "away");

                if (homeComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(CreateTeamData(
                        homeComptetitor,
                        awayComptetitor,
                        gameData.Week.Number));
                }
                if (awayComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(CreateTeamData(
                        awayComptetitor,
                        homeComptetitor,
                        gameData.Week.Number));
                }
            }

            return teamData;
        }

        private static BigTenTeamGame CreateTeamData(
            BigTenCompetitorDto team,
            BigTenCompetitorDto opponent,
            int week)
        {
            return new BigTenTeamGame
            {
                FullName = team.Team.DisplayName,
                Location = team.Team.Location,
                Abbreviation = team.Team.Abbreviation,
                Week = week,
                Winner = team.Winner,
                HomeTeam = team.HomeAway == "home",
                OpponentLocation = opponent.Team.Location,
                Score = $"{team.Score}-{opponent.Score}",
                IsBigTen = team.Team.ConferenceId == BigTenGroupNumber
            };
        }
    }
}