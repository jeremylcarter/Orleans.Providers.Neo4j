using Neo4j.Driver;
using Orleans.Providers.Neo4j.Tests.Common;

namespace Orleans.Providers.Neo4j.Tests.Tests
{
    public class BasicTests
    {
        private TestOrleansCluster _testCluster;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            _testCluster = new TestOrleansCluster();
            await _testCluster.StartAsync();

            // Delete any nodes that may have been created by previous tests
            var context = _testCluster.GetService<INeo4jContext>();
            var session = context.AsyncSession();
            await session.RunAsync("MATCH (n:Actor {id: 'nickolasCage'}) DETACH DELETE n");
        }

        [Test, Order(1)]
        public async Task SimpleSetAndGet()
        {
            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            await nicholasCage.SetName("Nicholas Cage");

            var title = await nicholasCage.GetName();
            title.ShouldBe("Nicholas Cage");
        }


        [Test, Order(1)]
        public async Task SimpleSetAndLoad()
        {
            await _testCluster.DeactivateAllGrains();

            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            var title = await nicholasCage.GetName();
            title.ShouldBe("Nicholas Cage");
        }

        [Test, Order(2)]
        public async Task CheckTheNodeExists()
        {
            var context = _testCluster.GetService<INeo4jContext>();
            var session = context.AsyncSession();
            var result = await session.RunAsync("MATCH (n:Actor {id: 'nickolasCage'}) RETURN n");
            await result.FetchAsync();
            result.Current.ShouldNotBeNull();
            var node = result.Current.Values["n"] as INode;
            node.ShouldNotBeNull();
            node.Properties["name"].As<string>().ShouldNotBeNullOrEmpty();
            node.Properties["name"].As<string>().ShouldBe("Nicholas Cage");
        }

        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            await _testCluster.ShutdownAsync();
        }
    }
}