using ManagedCode.Storage.Core;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Tests.Cluster;
using Orleans.Indexing.Tests.Cluster.Fakes;
using Xunit;

namespace Orleans.Indexing.Tests.Lucene;

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
                collection.AddSingleton<IIndexService>(FakeServices.FakeLuceneWithStorageIndexService);
                collection.AddSingleton<IStorage>(FakeServices.FakeStorage);
            };
        }
    }
}