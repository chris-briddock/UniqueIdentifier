using System.Diagnostics;

namespace UniqueIdentifier.Console;

internal sealed class Program
{
    private Program() {}
    static void Main(string[] args)
    {
        List<Gusid> items = [];

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 1_000_000; i++)
        {
            items.Add(Gusid.New());
        }

       stopwatch.Stop();

       System.Console.WriteLine($"{stopwatch.ElapsedMilliseconds}");

        System.Console.ReadKey();
    }
}
