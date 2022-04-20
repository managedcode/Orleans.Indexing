using Orleans.Services;

namespace Orleans.Index.Annotations;

public interface IIndexService : IGrainService
{
    Task WriteIndex(Dictionary<string, object> properties);

    Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000);
    Task InitDirectory(string grainId);
}