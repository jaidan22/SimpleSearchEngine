namespace Search.Common.Models
{
    public class InvertedIndex
    {
        public Dictionary<string, List<IndexProperties>> Index { get; set; } = new();
    }
}
