using Indexer.Services;
using ShellProgressBar;
using System.Text.Json;

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

        const int BATCH_SIZE = 5000;
        int batchCounter = 0;


        var tokenizer = new Tokenizer();
        var indexBuilder = new IndexBuilder();

        var options = new ProgressBarOptions
        {
            ProgressCharacter = '=',
            ProgressBarOnBottom = true
        };

        var files = Directory.GetFiles(Path.Combine(dataFolder, "pages"), "*.txt");
        int docId = 1;
        using (var pbar = new ProgressBar(files.Length, "Building inverted index...", options))
        {
            foreach (var file in files)
            {
                indexBuilder.AddDocument(docId, file);
                var lines = File.ReadAllLines(file);
                int lineNo = 1;
                foreach (var line in lines)
                {
                    var tokens = tokenizer.Tokenize(line);
                    indexBuilder.UpdateIndex(docId, lineNo, tokens);
                    lineNo++;
                }

                if (docId % BATCH_SIZE == 0)
                {
                    indexBuilder.SavePartialIndex(batchCounter++, dataFolder);
                }

                docId++;
                pbar.Tick($" ({docId}/{files.Length}) :: Indexed {Path.GetFileName(file)}");
            }
        }

        if(indexBuilder.GetBatchIndex().Count > 0)
            indexBuilder.SavePartialIndex(batchCounter, dataFolder);

        var documents = indexBuilder.GetDocuments();
        File.WriteAllText(Path.Combine(dataFolder, "documents.json"),
            JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true }));

        documents.Clear();

        indexBuilder.MergePartialIndexes(dataFolder);

        Console.WriteLine($"Indexing complete. {documents.Count} lines indexed.");
    }
}
