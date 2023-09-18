using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    public class Neo4jGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly IDriver _driver;
        private readonly Neo4jGrainStorageOptions _options;

        public Neo4jGrainStorage(IOptions<Neo4jGrainStorageOptions> options)
        {
            _options = options.Value;
            _driver = GraphDatabase.Driver(_options.Uri, AuthTokens.Basic(_options.Username, _options.Password));
        }

        public async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession();

            var grainKey = grainId.Key.ToString();
            var result = await session.RunAsync($"MATCH (a:{grainType}) WHERE a.id = '{grainKey}' RETURN a.state AS state");

            var record = await result.SingleAsync();

            if (record != null)
            {
                var jsonState = record["state"].As<string>();

                var deserializedState = JsonSerializer.Deserialize<T>(jsonState);
                if (deserializedState != null)
                {
                    grainState.State = deserializedState;
                }
                grainState.RecordExists = true;
                grainState.ETag = record["etag"].As<string>();
            }
        }

        public async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession();

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
                    ON CREATE SET a.state = $state, a.eTag = '{newETag}'
                ", new { state = jsonState });
            }
            else
            {
                // Update the state and ETag for existing node
                var result = await session.RunAsync($@"
                    MATCH (a:{grainType} {{ id: '{grainKey}', eTag: '{currentETag}' }})
                    SET a.state = $state, a.eTag = '{newETag}'
                    RETURN a
                ", new { state = jsonState });
            }

            grainState.ETag = newETag;
        }

        public async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            using var session = _driver.AsyncSession();

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

    public class Neo4jGrainStorageOptions
    {
        public required string Uri { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}