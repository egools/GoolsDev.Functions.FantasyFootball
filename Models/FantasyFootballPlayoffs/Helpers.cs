using EspnDataService;
using System;
using System.Collections.Immutable;

namespace GoolsDev.Functions.FantasyFootball
{
    public static class Helpers
    {
        public static ImmutableHashSet<int> OffensiveSkillPositions = ImmutableHashSet.Create(1, 7, 8, 9, 10, 50, 70, 100, 101, 102, 103, 104, 111, 219);

        public static string CreatePlayerId(string playerId, string teamId, int year) => $"{playerId}.{teamId}.{year}";

        public static string CreateWeekId(int year, int seasonType, int week) => $"{year}.{seasonType}.{week}";

        public static bool IsOffensivePlayer(this NflRosterPlayer player)
        {
            return int.TryParse(player.PositionId, out var positionId)
                ? OffensiveSkillPositions.Contains(positionId)
                : false;
        }

        public static double CalculateFantasyPoints(Statline statline) => Math.Round(
            digits: 2,
            value: (statline.PassingYards * 0.04) +
                (statline.PassingTouchdowns * 4) +
                (statline.Interceptions * -2) +
                (statline.RushingYards * 0.1) +
                (statline.RushingTouchdowns * 6) +
                (statline.Receptions * 0.5) +
                (statline.ReceivingYards * 0.1) +
                (statline.ReceivingTouchdowns * 6) +
                (statline.FumblesLost * -2) +
                (statline.ReturnTouchdowns * 6)
            );

        public static (string positionName, string positionAbbreviation) GetPositionFromId(int positionId) => positionId switch
        {
            1 => ("Wide Receiver", "WR"),
            2 => ("Left Tackle", "LT"),
            3 => ("Left Guard", "LG"),
            4 => ("Center", "C"),
            5 => ("Right Guard", "RG"),
            6 => ("Right Tackle", "RT"),
            7 => ("Tight End", "TE"),
            8 => ("Quarterback", "QB"),
            9 => ("Running Back", "RB"),
            10 => ("Fullback", "FB"),
            11 => ("Left Defensive End", "LDE"),
            12 => ("Nose Tackle", "NT"),
            13 => ("Right Defensive End", "RDE"),
            14 => ("Left Outside Linebacker", "LOLB"),
            15 => ("Left Inside Linebacker", "LILB"),
            16 => ("Right Inside Linebacker", "RILB"),
            17 => ("Right Outside Linebacker", "ROLB"),
            18 => ("Left Cornerback", "LCB"),
            19 => ("Right Cornerback", "RCB"),
            20 => ("Strong Safety", "SS"),
            21 => ("Free Safety", "FS"),
            22 => ("Place kicker", "PK"),
            23 => ("Punter", "P"),
            24 => ("Left Defensive Tackle", "LDT"),
            25 => ("Right Defensive Tackle", "RDT"),
            26 => ("Weakside Linebacker", "WLB"),
            27 => ("Middle Linebacker", "MLB"),
            28 => ("Strongside Linebacker", "SLB"),
            29 => ("Cornerback", "CB"),
            30 => ("Linebacker", "LB"),
            31 => ("Defensive End", "DE"),
            32 => ("Defensive Tackle", "DT"),
            33 => ("Under Tackle", "UT"),
            34 => ("Nickel Back", "NB"),
            35 => ("Defensive Back", "DB"),
            36 => ("Safety", "S"),
            37 => ("Defensive Lineman", "DL"),
            39 => ("Long Snapper", "LS"),
            45 => ("Offensive Lineman", "OL"),
            46 => ("Offensive Tackle", "OT"),
            47 => ("Offensive Guard", "OG"),
            50 => ("Athlete", "ATH"),
            70 => ("Offense", "OFF"),
            71 => ("Defense", "DEF"),
            72 => ("Special Teams", "ST"),
            73 => ("Guard", "G"),
            74 => ("Tackle", "T"),
            75 => ("Nose Guard", "NG"),
            76 => ("Punt Returner", "PR"),
            77 => ("Kick Returner", "KR"),
            78 => ("Long Snapper", "LS"),
            79 => ("Holder", "H"),
            80 => ("Place Kicker", "PK"),
            90 => ("Inside Linebacker", "ILB"),
            91 => ("Center", "C"),
            94 => ("Punter", "P"),
            96 => ("Long Snapper", "LS"),
            100 => ("Flanker", "FL"),
            101 => ("Halfback", "HB"),
            102 => ("Tailback", "TB"),
            103 => ("Left Halfback", "LHB"),
            104 => ("Right Halfback", "RHB"),
            105 => ("Left Linebacker", "LLB"),
            106 => ("Right Linebacker", "RLB"),
            107 => ("Outside Linebacker", "OLB"),
            108 => ("Left Safety", "LSF"),
            109 => ("Right Safety", "RSF"),
            110 => ("Middle Guard", "MG"),
            111 => ("Split End", "SE"),
            218 => ("Setter", "SETTER"),
            219 => ("Back", "B"),
            264 => ("EDGE", "EDGE"),
            _ => ("Unknown", "-")
        };
    }
}
