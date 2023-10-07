using Neo4j.Driver;
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
            await nicholasCage.SetDateOfBirth(new DateTime(1964, 1, 7));

            var name = await nicholasCage.GetName();
            name.ShouldBe("Nicholas Cage");
        }


        [Test, Order(2)]
        public async Task SimpleSetAndLoad()
        {
            await _testCluster.DeactivateAllGrains();

            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            var name = await nicholasCage.GetName();
            var dateOfBirth = await nicholasCage.GetDateOfBirth();

            name.ShouldBe("Nicholas Cage");
            dateOfBirth.ShouldBe(new DateTime(1964, 1, 7));
        }

        [Test, Order(3)]
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
            node.Properties["dateOfBirth"].As<DateTime>().ShouldNotBe(default);
            node.Properties["dateOfBirth"].As<DateTime>().ShouldBe(new DateTime(1964, 1, 7));
        }

        [Test, Order(4)]
        public async Task AddAMovie()
        {
            var nicholasCage = _testCluster.GetGrain<IActorGrain>("nickolasCage");
            var movie = _testCluster.GetGrain<IMovieGrain>("theRock");
            await movie.SetTitle("The Rock");
            await movie.SetYear(1996);
            await nicholasCage.AddMovie("theRock");

            var movies = await nicholasCage.GetMovies();
            movies.Count.ShouldBe(1);
            movies.FirstOrDefault().ShouldBe("theRock");
        }


        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            await _testCluster.ShutdownAsync();
        }
    }
}