using System.IO.Compression;
using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using ManagedCode.Storage.Core.Models;
using Orleans.Concurrency;
using Orleans.Indexing.Abstractions;
using Directory = System.IO.Directory;

namespace Orleans.Indexing.Lucene.Services;

[Reentrant]
public class LuceneWithStorageIndexService : LuceneIndexService, IAsyncDisposable
{
    private readonly IStorage _storage;
    private readonly string _indexPath;
    private bool _isInitialized;

    public LuceneWithStorageIndexService(IStorage storage)
    {
        _storage = storage;
        _indexPath = Path.Combine(Path.GetTempPath(), "lucene");
    }

    public override async Task InitializeAsync()
    {
        if (_isInitialized) return;

        CreateFolders();
        await DownloadCacheAsync();
        InitWriters();
        InitSearcher();

        _isInitialized = true;
    }

    public async Task DownloadCacheAsync()
    {
        var blobs = _storage.GetBlobMetadataListAsync();

        await foreach (var blob in blobs)
        {
            var directoryName = Path.GetFileNameWithoutExtension(blob.Name);
            var directoryPath = Path.Combine(_indexPath, directoryName);

            var result = await _storage.DownloadAsync(blob.Name);

            if (result.IsSuccess)
            {
                ZipFile.ExtractToDirectory(result.Value!.FilePath, directoryPath);
                File.Delete(result.Value!.FilePath);
            }
        }
    }

    private async Task UploadFilesAsync(string directoryName)
    {
        var zipName = $"{directoryName}.zip";
        var path = Path.Combine(_indexPath, directoryName);
        var zipPath = Path.Combine(_indexPath, zipName);

        ZipFile.CreateFromDirectory(path, zipPath);

        var exists = await _storage.ExistsAsync(zipName);

        if (!exists.Value)
        {
            await _storage.DeleteAsync(zipName);
        }

        UploadOptions options = new()
        {
            FileName = zipName,
        };

        await _storage.UploadAsync(zipPath, options);

        File.Delete(zipPath);
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

    public async ValueTask DisposeAsync()
    {
        Analyzer.Dispose();

        foreach (var writer in Writers)
        {
            writer.Value.Flush(true, true);
            writer.Value.Dispose();
        }

        foreach (var tempDirectory in TempDirectories)
        {
            tempDirectory.Value.Dispose();
            await UploadFilesAsync(tempDirectory.Key);
        }
    }
}