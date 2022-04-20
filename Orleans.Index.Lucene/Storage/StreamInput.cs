using Lucene.Net.Store;

namespace Orleans.Index.Lucene.Storage;

public class StreamInput : Stream
{
    public IndexInput Input { get; }

    public StreamInput(IndexInput input)
    {
        Input = input;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override void Flush()
    {
    }

    public override long Length => Input.Length;

    public override long Position
    {
        get => Input.GetFilePointer();
        set => Input.Seek(value);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        long pos = Input.GetFilePointer();
        try
        {
            long len = Input.Length;
            if (count > (len - pos))
                count = (int) (len - pos);
            Input.ReadBytes(buffer, offset, count);
        }
        catch (Exception)
        {
        }

        return (int) (Input.GetFilePointer() - pos);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Input.Seek(offset);
                break;
            case SeekOrigin.Current:
                Input.Seek(Input.GetFilePointer() + offset);
                break;
            case SeekOrigin.End:
                throw new NotImplementedException();
        }

        return Input.GetFilePointer();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override void Close()
    {
        base.Close();
        Input.Dispose();
    }
}