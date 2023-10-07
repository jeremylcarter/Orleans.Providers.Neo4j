using Neo4j.Driver;
using Orleans.Providers.Neo4j.State;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStateConverter
    {
        private readonly Neo4jGrainStorageOptions _options;
        private readonly Dictionary<Type, INeo4jStateConverter> _conveters;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Neo4jGrainStateConverter(Neo4jGrainStorageOptions options)
        {
            _options = options;
            if (options.PropertyNameStyle == PropertyNameStyle.None)
            {
                _jsonOptions.PropertyNamingPolicy = null;
            }
            _conveters = new Dictionary<Type, INeo4jStateConverter>();
        }

        public void RegisterConverter(Type type, INeo4jStateConverter converter)
        {
            _conveters.Add(type, converter);
        }

        public void RegisterConverter<T>(INeo4jStateConverter<T> converter)
        {
            RegisterConverter(typeof(T), converter);
        }

        public void RegisterConverters(IDictionary<Type, INeo4jStateConverter> stateConverters)
        {
            foreach (var stateConverter in stateConverters)
            {
                RegisterConverter(stateConverter.Key, stateConverter.Value);
            }
        }

        public void ConvertInto<T>(INode node, IGrainState<T> grainState)
        {
            // If the type has a known converter then use it
            // _conveters use the default JSON converter
            if (_conveters.TryGetValue(typeof(T), out var converter))
            {
                grainState.State = ((INeo4jStateConverter<T>)converter).ConvertFrom(node.Properties);
            }
            else
            {
                var jsonState = node[_options.StatePropertyName].As<string>();
                var deserializedState = JsonSerializer.Deserialize<T>(jsonState, _jsonOptions);
                if (deserializedState != null)
                {
                    grainState.State = deserializedState;
                }
            }

            grainState.RecordExists = true;
            grainState.ETag = node[_options.ETagPropertyName].As<string>();
        }

        internal IDictionary<string, object> ConvertToParameters<T>(IGrainState<T> grainState, string newETag)
        {
            // All parameters must include an eTag
            var parameters = new Dictionary<string, object>
            {
                { _options.ETagPropertyName, newETag }
            };

            // The properties array spreads out into the Node as a KV pair.
            // If the state has a converter then use it
            // Otherwise use the JsonSerializer to produce a single state property
            if (_conveters.TryGetValue(typeof(T), out var converter))
            {
                parameters.Add("properties", ((INeo4jStateConverter<T>)converter).ConvertTo(grainState.State));
            }
            else
            {
                var jsonState = JsonSerializer.Serialize(grainState.State, _jsonOptions);
                parameters.Add("properties", new Dictionary<string, object> { { _options.StatePropertyName, jsonState } });
            }

            return parameters;
        }
    }
}
