using System.Security.Cryptography;

namespace Orleans.Providers.Neo4j.Storage
{
    internal static class Neo4jETagGenerator
    {
        internal static string Generate()
        {
            // This is not strong, or even a good way to generate an etag
            // But we are only using eTags for idempotency checks
            var byteArray = RandomNumberGenerator.GetBytes(8);
            return Convert.ToHexString(byteArray);
        }
    }
}
