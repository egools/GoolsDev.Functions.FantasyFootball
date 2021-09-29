using Microsoft.Azure.Functions.Worker.Configuration;
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
                    s.AddOptions<YahooServiceSettings>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("YahooServiceSettings").Bind(settings);
                    });
                    s.AddSingleton<IYahooService, YahooService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}