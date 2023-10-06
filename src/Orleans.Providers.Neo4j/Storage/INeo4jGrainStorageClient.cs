namespace Orleans.Providers.Neo4j.Storage
{
    public interface INeo4jGrainStorageClient
    {
        Task ClearStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState);
        Task ReadStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState);
        Task WriteStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState);
        ValueTask DisposeAsync();
    }
}