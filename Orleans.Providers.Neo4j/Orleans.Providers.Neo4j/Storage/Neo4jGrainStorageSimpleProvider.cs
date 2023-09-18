using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageSimpleProvider : INeo4JGrainStorageProvider
    {
        public async Task ReadAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {

        }

        public async Task WriteAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {

        }

        public async Task ClearAsync<T>(GrainId grainId, IGrainState<T> grainState)
        {

        }
    }

    public interface INeo4JGrainStorageProvider
    {
        Task ReadAsync<T>(GrainId grainId, IGrainState<T> grainState);
        Task WriteAsync<T>(GrainId grainId, IGrainState<T> grainState);
        Task ClearAsync<T>(GrainId grainId, IGrainState<T> grainState);
    }
}
