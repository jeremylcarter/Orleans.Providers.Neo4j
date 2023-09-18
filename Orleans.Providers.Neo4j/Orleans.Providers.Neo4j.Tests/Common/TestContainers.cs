using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestContainers
    {
        private IContainer _container { get; set; }
        public static string dbUsername { get; set; } = "neo4j";
        public static string dbPassword { get; set; } = "strip-furnace";
        public async Task CreateNeo4jContainerAsync()
        {
            _container = new ContainerBuilder()
                .WithImage("neo4j")
                .WithExposedPort("7687")
                .WithEnvironment("NEO4J_AUTH", $"{dbUsername}/{dbPassword}")
                .WithEnvironment("NEO4J_dbms_memory_pagecache_size", "1G")
                .WithEnvironment("NEO4J_dbms_memory_heap_max__size", "1G")
                .WithEnvironment("NEO4J_dbms_memory_heap_initial__size", "1G")
                .WithEnvironment("NEO4J_dbms_default__listen__address", "")
                .Build();

            // Start the container.
            await _container.StartAsync()
                .ConfigureAwait(false);

        }
        public async Task DestroyNeo4jContainerAsync()
        {
            await _container.StopAsync();
        }
    }
}
