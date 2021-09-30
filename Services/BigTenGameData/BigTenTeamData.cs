namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenTeamData
    {
        public BigTenTeamData(
            BigTenCompetitorDto team,
            BigTenCompetitorDto opponent,
            int week)
        {
            FullName = team.Team.DisplayName;
            Location = team.Team.Location;
            Abbreviation = team.Team.Abbreviation;
            Week = week;
            Winner = team.Winner;
            HomeTeam = team.HomeAway == "home";
            OpponentLocation = opponent.Team.Location;
            Score = $"{team.Score}-{opponent.Score}";
            GameString = Location + (HomeTeam ? " vs. " : " @ ") + OpponentLocation;
        }

        public string FullName { get; init; }
        public string Location { get; init; }
        public string Abbreviation { get; init; }
        public int Week { get; init; }
        public bool Winner { get; init; }
        public bool HomeTeam { get; init; }
        public string OpponentLocation { get; init; }
        public string Score { get; init; }
        public string GameString { get; init; }
    }
}