using Orleans.Index.Abstractions;
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

    public override async Task OnActivateAsync()
    {
        var id = this.GetPrimaryKeyString();
        await _indexService.InitDirectory(id);
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

        Dictionary<string, object> dictionary = new() {{Constants.GrainId, id}};

        WriteProperties(dictionary, this);

        await _indexService.WriteIndex(dictionary);
    }


    private void WriteProperties(Dictionary<string, object> dictionary, object obj)
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
                WriteProperties(dictionary, instance);
            }
            else
            {
                dictionary.Add($"{propInfo.Name}", $"{propInfo.GetValue(obj)}");
            }
        }
    }
}