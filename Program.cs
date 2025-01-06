using EspnDataService;
using GoolsDev.Functions.FantasyFootball;
using GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor;
using GoolsDev.Functions.FantasyFootball.Services;
using GoolsDev.Functions.FantasyFootball.Services.GitHub;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using YahooFantasyService;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(s =>
    {
        s.AddOptions<BigTenSurvivorSettings>()
        .Configure<IConfiguration>((settings, configuration) =>
        {
            configuration.GetSection("BigTenSurvivorSettings").Bind(settings);
        });
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

        s.AddOptions<GitHubCommitHandlerSettings>()
        .Configure<IConfiguration>((settings, configuration) =>
        {
            configuration.GetSection("GitHubSettings").Bind(settings);
        });

        s.AddSingleton<IYahooService, YahooService>()
        .AddSingleton<IEspnNflService, EspnNflService>()
        .AddSingleton<IGoogleSheetsService, GoogleSheetsService>()
        .AddSingleton<IBigTenGameDataService, BigTenGameDataService>()
        .AddSingleton<IGitHubCommitHandler, GitHubCommitHandler>()
        .AddSingleton(s =>
        {
            var endpoint = Environment.GetEnvironmentVariable("CosmosDbEndPoint");
            var key = Environment.GetEnvironmentVariable("CosmosDbKey");
            return new CosmosClient(endpoint, key,
            new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
            });
        });
    })
    .Build();

await host.RunAsync();