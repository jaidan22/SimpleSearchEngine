using Test.Indexer.Utils;

namespace Test.Indexer
{
    public class InvertedIndex
    {
        private Dictionary<string, List<Posting>> index = new();
        private Dictionary<int, Document> documents = new();

        public void AddDocument(Document doc)
        {
            documents[doc.Id] = doc;
            var tokens = TextProcessor.Tokenize(doc.Text);

            var termFreq = new Dictionary<string, int>();

            foreach (var token in tokens)
            {
                if (!termFreq.ContainsKey(token))
                    termFreq[token] = 0;
                termFreq[token]++;
            }

            foreach (var kvp in termFreq)
            {
                var term = kvp.Key;
                var freq = kvp.Value;

                if (!index.ContainsKey(term))
                    index[term] = new List<Posting>();

                index[term].Add(new Posting(doc.Id) { Frequency = freq });
            }
        }

        public List<(Document, int)> Search(string query)
        {
            var terms = TextProcessor.Tokenize(query);
            var docScores = new Dictionary<int, int>();

            foreach (var term in terms)
            {
                if (!index.ContainsKey(term))
                    continue;

                foreach (var posting in index[term])
                {
                    if (!docScores.ContainsKey(posting.DocId))
                        docScores[posting.DocId] = 0;

                    docScores[posting.DocId] += posting.Frequency;
                }
            }

            return docScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => (documents[kvp.Key], kvp.Value))
                .ToList();
        }
    }
}
