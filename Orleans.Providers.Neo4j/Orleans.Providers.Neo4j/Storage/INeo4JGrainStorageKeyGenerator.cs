using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    public interface INeo4JGrainStorageKeyGenerator
    {
        string Generate(GrainId grainId);
    }
}
