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
                // This is just a sandbox Neo4j instance, so no worries about the credentials being public
                // It is destroyed and recreated every 24 hours
                siloBuilder.Uri = "bolt://54.166.181.64:7687";
                siloBuilder.Database = "neo4j";
                siloBuilder.Username = "neo4j";
                siloBuilder.Password = "strip-furnace-gages";
            });
        }
    }
}
