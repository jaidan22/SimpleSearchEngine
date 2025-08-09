using System.Text.RegularExpressions;

namespace Indexer.Services
{
    public class Tokenizer
    {
        private readonly Stemmer _stemmer = new Stemmer();

        public List<string> Tokenize(string text)
        {
            var tokens = Regex.Matches(text.ToLower(), @"\b[a-z]{2,}\b")
                              .Select(m => m.Value)
                              .Where(t => !StopWords.Words.Contains(t))
                              .Select(t => _stemmer.Stem(t))
                              .ToList();
            return tokens;
        }
    }
}
