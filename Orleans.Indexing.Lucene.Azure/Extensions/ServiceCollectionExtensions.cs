using ManagedCode.Storage.Azure;
using ManagedCode.Storage.Azure.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Lucene.Services;

namespace Orleans.Indexing.Lucene.Azure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuceneIndexingWithAzureStore(this IServiceCollection serviceCollection, AzureStorageOptions options)
    {
        var storage = new AzureStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneWithStorageIndexService(storage));
    }

    public static IServiceCollection AddLuceneIndexingWithAzureStore(this IServiceCollection serviceCollection, Action<AzureStorageOptions> action)
    {
        var options = new AzureStorageOptions();
        action.Invoke(options);

        var storage = new AzureStorage(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new LuceneWithStorageIndexService(storage));
    }
}