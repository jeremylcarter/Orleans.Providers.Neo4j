using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.TestingHost;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    internal class TestConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(services =>
            {
                // Configure any services needed here
                services.AddSingleton(new Randomizer());
            });
            siloBuilder.AddMemoryGrainStorage("MemoryStorage");
            siloBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>("MemoryStreamProvider");
            siloBuilder.AddStreaming();
        }
    }
}
