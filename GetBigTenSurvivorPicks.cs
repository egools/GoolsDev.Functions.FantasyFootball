using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using GoolsDev.Functions.FantasyFootball.Services;
using GoolsDev.Functions.FantasyFootball.Services.BigTenGameData;
using GoolsDev.Functions.FantasyFootball.Services.GitHub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoolsDev.Functions.FantasyFootball
{
    public class GetBigTenSurvivorPicks
    {
        private const string BigTenGroupNumber = "5";
        private readonly IGoogleSheetsService _sheetsService;
        private readonly IBigTenGameDataService _gameDataService;
        private readonly IGitHubCommitHandler _commitHandler;

        public GetBigTenSurvivorPicks(
            IGoogleSheetsService sheetsService,
            IBigTenGameDataService gameDataService,
            IGitHubCommitHandler gitHubCommitHandler)
        {
            _sheetsService = sheetsService;
            _gameDataService = gameDataService;
            _commitHandler = gitHubCommitHandler;
        }

        [Function("GetBigTenSurvivorPicks")]
        public async Task Run([TimerTrigger("%GetSurvivorDataTimerSchedule%")] FunctionContext context)
        {
            var logger = context.GetLogger("GetBigTenSurvivorPicks");

            var survivorData = await _commitHandler.GetSurvivorDataFromRepo();
            if (survivorData.Schedule is null)
            {
                survivorData.Schedule = new List<SurvivorWeek>();
                var scheduleData = await _gameDataService.GetScheduleData();
                foreach (var entry in scheduleData)
                {
                    survivorData.Schedule.Add(new SurvivorWeek(
                        int.Parse(entry.Value),
                        DateTime.Parse(entry.StartDate),
                        DateTime.Parse(entry.EndDate)));
                }
            }

            var rows = await _sheetsService.GetRows("Form Responses 1", "A2", "O1000");
            var picks = rows
                .Select(row => row.Where(cell => !string.IsNullOrEmpty(cell)))
                .Where(row => row.Any())
                .Select(RowToPick);

            if (survivorData.Pickers is null)
            {
            }

            //var gamesDto = await _gameDataService.GetGameData(week.WeekNum);
            //week.Games = ParseDto(gamesDto);

            //await _commitHandler.CommitSurvivorData(new BigTenSurvivorData(), string.Empty);
        }

        private static SurvivorPick RowToPick(IEnumerable<string> row)
        {
            if (row.Count() != 4)
                throw new InvalidOperationException("Row does not contain correctly formatted data.");
            return new SurvivorPick
            {
                PickDateTime = DateTime.Parse(row.ElementAt(0)),
                Name = row.ElementAt(1),
                Week = GetWeekFromRowData(row.ElementAt(2)),
                Team = GetTeamFromRowData(row.ElementAt(3))
            };
        }

        private static int GetWeekFromRowData(string rowData)
        {
            var regex = new Regex("Week (?<WeekNum>\\d+).*", RegexOptions.Compiled);
            var matches = regex.Match(rowData);
            return int.Parse(matches.Groups["WeekNum"].Value);
        }

        private static string GetTeamFromRowData(string rowData)
        {
            var regex = new Regex("(?<TeamLocation>.*) \\(.*\\)", RegexOptions.Compiled);
            var matches = regex.Match(rowData);
            return matches.Groups["TeamLocation"].Value;
        }

        private static ICollection<BigTenTeamGame> ParseDto(BigTenGameDataDto gameData)
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