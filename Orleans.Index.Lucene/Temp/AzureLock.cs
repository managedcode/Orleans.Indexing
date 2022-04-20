using Lucene.Net.Store;
using ManagedCode.Storage.Core;

namespace Orleans.Index.Lucene.Temp;

public class AzureLock : Lock
{
    private readonly IStorage _storage;
    private readonly string _fileName;

    public AzureLock(IStorage storage, string fileName)
    {
        _storage = storage;
        _fileName = fileName;
    }

    public override bool Obtain()
    {
        try
        {
            _storage.SetLegalHold(_fileName, true);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        // throw new NotImplementedException();
    }

    public override bool IsLocked()
    {
        return _storage.HasLegalHold(_fileName);
    }

    public void BreakLock()
    {
        _storage.SetLegalHold(_fileName, false);
    }
}