using System.Threading.Tasks;
using Orleans.Index.Abstractions;
using Orleans.Index.Annotations;

namespace Orleans.Index.Tests.Grains;

public class AnotherTestGrain : IndexGrain, IAnotherTestGrain
{
    public AnotherTestGrain(IIndexService indexService) : base(indexService)
    {
        Class = new TestClass {IntValue = 3, StringValue = "Test"};
    }

    [Index] public TestClass Class { get; set; }

    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync();

        Class = new TestClass {IntValue = 3, StringValue = "Test"};

        await WriteIndexAsync();
    }

    public async Task UpdateIntValue(int value)
    {
        Class.IntValue = value;

        await WriteIndexAsync();
    }
}