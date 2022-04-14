using Orleans.Index.Lucene.Services;

namespace Orleans.Index.Tests.Cluster.Fakes;

public class FakeServices
{
    public static LuceneIndexService FakeLuceneIndexService { get; }

    static FakeServices()
    {
        FakeLuceneIndexService = new LuceneIndexService();
    }
}