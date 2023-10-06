using Neo4j.Driver;
using Orleans.Providers.Neo4j.Common;

namespace Orleans.Providers.Neo4j.Tests.Grains
{
    public class ActorGrain : Grain<ActorGrainState>, IActorGrain
    {
        public ValueTask<string> GetName()
        {
            return ValueTask.FromResult(State.Name);
        }

        public Task SetName(string name)
        {
            State.Name = name;
            return WriteStateAsync();
        }
    }

    [Serializable]
    public class ActorGrainState : IConvertableState<IReadOnlyDictionary<string, object>>
    {
        public string Name { get; set; }
        public bool IgnoreThis { get; set; }

        public void ConvertFrom(IReadOnlyDictionary<string, object> from)
        {
            Name = from["name"].As<string>();
        }

        public IReadOnlyDictionary<string, object> ConvertTo()
        {
            return new Dictionary<string, object>
            {
                { "name", Name }
            };
        }
    }

    public interface IActorGrain : IGrainWithStringKey
    {
        Task SetName(string name);
        ValueTask<string> GetName();
    }
}
