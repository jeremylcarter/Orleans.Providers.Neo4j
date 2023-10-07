using Neo4j.Driver;
using Orleans.Providers.Neo4j.State;

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
    public class ActorGrainState
    {
        public string Name { get; set; }
        public bool IgnoreThis { get; set; }
    }

    public class ActorGrainStateConverter : INeo4jStateConverter<ActorGrainState>
    {
        public ActorGrainState ConvertFrom(IReadOnlyDictionary<string, object> convertFrom)
        {
            return new ActorGrainState()
            {
                Name = convertFrom["name"].As<string>(),
            };
        }

        public IReadOnlyDictionary<string, object> ConvertTo(ActorGrainState from)
        {
            return new Dictionary<string, object>
           {
                { "name", from.Name }
           };
        }
    }
}

public interface IActorGrain : IGrainWithStringKey
{
    Task SetName(string name);
    ValueTask<string> GetName();
}
