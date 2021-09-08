using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YahooFantasyService;

namespace FantasyComponents.Factories
{
    public static class DraftFactory
    {
        private static readonly Regex PlayerKeyRegex = new Regex(@"\d+\.p\.(?<key>\d+)", RegexOptions.Compiled);

        public static Draft FromYahooDto(YahooLeague yLeague)
        {
            var draftType = yLeague.Settings.IsAuctionDraft ? DraftType.Auction : DraftType.Snake;
            var draft = new Draft(yLeague.LeagueKey, draftType);
            foreach (var pick in yLeague.DraftPicks)
            {
                var playerKey = PlayerKeyRegex.Match(pick.PlayerKey).Groups["key"];
                var pickKey = $"{pick.TeamKey}.p.{playerKey}";
                draft.DraftedPlayers.Add(new DraftedPlayer(
                    pickKey,
                    pick.TeamKey,
                    (short)pick.Round,
                    (short)pick.Pick,
                    (short)pick.Cost));
            }

            var draftOrder = new List<int>();
            for (int i = 0; i < yLeague.NumTeams; i++)
            {
                var teamId = draft.DraftedPlayers.First(d => d.DraftPosition == i + 1).TeamId;
                draftOrder.Add(int.Parse(teamId.Split('.').Last()));
            }
            draft.DraftOrder = draftOrder;

            return draft;
        }
    }
}