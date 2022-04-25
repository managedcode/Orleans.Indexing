using Xunit;

namespace Orleans.Indexing.Tests.Cluster;

[CollectionDefinition(nameof(ClusterCollection))]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
}