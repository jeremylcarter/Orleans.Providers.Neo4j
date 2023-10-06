using Neo4j.Driver;
using Orleans.Providers.Neo4j.Common;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageClient : INeo4jGrainStorageClient
    {
        private readonly IDriver _driver;
        private const string _fieldState = "state";
        private const string _fieldEtag = "eTag";
        private readonly Neo4jGrainStorageOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly Dictionary<string, Neo4jStateTypeMetadata> _metadata = new Dictionary<string, Neo4jStateTypeMetadata>();

        public Neo4jGrainStorageClient(Neo4jGrainStorageOptions options)
        {
            _options = options;
            _driver = GraphDatabase.Driver(options.Uri, AuthTokens.Basic(options.Username, options.Password));
            if (options.PropertyNameStyle == PropertyNameStyle.None)
            {
                _jsonOptions.PropertyNamingPolicy = null;
            }
        }

        public async Task ReadStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
            var result = await session.RunAsync($"MATCH (a:{grainType}) WHERE a.id = '{grainKey}' RETURN a");

            // Get the first record (or null)
            var cursor = await result.FetchAsync();

            if (cursor && result.Current != null)
            {
                // If the current record is a Node and it has a state property, deserialize it
                // It will return as the a field
                var node = result.Current["a"].As<INode>();
                if (node != null)
                {
                    ConvertInto(node, grainState);
                }
            }
        }

        private void ConvertInto<T>(INode node, IGrainState<T> grainState)
        {
            // If the state object is IConvertableState, use the ConvertFrom method
            // Otherwise use the JsonSerializer which uses a single state property
            if (grainState.State is IConvertableState<IReadOnlyDictionary<string, object>> convertableState)
            {
                convertableState.ConvertFrom(node.Properties);
            }
            else
            {
                var jsonState = node[_fieldState].As<string>();
                var deserializedState = JsonSerializer.Deserialize<T>(jsonState, _jsonOptions);
                if (deserializedState != null)
                {
                    grainState.State = deserializedState;
                }
            }
            grainState.RecordExists = true;
            grainState.ETag = node[_fieldEtag].As<string>();
        }

        public async Task WriteStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var currentETag = grainState.ETag;
            var newETag = Neo4jETagGenerator.Generate();

            grainState.RecordExists = true;

            // Convert the state to a dictionary of properties
            var parameters = ConvertToParameters(grainState, newETag);

            if (string.IsNullOrEmpty(currentETag))
            {
                // It's the first write, create the node with state and initial ETag
                await session.RunAsync($@"
                    MERGE (n:{grainType} {{ id: '{grainKey}' }})
                    ON CREATE SET n += $properties, n.{_fieldEtag} = $eTag
                ", parameters);
            }
            else
            {
                // Update the state and ETag for existing node
                // Notice we are matching the id and the ETag for idempotency purposes
                var result = await session.RunAsync($@"
                    MATCH (n:{grainType} {{ id: '{grainKey}', {_fieldEtag}: '{currentETag}' }})
                    SET n += $properties, n.{_fieldEtag} = $eTag
                    RETURN n
                ", parameters);
            }

            grainState.ETag = newETag;
        }

        private IReadOnlyDictionary<string, object> ConvertToParameters<T>(IGrainState<T> grainState, string eTag)
        {

            // All parameters must include an eTag
            var parameters = new Dictionary<string, object>
            {
                { "eTag", eTag }
            };

            // The properties array spreads out into the Node as a KV pair.
            // If the state is IConvertableState, use the ConvertTo method  
            // Otherwise use the JsonSerializer to produce a single state property
            if (grainState.State is IConvertableState<IReadOnlyDictionary<string, object>> convertableState)
            {
                parameters.Add("properties", convertableState.ConvertTo());
            }
            else
            {
                var jsonState = JsonSerializer.Serialize(grainState.State, _jsonOptions);
                parameters.Add("properties", new Dictionary<string, object> { { "state", jsonState } });
            }

            return parameters;
        }

        public async Task ClearStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
            await session.RunAsync($"MATCH (a:{grainType} {{ id: '{grainKey}' }}) DELETE a RETURN a");
            grainState.RecordExists = false;
        }

        public async ValueTask DisposeAsync()
        {
            if (_driver != null)
            {
                await _driver.DisposeAsync();
            }
        }
    }

    public class Neo4jStateTypeMetadata
    {
        public Type StateType { get; set; }
    }
}
