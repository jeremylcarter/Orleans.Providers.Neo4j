namespace Orleans.Providers.Neo4j.Tests.Models
{
    [Serializable]
    [GenerateSerializer]
    public class Actor
    {
        [Id(0)]
        public string Name { get; set; }
        [Id(1)]
        public DateTime? DateOfBirth { get; set; }
    }
}
