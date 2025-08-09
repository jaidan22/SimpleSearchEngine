using System.Text;
using System.Xml;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: WikiExtractor <path_to_enwiki-latest-pages-articles.xml> <output_directory>");
            return;
        }

        string inputFile = args[0];
        string outputDir = Path.Combine(args[1], "pages") ;

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File not found - {inputFile}");
            return;
        }

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        Console.WriteLine($"Extracting from: {inputFile}");
        Console.WriteLine($"Saving to: {outputDir}");

        using var fileStream = File.OpenRead(inputFile);

        ParseWikipediaXml(fileStream, outputDir);

        Console.WriteLine("✅ Extraction complete.");
    }

    static void ParseWikipediaXml(Stream xmlStream, string outputDir)
    {
        using var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings
        {
            IgnoreWhitespace = true
        });

        string title = null;
        string text = null;
        int count = 0;

        while (xmlReader.Read())
        {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                if (xmlReader.Name == "title")
                    title = xmlReader.ReadElementContentAsString();
                else if (xmlReader.Name == "text")
                    text = xmlReader.ReadElementContentAsString();
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(text))
            {
                if (!title.Contains(":")) // skip non-articles
                {
                    string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
                    File.WriteAllText(Path.Combine(outputDir, safeTitle + ".txt"), text, Encoding.UTF8);

                    count++;
                    if (count % 1000 == 0)
                        Console.WriteLine($"Extracted {count} articles...");
                }

                title = null;
                text = null;
            }
        }
    }
}
