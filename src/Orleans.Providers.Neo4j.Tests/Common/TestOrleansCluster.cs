using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestOrleansCluster
    {
        private TestDatabaseContainer testDatabaseContainer;
        public IServiceProvider Services { get; private set; }
        public IGrainFactory GrainFactory { get; private set; }
        public IHost Host { get; private set; }
        public IClusterClient Client { get; private set; }

        public async Task<IServiceProvider> StartAsync(
            Action<ISiloBuilder> siloBuilderAction = null,
            Action<IHostBuilder> hostBuilderAction = null)
        {
            // Attempt to read the app settings before we build the host to know if we need to create a container
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true)
               .Build();

            var options = new TestOrleansClusterOptions();

            var shouldCreateContainer = configuration.GetValue<bool>("TestContainer:Enabled", true);
            if (shouldCreateContainer)
            {
                testDatabaseContainer = new TestDatabaseContainer();
                await testDatabaseContainer.CreateAsync();
                options.CreateContainer = true;
                options.Neo4j = new TestOrleansClusterNeo4jOptions
                {
                    Uri = testDatabaseContainer.ConnectionString,
                    Username = testDatabaseContainer.Username,
                    Password = testDatabaseContainer.Password,
                    Database = testDatabaseContainer.Database
                };
            }
            else if (configuration.GetValue<bool>("Neo4j:Enabled", false))
            {
                var neo4jSection = configuration.GetSection("Neo4j");
                options.Neo4j = new TestOrleansClusterNeo4jOptions
                {
                    Uri = neo4jSection.GetValue<string>("Uri"),
                    Username = neo4jSection.GetValue<string>("Username"),
                    Password = neo4jSection.GetValue<string>("Password"),
                    Database = neo4jSection.GetValue<string>("Database")
                };
                // Ensure the database can be connected to
                var driver = GraphDatabase.Driver(options.Neo4j.Uri, AuthTokens.Basic(options.Neo4j.Username, options.Neo4j.Password));
                await driver.VerifyConnectivityAsync();
                await driver.DisposeAsync();
            }

            string serviceId = Guid.NewGuid().ToString();
            string clusterId = Guid.NewGuid().ToString();

            // Pick a random port for the silo and the gateway
            int siloPort = new Randomizer().Number(10000, 20000);
            int gatewayPort = siloPort + 1;

            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Create a database context for other tests to use
                    var neo4jContext = Neo4jContextFactory.Create(options.Neo4j.Uri, options.Neo4j.Username, options.Neo4j.Password);
                    services.AddSingleton(neo4jContext);
                })
                .UseOrleans((hostContext, siloBuilder) =>
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
                        .AddNeo4jGrainStorageAsDefault(storageOptions =>
                        {
                            storageOptions.Uri = options.Neo4j.Uri;
                            storageOptions.Database = options.Neo4j.Database;
                            storageOptions.Username = options.Neo4j.Username;
                            storageOptions.Password = options.Neo4j.Password;
                        })
                        .AddStreaming();
                    // Allow the test to configure the silo builder
                    // siloBuilderAction?.Invoke(siloBuilder);
                })
            .ConfigureLogging(logging => logging.AddConsole());

            // Allow the test to configure the host builder
            hostBuilderAction?.Invoke(hostBuilder);

            var host = hostBuilder.Build();

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
            // If an Orleans client was created, then force all grains to deactivate
            if (Client != null)
            {
                var managementGrain = Client.GetGrain<IManagementGrain>(0);
                await managementGrain.ForceActivationCollection(new TimeSpan(0));
            }

            // If a host was created, shut it down
            if (Host != null)
            {
                await Host.StopAsync();
            }

            // If a test database container was created, shut it down
            if (testDatabaseContainer != null)
            {
                await testDatabaseContainer?.ShutdownAsync();
            }
        }
    }
}
