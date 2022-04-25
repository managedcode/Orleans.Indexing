using System.Threading.Tasks;

namespace Orleans.Indexing.Tests.Grains;

public interface IAnotherTestGrain : IGrainWithStringKey
{
    Task UpdateIntValue(int value);
}