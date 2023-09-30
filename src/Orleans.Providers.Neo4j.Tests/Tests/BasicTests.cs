using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using Orleans.Providers.Neo4j.Tests.Common;
using Orleans.Providers.Neo4j.Tests.Grains;

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
        }

        [Test]
        public async Task SimpleSetAndGet()
        {
            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            await nicholasCage.SetName("Nicholas Cage");

            var title = await nicholasCage.GetName();
            title.ShouldBe("Nicholas Cage");
        }

        [Test]
        public async Task CheckTheNodeExists()
        {
            var context = _testCluster.GetService<INeo4jContext>();
            var session = context.AsyncSession();
            var result = await session.RunAsync("MATCH (n:Actor {id: 'nickolasCage'}) RETURN n");
            await result.FetchAsync();
            result.Current.ShouldNotBeNull();
            var node = result.Current.Values["n"] as INode;
            node.ShouldNotBeNull();
            node.Properties["state"].As<string>().ShouldNotBeNullOrEmpty();
            var state = node.Properties["state"].As<string>();
            var jsonState = JObject.Parse(state);
            jsonState["name"].Value<string>().ShouldBe("Nicholas Cage");
        }

        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            await _testCluster.ShutdownAsync();
        }
    }
}