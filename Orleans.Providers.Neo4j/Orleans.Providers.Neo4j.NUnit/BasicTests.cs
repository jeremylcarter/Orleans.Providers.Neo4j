using Orleans.Providers.Neo4j.NUnit.Grains;

namespace Orleans.Providers.Neo4j.NUnit
{
    public class BasicTests
    {
        private TestOrleansCluster _testCluster;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            _testCluster = new TestOrleansCluster();
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