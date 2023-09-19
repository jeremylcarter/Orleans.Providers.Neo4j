namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestOrleansClusterOptions
    {
        public TestOrleansClusterNeo4jOptions Neo4j { get; set; }
    }

    public class TestOrleansClusterNeo4jOptions
    {
        public required string Uri { get; set; } = "bolt://localhost:7687";
        public required string Database { get; set; } = "neo4j";
        public required string Username { get; set; } = "neo4j";
        public required string Password { get; set; } = "neo4j";
    }
}