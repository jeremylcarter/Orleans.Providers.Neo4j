using Neo4j.Driver;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

namespace Orleans.Providers.Neo4j.Storage
{
    internal class Neo4jSimpleGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly INeo4JGrainStorageGenerator _generator;
        private readonly string _storageName;
        private readonly Neo4jGrainStorageOptions _options;
        private readonly Neo4jGrainStorageClient _client;

        public Neo4jSimpleGrainStorage(string storageName, Neo4jGrainStorageOptions options)
        {
            _storageName = storageName;
            _options = options;
            _generator = options.Generator ?? new Neo4jGrainStorageGenerator();
            _client = new Neo4jGrainStorageClient(options);
        }

        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);
            await _client.ReadStateAsync(grainType, grainKey, grainState);
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);
            await _client.WriteStateAsync(grainType, grainKey, grainState);
        }

        public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = _generator.GenerateType(grainId);
            await _client.ClearStateAsync(grainType, grainKey, grainState);
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
            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}