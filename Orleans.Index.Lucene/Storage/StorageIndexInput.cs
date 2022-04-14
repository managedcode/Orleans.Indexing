using Lucene.Net.Store;

namespace Orleans.Index.Lucene;

public class StorageIndexInput : IndexInput
{
    public StorageIndexInput(string resourceDescription) : base(resourceDescription)
    {
    }

    public override byte ReadByte()
    {
        // throw new NotImplementedException();
        return 1;
    }

    public override void ReadBytes(byte[] b, int offset, int len)
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

    public override long Length { get; }
}