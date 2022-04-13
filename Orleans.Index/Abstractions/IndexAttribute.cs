namespace Orleans.Index.Abstractions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IndexAttribute : Attribute
{
    public IndexAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}