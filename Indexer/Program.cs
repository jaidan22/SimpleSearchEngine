using Indexer.Services;
using Search.Common.Services;
using ShellProgressBar;

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

        const int SHARD_SIZE = 6;

        var tokenizer = new Tokenizer();
        var indexBuilder = new IndexBuilder(SHARD_SIZE);

        var options = new ProgressBarOptions
        {
            ProgressBarOnBottom = true
        };

        var files = Directory.GetFiles(Path.Combine(dataFolder, "pages"), "*.txt");
        int docId = 1;
        using (var pbar = new ProgressBar(files.Length, "Building inverted index...", options))
        {
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                int lineNo = 1;
                int shardIndex = GetShardIndex(Path.GetFileNameWithoutExtension(file), SHARD_SIZE);

                indexBuilder.AddDocument(docId, shardIndex, file);

                foreach (var line in lines)
                {
                    var tokens = tokenizer.Tokenize(line);
                    indexBuilder.UpdateIndex(docId, shardIndex, lineNo, tokens);
                    lineNo++;
                }

                docId++;
                pbar.Tick($" ({docId}/{files.Length}) :: Indexed {Path.GetFileName(file)}");
            }
        }

        indexBuilder.SaveIndex(dataFolder);
        indexBuilder.SaveDocument(dataFolder);

        Console.WriteLine($"✅ Indexing completed");
    }

    private static int GetShardIndex(string docName, int shardSize)
        => Math.Abs(docName.GetHashCode()) % shardSize;
}
