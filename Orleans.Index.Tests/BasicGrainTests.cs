using System;
using FluentAssertions;
using Orleans.Index.Tests.Cluster;
using Orleans.Index.Tests.Grains;
using Xunit;

namespace Orleans.Index.Tests
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

            // var result = await grain.Register(command);
            // result.Should().NotBeNull();
        }
    }
}