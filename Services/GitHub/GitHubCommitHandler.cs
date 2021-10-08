using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Services.GitHub
{
    public class GitHubCommitHandler : IGitHubCommitHandler
    {
        private GitHubCommitHandlerSettings _settings;
        private GitHubClient _gitHubClient;

        public GitHubCommitHandler(IOptions<GitHubCommitHandlerSettings> options)
        {
            _settings = options.Value;
            _gitHubClient = new GitHubClient(new ProductHeaderValue(_settings.AppName))
            {
                Credentials = new Credentials(_settings.AccessToken)
            };
        }

        public async Task CommitSurvivorData(BigTenSurvivorData data)
        {
            var fileDetails = await GetDataFile();

            await _gitHubClient.Repository.Content.UpdateFile(
                _settings.RepoOwner,
                _settings.RepoName,
                _settings.DataFileName,
                new UpdateFileRequest(
                    $"Update survivor data",
                    "const survivorData = " + JsonConvert.SerializeObject(data, Formatting.Indented),
                    fileDetails.Sha));
        }

        public async Task<BigTenSurvivorData> GetSurvivorDataFromRepo()
        {
            var file = await GetDataFile();
            return JsonConvert.DeserializeObject<BigTenSurvivorData>(file.Content.Replace("const survivorData = ", ""));
        }

        private async Task<RepositoryContent> GetDataFile()
        {
            return (await _gitHubClient.Repository.Content.GetAllContents(
                _settings.RepoOwner,
                _settings.RepoName,
                _settings.DataFileName))[0];
        }
    }
}