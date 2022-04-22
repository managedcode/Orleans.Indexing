using System.Threading.Tasks;

namespace Orleans.Index.Tests.Grains;

public interface IAnotherTestGrain : IGrainWithStringKey
{
    Task UpdateIntValue(int value);
}