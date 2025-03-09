namespace System;

/// <summary>
/// Represents a globally unique sequential identifier.
/// </summary>
public readonly struct Gusid : IComparable, IComparable<Gusid>, IEquatable<Gusid>
{
    private const int Size = 16;
    private readonly byte[] _value;
    /// <summary>
    /// Initializes a new instance of the <see cref="Gusid"/>
    /// </summary>
    /// <param name="value">An array of bytes used to store the byte values that are then converted to a string.</param>
    /// <exception cref="ArgumentException">Thrown when the value is not 16 bytes long.</exception>
    private Gusid(byte[] value) : this()
    {
        if (value.Length != Size)
            throw new ArgumentException($"Gusid must be {Size} bytes long.", nameof(value));
        _value = value;
    }

    /// <summary>
    /// Generates a new Gusid (Globally Unique Sequential Identifier).
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="Gusid"/> containing a unique identifier.
    /// </returns>
    /// <remarks>
    /// The identifier is composed of a 4-byte timestamp (seconds since Unix epoch) 
    /// followed by 12 random bytes, ensuring both uniqueness and sequentiality.
    /// </remarks>
    public static Gusid New()
    {
        Span<byte> value = stackalloc byte[Size];
        Span<byte> random = stackalloc byte[12];
        Span<byte> timeStampBytes = stackalloc byte[4];

        // Get timestamp
        timeStampBytes = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        // Get random bytes
        Random.Shared.NextBytes(random);

        // Combine timestamp and random bytes
        timeStampBytes.CopyTo(value);
        random.CopyTo(value[4..]);

        return new Gusid(value.ToArray());
    }

    /// <summary>
    /// Converts the string representation of a Gusid to its <see cref="Gusid"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the Gusid to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="Gusid"/> equivalent to the Gusid contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when the Gusid is not in the correct format.</exception>
    public static Gusid Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("invalid gusid format.");
    }

    /// <summary>
    /// Converts the string representation of a Gusid to its <see cref="Gusid"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the Gusid to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="Gusid"/> equivalent of the Gusid contained in <paramref name="s"/>, if the conversion succeeded, or default if the conversion failed.</param>
    /// <returns><see langword="true"/> if the Gusid was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Gusid result)
    {
        if (s.Length != Size * 2) // Each byte is represented as 2 hex characters
        {
            result = default;
            return false;
        }

        var bytes = new byte[Size];
        for (int i = 0; i < Size; i++)
        {
            if (!byte.TryParse(s.Slice(i * 2, 2), System.Globalization.NumberStyles.HexNumber, provider, out bytes[i]))
            {
                result = default;
                return false;
            }
        }

        result = new Gusid(bytes);
        return true;
    }

    /// <summary>
    /// Converts the string representation of a Gusid to its <see cref="Gusid"/> equivalent.
    /// </summary>
    /// <param name="other">A string containing the Gusid to convert.</param>
    /// <returns>A <see cref="Gusid"/> equivalent to the Gusid contained in <paramref name="other"/>.</returns>
    public bool Equals(Gusid other) => _value.AsSpan().SequenceEqual(other._value.AsSpan());

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is Gusid other && Equals(other);

    /// <summary>
    /// Returns a string representation of the <see cref="Gusid"/>
    /// </summary>
    /// <param name="format">
    /// A format string. 
    // </param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.  
    /// </param>
    /// <returns>A string representation of the <see cref="Gusid"/></returns>
    public readonly override string ToString()
    {
        return Convert.ToHexStringLower(_value);
    }

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the object is not a Gusid. 
    /// </exception>
    public int CompareTo(object? obj)
    {
        if (obj is Gusid other)
            return CompareTo(other);

        throw new ArgumentException("Object is not a Gusid.");
    }
    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared.
    /// </returns>
    public int CompareTo(Gusid other)
    {
        for (int i = 0; i < Size; i++)
        {
            if (_value[i] < other._value[i]) return -1;
            if (_value[i] > other._value[i]) return 1;
        }
        return 0;
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

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.AddBytes(_value);
        return hash.ToHashCode();
    }
}