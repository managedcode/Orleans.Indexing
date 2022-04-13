using Orleans.Services;

namespace Orleans.Indexing;

public interface IIndexService : IGrainService
{
    // Task WriteIndex(GrainDocument document);

    // Task<TopDocs> QueryByField(string field, string query, int take = 1000);

    Task<IList<string>> GetGrainIdsByQuery(string field, string query, int take = 1000);
}