using EspnDataService;
using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoolsDev.Functions.FantasyFootball
{
    public static class GameDataMapper
    {
        private const string BigTenGroupNumber = "5";

        public static ICollection<SurvivorWeek> MapSchedule(IEnumerable<BigTenCalendarEntryDto> scheduleData, int startWeek, int endWeek)
        {
            var schedule = new List<SurvivorWeek>();
            foreach (var entry in scheduleData)
            {
                var week = int.Parse(entry.Value);
                if (week >= startWeek && week <= endWeek)
                {
                    schedule.Add(new SurvivorWeek(
                        week,
                        DateTime.Parse(entry.StartDate),
                        DateTime.Parse(entry.EndDate)));
                }
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
                var completed = game.Competitions.First().Status.Type.Completed;
                var gameDateTime = DateTime.Parse(game.Competitions.First().Date);

                if (homeComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(CreateTeamData(
                        homeComptetitor,
                        awayComptetitor,
                        gameData.Week.Number,
                        completed,
                        gameDateTime));
                }
                if (awayComptetitor.Team.ConferenceId == BigTenGroupNumber)
                {
                    teamData.Add(CreateTeamData(
                        awayComptetitor,
                        homeComptetitor,
                        gameData.Week.Number,
                        completed,
                        gameDateTime));
                }
            }

            return teamData;
        }

        private static BigTenTeamGame CreateTeamData(
            BigTenCompetitorDto team,
            BigTenCompetitorDto opponent,
            int week,
            bool completed,
            DateTime gameDateTime)
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
                IsBigTen = team.Team.ConferenceId == BigTenGroupNumber,
                IsCompleted = completed,
                GameDateTime = gameDateTime
            };
        }
    }
}