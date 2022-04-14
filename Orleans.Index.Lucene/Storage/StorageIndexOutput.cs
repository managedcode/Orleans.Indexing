using Lucene.Net.Store;

namespace Orleans.Index.Lucene;

public class StorageIndexOutput : IndexOutput
{
    public override void WriteByte(byte b)
    {
        // throw new NotImplementedException();
    }

    public override void WriteBytes(byte[] b, int offset, int length)
    {
        // throw new NotImplementedException();
    }

    public override void Flush()
    {
        // throw new NotImplementedException();
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }

    public override long GetFilePointer()
    {
        // throw new NotImplementedException();
        return 10;
    }

    public override void Seek(long pos)
    {
        // throw new NotImplementedException();
    }

    public override long Checksum { get; }
}