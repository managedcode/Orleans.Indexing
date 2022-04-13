using Microsoft.Extensions.DependencyInjection;
using Orleans.Index.Annotations;
using Orleans.Index.Tests.Cluster;
using Orleans.Index.Tests.Cluster.Fakes;
using Xunit;

namespace Orleans.Index.Tests.Lucene;

[CollectionDefinition(nameof(LuceneHubClusterCollection))]
public class LuceneHubClusterCollection : ICollectionFixture<LuceneHubClusterCollection.WinktClusterFixture>
{
    public class WinktClusterFixture : ClusterFixture
    {
        static WinktClusterFixture()
        {
            SiloBuilderConfigurator.ServiceConfigurationAction = collection =>
            {
                collection.AddLogging();
                collection.AddSingleton<IIndexService>(FakeServices.FakeLuceneIndexService);
            };
        }
    }
}