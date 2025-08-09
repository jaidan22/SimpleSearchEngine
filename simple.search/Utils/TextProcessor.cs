using System.Text.RegularExpressions;

namespace Test.Indexer.Utils
{
    public static class TextProcessor
    {
        private static HashSet<string> Stopwords = new HashSet<string>
        {
            "the", "is", "in", "and", "of", "to", "a", "an" // Add more
        };

        public static List<string> Tokenize(string text)
        {
            return Regex.Split(text.ToLower(), @"\W+")
                .Where(token => !string.IsNullOrWhiteSpace(token) && !Stopwords.Contains(token))
                .ToList();
        }
    }
}
