using System.Threading.Tasks;

namespace Orleans.Index.Tests.Grains;

public interface ITestGrain : IGrainWithStringKey
{
    Task UpdateIntValue(int value);
}