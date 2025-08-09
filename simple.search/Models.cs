namespace Test.Indexer
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class Posting
    {
        public int DocId { get; set; }
        public int Frequency { get; set; }

        public Posting(int docId)
        {
            DocId = docId;
            Frequency = 1;
        }
    }
}
