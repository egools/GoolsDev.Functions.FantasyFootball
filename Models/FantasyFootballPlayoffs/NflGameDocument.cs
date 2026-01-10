using System;

namespace GoolsDev.Functions.FantasyFootball
{
    public record NflGameDocument
    (
        string Id,
        string WeekId,
        string GameName,
        bool HadStatsPulled,
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
        string Location,
        string Nickname,
        string ShortName,
        string Color,
        string AlternateColor,
        bool IsActive,
        double Score,
        bool Winner,
        string GameStatus,
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
