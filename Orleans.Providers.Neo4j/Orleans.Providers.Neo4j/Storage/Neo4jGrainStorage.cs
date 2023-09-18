using Neo4j.Driver;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly IDriver _driver;
        private readonly string _storageName;
        private readonly Neo4jGrainStorageOptions _options;
        private const string _fieldState = "state";
        private const string _fieldEtag = "eTag";

        public Neo4jGrainStorage(string storageName, Neo4jGrainStorageOptions options)
        {
            _storageName = storageName;
            _options = options;
            _driver = GraphDatabase.Driver(options.Uri, AuthTokens.Basic(options.Username, options.Password));
        }

        public async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var grainKey = grainId.Key.ToString();
            var result = await session.RunAsync($"MATCH (a:{grainType}) WHERE a.id = '{grainKey}' RETURN a.{_fieldState} AS state");

            // Get the first record (or null)
            var cursor = await result.FetchAsync();

            if (cursor && result.Current != null)
            {
                var jsonState = result.Current[_fieldState].As<string>();

                var deserializedState = JsonSerializer.Deserialize<T>(jsonState);
                if (deserializedState != null)
                {
                    grainState.State = deserializedState;
                }
                grainState.RecordExists = true;
                grainState.ETag = result.Current[_fieldEtag].As<string>();
            }
        }

        public async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var grainKey = grainId.Key.ToString();
            var jsonState = JsonSerializer.Serialize(grainState.State);

            var currentETag = grainState.ETag;
            var newETag = Guid.NewGuid().ToString();

            grainState.RecordExists = true;

            if (string.IsNullOrEmpty(currentETag))
            {
                // It's the first write, create the node with state and initial ETag
                await session.RunAsync($@"
                    MERGE (a:{grainType} {{ id: '{grainKey}' }})
                    ON CREATE SET a.{_fieldState} = $state, a.{_fieldEtag} = '{newETag}'
                ", new { state = jsonState });
            }
            else
            {
                // Update the state and ETag for existing node
                var result = await session.RunAsync($@"
                    MATCH (a:{grainType} {{ id: '{grainKey}', {_fieldEtag}: '{currentETag}' }})
                    SET a.{_fieldState} = $state, a.eTag = '{newETag}'
                    RETURN a
                ", new { state = jsonState });
            }

            grainState.ETag = newETag;
        }

        public async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));

            var grainKey = grainId.Key;
            await session.RunAsync($"MATCH (a:{grainType} {{ id: '{grainKey}' }}) DELETE a RETURN a");

            grainState.RecordExists = false;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<Neo4jGrainStorage>(), ServiceLifecycleStage.ApplicationServices, OnStart, OnStop);
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