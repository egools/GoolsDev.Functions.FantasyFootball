using Flurl.Http.Configuration;
using GoolsDev.Functions.FantasyFootball.Services;
using GoolsDev.Functions.FantasyFootball.Services.BigTenGameData;
using GoolsDev.Functions.FantasyFootball.Services.GitHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using YahooFantasyService;

namespace GoolsDev.Functions.FantasyFootball
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(s =>
                {
                    s.AddOptions<GoogleSheetsServiceSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("GoogleSheetsSettings").Bind(settings);
                    });
                    s.AddOptions<YahooServiceSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("YahooServiceSettings").Bind(settings);
                    });
                    s.AddOptions<BigTenGameDataServiceSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("GameData").Bind(settings);
                    });

                    s.AddOptions<GitHubCommitHandlerSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("GitHubSettings").Bind(settings);
                    });

                    s.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()
                    .AddSingleton<IYahooService, YahooService>()
                    .AddSingleton<IGoogleSheetsService, GoogleSheetsService>()
                    .AddSingleton<IBigTenGameDataService, BigTenGameDataService>()
                    .AddSingleton<IGitHubCommitHandler, GitHubCommitHandler>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}