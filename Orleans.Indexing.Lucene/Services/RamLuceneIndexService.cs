using System.IO.Compression;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using ManagedCode.Storage.Core;
using ManagedCode.Storage.Core.Models;
using Orleans.Concurrency;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Lucene.Options;
using Directory = System.IO.Directory;

namespace Orleans.Indexing.Lucene.Services;

[Reentrant]
public class RamLuceneIndexService : LuceneIndexService
{
    public RamLuceneIndexService()
    {
        CreateDirectories();
        InitWriters();
        InitSearcher();
    }

    public void CreateDirectories()
    {
        var grainTypes = GetEnumerableOfType<IndexGrain>();

        foreach (var grainType in grainTypes)
        {
            TempDirectories.Add(grainType.Name, new RAMDirectory());
        }
    }
}