namespace Orleans.Providers.Neo4j.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NodeIdAttribute : Attribute
    {
        public NodeIdAttribute(string property = "id")
        {
            Property = property;
        }
        public string Property { get; }
    }
}