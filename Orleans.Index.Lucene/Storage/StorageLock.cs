using Lucene.Net.Store;
using ManagedCode.Storage.Core;

namespace Orleans.Index.Lucene.Storage;

public class StorageLock : Lock
{
    private readonly IStorage _storage;
    private readonly string _fileName;

    public StorageLock(IStorage storage, string fileName)
    {
        _storage = storage;
        _fileName = fileName;
    }

    public override bool Obtain()
    {
        CheckFileAndUpload();

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
        CheckFileAndUpload();

        return _storage.HasLegalHold(_fileName).Result;
    }

    public void BreakLock()
    {
        _storage.SetLegalHold(_fileName, false);
    }

    private void CheckFileAndUpload()
    {
        if (_storage.ExistsAsync(_fileName).Result)
        {
            return;
        }

        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(_fileName);
            _storage.UploadStreamAsync(_fileName, stream).Wait();
        }
    }
}