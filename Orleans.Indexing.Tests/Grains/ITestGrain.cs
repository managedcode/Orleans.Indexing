using System.Threading.Tasks;

namespace Orleans.Indexing.Tests.Grains;

public interface ITestGrain : IGrainWithStringKey
{
    Task UpdateIntValue(int value);
}