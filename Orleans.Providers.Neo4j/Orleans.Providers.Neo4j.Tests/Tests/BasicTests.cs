using Orleans.Providers.Neo4j.Tests.Common;
using Orleans.Providers.Neo4j.Tests.Grains;

namespace Orleans.Providers.Neo4j.Tests.Tests
{
    public class BasicTests
    {
        private TestOrleansCluster _testCluster;
        private TestDatabaseContainer _testDatabaseContainer;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            _testDatabaseContainer = new TestDatabaseContainer();
            await _testDatabaseContainer.CreateAsync();

            _testCluster = new TestOrleansCluster();

            var options = new TestOrleansClusterOptions
            {
                Neo4j = new TestOrleansClusterNeo4jOptions
                {
                    Uri = _testDatabaseContainer.ConnectionString,
                    Username = _testDatabaseContainer.Username,
                    Password = _testDatabaseContainer.Password,
                    Database = _testDatabaseContainer.Database
                }
            };
            await _testCluster.StartAsync(options);
        }

        [Test]
        public async Task SimpleSetAndGet()
        {
            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            await nicholasCage.SetName("Nicholas Cage");

            var title = await nicholasCage.GetName();
            title.ShouldBe("Nicholas Cage");
        }

        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            await _testCluster.ShutdownAsync();
            await _testDatabaseContainer.ShutdownAsync();
        }
    }
}