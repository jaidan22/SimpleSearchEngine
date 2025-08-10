## Search Engine

Built on wikipedia dump https://dumps.wikimedia.org/enwiki/20250501/enwiki-20250501-pages-articles-multistream1.xml-p1p41242.bz2
* Finds documents that contain all tokens in the query (intersection logic)
* Matches phrases using positional indexing

### Index
* Index maps each token to:
  * Document Name
  * Line Number
  * Token Position (0-based in line)
```csharp
"wiktionary": [
    {
      "DocId": 4,
      "LineNo": 1,
      "Position": 0
    },
    {
      "DocId": 5,
      "LineNo": 1,
      "Position": 0
    },
    .
    .
]
```

### Query Processing
* Finds intersection of postings lists for all query terms
* Uses token positions to ensure terms appear in sequence and next to each other


