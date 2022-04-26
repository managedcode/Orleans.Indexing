using Lucene.Net.Store;
using Orleans.Indexing.Abstractions;
using Directory = System.IO.Directory;

namespace Orleans.Indexing.Lucene.Services;

public class FSLuceneIndexService : LuceneIndexService
{
    private readonly string _indexPath;

    public FSLuceneIndexService(string? indexPath)
    {
        _indexPath = indexPath ?? Path.Combine(Path.GetTempPath(), "lucene");

        CreateFolders();
        InitWriters();
        InitSearcher();
    }

    public void CreateFolders()
    {
        if (Directory.Exists(_indexPath))
        {
            Directory.Delete(_indexPath, true);
        }

        var grainTypes = GetEnumerableOfType<IndexGrain>();

        foreach (var grainType in grainTypes)
        {
            var directoryPath = Path.Combine(_indexPath, grainType.Name);

            Directory.CreateDirectory(directoryPath);

            var directory = FSDirectory.Open(directoryPath);
            TempDirectories.Add(grainType.Name, directory);
        }
    }
}