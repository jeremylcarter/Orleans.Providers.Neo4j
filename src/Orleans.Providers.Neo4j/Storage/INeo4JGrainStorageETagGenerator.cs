namespace Orleans.Providers.Neo4j.Storage
{
    public interface INeo4jGrainStorageETagGenerator
    {
        string Generate(string grainType, string grainKey, string currentETag);
    }
}
