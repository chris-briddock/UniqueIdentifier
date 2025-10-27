using System;
using System.Globalization;

namespace UniqueIdentifier; // Or a namespace of your choice

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
    /// This method is allocation-free. The identifier is composed of a 4-byte
    /// timestamp (seconds since Unix epoch) followed by 12 random bytes
    /// (split into three 4-byte chunks), ensuring both uniqueness and sequentiality.
    /// The first uint (_a) is the timestamp, making sorting by Gusid equivalent to sorting by creation time.
    /// </remarks>
    public static Gusid New()
    {
        // Get a 4-byte timestamp stored directly as a uint.
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Span<byte> randomBytes = stackalloc byte[12];

        // Get 12 random bytes on the stack.
        Random.Shared.NextBytes(randomBytes);

        // Convert the 12 random bytes into three 32-bit unsigned integers.
        var r1 = BitConverter.ToUInt32(randomBytes.Slice(0, 4));
        var r2 = BitConverter.ToUInt32(randomBytes.Slice(4, 4));
        var r3 = BitConverter.ToUInt32(randomBytes.Slice(8, 4));

        return new Gusid(timestamp, r1, r2, r3);
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