using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestOrleansCluster
    {
        public IServiceProvider Services { get; private set; }
        public IGrainFactory GrainFactory { get; private set; }
        public IHost Host { get; private set; }
        public IClusterClient Client { get; private set; }

        public async Task<IServiceProvider> StartAsync(TestOrleansClusterOptions options, ISiloBuilder builder = null)
        {
            string serviceId = Guid.NewGuid().ToString();
            string clusterId = Guid.NewGuid().ToString();

            // Pick a random port for the silo and the gateway
            int siloPort = new Randomizer().Number(10000, 20000);
            int gatewayPort = siloPort + 1;

            var host = new HostBuilder().UseOrleans((hostContext, siloBuilder) =>
            {
                siloBuilder
                    .UseLocalhostClustering()
                    .ConfigureEndpoints(siloPort, gatewayPort)
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = clusterId;
                        options.ServiceId = serviceId;
                    })
                    .ConfigureServices(services =>
                    {
                        // Configure any services needed here
                        services.AddSingleton(new Randomizer());
                    })
                    .AddMemoryGrainStorage("MemoryStorage")
                    .AddMemoryStreams<DefaultMemoryMessageBodySerializer>("MemoryStreamProvider")
                    .AddNeo4jGrainStorageAsDefault(siloBuilder =>
                    {
                        siloBuilder.Uri = options.Neo4j.Uri;
                        siloBuilder.Database = options.Neo4j.Database;
                        siloBuilder.Username = options.Neo4j.Username;
                        siloBuilder.Password = options.Neo4j.Password;
                    })
                    .AddStreaming();
            })
            .ConfigureLogging(logging => logging.AddConsole())
            .Build();

            await host.StartAsync();

            Host = host;
            Services = host.Services;
            Client = host.Services.GetRequiredService<IClusterClient>();
            GrainFactory = host.Services.GetRequiredService<IGrainFactory>();
            return Services;
        }

        public Randomizer GetRandomizer()
        {
            return Services!.GetService<Randomizer>()!;
        }

        public string RandomCode(int length = 8)
        {
            return new Bogus.DataSets.Finance().Account(length);
        }

        public string RandomAlphaNumeric(int length = 10)
        {
            return GetRandomizer().AlphaNumeric(length);
        }

        public T GetService<T>()
        {
            return Services!.GetService<T>() ?? throw new Exception($"Service {typeof(T).Name} not found");
        }

        public T GetGrain<T>(Guid id) where T : IGrainWithGuidKey
        {
            return GrainFactory!.GetGrain<T>(id);
        }

        public T GetGrain<T>(string id) where T : IGrainWithStringKey
        {
            return GrainFactory!.GetGrain<T>(id);
        }

        public T GetGrain<T>(int id) where T : IGrainWithIntegerKey
        {
            return GrainFactory!.GetGrain<T>(id);
        }

        public async Task ShutdownAsync()
        {
            var managementGrain = Client.GetGrain<IManagementGrain>(0);
            await managementGrain.ForceActivationCollection(new TimeSpan(0));
            await Host.StopAsync();
        }
    }


}
