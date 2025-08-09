using Search.Common.Models;
using System.Text.Json;

namespace Indexer.Services
{
    public class IndexBuilder
    {
        private readonly Dictionary<string, List<IndexProperties>> _batchInvertedIndex = new();
        private readonly List<Document> _documents = new();

        public void AddDocument(int docId, string filePath)
            => _documents.Add(new Document { Id = docId, FilePath = filePath });

        public void UpdateIndex(int docId, int lineNo, List<string> tokens)
        {
            foreach (var token in tokens.Distinct())
            {
                if (!_batchInvertedIndex.ContainsKey(token))
                    _batchInvertedIndex[token] = new List<IndexProperties>();
                _batchInvertedIndex[token].Add(new IndexProperties { DocId = docId, LineNo = lineNo });
            }
        }

        public List<Document> GetDocuments() => _documents;

        public Dictionary<string, List<IndexProperties>> GetBatchIndex() => _batchInvertedIndex;

        public void SavePartialIndex(int batchNumber, string dataFolder)
        {
            string path = Path.Combine(dataFolder, "partial_index", $"inverted_index_part_{batchNumber}.json");
            File.WriteAllText(path, JsonSerializer.Serialize(_batchInvertedIndex));
            _batchInvertedIndex.Clear();
        }

        public void MergePartialIndexes(string folder)
        {
            var finalIndex = new Dictionary<string, List<IndexProperties>>();

            foreach (var file in Directory.GetFiles(Path.Combine(folder, "partial_index"), "inverted_index_part_*.json"))
            {
                var partial = JsonSerializer.Deserialize<Dictionary<string, List<IndexProperties>>>(File.ReadAllText(file));
                foreach (var kv in partial)
                {
                    if (!finalIndex.ContainsKey(kv.Key))
                        finalIndex[kv.Key] = new List<IndexProperties>();
                    finalIndex[kv.Key].AddRange(kv.Value);
                }
            }

            File.WriteAllText(Path.Combine(folder, "inverted_index.json"),
                JsonSerializer.Serialize(finalIndex, new JsonSerializerOptions { WriteIndented = true }));
        }

    }
}
