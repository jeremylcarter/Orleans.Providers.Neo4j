namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestOrleansClusterOptions
    {
        public bool CreateContainer { get; set; } = true;
        public TestOrleansClusterNeo4jOptions Neo4j { get; set; }
    }

    public class TestOrleansClusterNeo4jOptions
    {
        public string Uri { get; set; } = "bolt://localhost:7687";
        public string Database { get; set; } = "neo4j";
        public string Username { get; set; } = "neo4j";
        public string Password { get; set; } = "neo4j";
    }
}