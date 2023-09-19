namespace Orleans.Providers.Neo4j.Tests.Grains
{
    public class MovieGrain : Grain<MovieGrainState>, IMovieGrain
    {
        public ValueTask<string> GetTitle()
        {
            return ValueTask.FromResult(State.Title);
        }

        public Task SetTitle(string title)
        {
            State.Title = title;
            return WriteStateAsync();
        }
    }

    [Serializable]
    public class MovieGrainState
    {
        [Property("title")]
        public string Title { get; set; }
    }

    public interface IMovieGrain : IGrainWithIntegerKey
    {
        Task SetTitle(string title);
        ValueTask<string> GetTitle();
    }
}
