namespace Orleans.Providers.Neo4j.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute() { }
        public PropertyAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
