using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageKeyGenerator : INeo4JGrainStorageKeyGenerator
    {
        public string Generate(GrainId grainId)
        {
            return grainId.Key.ToString();
        }
    }
}
