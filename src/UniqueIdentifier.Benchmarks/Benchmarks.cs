using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace UniqueIdentifier.Benchmarks;

/// <summary>
/// Fair, apples-to-apples comparison of <see cref="Gusid"/> and <see cref="Guid"/>.
///
/// Pure-operation benchmarks (equality, comparison, hashing, parsing, serialization,
/// sorting, HashSet insertion) use test data prepared in <see cref="Setup"/> so the
/// cost of identifier generation never contaminates the measured operation.
///
/// Generation and end-to-end workload benchmarks (generate+sort, generate+dedupe)
/// intentionally include generation and are labelled accordingly.
/// </summary>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[RankColumn]
public class Benchmarks
{
    /// <summary>Workload size used by bulk, sorting, and HashSet benchmarks.</summary>
    private const int Count = 1000;

    // ----- Pure-operation data (single values) -----
    private Gusid _gusid1;
    private Gusid _gusidEqual;
    private Gusid _gusidDifferent;
    private Guid _guid1;
    private Guid _guidEqual;
    private Guid _guidDifferent;
    private string _gusidString = null!;
    private string _guidString = null!;
    private char[] _gusidFormatDestination = null!;

    // ----- HashSet data -----
    private Gusid[] _gusidSetSource = null!;
    private Guid[] _guidSetSource = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gusid1 = Gusid.New();
        _gusidEqual = _gusid1;
        _gusidDifferent = Gusid.New();
        while (_gusidDifferent == _gusid1)
            _gusidDifferent = Gusid.New();

        _guid1 = Guid.NewGuid();
        _guidEqual = _guid1;
        _guidDifferent = Guid.NewGuid();
        while (_guidDifferent == _guid1)
            _guidDifferent = Guid.NewGuid();

        _gusidString = _gusid1.ToString();
        _guidString = _guid1.ToString();
        _gusidFormatDestination = new char[32];

        _gusidSetSource = new Gusid[Count];
        _guidSetSource = new Guid[Count];
        for (var i = 0; i < Count; i++)
        {
            _gusidSetSource[i] = Gusid.New();
            _guidSetSource[i] = Guid.NewGuid();
        }
    }

    // =====================================================================
    // Generation — measures identifier creation itself.
    // =====================================================================

    [Benchmark(Description = "Gusid.New()")]
    [BenchmarkCategory("Generation")]
    public Gusid GusidGeneration() => Gusid.New();

    [Benchmark(Description = "Guid.NewGuid()")]
    [BenchmarkCategory("Generation")]
    public Guid GuidGeneration() => Guid.NewGuid();

    // =====================================================================
    // Bulk generation — end-to-end creation of 1,000 identifiers.
    // =====================================================================

    [Benchmark(Description = "Gusid.New() x1000")]
    [BenchmarkCategory("BulkGeneration")]
    public Gusid[] GusidBulkGeneration()
    {
        var values = new Gusid[Count];
        for (var i = 0; i < Count; i++)
            values[i] = Gusid.New();
        return values;
    }

    [Benchmark(Description = "Guid.NewGuid() x1000")]
    [BenchmarkCategory("BulkGeneration")]
    public Guid[] GuidBulkGeneration()
    {
        var values = new Guid[Count];
        for (var i = 0; i < Count; i++)
            values[i] = Guid.NewGuid();
        return values;
    }

    // =====================================================================
    // Pure operations — pre-generated operands; no generation in the loop.
    // =====================================================================

    [Benchmark(Description = "Gusid == Gusid (equal)")]
    [BenchmarkCategory("Operations")]
    public bool GusidEqualityEqual() => _gusid1 == _gusidEqual;

    [Benchmark(Description = "Gusid == Gusid (different)")]
    [BenchmarkCategory("Operations")]
    public bool GusidEqualityDifferent() => _gusid1 == _gusidDifferent;

    [Benchmark(Description = "Guid == Guid (equal)")]
    [BenchmarkCategory("Operations")]
    public bool GuidEqualityEqual() => _guid1 == _guidEqual;

    [Benchmark(Description = "Guid == Guid (different)")]
    [BenchmarkCategory("Operations")]
    public bool GuidEqualityDifferent() => _guid1 == _guidDifferent;

    [Benchmark(Description = "Gusid.CompareTo")]
    [BenchmarkCategory("Operations")]
    public int GusidComparison() => _gusid1.CompareTo(_gusidDifferent);

    [Benchmark(Description = "Guid.CompareTo")]
    [BenchmarkCategory("Operations")]
    public int GuidComparison() => _guid1.CompareTo(_guidDifferent);

    [Benchmark(Description = "Gusid.GetHashCode()")]
    [BenchmarkCategory("Operations")]
    public int GusidHashing() => _gusid1.GetHashCode();

    [Benchmark(Description = "Guid.GetHashCode()")]
    [BenchmarkCategory("Operations")]
    public int GuidHashing() => _guid1.GetHashCode();

    // =====================================================================
    // Parsing
    // =====================================================================

    [Benchmark(Description = "Gusid.Parse")]
    [BenchmarkCategory("Parsing")]
    public Gusid GusidParsing() => Gusid.Parse(_gusidString);

    [Benchmark(Description = "Guid.Parse")]
    [BenchmarkCategory("Parsing")]
    public Guid GuidParsing() => Guid.Parse(_guidString);

    // =====================================================================
    // Serialization
    // =====================================================================

    [Benchmark(Description = "Gusid.ToString()")]
    [BenchmarkCategory("Serialization")]
    public string GusidToString() => _gusid1.ToString();

    [Benchmark(Description = "Guid.ToString()")]
    [BenchmarkCategory("Serialization")]
    public string GuidToString() => _guid1.ToString();

    // =====================================================================
    // HashSet (pure) — insertion of pre-generated values.
    // =====================================================================

    [Benchmark(Description = "HashSet<Gusid>.Add x1000")]
    [BenchmarkCategory("Collections")]
    public HashSet<Gusid> GusidHashSet()
    {
        var set = new HashSet<Gusid>(Count);
        for (var i = 0; i < Count; i++)
            set.Add(_gusidSetSource[i]);
        return set;
    }

    [Benchmark(Description = "HashSet<Guid>.Add x1000")]
    [BenchmarkCategory("Collections")]
    public HashSet<Guid> GuidHashSet()
    {
        var set = new HashSet<Guid>(Count);
        for (var i = 0; i < Count; i++)
            set.Add(_guidSetSource[i]);
        return set;
    }

    // =====================================================================
    // End-to-end workloads — deliberately include generation.
    // =====================================================================

    [Benchmark(Description = "Gusid.New() x1000 + Sort")]
    [BenchmarkCategory("Workloads")]
    public Gusid[] GusidGenerateAndSort()
    {
        var values = new Gusid[Count];
        for (var i = 0; i < Count; i++)
            values[i] = Gusid.New();
        Array.Sort(values);
        return values;
    }

    [Benchmark(Description = "Guid.NewGuid() x1000 + Sort")]
    [BenchmarkCategory("Workloads")]
    public Guid[] GuidGenerateAndSort()
    {
        var values = new Guid[Count];
        for (var i = 0; i < Count; i++)
            values[i] = Guid.NewGuid();
        Array.Sort(values);
        return values;
    }

    [Benchmark(Description = "Gusid.New() x1000 + HashSet dedupe")]
    [BenchmarkCategory("Workloads")]
    public bool GusidGenerateAndDedupe()
    {
        var set = new HashSet<Gusid>(Count);
        for (var i = 0; i < Count; i++)
        {
            if (!set.Add(Gusid.New()))
                return false;
        }
        return true;
    }

    [Benchmark(Description = "Guid.NewGuid() x1000 + HashSet dedupe")]
    [BenchmarkCategory("Workloads")]
    public bool GuidGenerateAndDedupe()
    {
        var set = new HashSet<Guid>(Count);
        for (var i = 0; i < Count; i++)
        {
            if (!set.Add(Guid.NewGuid()))
                return false;
        }
        return true;
    }
}

