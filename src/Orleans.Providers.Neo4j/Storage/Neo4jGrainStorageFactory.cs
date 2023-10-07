using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers.Neo4j.State;
using Orleans.Storage;

namespace Orleans.Providers.Neo4j.Storage
{
    internal static class Neo4jGrainStorageFactory
    {
        internal static IGrainStorage CreateGrainStorage(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<Neo4jGrainStorageOptions>>();
            var options = optionsMonitor.Get(name);

            // State converters are loaded from the current AppDomain if not supplied
            if (options.StateConverters == null)
            {
                options.StateConverters = GetStateConverters();
            }

            return ActivatorUtilities.CreateInstance<Neo4jSimpleGrainStorage>(services, name, options);
        }

        private static IDictionary<Type, INeo4jStateConverter> GetStateConverters()
        {
            // Find all INeo4jStateConverter implementations and their first generic argument
            // Add them to a map of state type to state converter instance
            var stateConverters = new Dictionary<Type, INeo4jStateConverter>();

            var converterTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(INeo4jStateConverter).IsAssignableFrom(t));

            foreach (var converterType in converterTypes)
            {
                // The converter implements INeo4jStateConverter<T> so we need to get the T
                var interfaceType = converterType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INeo4jStateConverter<>));
                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    var stateType = genericArguments[0];
                    var converter = Activator.CreateInstance(converterType) as INeo4jStateConverter;
                    stateConverters.Add(stateType, converter);
                }
            }
            return stateConverters;
        }
    }
}