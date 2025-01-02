using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GoolsDev.Functions.FantasyFootball
{
    public record NflGameDocument
    (
        string Id,
        string WeekId,
        string GameName,
        NflGameTeam HomeTeam,
        NflGameTeam AwayTeam,
        NflGameStatus Status,
        int Year,
        int Week,
        NflSeasonType SeasonType
    );

    public record NflGameStatus
    (
        DateTime StartDate,
        bool Complete,
        int Period,
        string Clock
    );

    public record NflGameTeam(
        string TeamId,
        string ShortName,
        string Location,
        bool IsActive,
        string Mascot,
        double Score,
        IEnumerable<double> LineScore,
        NflRecord Record
    );

    public record NflRecord
    (
        string Overall,
        string Home,
        string Away
    );

    public enum NflSeasonType
    {
        None,
        Preseason,
        RegularSeason,
        PostSeason,
        OffSeason
    }
}
