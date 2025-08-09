using Test.Indexer;

var index = new InvertedIndex();

// Sample documents
index.AddDocument(new Document { Id = 1, Title = "Python", Text = "Python is a programming language. Python is simple." });
index.AddDocument(new Document { Id = 2, Title = "C#", Text = "C# is a language developed by Microsoft. Not very similar to Python" });
index.AddDocument(new Document { Id = 3, Title = "Dotnet", Text = ".NET is a developer platform for building apps." });

Console.WriteLine("Enter your search query:");
var query = Console.ReadLine();

var results = index.Search(query);
foreach (var (doc, score) in results)
{
    Console.WriteLine($"[Score: {score}] {doc.Title} - {doc.Text}");
}
