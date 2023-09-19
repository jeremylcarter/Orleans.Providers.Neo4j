using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    internal interface INeo4JGrainStorageProvider
    {
        Task ReadAsync<T>(GrainId grainId, IGrainState<T> grainState);
        Task WriteAsync<T>(GrainId grainId, IGrainState<T> grainState);
        Task ClearAsync<T>(GrainId grainId, IGrainState<T> grainState);
    }
}
