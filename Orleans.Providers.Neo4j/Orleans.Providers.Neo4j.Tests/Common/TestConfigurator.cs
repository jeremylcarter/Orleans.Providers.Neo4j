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
            siloBuilder.AddNeo4jGrainStorageAsDefault(siloBuilder =>
            {
                // This is just a neo4j container that is thrown away at the end of each test, so no worries about the credentials being public
                // It is destroyed and recreated every 24 hours
                siloBuilder.Uri = "bolt://localhost:7687";
                siloBuilder.Database = "neo4j";
                siloBuilder.Username = TestContainers.dbUsername;
                siloBuilder.Password = TestContainers.dbPassword;
            });
        }
    }
}
