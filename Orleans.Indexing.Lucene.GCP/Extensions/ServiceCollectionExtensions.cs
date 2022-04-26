using ManagedCode.Storage.Gcp;
using ManagedCode.Storage.Gcp.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Lucene.Services;

namespace Orleans.Indexing.Lucene.Gcp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuceneIndexingWithGcpStore(this IServiceCollection serviceCollection, GCPStorageOptions options)
    {
        var storage = new GCPStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneIndexService(storage));
    }

    public static IServiceCollection AddLuceneIndexingWithGcpStore(this IServiceCollection serviceCollection, Action<GCPStorageOptions> action)
    {
        var options = new GCPStorageOptions();
        action.Invoke(options);

        var storage = new GCPStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneIndexService(storage));
    }
}