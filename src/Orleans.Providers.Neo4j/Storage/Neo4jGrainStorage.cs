using Neo4j.Driver;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    internal class Neo4jSimpleGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly IDriver _driver;
        private readonly INeo4JGrainStorageGenerator _generator;
        private readonly string _storageName;
        private readonly Neo4jGrainStorageOptions _options;
        private const string _fieldState = "state";
        private const string _fieldEtag = "eTag";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Neo4jSimpleGrainStorage(string storageName, Neo4jGrainStorageOptions options)
        {
            _storageName = storageName;
            _options = options;
            _driver = GraphDatabase.Driver(options.Uri, AuthTokens.Basic(options.Username, options.Password));
            _generator = options.Generator ?? new Neo4jGrainStorageGenerator();
            if (options.PropertyNameStyle == PropertyNameStyle.None)
            {
                _jsonOptions.PropertyNamingPolicy = null;
            }
        }

        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);
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
                    var jsonState = node[_fieldState].As<string>();

                    var deserializedState = JsonSerializer.Deserialize<T>(jsonState, _jsonOptions);
                    if (deserializedState != null)
                    {
                        grainState.State = deserializedState;
                    }
                    grainState.RecordExists = true;
                    grainState.ETag = node[_fieldEtag].As<string>();
                }
            }
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);

            var jsonState = JsonSerializer.Serialize(grainState.State, _jsonOptions);

            var currentETag = grainState.ETag;
            var newETag = Neo4jETagGenerator.Generate();

            grainState.RecordExists = true;

            if (string.IsNullOrEmpty(currentETag))
            {
                var s = $@"
                    MERGE (a:{grainType} {{ id: '{grainKey}' }})
                    ON CREATE SET a.{_fieldState} = $state, a.{_fieldEtag} = $eTag'
                ";
                // It's the first write, create the node with state and initial ETag
                await session.RunAsync($@"
                    MERGE (a:{grainType} {{ id: '{grainKey}' }})
                    ON CREATE SET a.{_fieldState} = $state, a.{_fieldEtag} = $eTag
                ", new { state = jsonState, eTag = newETag });
            }
            else
            {
                // Update the state and ETag for existing node
                // Notice we are matching the id and the ETag for idempotency purposes
                var result = await session.RunAsync($@"
                    MATCH (a:{grainType} {{ id: '{grainKey}', {_fieldEtag}: '{currentETag}' }})
                    SET a.{_fieldState} = $state, a.{_fieldEtag} = $eTag
                    RETURN a
                ", new { state = jsonState, eTag = newETag });
            }

            grainState.ETag = newETag;
        }

        public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);

            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
            await session.RunAsync($"MATCH (a:{grainType} {{ id: '{grainKey}' }}) DELETE a RETURN a");
            grainState.RecordExists = false;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<Neo4jSimpleGrainStorage>(), ServiceLifecycleStage.ApplicationServices, OnStart, OnStop);
        }

        private Task OnStart(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private async Task OnStop(CancellationToken ct)
        {
            if (_driver != null)
            {
                await _driver.DisposeAsync();
            }
        }
    }
}