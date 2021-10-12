namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class BigTenTeamGame
    {
        public string FullName { get; init; }
        public string Location { get; init; }
        public string Abbreviation { get; init; }
        public int Week { get; init; }
        public bool Winner { get; set; }
        public bool HomeTeam { get; init; }
        public string OpponentLocation { get; init; }
        public string Score { get; set; }
        public bool IsBigTen { get; init; }
        public bool IsCompleted { get; init; }

        public override string ToString()
        {
            return Location + (HomeTeam ? " vs. " : " @ ") + OpponentLocation;
        }
    }
}