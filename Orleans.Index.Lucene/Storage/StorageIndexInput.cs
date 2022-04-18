using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Index.Lucene.Storage;

public class StorageIndexInput : IndexInput
{
    private readonly Mutex _fileMutex;
    private readonly IStorage _storage;
    private readonly StorageDirectory _directory;
    private readonly IndexInput _indexInput;

    public StorageIndexInput(string name, StorageDirectory directory, IStorage storage) : base(name)
    {
        _fileMutex = StorageMutexManager.GrabMutex(name);
        _fileMutex.WaitOne();

        _storage = storage;
        // _directory = directory;
        //
        // if (!directory.FileExists(name))
        // {
        //     using (var output = directory.CachedDirectory.CreateOutput(name, IOContext.DEFAULT))
        //     {
        //         // get the blob
        //         output.Flush();
        //     }
        // }
        //
        // _indexInput = _directory.CachedDirectory.OpenInput(name, IOContext.DEFAULT);

        bool fileNeeded = false;
        if (!directory.CachedDirectory.FileExists(name))
        {
            fileNeeded = true;
        }
        else
        {
            long cachedLength = directory.CachedDirectory.FileLength(name);
            long blobLength = _storage.GetBlobAsync(name).Result.Length;

            if (cachedLength != blobLength)
                fileNeeded = true;
        }

        // if the file does not exist
        // or if it exists and it is older then the lastmodified time in the blobproperties (which always comes from the blob storage)
        if (fileNeeded)
        {
            using (StreamOutput fileStream = directory.CreateCachedOutputAsStream(name))
            {
                // get the blob
                var stream = _storage.DownloadAsStreamAsync(name).Result;
                stream.CopyTo(fileStream);

                fileStream.Flush();
            }
        }

        // and open it as our input, this is now available forevers until new file comes along
        _indexInput = directory.CachedDirectory.OpenInput(name, IOContext.DEFAULT);

        _fileMutex.ReleaseMutex();
    }

    public override byte ReadByte()
    {
        return _indexInput.ReadByte();
    }

    public override void ReadBytes(byte[] b, int offset, int len)
    {
        _indexInput.ReadBytes(b, offset, len);
    }

    protected override void Dispose(bool disposing)
    {
        _fileMutex.WaitOne();

        _indexInput.Dispose();

        _fileMutex.ReleaseMutex();
    }

    public override long GetFilePointer()
    {
        return _indexInput.GetFilePointer();
    }

    public override void Seek(long pos)
    {
        _indexInput.Seek(pos);
    }

    public override long Length => _indexInput.Length;
}