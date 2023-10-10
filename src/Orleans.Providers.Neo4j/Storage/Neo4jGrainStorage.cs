using Orleans.Providers.Neo4j.Annotations;
using Orleans.Runtime;
using Orleans.Storage;
using System.Reflection;

namespace Orleans.Providers.Neo4j.Storage
{
    internal class Neo4jSimpleGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly INeo4jGrainStorageKeyGenerator _generator;
        private readonly string _storageName;
        private readonly Neo4jGrainStorageOptions _options;
        private readonly INeo4jGrainStorageClient _client;
        private readonly Dictionary<Type, string> _grainTypes = new Dictionary<Type, string>();

        public Neo4jSimpleGrainStorage(string storageName, Neo4jGrainStorageOptions options)
        {
            _storageName = storageName;
            _options = options;
            _generator = options.KeyGenerator ?? new Neo4jGrainStorageKeyGenerator();
            _client = options.StorageClient ?? new Neo4jGrainStorageClient(options);
        }

        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = GetOrGenerateType(typeof(T), grainId);
            await _client.ReadStateAsync(grainType, grainKey, grainState);
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = GetOrGenerateType(typeof(T), grainId);
            await _client.WriteStateAsync(grainType, grainKey, grainState);
        }

        public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var grainKey = _generator.GenerateKey(grainId);
            var grainType = GetOrGenerateType(typeof(T), grainId);
            await _client.ClearStateAsync(grainType, grainKey, grainState);
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<Neo4jSimpleGrainStorage>(), ServiceLifecycleStage.ApplicationServices, OnStart, OnStop);
        }

        private string GetOrGenerateType(Type stateType, GrainId grainId)
        {
            if (_grainTypes.TryGetValue(stateType, out var grainType))
            {
                return grainType;
            }

            // Does the type have an annotation?
            var nodeLabel = stateType.GetCustomAttribute<NodeLabelAttribute>();
            if (nodeLabel != null)
            {
                _grainTypes.Add(stateType, nodeLabel.Label);
                return nodeLabel.Label;
            }

            // Is there a converter for this type? Does it have an annotation?
            if (_options.StateConverters != null &&
                _options.StateConverters.TryGetValue(stateType, out var converter))
            {
                var converterType = converter.GetType();
                nodeLabel = converterType.GetCustomAttribute<NodeLabelAttribute>();
                if (nodeLabel != null)
                {
                    _grainTypes.Add(stateType, nodeLabel.Label);
                    return nodeLabel.Label;
                }
            }

            // Fallback to the generator which uses the Grain's runtime type.
            var generatedType = _generator.GenerateType(grainId);
            if (string.IsNullOrEmpty(grainType))
            {
                return grainId.Type.ToString();
            }

            return generatedType;
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