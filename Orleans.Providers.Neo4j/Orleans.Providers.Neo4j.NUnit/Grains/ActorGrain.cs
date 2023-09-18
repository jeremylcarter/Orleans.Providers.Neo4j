namespace Orleans.Providers.Neo4j.NUnit.Grains
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

    public class ActorGrainState
    {
        public string Name { get; set; }
    }

    public interface IActorGrain : IGrainWithStringKey
    {
        Task SetName(string name);
        ValueTask<string> GetName();
    }
}
