using ManagedCode.Storage.Aws;
using ManagedCode.Storage.Aws.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Lucene.Services;

namespace Orleans.Indexing.Lucene.Aws.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuceneIndexingWithGcpStore(this IServiceCollection serviceCollection, AWSStorageOptions options)
    {
        var storage = new AWSStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneIndexService(storage));
    }

    public static IServiceCollection AddLuceneIndexingWithGcpStore(this IServiceCollection serviceCollection, Action<AWSStorageOptions> action)
    {
        var options = new AWSStorageOptions();
        action.Invoke(options);

        var storage = new AWSStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneIndexService(storage));
    }
}