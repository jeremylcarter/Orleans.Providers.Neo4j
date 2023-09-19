using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Providers.Neo4j.Storage;
using Orleans.Runtime;
using Orleans.Storage;

namespace Microsoft.Extensions.Hosting
{
    public static class HostingExtensions
    {
        public static ISiloBuilder AddNeo4jGrainStorageAsDefault(this ISiloBuilder builder, Action<Neo4jGrainStorageOptions> configureOptions)
        {
            return builder.AddNeo4jGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
        }

        public static ISiloBuilder AddNeo4jGrainStorage(this ISiloBuilder builder, string name, Action<Neo4jGrainStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddNeo4jGrainStorage(name, configureOptions));
        }

        public static IServiceCollection AddNeo4jGrainStorage(this IServiceCollection services, string name,
         Action<Neo4jGrainStorageOptions> configureOptions)
        {
            return services.AddNeo4jGrainStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddNeo4jGrainStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<Neo4jGrainStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<Neo4jGrainStorageOptions>(name));

            if (string.Equals(name, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
            {
                services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            }

            services.ConfigureNamedOptionForLogging<Neo4jGrainStorageOptions>(name);

            return services.AddSingletonNamedService(name, Neo4jGrainStorageFactory.CreateSimpleGrainStorage)
                           .AddSingletonNamedService(name, (sp, sn) => (ILifecycleParticipant<ISiloLifecycle>)sp.GetRequiredServiceByName<IGrainStorage>(sn));
        }
    }
}
