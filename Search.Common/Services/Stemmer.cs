using System.Text.RegularExpressions;

namespace Search.Common.Services
{
    public class Stemmer
    {
        public string Stem(string word)
        {
            // Very basic stemming — replace with a full Porter stemmer for production
            word = Regex.Replace(word, "(ing|ed|ly)$", "");
            return word;
        }
    }
}
