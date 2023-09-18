namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageOptions
    {
        public required string Uri { get; set; }
        public required string Database { get; set; } = "neo4j";
        public required string Username { get; set; }
        public required string Password { get; set; }
        public INeo4JGrainStorageKeyGenerator KeyGenerator { get; set; }
    }
}