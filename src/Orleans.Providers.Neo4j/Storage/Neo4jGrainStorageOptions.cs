using Orleans.Providers.Neo4j.State;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageOptions
    {
        public required string Uri { get; set; }
        public required string Database { get; set; } = "neo4j";
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string StatePropertyName { get; set; } = "state";
        public string ETagPropertyName { get; set; } = "eTag";
        public required PropertyNameStyle PropertyNameStyle = PropertyNameStyle.CamelCase;
        public INeo4JGrainStorageGenerator Generator { get; set; }
        public INeo4jGrainStorageClient StorageClient { get; set; }
        public IDictionary<Type, INeo4jStateConverter> StateConverters { get; internal set; }
    }

    public enum PropertyNameStyle
    {
        CamelCase,
        None,
    }
}