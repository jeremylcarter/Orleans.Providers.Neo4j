using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public static class Neo4jContextFactory
    {
        public static INeo4jContext Create(string uri, string username, string password)
        {
            return new Neo4jContext(uri, AuthTokens.Basic(username, password));
        }
    }
}
