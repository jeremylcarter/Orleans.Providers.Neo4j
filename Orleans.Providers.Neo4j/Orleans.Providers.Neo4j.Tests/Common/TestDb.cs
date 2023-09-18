using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver;
using NUnit.Framework.Internal;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public class TestDb
    {
        public string connectionString { get; set; } = $"bolt://localhost:7687";
        public string connectionPassword { get; set; } = TestContainers.dbPassword;

        public string connectionUsername { get; set; } = TestContainers.dbUsername;

        public async Task Connect()
        {
            await using var driver = GraphDatabase.Driver(connectionString, AuthTokens.Basic(connectionUsername, connectionPassword));
            await driver.VerifyConnectivityAsync();
        }
    }
}
