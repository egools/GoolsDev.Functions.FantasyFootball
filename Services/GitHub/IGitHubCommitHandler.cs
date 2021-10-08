using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services.GitHub
{
    public interface IGitHubCommitHandler
    {
        Task CommitSurvivorData(BigTenSurvivorData data);

        Task<BigTenSurvivorData> GetSurvivorDataFromRepo();
    }
}