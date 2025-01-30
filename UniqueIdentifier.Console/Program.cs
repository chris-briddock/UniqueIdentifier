using System.Text;

namespace UniqueIdentifier.Console;

internal class Program
{
    static void Main(string[] args)
    {
        List<Gusid> items = [];
        for (int i = 0; i < 10; i++)
        {
            items.Add(Gusid.New());
        }

        items.Sort();

        foreach (var item in items)
        {
            System.Console.WriteLine(item);
        }

        System.Console.ReadKey();
    }
}
