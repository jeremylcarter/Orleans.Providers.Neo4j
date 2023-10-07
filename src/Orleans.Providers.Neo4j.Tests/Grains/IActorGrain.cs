namespace Orleans.Providers.Neo4j.Tests.Grains;

public interface IActorGrain : IGrainWithStringKey
{
    Task SetName(string name);
    ValueTask<string> GetName();
    Task SetDateOfBirth(DateTime dateTime);
    ValueTask<DateTime?> GetDateOfBirth();
    Task AddMovie(string movieId);
    Task<List<string>> GetMovies();
}
