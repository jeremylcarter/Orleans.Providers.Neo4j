# Orleans.Providers.Neo4j

[![NuGet version](https://badge.fury.io/nu/Orleans.Providers.Neo4j.svg)](https://badge.fury.io/nu/Orleans.Providers.Neo4j)

A Neo4j implementation of the Orleans Providers. 

This includes:

 * Membership (Planned)
 * Reminders (Planned)
 * Storage (Beta)

## Storage

There are two storage options included in this provider.
The first is simple an unobtrusive, 
the entire state object is serialized as JSON and stored inside the `state` property of the node.

For example:

```csharp
public class MyState
{
	public string Name { get; set; }
	public int Age { get; set; }
}
```

Will be stored as a JSON string on the node as:

```
"state" : "{ \"Name\": \"John\", \"Age\": 42 }",
"eTag" : "FC14312382"
```


The second is complex, but allows for more querying the state. 
Every state object is flattened out into a dictionary by creating a `INeo4jStateConverter` for the specified state type.

For example:

```csharp
public class MyState
{
	public string Name { get; set; }
	public int Age { get; set; }
}

public class MyStateConverter : INeo4jStateConverter<MyState>
{
	public Dictionary<string, object> ConvertFrom(MyState state)
	{
		return new Dictionary<string, object>
		{
			{ "Name", state.Name },
			{ "Age", state.Age }
		};
	}

	public MyState ConvertTo(Dictionary<string, object> dictionary)
	{
		return new MyState
		{
			Name = dictionary["Name"].ToString(),
			Age = int.Parse(dictionary["Age"].ToString())
		};
	}
}
```

This will be stored on the node as:

```
"name" : "John",
"age" : 42,
"eTag" : "FC14312382"
```

## Options and Configuration

The options class is `Neo4jGrainStorageOptions`.

It lists the following options:

 * `Uri` - The URI for the `neo4j` host.
 * `Database` - The database to use. Defaults to `neo4j`.
 * `Username` - The username to use when connecting to the database. Defaults to `neo4j`.
 * `Password` - The password to use when connecting to the database. Defaults to `neo4j`.
 * `StatePropertyName` - The name of the property to store the state on. Defaults to `state`.
 * `ETagPropertyName` - The name of the property to store the eTag on. Defaults to `eTag`.
 * `JsonSerializerOptions` - Override the default `JsonSerializerOptions` options.
 * `KeyGenerator` - Override how Keys/ID's are generated.
 * `ETagGenerator` - Override how eTags are generated.
 * `StorageClient` - Override the Storage Client with your own implementation.
 * `StateConverters` - Manually wire up custom state converters. By default they are discovered at runtime.

Setup:

Simply add `AddNeo4jGrainStorage` or `AddNeo4jGrainStorageAsDefault` on your builder configuration.

```csharp
builder.AddNeo4jGrainStorageAsDefault(storageOptions =>
{
    storageOptions.Uri = "127.0.0.1";
    storageOptions.Database = "neo4j";
    storageOptions.Username = "neo4j";
    storageOptions.Password = "Password1";
})
```
