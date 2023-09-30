namespace Orleans.Providers.Neo4j.Common;

public interface IConvertableState<TConvertTo>
{
    TConvertTo ConvertTo();
    void ConvertFrom(TConvertTo convertFrom);
}
