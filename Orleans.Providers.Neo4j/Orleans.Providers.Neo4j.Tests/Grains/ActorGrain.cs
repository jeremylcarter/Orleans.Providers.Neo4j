using System.ComponentModel.DataAnnotations.Schema;

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
        [Property("name")]
        public string Name { get; set; }
        [NotMapped]
        public bool IgnoreThis { get; set; }
    }

    public interface IActorGrain : IGrainWithStringKey
    {
        Task SetName(string name);
        ValueTask<string> GetName();
    }
}
