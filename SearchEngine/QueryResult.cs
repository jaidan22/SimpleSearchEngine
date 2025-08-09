namespace SearchEngine
{
    public class QueryResult
    {
        public string DocName { get; set; }
        public string FilePath { get; set; }
        public List<int> Lines { get; set; }
    }
}
