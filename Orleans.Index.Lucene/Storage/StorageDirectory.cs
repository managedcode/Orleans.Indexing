using Lucene.Net.Store;
using ManagedCode.Storage.Core;

namespace Orleans.Index.Lucene.Storage;

public class StorageDirectory : BaseDirectory
{
    private readonly IStorage _storage;

    public StorageDirectory(IStorage storage)
    {
        _storage = storage;
    }

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
        // throw new NotImplementedException();
        return 10;
    }

    public override IndexOutput CreateOutput(string name, IOContext context)
    {
        IndexOutput output = new StorageIndexOutput();

        return output;
    }

    public override void Sync(ICollection<string> names)
    {
        // throw new NotImplementedException();
    }

    public override IndexInput OpenInput(string name, IOContext context)
    {
        IndexInput input = new StorageIndexInput("fdsfsd");
        // throw new NotImplementedException();
        return input;
    }

    private Dictionary<string, StorageLock> _locks = new Dictionary<string, StorageLock>();

    /// <summary>Construct a {@link Lock}.</summary>
    /// <param name="name">the name of the lock file
    /// </param>
    public override Lock MakeLock(System.String name)
    {
        lock (_locks)
        {
            if (!_locks.ContainsKey(name))
                _locks.Add(name, new StorageLock());
            return _locks[name];
        }
    }

    public override void ClearLock(string name)
    {
        lock (_locks)
        {
            if (_locks.ContainsKey(name))
            {
                // _locks[name].BreakLock();
            }
        }

        // _cacheDirectory.ClearLock(name);
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }
}