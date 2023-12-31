﻿using Neo4j.Driver;
using Orleans.Providers.Neo4j.Annotations;
using Orleans.Providers.Neo4j.State;

namespace Orleans.Providers.Neo4j.Tests.Grains;

public class ActorGrain : Grain<ActorGrainState>, IActorGrain
{
    public Task SetName(string name)
    {
        State.Name = name;
        return WriteStateAsync();
    }
    public ValueTask<string> GetName()
    {
        return ValueTask.FromResult(State.Name);
    }

    public Task SetDateOfBirth(DateTime dateTime)
    {
        State.DateOfBirth = dateTime;
        return WriteStateAsync();
    }

    public ValueTask<DateTime?> GetDateOfBirth()
    {
        return ValueTask.FromResult(State.DateOfBirth);
    }

    public Task AddMovie(string movieId)
    {
        State.Movies ??= new HashSet<string>();
        State.Movies.Add(movieId);
        return WriteStateAsync();
    }

    public Task<List<string>> GetMovies()
    {
        return Task.FromResult(State.Movies.ToList());
    }
}

[Serializable]
public class ActorGrainState
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IgnoreThis { get; set; }
    public HashSet<string> Movies { get; set; }
}

[NodeLabel("Actor")]
public class ActorGrainStateConverter : INeo4jStateConverter<ActorGrainState>
{
    public ActorGrainState ConvertFrom(IReadOnlyDictionary<string, object> convertFrom)
    {
        return new ActorGrainState()
        {
            Name = convertFrom["name"].As<string>(),
            DateOfBirth = convertFrom["dateOfBirth"].As<DateTime?>(),
        };
    }

    public IReadOnlyDictionary<string, object> ConvertTo(ActorGrainState from)
    {
        return new Dictionary<string, object>
       {
            { "name", from.Name },
            { "dateOfBirth", from.DateOfBirth },
       };
    }
}
