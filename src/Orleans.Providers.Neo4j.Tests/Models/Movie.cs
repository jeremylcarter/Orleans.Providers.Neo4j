namespace Orleans.Providers.Neo4j.Tests.Models
{
    [Serializable]
    [GenerateSerializer]
    public class Movie
    {
        [Id(0)]
        public string Title { get; set; }
        [Id(1)]
        public int? Year { get; set; }
    }
}
