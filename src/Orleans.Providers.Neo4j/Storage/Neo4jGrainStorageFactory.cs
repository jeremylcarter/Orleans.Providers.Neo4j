using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;

namespace Orleans.Providers.Neo4j.Storage
{
    internal static class Neo4jGrainStorageFactory
    {
        internal static IGrainStorage CreateGrainStorage(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<Neo4jGrainStorageOptions>>();
            var options = optionsMonitor.Get(name);
            return ActivatorUtilities.CreateInstance<Neo4jSimpleGrainStorage>(services, name, options);
        }
    }
}