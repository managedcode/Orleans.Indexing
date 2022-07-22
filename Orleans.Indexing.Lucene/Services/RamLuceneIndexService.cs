using Lucene.Net.Store;
using Orleans.Concurrency;
using Orleans.Indexing.Abstractions;

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

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
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