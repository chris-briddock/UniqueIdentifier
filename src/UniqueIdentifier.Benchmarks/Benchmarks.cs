using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace UniqueIdentifier.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[RankColumn]
public class Benchmarks
{

    [Benchmark(Description = "GUSID Generation")]
    [Arguments(1)]
    public Gusid BenchmarkGeneration(int count)
    {

        return Gusid.New();
    }

    [Benchmark(Description = "GUSID Bulk Generation")]
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

    [Benchmark(Description = "GUSID Parsing")]
     [Arguments(1)]
    public Gusid BenchmarkParsing()
    {
        string sampleGusid = "01234567890ABCDEF0123456789ABCDE";
        return Gusid.Parse(sampleGusid);
    }

    [Benchmark(Description = "GUSID Comparison")]
     [Arguments(1)]
    public int BenchmarkComparison()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();
        return gusid1.CompareTo(gusid2);
    }

    [Benchmark(Description = "GUSID Sorting")]
    [Arguments(1000)]
    public void BenchmarkSorting(int count)
    {
        Gusid[] guids = new Gusid[count];
        for (int i = 0; i < count; i++)
        {
            guids[i] = Gusid.New();
        }
        Array.Sort(guids);
    }

    [Benchmark(Description = "GUSID Serialization")]
     [Arguments(1)]
    public string BenchmarkToString()
    {
        var gusid = Gusid.New();
        return gusid.ToString();
    }

    [Benchmark(Description = "GUSID Uniqueness")]
    [Arguments(1000)]
    public bool BenchmarkUniqueness(int count)
    {
        HashSet<Gusid> uniqueSet = [];
        for (int i = 0; i < count; i++)
        {
            var gusid = Gusid.New();
            if (!uniqueSet.Add(gusid))
            {
                return false;
            }
        }
        return true;
    }

    [Benchmark(Description = "GUSID Equality")]
     [Arguments(1)]
    public bool BenchmarkEquality()
    {
        var gusid1 = Gusid.New();
        var gusid2 = gusid1;
        return gusid1 == gusid2;
    }

    [Benchmark(Description = "GUSID Hashing")]
     [Arguments(1)]
    public int BenchmarkHashCode()
    {
        var gusid = Gusid.New();
        return gusid.GetHashCode();
    }

    // Benchmark for Guid
    [Benchmark(Description = "Guid Generation")]
    [Arguments(1)]
    public Guid BenchmarkGuidGeneration(int count)
    {
        return Guid.NewGuid();
    }

    [Benchmark(Description = "Guid Bulk Generation")]
    [Arguments(1000)]
    public Guid[] BenchmarkGuidBulkGeneration(int count)
    {
        Guid[] guids = new Guid[count];
        for (int i = 0; i < count; i++)
        {
            guids[i] = Guid.NewGuid();
        }
        return guids;
    }

    [Benchmark(Description = "Guid Parsing")]
    [Arguments(1)]
    public Guid BenchmarkGuidParsing()
    {
        string sampleGuid = "01234567-89AB-CDEF-0123-456789ABCDEF";
        return Guid.Parse(sampleGuid);
    }

    [Benchmark(Description = "Guid Comparison")]
    [Arguments(1)]
    public int BenchmarkGuidComparison()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        return guid1.CompareTo(guid2);
    }

    [Benchmark(Description = "Guid Sorting")]
    [Arguments(1000)]
    public Guid[] BenchmarkGuidSorting(int count)
    {
        Guid[] guids = new Guid[count];
        for (int i = 0; i < count; i++)
        {
            guids[i] = Guid.NewGuid();
        }
        Array.Sort(guids);
        return guids;
    }

    [Benchmark(Description = "Guid Serialization")]
    [Arguments(1)]
    public string BenchmarkGuidToString()
    {
        var guid = Guid.NewGuid();
        return guid.ToString();
    }

    [Benchmark(Description = "Guid Uniqueness")]
    [Arguments(1000)]
    public bool BenchmarkGuidUniqueness(int count)
    {
        HashSet<Guid> uniqueSet = [];
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewGuid();
            if (!uniqueSet.Add(guid))
            {
                return false;
            }
        }
        return true;
    }

    [Benchmark(Description = "Guid Equality")]
    [Arguments(1)]
    public bool BenchmarkGuidEquality()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = guid1;
        return guid1 == guid2;
    }

    [Benchmark(Description = "Guid Hashing")]
    [Arguments(1)]
    public int BenchmarkGuidHashCode()
    {
        var guid = Guid.NewGuid();
        return guid.GetHashCode();
    }
}
