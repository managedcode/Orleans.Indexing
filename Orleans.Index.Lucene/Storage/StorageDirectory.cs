using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Index.Lucene.Storage;

public class StorageDirectory : BaseDirectory
{
    private readonly IStorage _storage;

    public StorageDirectory(IStorage storage)
    {
        _storage = storage;

        var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "storage");
        var azureDir = new DirectoryInfo(cachePath);

        if (!azureDir.Exists) azureDir.Create();

        var catalogPath = Path.Combine(cachePath, "catalog");

        var catalogDir = new DirectoryInfo(catalogPath);

        if (!catalogDir.Exists) catalogDir.Create();

        CachedDirectory = FSDirectory.Open(catalogPath);
    }


    public Directory CachedDirectory { get; }

    public override string[] ListAll()
    {
        var blobs = _storage.GetBlobListAsync().ToArrayAsync().Result;

        return blobs.Select(b => b.Name).ToArray();
    }

    public override bool FileExists(string name)
    {
        return _storage.ExistsAsync(name).Result;
    }

    public override void DeleteFile(string name)
    {
        _storage.DeleteAsync(name).Wait();
    }

    public override long FileLength(string name)
    {
        var blob = _storage.GetBlobAsync(name).Result;

        return blob.Length;
    }

    public override IndexOutput CreateOutput(string name, IOContext context)
    {
        IndexOutput output = CachedDirectory.CreateOutput(name, context);

        return output;
    }

    public override void Sync(ICollection<string> names)
    {
        // throw new NotImplementedException();
    }

    public override IndexInput OpenInput(string name, IOContext context)
    {
        IndexInput input = CachedDirectory.OpenInput(name, context);
        return input;
    }

    private readonly Dictionary<string, StorageLock> _locks = new();

    public override Lock MakeLock(string name)
    {
        lock (_locks)
        {
            if (!_locks.ContainsKey(name))
                _locks.Add(name, new StorageLock(_storage, name));
            return _locks[name];
        }
    }

    public override void ClearLock(string name)
    {
        lock (_locks)
        {
            if (_locks.ContainsKey(name))
            {
                _locks[name].BreakLock();
            }
        }

        // _cacheDirectory.ClearLock(name);
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }
}