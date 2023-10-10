using Neo4j.Driver;

namespace Orleans.Providers.Neo4j.State
{
    public interface INodeState
    {
        string Label { get; set; }
        IDictionary<string, object> Properties { get; set; }

        T Get<T>(string name)
        {
            return Properties[name].As<T>();
        }
    }

    public class NodeState : INodeState
    {
        public string Label { get; set; }
        public IDictionary<string, object> Properties { get; set; }

        public NodeState() { }

        public NodeState(string label, IDictionary<string, object> properties)
        {
            Label = label;
            Properties = properties;
        }

        public NodeState(INode node)
        {
            Label = node.Labels[0];
            Properties = node.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
