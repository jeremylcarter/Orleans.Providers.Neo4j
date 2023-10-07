using System.Security.Cryptography;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageETagGenerator : INeo4jGrainStorageETagGenerator
    {
        public string Generate(string grainType, string grainKey, string currentETag)
        {
            // This is not strong, or even a good way to generate an etag
            // But we are only using eTags for idempotency checks
            // Consider your own method by implementing INeo4JGrainStorageETagGenerator
            var byteArray = RandomNumberGenerator.GetBytes(8);
            return Convert.ToHexString(byteArray).ToLower();
        }
    }
}
