using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorageClient : INeo4jGrainStorageClient
    {
        private readonly IDriver _driver;
        private readonly Neo4jGrainStorageOptions _options;
        private readonly Neo4jGrainStateConverter _converter;
        private readonly INeo4jGrainStorageETagGenerator _eTagGenerator;

        public Neo4jGrainStorageClient(Neo4jGrainStorageOptions options)
        {
            _options = options;
            _driver = GraphDatabase.Driver(options.Uri, AuthTokens.Basic(options.Username, options.Password));
            _converter = new Neo4jGrainStateConverter(_options);
            _eTagGenerator = _options.ETagGenerator ?? new Neo4jGrainStorageETagGenerator();
            if (_options.StateConverters != null)
            {
                _converter.RegisterConverters(_options.StateConverters);
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
                    _converter.ConvertInto(node, grainState);
                }
            }
        }

        public async Task WriteStateAsync<T>(string grainType, string grainKey, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var currentETag = grainState.ETag;
            var newETag = _eTagGenerator.Generate(grainType, grainKey, currentETag);

            grainState.RecordExists = true;

            // Convert the state to a dictionary of properties
            var parameters = _converter.ConvertToParameters(grainState, newETag);

            if (string.IsNullOrEmpty(currentETag))
            {
                // It's the first write, create the node with state and initial ETag
                await session.RunAsync($@"
                    MERGE (n:{grainType} {{ id: '{grainKey}' }})
                    ON CREATE SET n += $properties, n.{_options.ETagPropertyName} = $eTag", parameters);
            }
            else
            {
                // Update the state and ETag for existing node
                // Notice we are matching the id and the ETag for idempotency purposes
                var result = await session.RunAsync($@"
                    MATCH (n:{grainType} {{ id: '{grainKey}', {_options.ETagPropertyName}: '{currentETag}' }})
                    SET n += $properties, n.{_options.ETagPropertyName} = $eTag
                    RETURN n", parameters);
            }

            grainState.ETag = newETag;
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
}
