namespace Orleans.Providers.Neo4j.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeLabelAttribute : Attribute
    {
        public NodeLabelAttribute(string label)
        {
            Label = label;
        }
        public string Label { get; }
    }
}