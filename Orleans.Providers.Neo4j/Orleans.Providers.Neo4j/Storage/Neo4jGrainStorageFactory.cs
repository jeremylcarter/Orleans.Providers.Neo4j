using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;

namespace Orleans.Providers.Neo4j.Storage
{
    public static class Neo4jGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<Neo4jGrainStorageOptions>>();
            return ActivatorUtilities.CreateInstance<Neo4jGrainStorage>(services, name, optionsMonitor.Get(name));
        }
    }
}