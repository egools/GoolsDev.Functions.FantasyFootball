using System.Collections.Generic;

namespace GoolsDev.Functions.FantasyFootball
{
    public record NflPlayerStatsDocument(
        string Id,
        string PlayerId,
        string Year,
        string TeamId,
        string TeamShortName,
        string FirstName,
        string LastName
    )
    {
        public string JerseyNumber { get; set; }
        public string PositionId { get; set; }
        public string Position { get; set; }
        public List<NflPlayerStatline> Statlines { get; set; }
    };

    public record NflPlayerStatline
    (
        int PassingYards,
        int PassingTouchdowns,
        int Interceptions,
        int RushingYards,
        int RushingTouchdowns,
        int Receptions,
        int ReceivingYards,
        int ReceivingTouchdowns,
        int ReturnTouchdowns,
        int FumblesLost
    )
    {
        public double FantasyPoints { get; set; }
    };
}
