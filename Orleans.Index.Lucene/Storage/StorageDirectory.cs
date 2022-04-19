using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Index.Lucene.Storage;

public class StorageDirectory : BaseDirectory
{
    private readonly IStorage _storage;
    private readonly Dictionary<string, StorageIndexOutput> _nameCache = new();

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
        var indexOutput = new StorageIndexOutput(name, this, _storage);

        return indexOutput;
    }

    public override void Sync(ICollection<string> names)
    {
        foreach (var name in names)
        {
            if (_nameCache.ContainsKey(name))
            {
                _nameCache[name].Flush();
            }
        }
    }

    public override IndexInput OpenInput(string name, IOContext context)
    {
        if (!_storage.ExistsAsync(name).Result)
            throw new FileNotFoundException(name);

        return new StorageIndexInput(name, this, _storage);
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

        CachedDirectory.ClearLock(name);
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }

    public StreamOutput CreateCachedOutputAsStream(string name)
    {
        return new StreamOutput(CachedDirectory.CreateOutput(name, IOContext.DEFAULT));
    }
}