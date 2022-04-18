using Azure.Storage.Blobs.Models;
using ManagedCode.Storage.Azure;
using ManagedCode.Storage.Azure.Options;
using ManagedCode.Storage.Core;
using Orleans.Index.Lucene.Services;

namespace Orleans.Index.Tests.Cluster.Fakes;

public class FakeServices
{
    public static LuceneIndexService FakeLuceneIndexService { get; }
    public static IStorage FakeStorage { get; }

    static FakeServices()
    {
        AzureStorageOptions options = new()
        {
            ConnectionString =
                "DefaultEndpointsProtocol=https;AccountName=winktblobtest;AccountKey=V7wWKnqRsSPqog4NhVchzguMBc6aDw6LHjD0Si/z1mAUYGaWNpUccoxitFHeVsQPmmIOsQrUbSm7+AStw+amcg==;EndpointSuffix=core.windows.net",
            Container = "testcatalog",
        };

        FakeStorage = new AzureStorage(options);
        FakeLuceneIndexService = new LuceneIndexService(FakeStorage);
    }
}