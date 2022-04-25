using System;
using FluentAssertions;
using Orleans.Indexing.Tests.Cluster;
using Orleans.Indexing.Tests.Grains;
using Xunit;

namespace Orleans.Indexing.Tests
{
    [Collection(nameof(UsersHubClusterCollection))]
    public class BasicGrainTests
    {
        private readonly ClusterFixture _fixture;

        public BasicGrainTests(UsersHubClusterCollection.WinktClusterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void BaseTest()
        {
            var grain = _fixture.Cluster.Client.GetGrain<ITestGrain>(Guid.NewGuid().ToString());

            grain.Should().NotBeNull();
        }
    }
}