using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class Neo4jContext : INeo4jContext
    {
        private readonly IDriver _driver;

        public Neo4jContext(string uri, string username, string password)
            : this(uri, AuthTokens.Basic(username, password))
        {
        }

        public Neo4jContext(string uri, IAuthToken token)
            : this(GraphDatabase.Driver(uri, token))
        {
        }

        public Neo4jContext(IDriver driver)
        {
            _driver = driver;
        }

        public Task<IServerInfo> GetServerInfoAsync()
        {
            return _driver.GetServerInfoAsync();
        }

        public IAsyncSession AsyncSession()
        {
            return _driver.AsyncSession();
        }

        public ValueTask DisposeAsync()
        {
            return _driver.DisposeAsync();
        }
    }
}
