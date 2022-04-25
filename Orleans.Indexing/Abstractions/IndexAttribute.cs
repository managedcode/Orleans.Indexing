namespace Orleans.Indexing.Abstractions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IndexAttribute : Attribute
{
    public IndexAttribute()
    {
    }

    public IndexAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}