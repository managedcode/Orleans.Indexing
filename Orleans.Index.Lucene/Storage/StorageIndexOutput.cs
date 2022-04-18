using Lucene.Net.Store;
using ManagedCode.Storage.Core;

namespace Orleans.Index.Lucene.Storage;

public class StorageIndexOutput : IndexOutput
{
    private readonly string _name;
    private readonly IStorage _storage;
    private readonly StorageDirectory _directory;
    private readonly IndexOutput _indexOutput;
    private readonly Mutex _fileMutex;

    public StorageIndexOutput(string name, StorageDirectory directory, IStorage storage)
    {
        _fileMutex = StorageMutexManager.GrabMutex(name);
        _fileMutex.WaitOne();
        _name = name;
        _storage = storage;
        _directory = directory;
        _indexOutput = _directory.CreateOutput(name, IOContext.DEFAULT);

        _fileMutex.ReleaseMutex();
    }

    public override void WriteByte(byte b)
    {
        _indexOutput.WriteByte(b);
    }

    public override void WriteBytes(byte[] b, int offset, int length)
    {
        _indexOutput.WriteBytes(b, offset, length);
    }

    public override void Flush()
    {
        _indexOutput.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            _fileMutex.WaitOne();

            using (var blobStream = new StreamInput(_directory.CachedDirectory.OpenInput(_name, IOContext.DEFAULT)))
            {
                _storage.UploadStreamAsync(_name, blobStream).Wait();
            }
        }
        finally
        {
            _indexOutput.Dispose();

            _fileMutex.ReleaseMutex();
        }
    }

    public override long GetFilePointer()
    {
        return _indexOutput.GetFilePointer();
    }

    public override void Seek(long pos)
    {
        _indexOutput.Seek(pos);
    }

    public override long Checksum => _indexOutput.Checksum;
}