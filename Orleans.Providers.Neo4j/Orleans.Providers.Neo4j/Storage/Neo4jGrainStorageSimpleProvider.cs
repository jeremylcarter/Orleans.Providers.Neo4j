using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    internal class Neo4jGrainStorageSimpleProvider : INeo4JGrainStorageProvider
    {
        Task INeo4JGrainStorageProvider.ReadAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {
            throw new NotImplementedException();
        }

        Task INeo4JGrainStorageProvider.WriteAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {
            throw new NotImplementedException();
        }

        Task INeo4JGrainStorageProvider.ClearAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {
            throw new NotImplementedException();
        }
    }
}
