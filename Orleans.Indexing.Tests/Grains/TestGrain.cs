using System.Threading.Tasks;
using Orleans.Indexing.Abstractions;
using Orleans.Indexing.Annotations;

namespace Orleans.Indexing.Tests.Grains;

public class TestClass
{
    [Index] public int IntValue { get; set; }

    [Index] public string StringValue { get; set; }
}

public class TestGrain : IndexGrain, ITestGrain
{
    public TestGrain(IIndexService indexService) : base(indexService)
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