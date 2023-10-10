using Orleans.Providers.Neo4j.Annotations;

namespace Orleans.Providers.Neo4j.Tests.Grains;

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

    public ValueTask<int?> GetYear()
    {
        return ValueTask.FromResult(State.Year);
    }

    public Task SetYear(int year)
    {
        State.Year = year;
        return WriteStateAsync();
    }
}

[Serializable]
[NodeLabel("Movie")]
public class MovieGrainState
{
    public string Title { get; set; }
    public int? Year { get; set; }
}

public interface IMovieGrain : IGrainWithStringKey
{
    Task SetTitle(string title);
    ValueTask<string> GetTitle();
    Task SetYear(int year);
    ValueTask<int?> GetYear();
}
