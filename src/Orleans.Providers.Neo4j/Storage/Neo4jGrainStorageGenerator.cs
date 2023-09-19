using Orleans.Runtime;

namespace Orleans.Providers.Neo4j.Storage
{
    sealed class Neo4jGrainStorageGenerator : INeo4JGrainStorageGenerator
    {
        public string GenerateKey(GrainId grainId)
        {
            return grainId.Key.ToString();
        }

        public string GenerateType(GrainId grainId)
        {
            var type = grainId.Type.ToString();
            if (string.IsNullOrEmpty(type)) return type;
            return char.ToUpper(type[0]) + type[1..];
        }
    }
}