/// <summary>
/// Pure sorting benchmarks. Kept in a dedicated class so the
/// <see cref="IterationSetup"/> method that restores the pristine unsorted
/// arrays applies only to these benchmarks — sorting mutates its input, so
/// every measured iteration must start from identical data or later
/// iterations would sort an already-sorted array.
/// </summary>
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[MemoryDiagnoser]
[ThreadingDiagnoser]
[RankColumn]
public class SortingBenchmarks
{
    private const int Count = 1000;

    private Gusid[] _gusidSource = null!;
    private Guid[] _guidSource = null!;
    private Gusid[] _gusidWork = null!;
    private Guid[] _guidWork = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Build both arrays from the same seed so the input distributions are
        // as equivalent as possible across the two types.
        var random = new Random(42);
        _gusidSource = new Gusid[Count];
        _guidSource = new Guid[Count];
        for (var i = 0; i < Count; i++)
        {
            var bytes = new byte[16];
            random.NextBytes(bytes);
            _gusidSource[i] = Gusid.Parse(Convert.ToHexString(bytes).ToLowerInvariant());
            _guidSource[i] = new Guid(bytes);
        }

        _gusidWork = new Gusid[Count];
        _guidWork = new Guid[Count];
    }

    // Copies are unmeasured setup work; only Array.Sort runs inside the benchmark.
    [IterationSetup]
    public void ResetArrays()
    {
        Array.Copy(_gusidSource, _gusidWork, Count);
        Array.Copy(_guidSource, _guidWork, Count);
    }

    [Benchmark(Description = "Array.Sort(Gusid[1000])")]
    [BenchmarkCategory("Sorting")]
    public Gusid[] GusidSorting()
    {
        Array.Sort(_gusidWork);
        return _gusidWork;
    }

    [Benchmark(Description = "Array.Sort(Guid[1000])")]
    [BenchmarkCategory("Sorting")]
    public Guid[] GuidSorting()
    {
        Array.Sort(_guidWork);
        return _guidWork;
    }
}
