namespace Orleans.Providers.Neo4j.Common
{
    public interface INodeState
    {
        string ElementId { get; }
        List<string> Labels { get; }
        Dictionary<string, object> Properties { get; }
    }
}
