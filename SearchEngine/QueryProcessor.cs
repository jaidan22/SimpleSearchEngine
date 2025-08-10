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
                var docCount = 0;
                for (int shardId = 0; shardId < _numShards; shardId++)
                {
                    var shardedIndexPath = Path.Combine(_dataFolder, "index", $"inverted_index_shard{shardId}.json");
                    var shardedDocPath = Path.Combine(_dataFolder, "documents", $"documents_shard{shardId}.json");
                    if (!File.Exists(shardedIndexPath) || !File.Exists(shardedDocPath))
                        continue;

                    var index = LoadFile<InvertedIndex>(shardedIndexPath);
                    var document = LoadFile<DocumentIndex>(shardedDocPath);

                    docCount += document.Count;

                    var postingsLists = queryTokens
                        .Select(t => index.ContainsKey(t) ? index[t] : new List<IndexProperties>())
                        .ToList();

                    if (postingsLists.Any(p => p.Count == 0))
                        break; // at least one token not found

                    var firstTokenGroups = postingsLists[0]
                        .GroupBy(p => (p.DocId, p.LineNo))
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Position)
                        .ToHashSet());

                    foreach (var docLine in firstTokenGroups.Keys)
                    {
                        var possiblePositions = firstTokenGroups[docLine];

                        foreach (var startPos in possiblePositions)
                        {
                            bool match = true;
                            for (int i = 1; i < queryTokens.Count; i++)
                            {
                                var nextTokenPositions = postingsLists[i]
                                    .Where(p => p.DocId == docLine.DocId && p.LineNo == docLine.LineNo)
                                    .Select(p => p.Position)
                                    .ToHashSet();

                                // PHRASE MATCHING
                                if (!nextTokenPositions.Contains(startPos + i))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                var key = document[docLine.DocId];
                                if (!results.ContainsKey(key))
                                    results[key] = new HashSet<int>();
                                results[key].Add(docLine.LineNo);
                                break;
                            }
                        }
                    }

                    pbar.Tick($"Searched {docCount} Documents, found {results.Count} matches");
                }
            }
            return results.Select(r => new QueryResult
            {
                DocName = Path.GetFileNameWithoutExtension(r.Key),
                FilePath = r.Key,
                Lines = r.Value.OrderBy(x => x).ToList()
            }).OrderBy(x => x.Lines.Count).ToList();
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
