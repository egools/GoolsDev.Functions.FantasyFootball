using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public class CosmosGames : ICosmosGames
    {
        private readonly Container _gamesContainer;

        public CosmosGames(CosmosClient cosmosClient)
        {
            _gamesContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-games");
        }

        public async Task<List<NflGameDocument>> GetGames(int year, int seasonType, int week)
        {
            var weekId = Helpers.CreateWeekId(year, seasonType, week);
            var query = new QueryDefinition(
                "select * from games g where g.weekId = @WeekId")
                .WithParameter("@WeekId", weekId);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(weekId),
                MaxItemCount = 16
            };

            var games = new List<NflGameDocument>();
            using var resultSet = _gamesContainer.GetItemQueryIterator<NflGameDocument>(query, requestOptions: requestOptions);
            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                games.AddRange(response.ToList());
            }
            return games;
        }

        public async Task<List<NflGameDocument>> GetGames(int year)
        {
            var query = new QueryDefinition(
                "select * from games g where g.year = @Year")
                .WithParameter("@Year", year);
            var requestOptions = new QueryRequestOptions()
            {
                MaxItemCount = 16
            };

            var games = new List<NflGameDocument>();
            using var resultSet = _gamesContainer.GetItemQueryIterator<NflGameDocument>(query, requestOptions: requestOptions);
            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                games.AddRange(response.ToList());
            }
            return games;
        }

        public async Task<ItemResponse<NflGameDocument>> UpsertGame(NflGameDocument gameDoc)
        {
            return await _gamesContainer.UpsertItemAsync(gameDoc, new PartitionKey(gameDoc.WeekId));
        }
    }

    public interface ICosmosGames
    {
        Task<List<NflGameDocument>> GetGames(int year);
        Task<List<NflGameDocument>> GetGames(int year, int seasonType, int week);
        Task<ItemResponse<NflGameDocument>> UpsertGame(NflGameDocument gameDoc);
    }
}
