using ManagedCode.Storage.FileSystem;
using ManagedCode.Storage.FileSystem.Options;
using ManagedCode.Storage.Core;
using Orleans.Index.Lucene.Services;

namespace Orleans.Index.Tests.Cluster.Fakes;

public class FakeServices
{
    public static LuceneIndexService FakeLuceneIndexService { get; }
    public static IStorage FakeStorage { get; }

    static FakeServices()
    {
        FileSystemStorageOptions options = new()
        {
            BaseFolder = System.IO.Path.GetTempPath()
        };

        FakeStorage = new FileSystemStorage(options);
        FakeLuceneIndexService = new LuceneIndexService(FakeStorage);
    }
}