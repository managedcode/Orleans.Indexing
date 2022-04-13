using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Orleans.Index.Tests.Cluster;

[CollectionDefinition(nameof(UsersHubClusterCollection))]
public class UsersHubClusterCollection : ICollectionFixture<UsersHubClusterCollection.WinktClusterFixture>
{
    public class WinktClusterFixture : ClusterFixture
    {
        static WinktClusterFixture()
        {
            SiloBuilderConfigurator.ServiceConfigurationAction = collection => { collection.AddLogging(); };
        }
    }
}