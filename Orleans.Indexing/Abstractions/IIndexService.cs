using Orleans.Services;

namespace Orleans.Indexing.Abstractions;

public interface IIndexService : IGrainService
{
    Task WriteIndex(Dictionary<string, object> properties);

    Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000);
}