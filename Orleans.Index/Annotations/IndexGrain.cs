using Orleans.Index.Abstractions;
using Orleans.Indexing;
using Orleans.Placement;

namespace Orleans.Index.Annotations;

[PreferLocalPlacement]
public abstract class IndexGrain : Grain
{
    private readonly IIndexService _indexService;

    protected IndexGrain(IIndexService indexService)
    {
        _indexService = indexService;
    }

    protected async Task WriteIndexAsync()
    {
        string id;
        try
        {
            id = this.GetPrimaryKeyString();
        }
        catch
        {
            id = Guid.NewGuid().ToString();
        }

        var doc = new GrainDocument(id);
        WriteProperties(doc, this);

        await _indexService.WriteIndex(doc);
    }

    private void WriteProperties(GrainDocument doc, object obj)
    {
        var properties = obj.GetType().GetProperties();

        foreach (var propInfo in properties)
        {
            var indexAttrs = propInfo.GetCustomAttributes(typeof(IndexAttribute), false);

            if (indexAttrs.Length == 0) continue;

            var type = propInfo.PropertyType;
            if (!(type.IsPrimitive || type == typeof(decimal) || type == typeof(string)))
            {
                var instance = propInfo.GetValue(obj);
                WriteProperties(doc, instance);
            }
            else
            {
                doc.LuceneDocument.Add(new StringField($"{propInfo.Name}", $"{propInfo.GetValue(obj)}", Field.Store.YES));
            }
        }
    }
}