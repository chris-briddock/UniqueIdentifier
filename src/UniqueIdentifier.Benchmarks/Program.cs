using BenchmarkDotNet.Running;

namespace UniqueIdentifier.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}
