using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Index.Lucene.Storage;

public class StorageIndexInput : IndexInput
{
    private readonly IStorage _storage;
    private readonly StorageDirectory _directory;
    private readonly IndexInput _indexInput;

    public StorageIndexInput(string name, StorageDirectory directory) : base(name)
    {
        _directory = directory;
        
        if (!directory.FileExists(name))
        {
            using (var output = directory.CachedDirectory.CreateOutput(name, IOContext.DEFAULT))
            {
                // get the blob
                output.Flush();
            }
        }

        _indexInput = _directory.CachedDirectory.OpenInput(name, IOContext.DEFAULT);
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
        _indexInput.Dispose();
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