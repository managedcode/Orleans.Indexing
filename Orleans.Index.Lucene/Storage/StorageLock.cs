using Lucene.Net.Store;

namespace Orleans.Index.Lucene;

public class StorageLock : Lock
{
    public override bool Obtain()
    {
        // throw new NotImplementedException();
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }

    public override bool IsLocked()
    {
        // throw new NotImplementedException();

        return false;
    }
}