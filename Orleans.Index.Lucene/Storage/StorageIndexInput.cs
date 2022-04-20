using System.Diagnostics;
using Lucene.Net.Store;
using ManagedCode.Storage.Core;

namespace Orleans.Index.Lucene.Storage;

public class StorageIndexInput : IndexInput
{
    private readonly string _name;
    private readonly Mutex _fileMutex;
    private readonly IStorage _storage;
    private readonly StorageDirectory _directory;
    private IndexInput _indexInput;

    public StorageIndexInput(string name, StorageDirectory directory, IStorage storage) : base(name)
    {
        _name = name;
        _storage = storage;
        _directory = directory;

        _fileMutex = StorageMutexManager.GrabMutex(name);
        _fileMutex.WaitOne();

        bool fileNeeded = false;

        if (!directory.CachedDirectory.FileExists(name))
        {
            fileNeeded = true;
        }
        else
        {
            long cachedLength = directory.CachedDirectory.FileLength(name);
            long blobLength = _storage.GetBlob(name).Length;

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
                var stream = _storage.DownloadAsStream(name);
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
        if (len == 316)
            Debugger.Break();

        _indexInput.ReadBytes(b, offset, len);
    }

    protected override void Dispose(bool disposing)
    {
        _fileMutex.WaitOne();
        try
        {
            _indexInput.Dispose();
            _indexInput = null;
            GC.SuppressFinalize(this);
        }
        finally
        {
            _fileMutex.ReleaseMutex();
        }
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

    public override object Clone()
    {
        var clone = new StorageIndexInput(_name, _directory, _storage);
        clone.Seek(GetFilePointer());
        return clone;
    }
}