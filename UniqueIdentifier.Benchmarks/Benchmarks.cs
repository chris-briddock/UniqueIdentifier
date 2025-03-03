using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace UniqueIdentifier.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[RankColumn]
public class Benchmarks
{
    private const int IterationCount = 10000;

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
    public Gusid BenchmarkParsing()
    {
        string sampleGusid = "01234567890ABCDEF0123456789ABCDE";
        return Gusid.Parse(sampleGusid);
    }

    [Benchmark(Description = "GUSID Comparison")]
    public int BenchmarkComparison()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();
        return gusid1.CompareTo(gusid2);
    }

    [Benchmark(Description = "GUSID Sorting")]
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

    [Benchmark(Description = "GUSID Serialization")]
    public string BenchmarkToString()
    {
        var gusid = Gusid.New();
        return gusid.ToString();
    }

    [Benchmark(Description = "GUSID Uniqueness")]
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

    [Benchmark(Description = "GUSID Equality")]
    public bool BenchmarkEquality()
    {
        var gusid1 = Gusid.New();
        var gusid2 = gusid1;
        return gusid1 == gusid2;
    }

    [Benchmark(Description = "GUSID Hashing")]
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
    public Guid BenchmarkGuidParsing()
    {
        string sampleGuid = "01234567-89AB-CDEF-0123-456789ABCDEF";
        return Guid.Parse(sampleGuid);
    }

    [Benchmark(Description = "Guid Comparison")]
    public int BenchmarkGuidComparison()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        return guid1.CompareTo(guid2);
    }

    [Benchmark(Description = "Guid Sorting")]
    public Guid[] BenchmarkGuidSorting()
    {
        Guid[] guids = new Guid[IterationCount];
        for (int i = 0; i < IterationCount; i++)
        {
            guids[i] = Guid.NewGuid();
        }
        Array.Sort(guids);
        return guids;
    }

    [Benchmark(Description = "Guid Serialization")]
    public string BenchmarkGuidToString()
    {
        var guid = Guid.NewGuid();
        return guid.ToString();
    }

    [Benchmark(Description = "Guid Uniqueness")]
    public bool BenchmarkGuidUniqueness()
    {
        HashSet<Guid> uniqueSet = new HashSet<Guid>();
        for (int i = 0; i < IterationCount; i++)
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
    public bool BenchmarkGuidEquality()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = guid1;
        return guid1 == guid2;
    }

    [Benchmark(Description = "Guid Hashing")]
    public int BenchmarkGuidHashCode()
    {
        var guid = Guid.NewGuid();
        return guid.GetHashCode();
    }

    // Benchmark for ULID
    [Benchmark(Description = "ULID Generation")]
    [Arguments(1)]
    public Ulid BenchmarkUlidGeneration(int count)
    {
        return Ulid.NewUlid();
    }

    [Benchmark(Description = "ULID Bulk Generation")]
    [Arguments(1000)]
    public Ulid[] BenchmarkUlidBulkGeneration(int count)
    {
        Ulid[] ulids = new Ulid[count];
        for (int i = 0; i < count; i++)
        {
            ulids[i] = Ulid.NewUlid();
        }
        return ulids;
    }

    [Benchmark(Description = "ULID Parsing")]
    public Ulid BenchmarkUlidParsing()
    {
        string sampleUlid = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
        return Ulid.Parse(sampleUlid);
    }

    [Benchmark(Description = "ULID Comparison")]
    public int BenchmarkUlidComparison()
    {
        var ulid1 = Ulid.NewUlid();
        var ulid2 = Ulid.NewUlid();
        return ulid1.CompareTo(ulid2);
    }

    [Benchmark(Description = "ULID Sorting")]
    public Ulid[] BenchmarkUlidSorting()
    {
        Ulid[] ulids = new Ulid[IterationCount];
        for (int i = 0; i < IterationCount; i++)
        {
            ulids[i] = Ulid.NewUlid();
        }
        Array.Sort(ulids);
        return ulids;
    }

    [Benchmark(Description = "ULID Serialization")]
    public string BenchmarkUlidToString()
    {
        var ulid = Ulid.NewUlid();
        return ulid.ToString();
    }

    [Benchmark(Description = "ULID Uniqueness")]
    public bool BenchmarkUlidUniqueness()
    {
        HashSet<Ulid> uniqueSet = new HashSet<Ulid>();
        for (int i = 0; i < IterationCount; i++)
        {
            var ulid = Ulid.NewUlid();
            if (!uniqueSet.Add(ulid))
            {
                return false;
            }
        }
        return true;
    }

    [Benchmark(Description = "ULID Equality")]
    public bool BenchmarkUlidEquality()
    {
        var ulid1 = Ulid.NewUlid();
        var ulid2 = ulid1;
        return ulid1 == ulid2;
    }

    [Benchmark(Description = "ULID Hashing")]
    public int BenchmarkUlidHashCode()
    {
        var ulid = Ulid.NewUlid();
        return ulid.GetHashCode();
    }
}
