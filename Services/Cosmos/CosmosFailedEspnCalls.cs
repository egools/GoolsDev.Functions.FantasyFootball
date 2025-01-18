using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball
{
    public interface ICosmosFailedEspnCalls
    {
        Task<ItemResponse<FailedEspnCallDocument>> LogFailedCall(FailedEspnCallDocument failedCall);
    }

    public class CosmosFailedEspnCalls : ICosmosFailedEspnCalls
    {
        private readonly Container _callsContainer;

        public CosmosFailedEspnCalls(CosmosClient cosmosClient)
        {
            _callsContainer = cosmosClient.GetContainer("cosmos-bmgc-goolsdev", "failed-espn-calls");
        }

        public async Task<ItemResponse<FailedEspnCallDocument>> LogFailedCall(FailedEspnCallDocument failedCall)
        {
            return await _callsContainer.UpsertItemAsync(failedCall, new PartitionKey(failedCall.GameId));
        }
    }
}
