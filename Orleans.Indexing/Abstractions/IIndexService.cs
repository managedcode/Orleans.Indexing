using Orleans.Services;

namespace Orleans.Indexing.Abstractions;

public interface IIndexService : IGrainService
{
    Task WriteIndex(Dictionary<string, object> properties);

    Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000);

    Task<IList<string>> GetGrainIdsByQuery<T>(string field, string query, int take = 1000) where T : IndexGrain;
    Task InitializeAsync();
}