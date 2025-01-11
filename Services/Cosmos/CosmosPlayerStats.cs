using EspnDataService;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services
{
    public class CosmosPlayerStats : ICosmosPlayerStats
    {
        private readonly Container _statsContainer;
        private TransactionalBatch _batch;

        public CosmosPlayerStats(CosmosClient cosmosClient)
        {
            _statsContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "nfl-player-stats");
        }

        public async Task<List<NflPlayerStatsDocument>> GetPlayerStats(int year, IEnumerable<string> playerIds)
        {
            var query = new QueryDefinition(
               "select * from players p " +
               "where p.year = @Year " +
               "and array_contains_any(@PlayerIds, p.playerId)")
               .WithParameter("@Year", year)
               .WithParameter("@PlayerIds", playerIds);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(year),
                MaxItemCount = 16
            };

            using var resultSet = _statsContainer.GetItemQueryIterator<NflPlayerStatsDocument>(query, requestOptions: requestOptions);
            var players = new List<NflPlayerStatsDocument>();
            while (resultSet.HasMoreResults)
            {
                FeedResponse<NflPlayerStatsDocument> response = await resultSet.ReadNextAsync();
                players.AddRange(response.ToList());
            }

            return players;
        }

        public async Task<List<NflPlayerStatsDocument>> GetGamePlayerDocs(string teamId1, string teamId2, int year)
        {
            var query = new QueryDefinition("select * from players p where p.year = @Year and (p.teamId = @TeamId1 OR p.teamId = @TeamId2)")
                .WithParameter("@Year", year)
                .WithParameter("@TeamId1", teamId1)
                .WithParameter("@TeamId2", teamId2);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(year),
                MaxItemCount = 16
            };

            using var resultSet = _statsContainer.GetItemQueryIterator<NflPlayerStatsDocument>(query, requestOptions: requestOptions);
            var players = new List<NflPlayerStatsDocument>();
            while (resultSet.HasMoreResults)
            {
                FeedResponse<NflPlayerStatsDocument> response = await resultSet.ReadNextAsync();
                players.AddRange(response.ToList());
            }
            return players;
        }

        public async Task<List<NflPlayerStatsDocument>> GetAllPlayers(int year)
        {
            var query = new QueryDefinition("select * from players p where p.year = @Year")
                .WithParameter("@Year", year);
            var requestOptions = new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(year),
                MaxItemCount = 16
            };

            using var resultSet = _statsContainer.GetItemQueryIterator<NflPlayerStatsDocument>(query, requestOptions: requestOptions);
            var players = new List<NflPlayerStatsDocument>();
            while (resultSet.HasMoreResults)
            {
                FeedResponse<NflPlayerStatsDocument> response = await resultSet.ReadNextAsync();
                players.AddRange(response.ToList());
            }
            return players;
        }

        public async Task<ItemResponse<NflPlayerStatsDocument>> UpsertPlayer(NflPlayerStatsDocument playerDoc)
        {
            return await _statsContainer.UpsertItemAsync(playerDoc);
        }

        public async Task PatchStatlines(string playerId, int year, List<NflPlayerStatline> statlines)
        {
            await _statsContainer.PatchItemAsync<NflPlayerStatsDocument>(
                playerId,
                new PartitionKey(year),
                patchOperations: [PatchOperation.Set($"/statlines", statlines)]);
        }

        public void CreateBatch(int year) => _batch = _statsContainer.CreateTransactionalBatch(new PartitionKey(year));

        public void AddPlayerToBatch(NflPlayerStatsDocument playerDoc) => _batch.UpsertItem(playerDoc);

        public void PatchStatlinesInBatch(string playerId, List<NflPlayerStatline> statlines)
        {
            _batch.PatchItem(
                playerId,
                patchOperations: [PatchOperation.Set($"/statlines", statlines)]
            );
        }

        public async Task<bool> ExecuteBatch()
        {
            return (await _batch.ExecuteAsync()).IsSuccessStatusCode;
        }

    }

    public interface ICosmosPlayerStats
    {
        Task<List<NflPlayerStatsDocument>> GetAllPlayers(int year);
        Task<List<NflPlayerStatsDocument>> GetGamePlayerDocs(string teamId1, string teamId2, int year);
        Task<List<NflPlayerStatsDocument>> GetPlayerStats(int year, IEnumerable<string> playerIds);
        Task<ItemResponse<NflPlayerStatsDocument>> UpsertPlayer(NflPlayerStatsDocument playerDoc);
        Task PatchStatlines(string playerId, int year, List<NflPlayerStatline> statlines);
        void CreateBatch(int year);
        void AddPlayerToBatch(NflPlayerStatsDocument playerDoc);
        void PatchStatlinesInBatch(string playerId, List<NflPlayerStatline> statlines);
        Task<bool> ExecuteBatch();
    }
}
