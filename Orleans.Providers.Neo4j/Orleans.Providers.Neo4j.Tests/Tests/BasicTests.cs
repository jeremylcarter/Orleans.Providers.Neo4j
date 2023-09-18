using Orleans.Providers.Neo4j.Tests.Common;
using Orleans.Providers.Neo4j.Tests.Grains;

namespace Orleans.Providers.Neo4j.Tests.Tests
{
    public class BasicTests
    {
        private TestOrleansCluster _testCluster;
        private TestContainers _testContainers;
        private TestDb _testDb;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            _testCluster = new TestOrleansCluster();
            _testContainers = new TestContainers();
            _testDb = new TestDb();
            await _testContainers.CreateNeo4jContainerAsync();
            await _testDb.Connect();
            await _testCluster.BuildAsync();
        }

        [Test]
        public async Task SimpleSetAndGet()
        {
            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            await nicholasCage.SetName("Nicholas Cage");

            var title = await nicholasCage.GetName();
            title.ShouldBe("Nicholas Cage");
        }
    }
}