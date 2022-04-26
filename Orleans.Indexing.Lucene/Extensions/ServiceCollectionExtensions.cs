using Microsoft.Extensions.DependencyInjection;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Lucene.Options;
using Orleans.Indexing.Lucene.Services;

namespace Orleans.Indexing.Lucene.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuceneIndexingWithRamStore(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<IIndexService>(_ => new RamLuceneIndexService());
    }

    public static IServiceCollection AddLuceneIndexingWithFSStore(this IServiceCollection serviceCollection, Action<FSStoreOptions> action)
    {
        var options = new FSStoreOptions();
        action.Invoke(options);

        return serviceCollection.AddScoped<IIndexService>(_ => new FSLuceneIndexService(options));
    }

    public static IServiceCollection AddLuceneIndexingWithFSStore(this IServiceCollection serviceCollection, FSStoreOptions options)
    {
        return serviceCollection.AddScoped<IIndexService>(_ => new FSLuceneIndexService(options));
    }
}