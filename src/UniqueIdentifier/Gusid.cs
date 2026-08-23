using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace UniqueIdentifier;

/// <summary>
/// Represents a globally unique sequential identifier (Gusid).
/// This struct is optimized to be allocation-free by storing its 16-byte
/// value as four 32-bit unsigned integers instead of a managed byte array.
/// </summary>
public readonly struct Gusid : 
    IComparable, 
    IComparable<Gusid>, 
    IEquatable<Gusid>, 
    IFormattable
{
    // Timestamp cache.
    //
    // DateTimeOffset.UtcNow.ToUnixTimeSeconds() is one of the slowest parts
    // of identifier generation on most platforms: it performs a wall-clock
    // read followed by epoch-conversion arithmetic. Stopwatch.GetTimestamp()
    // is significantly cheaper (a single monotonic counter read with no
    // epoch math), so the expensive conversion is performed at most once per
    // elapsed second and every other call within that second simply reloads
    // the cached value.
    //
    // The monotonic counter also makes the timestamp immune to system clock
    // adjustments: it is non-decreasing for the lifetime of the process,
    // which preserves the sequentiality guarantee of the identifier.
    private static readonly long s_startTimestamp = Stopwatch.GetTimestamp();
    private static readonly long s_baseUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static volatile uint s_cachedTimestamp; // 0 forces a refresh on the very first call
    private static long s_nextSecondBoundary; // read and written via interlocked semantics on 64-bit runtimes

    // Secure generation state.
    //
    // RandomNumberGenerator.Fill performs an OS syscall on every call, which
    // dominates the cost of secure generation. Instead, each thread keeps a
    // buffer of CSPRNG output and refills it only when exhausted. Every byte
    // is drawn from the OS CSPRNG and is used exactly once, so the output
    // remains cryptographically secure while amortizing the syscall cost
    // across hundreds of identifiers.
    private const int RandomBytesPerId = 12;
    private const int SecureBufferSize = 512 * RandomBytesPerId; // 512 IDs per OS refill

    [ThreadStatic]
    private static byte[]? t_secureBuffer;

    [ThreadStatic]
    private static int t_secureOffset;

    // The 16 bytes are stored internally as four 32-bit unsigned integers.
    // This makes Gusid a true 16-byte value type, avoiding heap
    // allocations for its internal state.
    private readonly uint _a;
    private readonly uint _b;
    private readonly uint _c;
    private readonly uint _d;

    /// <summary>
    /// Initializes a new instance of the <see cref="Gusid"/> struct.
    /// This private constructor is used by factory methods like New() and TryParse()
    /// to directly create a Gusid from its constituent 32-bit parts.
    /// </summary>
    private Gusid(uint a, uint b, uint c, uint d)
    {
        _a = a;
        _b = b;
        _c = c;
        _d = d;
    }

    /// <summary>
    /// Generates a new Gusid (Globally Unique Sequential Identifier).
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="Gusid"/> containing a unique identifier.
    /// </returns>
    /// <remarks>
    /// This method is allocation-free (after the first call on each thread) and
    /// lock-free. Random bytes are drawn from the operating system's CSPRNG. To avoid an
    /// expensive syscall per identifier, each thread maintains a buffer of
    /// CSPRNG output that is refilled only when exhausted (once per 512 IDs).
    /// Every random byte is used exactly once, preserving the security
    /// properties of the underlying generator.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Gusid New()
    {
        var buffer = t_secureBuffer;
        var offset = t_secureOffset;

        if (buffer is null || offset + RandomBytesPerId > buffer.Length)
        {
            if (buffer is null)
            {
                buffer = new byte[SecureBufferSize];
                t_secureBuffer = buffer;
            }

            RandomNumberGenerator.Fill(buffer);
            offset = 0;
        }

        // Reinterpret the 12 buffered bytes as three uints without any
        // bounds checking or copying.
        var randomUInts = MemoryMarshal.Cast<byte, uint>(
            buffer.AsSpan(offset, RandomBytesPerId));

        t_secureOffset = offset + RandomBytesPerId;

        return new Gusid(GetTimestamp(), randomUInts[0], randomUInts[1], randomUInts[2]);
    }

    /// <summary>
    /// Returns the current Unix time in seconds, served from a process-wide
    /// cache that is refreshed at most once per elapsed second.
    /// </summary>
    /// <remarks>
    /// The fast path is a cheap monotonic counter read plus two volatile
    /// reads, avoiding the wall-clock and epoch-conversion work of
    /// <see cref="DateTimeOffset.UtcNow"/> on every identifier.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetTimestamp()
    {
        var now = Stopwatch.GetTimestamp();
        var timestamp = s_cachedTimestamp;
        // A benign data race here is harmless: concurrent refreshes compute
        // the same second value, and a slightly late refresh only means the
        // wall-clock read happens one call later.
        if (timestamp == 0 || now >= Interlocked.Read(ref s_nextSecondBoundary))
        {
            return RefreshTimestamp(now);
        }
        return timestamp;
    }

    /// <summary>
    /// Slow path for <see cref="GetTimestamp"/>: performs the epoch
    /// conversion once and caches the result together with the monotonic
    /// counter value at which the current second expires.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint RefreshTimestamp(long now)
    {
        var elapsed = now - s_startTimestamp;
        var frequency = Stopwatch.Frequency;
        var wholeSeconds = elapsed / frequency;
        var remainder = elapsed % frequency;

        var timestamp = (uint)(s_baseUnixSeconds + wholeSeconds);
        s_cachedTimestamp = timestamp;
        // Use Interlocked.Exchange so the 64-bit write is atomic on 32-bit
        // runtimes and participates in the same happens-before edge that
        // protects the volatile timestamp write above.
        Interlocked.Exchange(ref s_nextSecondBoundary, now + (frequency - remainder));
        return timestamp;
    }

    /// <summary>
    /// Converts the string representation of a Gusid to its <see cref="Gusid"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the Gusid to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored for hex parsing).</param>
    /// <returns>A <see cref="Gusid"/> equivalent to the Gusid contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when the Gusid is not in the correct 32-character hex format.</exception>
    public static Gusid Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("Invalid Gusid format. Expected a 32-character lowercase hexadecimal string.");
    }

    /// <summary>
    /// Converts the string representation of a Gusid to its <see cref="Gusid"/> equivalent.
    /// This method is allocation-free.
    /// </summary>
    /// <param name="s">A string containing the Gusid to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored for hex parsing).</param>
    /// <param name="result">When this method returns, contains the <see cref="Gusid"/> equivalent of the Gusid contained in <paramref name="s"/>, if the conversion succeeded, or default if the conversion failed.</param>
    /// <returns><see langword="true"/> if the Gusid was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Gusid result)
    {
        const int HexLength = 16 * 2; // 16 bytes == 32 hex characters
        if (s.Length != HexLength)
        {
            result = default;
            return false;
        }
        
        // Parse the hex string directly into four uints, avoiding byte array allocation.
        // Slices are used to parse each 8-character segment of the string.
        if (uint.TryParse(s.Slice(0, 8), NumberStyles.HexNumber, provider, out var a) &&
            uint.TryParse(s.Slice(8, 8), NumberStyles.HexNumber, provider, out var b) &&
            uint.TryParse(s.Slice(16, 8), NumberStyles.HexNumber, provider, out var c) &&
            uint.TryParse(s.Slice(24, 8), NumberStyles.HexNumber, provider, out var d))
        {
            result = new Gusid(a, b, c, d);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// This operation is highly efficient as it's a direct field comparison.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Gusid other) => _a == other._a && _b == other._b && _c == other._c && _d == other._d;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj) => obj is Gusid other && Equals(other);

    /// <summary>
    /// Returns a 32-character lowercase hexadecimal string representation of the <see cref="Gusid"/>.
    /// This method is allocation-free except for the final string object.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        // Allocate the required 32 characters on the stack.
        Span<char> buffer = stackalloc char[16 * 2];
        
        // Use the efficient TryFormat to write each part of the Gusid into the buffer.
        // "x8" ensures an 8-digit lowercase hexadecimal representation, padding with zeros if needed.
        _a.TryFormat(buffer, out _, "x8");
        _b.TryFormat(buffer[8..], out _, "x8");
        _c.TryFormat(buffer[16..], out _, "x8");
        _d.TryFormat(buffer[24..], out _, "x8");

        return new string(buffer);
    }
    
    /// <summary>
    /// Returns a 32-character lowercase hexadecimal string representation of the <see cref="Gusid"/>.
    /// </summary>
    public override string ToString() => ToString("x", null);

    /// <summary>
    /// Compares the current instance with another <see cref="Gusid"/>.
    /// This operation is highly efficient and leverages the timestamp for sequential sorting.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Gusid other)
    {
        // Compare the timestamp first for sorting.
        var aComparison = _a.CompareTo(other._a);
        if (aComparison != 0) return aComparison;
        
        // If timestamps are equal, compare the remaining random parts.
        if (_b != other._b) return _b.CompareTo(other._b);
        if (_c != other._c) return _c.CompareTo(other._c);
        return _d.CompareTo(other._d);
    }

    /// <summary>
    /// Compares the current instance with another object.
    /// </summary>
    public int CompareTo(object? obj)
    {
        if (obj is Gusid other)
            return CompareTo(other);

        throw new ArgumentException("Object is not a Gusid.");
    }
    
    /// <summary>
    /// Returns a hash code for the current <see cref="Gusid"/>.
    /// This operation is highly efficient by combining the hash codes of the internal fields.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        // The HashCode struct provides a high-quality way to combine hash codes.
        // As a struct, this operation is allocation-free.
        var hash = new HashCode();
        hash.Add(_a);
        hash.Add(_b);
        hash.Add(_c);
        hash.Add(_d);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(Gusid left, Gusid right) => left.Equals(right);
    /// <inheritdoc/>
    public static bool operator !=(Gusid left, Gusid right) => !(left == right);
    /// <inheritdoc/>
    public static bool operator <(Gusid left, Gusid right) => left.CompareTo(right) < 0;
    /// <inheritdoc/>
    public static bool operator <=(Gusid left, Gusid right) => left.CompareTo(right) <= 0;
    /// <inheritdoc/>
    public static bool operator >(Gusid left, Gusid right) => left.CompareTo(right) > 0;
    /// <inheritdoc/>
    public static bool operator >=(Gusid left, Gusid right) => left.CompareTo(right) >= 0;
}
