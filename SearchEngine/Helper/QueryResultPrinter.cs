namespace SearchEngine.Helper
{
    public static class QueryResultPrinter
    {
        public static void Print(this List<QueryResult> results)
        {
            if (results == null || results.Count == 0)
            {
                Console.WriteLine("No results found.");
                return;
            }

            foreach (var result in results)
            {
                Console.WriteLine($"📄 Document: {result.DocName}");
                Console.WriteLine($"   📂 Path: {result.FilePath}");

                if (result.Lines != null && result.Lines.Any())
                    Console.WriteLine($"   📌 Lines: {string.Join(", ", result.Lines)}");
                else
                    Console.WriteLine("   📌 Lines: None");

                Console.WriteLine(new string('-', 50));
            }
        }
    }
}
