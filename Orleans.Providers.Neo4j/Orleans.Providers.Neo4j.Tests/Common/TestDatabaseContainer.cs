using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestDatabaseContainer
    {
        private IContainer Container { get; set; }
        public IDriver Driver { get; set; }
        public string ConnectionString { get; private set; }
        public string Username { get; }
        public string Password { get; }
        public string Database { get; }
        public int Port { get; }

        public TestDatabaseContainer(string username = "neo4j", string password = "5yWFV6Od12",
            string database = "neo4j", int port = 7687)
        {
            Username = username;
            Password = password;
            Database = database;
            Port = port;
        }

        public async Task CreateAsync(string version = "5.12.0-community")
        {
            Container = new ContainerBuilder()
            .WithImage($"neo4j:{version}")
                .WithExposedPort(Port)
                .WithPortBinding(Port, Port)
                .WithEnvironment("NEO4J_AUTH", $"{Username}/{Password}")
                .WithEnvironment("NEO4J_dbms_memory_pagecache_size", "1G")
                .WithEnvironment("NEO4J_dbms_memory_heap_max__size", "1G")
                .WithEnvironment("NEO4J_dbms_memory_heap_initial__size", "1G")
                .WithEnvironment("NEO4J_dbms_default__listen__address", "")
                .Build();

            // Start the container.
            await Container.StartAsync();
            await WaitForLogMessageAsync("Started");

            var connectionString = $"bolt://{Container.Hostname}:{Port}";
            Driver = GraphDatabase.Driver(connectionString, AuthTokens.Basic(Username, Password));
            await Driver.VerifyConnectivityAsync();
            ConnectionString = connectionString;
        }
        public async Task ShutdownAsync()
        {
            await Driver.DisposeAsync();
            await Container.StopAsync();
        }

        private async Task WaitForLogMessageAsync(string targetMessage, int maxRetries = 60, CancellationToken cancellationToken = default)
        {
            int attempt = 0;

            while (!cancellationToken.IsCancellationRequested && attempt < maxRetries)
            {
                var log = await Container.GetLogsAsync();

                if (log.ToString().Contains(targetMessage))
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                attempt++;
            }
        }
    }
}
