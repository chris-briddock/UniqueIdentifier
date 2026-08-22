# UniqueIdentifier (Gusid)

A globally unique, sortable, 16-byte identifier for .NET, stored as four `uint`s so it is fully allocation-free.

A `Gusid` is composed of a 4-byte Unix timestamp (seconds) followed by 12 random bytes, so sorting by `Gusid` is equivalent to sorting by creation time.

```csharp
var id       = Gusid.New();      // fast, non-cryptographic randomness
var secure   = Gusid.New(true);  // cryptographically secure (buffered OS CSPRNG)
var text     = id.ToString();    // 32-char lowercase hex
var parsed   = Gusid.Parse(text);
```

## Performance

Generation is now faster than `Guid.NewGuid()` on both paths. Timestamp reads are cached (refreshed once per second via a monotonic counter), and secure generation amortizes the OS CSPRNG syscall across 512 identifiers per thread.

| Operation                | Gusid       | `Guid.NewGuid()` |
|--------------------------|-------------|------------------|
| Generation (insecure)    | ~72 ns/op   | ~590 ns/op       |
| Generation (secure)      | ~45 ns/op   | ~590 ns/op       |

Numbers are medians from a Release-mode microbenchmark on .NET 10 (Linux x64); treat them as indicative only. Older benchmark tables have been removed because they were stale.

To reproduce the full benchmark suite (generation, parsing, comparison, sorting, hashing, uniqueness):

```bash
dotnet run -c Release --project src/UniqueIdentifier.Benchmarks
```

## Project layout

- `src/UniqueIdentifier` — the library (targets `netstandard2.1`)
- `src/UniqueIdentifier.Tests` — unit tests
- `src/UniqueIdentifier.Benchmarks` — BenchmarkDotNet suite
- `src/UniqueIdentifier.Console` — sample console app
