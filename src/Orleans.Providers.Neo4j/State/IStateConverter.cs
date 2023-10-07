namespace Orleans.Providers.Neo4j.State;

public interface IStateConverter<TConvertFrom, TConvertTo>
{
    TConvertTo ConvertTo(TConvertFrom from);
    TConvertFrom ConvertFrom(TConvertTo convertFrom);
}

public interface INeo4jStateConverter { }
public interface INeo4jStateConverter<TState> :
    INeo4jStateConverter, IStateConverter<TState, IReadOnlyDictionary<string, object>>
{
}