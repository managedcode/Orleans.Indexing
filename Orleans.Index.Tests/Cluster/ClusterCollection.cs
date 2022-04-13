using Xunit;

namespace Orleans.Index.Tests.Cluster;

[CollectionDefinition(nameof(ClusterCollection))]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
}