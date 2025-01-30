using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace UniqueIdentifier.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[RankColumn]
public class Benchmarks
{
    private const int IterationCount = 10000;

    [Benchmark(Description = "Generation")]
    [Arguments(1)]
    public Gusid BenchmarkGeneration(int count)
    {

        return Gusid.New();
    }

    [Benchmark(Description = "Generation")]
    [Arguments(1000)]
    public Gusid[] BenchmarkBulkGeneration(int count)
    {
        Gusid[] guids = new Gusid[count];
        for (int i = 0; i < count; i++)
        {
            guids[i] = Gusid.New();
        }
        return guids;
    }

    [Benchmark(Description = "Parsing")]
    public Gusid BenchmarkParsing()
    {
        string sampleGusid = "01234567890ABCDEF0123456789ABCDE";
        return Gusid.Parse(sampleGusid);
    }

    [Benchmark(Description = "Comparison")]
    public int BenchmarkComparison()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();
        return gusid1.CompareTo(gusid2);
    }

    [Benchmark(Description = "Sorting")]
    public Gusid[] BenchmarkSorting()
    {
        Gusid[] guids = [];
        for (int i = 0; i < IterationCount; i++)
        {
            guids[i] = Gusid.New();
        }
        Array.Sort(guids);
        return guids;
    }

    [Benchmark(Description = "Serialization")]
    public string BenchmarkToString()
    {
        var gusid = Gusid.New();
        return gusid.ToString();
    }

    [Benchmark(Description = "Uniqueness")]
    public bool BenchmarkUniqueness()
    {
        HashSet<Gusid> uniqueSet = [];
        for (int i = 0; i < IterationCount; i++)
        {
            var gusid = Gusid.New();
            if (!uniqueSet.Add(gusid))
            {
                return false;
            }
        }
        return true;
    }

    [Benchmark(Description = "Equality")]
    public bool BenchmarkEquality()
    {
        var gusid1 = Gusid.New();
        var gusid2 = gusid1;
        return gusid1 == gusid2;
    }

    [Benchmark(Description = "Hashing")]
    public int BenchmarkHashCode()
    {
        var gusid = Gusid.New();
        return gusid.GetHashCode();
    }
}
