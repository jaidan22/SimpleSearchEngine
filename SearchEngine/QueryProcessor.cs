using Search.Common.Models;
using Search.Common.Services;
using ShellProgressBar;
using System.Text.Json;

namespace SearchEngine
{
    public class QueryProcessor
    {
        private readonly int _numShards;
        private readonly string _dataFolder;

        public QueryProcessor(string dataFolder)
        {
            _numShards = GetNumberOfShards(dataFolder);
            _dataFolder = dataFolder;
        }

        public List<QueryResult> Search(string query)
        {
            var tokenizer = new Tokenizer();

            var queryTokens = tokenizer.Tokenize(query);

            var results = new Dictionary<string, HashSet<int>>();

            using (var pbar = new ProgressBar(_numShards, "Searching in shards..."))
            {
                for (int shardId = 0; shardId < _numShards; shardId++)
                {
                    var shardedIndexPath = Path.Combine(_dataFolder, "index", $"inverted_index_shard{shardId}.json");
                    var shardedDocPath = Path.Combine(_dataFolder, "documents", $"documents_shard{shardId}.json");
                    if (!File.Exists(shardedIndexPath) || !File.Exists(shardedDocPath))
                        continue;

                    var index = LoadFile<InvertedIndex>(shardedIndexPath);
                    var document = LoadFile<DocumentIndex>(shardedDocPath);

                    foreach (var token in queryTokens)
                    {
                        if (index != null && index.ContainsKey(token))
                        {
                            foreach (var properties in index[token])
                            {
                                var key = document[properties.DocId];
                                if (!results.ContainsKey(key))
                                    results[key] = new HashSet<int>();
                                results[key].Add(properties.LineNo);
                            }
                        }
                    }
                    pbar.Tick($"Searched {document.Count} Documents, found {results.Count} matches");
                }
            }
            return results.Select(r => new QueryResult
            {
                DocName = Path.GetFileNameWithoutExtension(r.Key),
                FilePath = r.Key,
                Lines = r.Value.OrderBy(x => x).ToList()
            }).ToList();
        }

        private T LoadFile<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        private int GetNumberOfShards(string dataFolder)
        {
            return Directory.GetFiles(Path.Combine(dataFolder, "index")).Length;
        }
    }
}
