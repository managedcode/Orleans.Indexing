using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Indexing.Lucene.Storage;

public class StorageDirectory : BaseDirectory
{
    private readonly IStorage _storage;

    public StorageDirectory(IStorage storage)
    {
        _storage = storage;
        CachedDirectory = new RAMDirectory();

        LoadFilesFromStorage();
    }

    public Directory CachedDirectory { get; }

    public override string[] ListAll()
    {
        return CachedDirectory.ListAll();
    }

    public override bool FileExists(string name)
    {
        return CachedDirectory.FileExists(name);
    }

    public override void DeleteFile(string name)
    {
        CachedDirectory.DeleteFile(name);

        // if (_storage.Exists(name))
        //     _storage.Delete(name);
    }

    public override long FileLength(string name)
    {
        return CachedDirectory.FileLength(name);
    }

    public override IndexOutput CreateOutput(string name, IOContext context)
    {
        return CachedDirectory.CreateOutput(name, context);
    }

    public override void Sync(ICollection<string> names)
    {
        foreach (var name in names)
        {
            UploadFile(name);
        }
    }

    public override IndexInput OpenInput(string name, IOContext context)
    {
        return CachedDirectory.OpenInput(name, context);
    }

    public override Lock MakeLock(string name)
    {
        return CachedDirectory.MakeLock(name);
    }

    public override void ClearLock(string name)
    {
        CachedDirectory.ClearLock(name);
    }

    private void UploadFile(string name)
    {
        using (var blobStream = new StreamInput(CachedDirectory.OpenInput(name, IOContext.DEFAULT)))
        {
            try
            {
                _storage.UploadStream(name, blobStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
    }

    public void LoadFilesFromStorage()
    {
        var files = _storage.GetBlobList();

        foreach (var file in files)
        {
            using (var fileStream = CreateCachedOutputAsStream(file.Name))
            {
                // get the blob
                var stream = _storage.DownloadAsStream(file)!;
                stream.CopyTo(fileStream);

                stream.Dispose();

                fileStream.Flush();
            }
        }
    }

    public StreamOutput CreateCachedOutputAsStream(string name)
    {
        return new StreamOutput(CachedDirectory.CreateOutput(name, IOContext.DEFAULT));
    }
}