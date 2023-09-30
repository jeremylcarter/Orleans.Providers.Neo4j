namespace Orleans.Providers.Neo4j.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var item in other)
            {
                source[item.Key] = item.Value;
            }
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IReadOnlyDictionary<TKey, TValue> other)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var item in other)
            {
                source[item.Key] = item.Value;
            }
        }
    }
}
