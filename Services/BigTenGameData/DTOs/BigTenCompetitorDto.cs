namespace GoolsDev.Functions.FantasyFootball.Services.BigTenGameData
{
    public class BigTenCompetitorDto
    {
        public string Id { get; init; }
        public string HomeAway { get; init; }
        public bool Winner { get; init; }
        public string Score { get; init; }
        public BigTenTeamDto Team { get; init; }
    }
}