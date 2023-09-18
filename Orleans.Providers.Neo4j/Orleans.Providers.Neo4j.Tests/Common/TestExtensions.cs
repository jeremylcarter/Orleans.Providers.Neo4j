using Orleans.TestingHost;

namespace Orleans.Providers.Neo4j.Tests.Common
{
    public static class TestExtensions
    {
        /// <summary>
        /// Returns the underlying IServiceProvider for a given TestCluster.
        /// </summary>
        /// <param name="testCluster"></param>
        public static IServiceProvider GetServiceProvider(this TestCluster testCluster)
        {
            return ((InProcessSiloHandle)testCluster.Primary).SiloHost.Services;
        }

        /// <summary>
        /// Performs a hard kill on the TestCluster. This will not free up resources correctly.
        /// </summary>
        /// <param name="testCluster"></param>
        public static async Task TeardownAsync(this TestCluster testCluster)
        {
            try
            {
                foreach (var item in testCluster.SecondarySilos)
                {
                    await item.StopSiloAsync(false);
                    await testCluster.KillSiloAsync(item);
                }
                await testCluster.Primary.StopSiloAsync(false);
                await testCluster.KillSiloAsync(testCluster.Primary);
                await testCluster.StopClusterClientAsync();
                testCluster.Primary?.Dispose();
                foreach (var item in testCluster.SecondarySilos)
                {
                    item.Dispose();
                }
                testCluster.PortAllocator?.Dispose();
                testCluster.Dispose();
            }
            catch (Exception)
            {
                // Ignore this exception as this is a hard kill.
            }
        }
    }
}
