using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Orleans.Providers.Neo4j.NUnit
{
    public class TestOrleansCluster
    {
        private TestCluster _cluster;
        private IServiceProvider _services;
        public IGrainFactory GrainFactory => _cluster?.GrainFactory;
        public TestCluster Cluster => _cluster;

        public async Task<IServiceProvider> BuildAsync()
        {
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestConfigurator>();

            _cluster = builder.Build();
            await _cluster.DeployAsync();
            _services = _cluster.GetServiceProvider();
            return _services;
        }

        public Randomizer GetRandomizer()
        {
            return _services!.GetService<Randomizer>()!;
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
            return _services!.GetService<T>() ?? throw new Exception($"Service {typeof(T).Name} not found");
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
            await _cluster.TeardownAsync();
        }
    }
}
