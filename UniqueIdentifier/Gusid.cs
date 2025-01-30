namespace UniqueIdentifier;

public readonly struct Gusid :
    IComparable, IComparable<Gusid>,
    IEquatable<Gusid>
{
    private const int Size = 16; // 128 bits (16 bytes)
    private readonly byte[] _value;


    // Private constructor to enforce creation via factory methods
    private Gusid(byte[] value) : this()
    {
        if (value.Length != Size)
            throw new ArgumentException($"Gusid must be {Size} bytes long.", nameof(value));
        _value = value;
    }

    // Factory method to create a new Gusid
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

        return new Gusid(value.ToArray()); // Convert Span to array
    }

    // parse from readonlyspan<char>
    public static Gusid Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("invalid gusid format.");
    }

    // TryParse from ReadOnlySpan<char>
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

    // Equals methods
    public bool Equals(Gusid other) => _value.AsSpan().SequenceEqual(other._value.AsSpan());

    public override bool Equals(object? obj) => obj is Gusid other && Equals(other);

    public readonly override string ToString()
    {
        return ToString(null, null);
    }

    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        return Convert.ToHexStringLower(_value);
    }

    // CompareTo methods
    public int CompareTo(object? obj)
    {
        if (obj is Gusid other)
            return CompareTo(other);

        throw new ArgumentException("Object is not a Gusid.");
    }

    public int CompareTo(Gusid other)
    {
        for (int i = 0; i < Size; i++)
        {
            if (_value[i] < other._value[i]) return -1;
            if (_value[i] > other._value[i]) return 1;
        }
        return 0;
    }

    // GetHashCode
    public override int GetHashCode() => HashCode.Combine(_value[0], _value[1], _value[2], _value[3]);

    // Operators
    public static bool operator ==(Gusid left, Gusid right) => left.Equals(right);
    public static bool operator !=(Gusid left, Gusid right) => !(left == right);
    public static bool operator <(Gusid left, Gusid right) => left.CompareTo(right) < 0;
    public static bool operator <=(Gusid left, Gusid right) => left.CompareTo(right) <= 0;
    public static bool operator >(Gusid left, Gusid right) => left.CompareTo(right) > 0;
    public static bool operator >=(Gusid left, Gusid right) => left.CompareTo(right) >= 0;
}