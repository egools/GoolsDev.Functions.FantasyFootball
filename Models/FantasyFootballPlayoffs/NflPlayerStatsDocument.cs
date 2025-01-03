namespace GoolsDev.Functions.FantasyFootball
{
    public record NflPlayerStatsDocument(
        string Id,
        string Year,
        string TeamId,
        string TeamShortName,
        string PlayerId,
        string FirstName,
        string LastName,
        int PassingYards,
        int PassingTouchdowns,
        int Interceptions,
        int RushingYards,
        int RushingTouchdowns,
        int Receptions,
        int ReceivingYards,
        int ReceivingTouchdowns,
        int ReturnTouchdowns,
        int FumblesLost,
        double FantasyPoints
    );
}
