using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public interface INeo4jContext
    {
        IAsyncSession AsyncSession();
        ValueTask DisposeAsync();
        Task<IServerInfo> GetServerInfoAsync();
    }
}