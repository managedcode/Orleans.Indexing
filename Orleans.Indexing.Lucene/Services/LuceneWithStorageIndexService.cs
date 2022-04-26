using System.IO.Compression;
using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using ManagedCode.Storage.Core.Models;
using Orleans.Concurrency;
using Orleans.Indexing.Abstractions;
using Directory = System.IO.Directory;

namespace Orleans.Indexing.Lucene.Services;

[Reentrant]
public class LuceneWithStorageIndexService : LuceneIndexService
{
    private readonly IStorage _storage;
    private readonly string _indexPath;

    public LuceneWithStorageIndexService(IStorage storage)
    {
        _storage = storage;
        _indexPath = Path.Combine(Path.GetTempPath(), "lucene");

        CreateFolders();
        DownloadCache();
        InitWriters();
        InitSearcher();
    }

    public void DownloadCache()
    {
        var blobs = _storage.GetBlobList().ToList();

        foreach (var blob in blobs)
        {
            var directoryName = Path.GetFileNameWithoutExtension(blob.Name);
            var directoryPath = Path.Combine(_indexPath, directoryName);

            var file = _storage.Download(blob)!;

            ZipFile.ExtractToDirectory(file.FilePath, directoryPath);
            File.Delete(file.FilePath);
        }
    }

    public override void Dispose()
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
            UploadFiles(tempDirectory.Key);
        }
    }

    private void UploadFiles(string directoryName)
    {
        var zipName = $"{directoryName}.zip";
        var path = Path.Combine(_indexPath, directoryName);
        var zipPath = Path.Combine(_indexPath, zipName);

        ZipFile.CreateFromDirectory(path, zipPath);

        BlobMetadata blobMetadata = new()
        {
            Name = zipName,

            // Dont work. Fix it.
            Rewrite = true,
        };

        if (_storage.Exists(blobMetadata))
        {
            _storage.Delete(blobMetadata);
        }

        _storage.UploadFile(blobMetadata, zipPath);

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
}