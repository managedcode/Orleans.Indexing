using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;


namespace Orleans.Index.Lucene.Temp;

public class AzureDirectory : Directory
{
    private IStorage _storage;
    private string containerName;
    private string subDirectory;

    private readonly Dictionary<string, AzureLock> _locks = new Dictionary<string, AzureLock>();
    private LockFactory _lockFactory = new NativeFSLockFactory();
    private readonly Dictionary<string, AzureIndexOutput> _nameCache = new Dictionary<string, AzureIndexOutput>();

    public override LockFactory LockFactory => _lockFactory;

    public AzureDirectory(IStorage storage) :
        this(storage, null, null)
    {
    }

    /// <summary>
    /// Create AzureDirectory
    /// </summary>
    /// <param name="storage">staorage account to use</param>
    /// <param name="catalog">name of catalog (folder in blob storage, can have subfolders like foo/bar)</param>
    /// <remarks>Default local cache is to use file system in user/appdata/AzureDirectory/Catalog</remarks>
    public AzureDirectory(
        IStorage storage,
        string catalog)
        : this(storage, catalog, null)
    {
    }

    /// <summary>
    /// Create an AzureDirectory
    /// </summary>
    /// <param name="storage">storage account to use</param>
    /// <param name="catalog">name of catalog (folder in blob storage, can have subfolders like foo/bar)</param>
    /// <param name="cacheDirectory">local Directory object to use for local cache</param>
    public AzureDirectory(
        IStorage storage,
        string catalog,
        Directory cacheDirectory)
    {
        if (storage == null)
            throw new ArgumentNullException("storage");

        if (string.IsNullOrEmpty(catalog))
            Name = "lucene";
        else
            Name = catalog.ToLower();

        _storage = storage;
        this.containerName = Name.Split('/').First();
        this.subDirectory = String.Join("/", Name.Split('/').Skip(1));

        _initCacheDirectory(cacheDirectory);
    }


    public string Name { get; set; }

    /// <summary>
    /// If set, this is the directory object to use as the local cache
    /// </summary>
    public Directory CacheDirectory { get; set; }

    public void ClearCache()
    {
        if (this.CacheDirectory != null)
        {
            foreach (string file in CacheDirectory.ListAll())
            {
                CacheDirectory.DeleteFile(file);
            }
        }
    }

    #region DIRECTORYMETHODS

    /// <summary>Returns an array of strings, one for each file in the directory. </summary>
    public override string[] ListAll()
    {
        var blobs = _storage.GetBlobListAsync().ToArrayAsync().Result;

        return blobs.Select(b => b.Name).ToArray();
    }

    /// <summary>Returns true if a file with the given name exists. </summary>
    [Obsolete("this method will be removed in 5.0")]
    public override bool FileExists(string name)
    {
        return _storage.ExistsAsync(name).Result;
    }

    /// <summary>Removes an existing file in the directory. </summary>
    public override void DeleteFile(string name)
    {
        _storage.DeleteAsync(name).Wait();
    }

    /// <summary>Returns the length of a file in the directory. </summary>
    public override long FileLength(string name)
    {
        var blob = _storage.DownloadAsync(name).Result;

        return blob.FileStream.Length;
    }

    public override void Sync(ICollection<string> names)
    {
        // TODO: This all is purely guesswork, no idea what has to be done here. -- Aviad.
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
        // TODO: Figure out how IOContext comes into play here. So far it doesn't -- Aviad
        try
        {
            // var blobName = GetBlobName(name);
            // var blob = BlobContainer.GetBlockBlobReference(blobName);
            // blob.FetchAttributes();
            return new AzureIndexInput(this, name, _storage);
        }
        catch (Exception err)
        {
            throw new FileNotFoundException(name, err);
        }
    }

    /// <summary>Construct a {@link Lock}.</summary>
    /// <param name="name">the name of the lock file
    /// </param>
    public override Lock MakeLock(string name)
    {
        lock (_locks)
        {
            if (!_locks.ContainsKey(name))
            {
                _locks.Add(name, new AzureLock(_storage, name));
            }

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
    }

    /// <summary>Closes the store. </summary>
    protected override void Dispose(bool disposing)
    {
        // BlobContainer = null;
        _storage = null;
    }

    public override void SetLockFactory(LockFactory lockFactory)
    {
        _lockFactory = lockFactory;
    }

    /// <summary>Creates a new, empty file in the directory with the given name.
    /// Returns a stream writing this file. 
    /// </summary>
    public override IndexOutput CreateOutput(string name, IOContext context)
    {
        // TODO: Figure out how IOContext comes into play here. So far it doesn't -- Aviad
        var blobName = GetBlobName(name);
        // var blob = BlobContainer.GetBlockBlobReference(blobName);
        var indexOutput = new AzureIndexOutput(this, name, _storage);
        _nameCache[name] = indexOutput;
        return indexOutput;
    }

    #endregion

    #region internal methods

    public string GetBlobName(string name)
    {
        if (this.subDirectory.Length > 1)
        {
            return $"{subDirectory}/{name}";
        }

        return name;
    }

    private void _initCacheDirectory(Directory cacheDirectory)
    {
        if (cacheDirectory != null)
        {
            // save it off
            this.CacheDirectory = cacheDirectory;
        }
        else
        {
            string cachePath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "lucene");
            System.IO.DirectoryInfo azureDir = new System.IO.DirectoryInfo(cachePath);
            if (!azureDir.Exists)
                azureDir.Create();

            string catalogPath = System.IO.Path.Combine(cachePath, this.Name);

            System.IO.DirectoryInfo catalogDir = new System.IO.DirectoryInfo(catalogPath);
            if (!catalogDir.Exists)
                catalogDir.Create();

            this.CacheDirectory = FSDirectory.Open(catalogPath);
        }
    }

    public StreamInput OpenCachedInputAsStream(string name)
    {
        return new StreamInput(this.CacheDirectory.OpenInput(name, IOContext.DEFAULT));
    }

    public StreamOutput CreateCachedOutputAsStream(string name)
    {
        return new StreamOutput(this.CacheDirectory.CreateOutput(name, IOContext.DEFAULT));
    }

    #endregion
}