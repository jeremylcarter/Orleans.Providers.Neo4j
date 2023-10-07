namespace Orleans.Providers.Neo4j.State
{
    public interface INodeState
    {
        string ElementId { get; }
        List<string> Labels { get; }
        Dictionary<string, object> Properties { get; }
        List<NodeRelationship> Relationships { get; }
    }

    public class NodeRelationship
    {
        string Type { get; }
        string SourceId { get; }
        string TargetId { get; }
    }
}
