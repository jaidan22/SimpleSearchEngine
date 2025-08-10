using Search.Common.Models;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace Indexer.Services
{
    public class IndexBuilder
    {
        private readonly List<InvertedIndex> _invertedIndices;
        private readonly List<DocumentIndex> _documents;
        private readonly int _numShards;
        private readonly JsonSerializerOptions _jsonSerialiseOptions = new() { WriteIndented = true };

        public IndexBuilder(int numShards)
        {
            _numShards = numShards;
            _invertedIndices = new List<InvertedIndex>(
                Enumerable.Range(0, _numShards).Select(_ => new InvertedIndex())
            );
            _documents = new List<DocumentIndex>(Enumerable.Range(0, _numShards).Select(_ => new DocumentIndex()));
        }

        public void AddDocument(int docId, int shardIndex, string filePath)
            => _documents[shardIndex].TryAdd(docId, filePath);

        public void UpdateIndex(int docId, int shardIndex, int lineNo, List<string> tokens)
        {
            int pos = 0;
            foreach (var token in tokens.Distinct())
            {
                if (!_invertedIndices[shardIndex].ContainsKey(token))
                    _invertedIndices[shardIndex][token] = new List<IndexProperties>();
                _invertedIndices[shardIndex][token].Add(new IndexProperties { DocId = docId, LineNo = lineNo, Position = pos });
                pos++;
            }
        }

        public List<InvertedIndex> GetBatchIndex() => _invertedIndices;

        public void SaveIndex(string dataFolder)
        {
            int i = 0;
            Directory.CreateDirectory(Path.Combine(dataFolder, "index"));
            Console.Write("\n\r [-] Writing index files");
            foreach (var index in _invertedIndices)
            {
                string path = Path.Combine(dataFolder, "index", $"inverted_index_shard{i}.json");
                File.WriteAllText(path, JsonSerializer.Serialize(index, _jsonSerialiseOptions));
                index.Clear();
                i++;
            }
            Console.Write("\r ✔ Created Index files");
        }

        public void SaveDocument(string dataFolder)
        {
            int i = 0;
            Directory.CreateDirectory(Path.Combine(dataFolder, "documents"));
            Console.Write("\n\r [-] Writing Document files");
            foreach (var doc in _documents)
            {
                string path = Path.Combine(dataFolder, "documents", $"documents_shard{i}.json");
                File.WriteAllText(path, JsonSerializer.Serialize(doc, _jsonSerialiseOptions));
                doc.Clear();
                i++;
            }
            Console.Write("\r ✔ Created Document files \n");
        }
    }
}
