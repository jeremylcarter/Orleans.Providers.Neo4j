using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    public interface INeo4JGrainStorageGenerator
    {
        string GenerateKey(GrainId grainId);
        string GenerateType(GrainId grainId);
    }
}
