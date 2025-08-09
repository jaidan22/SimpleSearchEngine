using SearchEngine;
using SearchEngine.Helper;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Indexer <path-to-data-folder>");
            return;
        }

        string dataFolder = args[0];

        var _queryProcessor = new QueryProcessor(dataFolder);
        while (true)
        {
            Console.Write("Search Term : ");
            string searchTerm = Console.ReadLine();
            var results = _queryProcessor.Search(searchTerm);
            results.Print();
        }
    }
}