using Lucene.Net.Store;

namespace Orleans.Index.Lucene.Storage;

public class StorageIndexOutput : IndexOutput
{
    private readonly StorageDirectory _directory;
    private readonly IndexOutput _indexOutput;

    public StorageIndexOutput(string name, StorageDirectory directory)
    {
        _directory = directory;
        _indexOutput = _directory.CreateOutput(name, IOContext.DEFAULT);
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
        _indexOutput.Dispose();
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