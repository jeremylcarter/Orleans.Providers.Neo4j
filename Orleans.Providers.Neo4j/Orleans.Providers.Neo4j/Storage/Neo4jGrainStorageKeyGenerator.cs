using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    sealed class Neo4jGrainStorageKeyGenerator : INeo4JGrainStorageKeyGenerator
    {
        public string Generate(GrainId grainId)
        {
            return grainId.Key.ToString();
        }
    }
}
