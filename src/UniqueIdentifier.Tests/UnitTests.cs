using System.Globalization;

namespace UniqueIdentifier.Tests;

public class Tests
{
    [Test]
    public void New_GeneratesUniqueIds_WhenCalledMultipleTimes()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();

        Assert.That(gusid1, Is.Not.EqualTo(gusid2));
    }

    [Test]
    public void New_GeneratesValidIds()
    {
        var gusid = Gusid.New();
        Assert.That(gusid.ToString().Length / 2, Is.EqualTo(16));
    }

    [Test]
    public void Parse_ValidHexString_ReturnsGusid()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var gusid = Gusid.Parse(hexString);

        Assert.That(hexString, Is.EqualTo(gusid.ToString()));
    }

    [Test]
    public void Parse_InvalidHexString_ThrowsFormatException()
    {
        var invalidHexString = "invalidhexstring";

        Assert.Throws<FormatException>(() => Gusid.Parse(invalidHexString));
    }

    [Test]
    public void TryParse_ValidHexString_ReturnsTrueAndGusid()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var result = Gusid.TryParse(hexString, CultureInfo.InvariantCulture, out var gusid);

        Assert.That(result, Is.True);
        Assert.That(hexString, Is.EqualTo(gusid.ToString()));
    }

    [Test]
    public void TryParse_InvalidHexString_ReturnsFalse()
    {
        var invalidHexString = "invalidhexstring";
        var result = Gusid.TryParse(invalidHexString, CultureInfo.InvariantCulture, out var gusid);

        Assert.That(result, Is.False);
        Assert.That(default(Gusid), Is.EqualTo(gusid));
    }

    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var gusid1 = Gusid.Parse(hexString);
        var gusid2 = Gusid.Parse(hexString);

        Assert.That(gusid1.Equals(gusid2), Is.True);
    }

    [Test]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();

        Assert.That(gusid1.Equals(gusid2), Is.False);
    }

    [Test]
    public void CompareTo_SameValues_ReturnsZero()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var gusid1 = Gusid.Parse(hexString);
        var gusid2 = Gusid.Parse(hexString);

        Assert.That(gusid1.CompareTo(gusid2), Is.EqualTo(0));
    }

    [Test]
    public void CompareTo_LesserValue_ReturnsNegative()
    {
        var gusid1 = Gusid.Parse("00000000000000000000000000000000");
        var gusid2 = Gusid.Parse("00000000000000000000000000000001");

        Assert.That(gusid1.CompareTo(gusid2), Is.LessThan(0));
    }

    [Test]
    public void CompareTo_GreaterValue_ReturnsPositive()
    {
        var gusid1 = Gusid.Parse("00000000000000000000000000000001");
        var gusid2 = Gusid.Parse("00000000000000000000000000000000");

        Assert.That(gusid1.CompareTo(gusid2), Is.GreaterThan(0));
    }

    [Test]
    public void CompareTo_WithNull_ThrowsArgumentException()
    {
        var gusid = Gusid.New();

        Assert.Throws<ArgumentException>(() => gusid.CompareTo(null));
    }

    [Test]
    public void CompareTo_WithNonGusidObject_ThrowsArgumentException()
    {
        var gusid = Gusid.New();
        var nonGusidObject = new object();

        Assert.Throws<ArgumentException>(() => gusid.CompareTo(nonGusidObject));
    }

    [Test]
    public void CompareTo_DifferentValues_ReturnsNonZero()
    {
        var gusid1 = Gusid.Parse("00000000000000000000000000000000");
        var gusid2 = Gusid.Parse("00000000000000000000000000000001");

        Assert.That(gusid1.CompareTo(gusid2), Is.LessThan(0));
        Assert.That(gusid2.CompareTo(gusid1), Is.GreaterThan(0));
    }

    [Test]
    public void Equals_WithNull_ReturnsFalse()
    {
        var gusid = Gusid.New();
        Assert.That(gusid.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var hashcode1 = Gusid.Parse(hexString).GetHashCode();
        var hashcode2 = Gusid.Parse(hexString).GetHashCode();

        Assert.That(hashcode1, Is.EqualTo(hashcode2));
    }

    [Test]
    public void GetHashCode_DifferentValues_ReturnsDifferentHashCode()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();

        Assert.That(gusid1.GetHashCode(), Is.Not.EqualTo(gusid2.GetHashCode()));
    }

    [Test]
    public void OperatorEquals_SameValues_ReturnsTrue()
    {
        var hexString = "0123456789abcdef0123456789abcdef";
        var gusid1 = Gusid.Parse(hexString);
        var gusid2 = Gusid.Parse(hexString);

        Assert.That(gusid1 == gusid2, Is.True);
    }

    [Test]
    public void OperatorEquals_DifferentValues_ReturnsFalse()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();

        Assert.That(gusid1 == gusid2, Is.False);
    }

    [Test]
    public void OperatorNotEquals_DifferentValues_ReturnsTrue()
    {
        var gusid1 = Gusid.New();
        var gusid2 = Gusid.New();

        Assert.That(gusid1 != gusid2, Is.True);
    }

    [Test]
    public void OperatorLessThan_LesserValue_ReturnsTrue()
    {
        var gusid1 = Gusid.Parse("00000000000000000000000000000000");
        var gusid2 = Gusid.Parse("00000000000000000000000000000001");

        Assert.That(gusid1 < gusid2, Is.True);
    }

    [Test]
    public void OperatorLessThan_GreaterValue_ReturnsFalse()
    {
        var gusid1 = Gusid.Parse("1123456789abcdef0123456789abcdef");
        var gusid2 = Gusid.Parse("0123456789abcdef0123456789abcdef");

        Assert.That(gusid1 < gusid2, Is.False);
    }

    [Test]
    public void OperatorLessThanOrEqual_LesserValue_ReturnsTrue()
    {
        var gusid1 = Gusid.Parse("0123456789abcdef0123456789abcdef");
        var gusid2 = Gusid.Parse("1123456789abcdef0123456789abcdef");

        Assert.That(gusid1 <= gusid2, Is.True);
    }

    [Test]
    public void Parse_TooLongHexString_ThrowsFormatException()
    {
        var longHexString = new string('a', 65); // 65 characters long
        Assert.Throws<FormatException>(() => Gusid.Parse(longHexString));
    }

    [Test]
    public void Parse_TooShortHexString_ThrowsFormatException()
    {
        var shortHexString = "123"; // 3 characters long
        Assert.Throws<FormatException>(() => Gusid.Parse(shortHexString));
    }

    [Test]
    public void Parse_CaseInsensitiveHexString_ReturnsGusid()
    {
        var hexString = "0123456789ABCDEF0123456789abcdef";
        var gusid = Gusid.Parse(hexString);

        Assert.That(hexString.ToLower(), Is.EqualTo(gusid.ToString().ToLower()));
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var gusid = Gusid.New();
        var stringRepresentation = gusid.ToString();

        Assert.That(stringRepresentation.Length, Is.EqualTo(32)); // Assuming 32 characters for a valid hex string
        Assert.That(System.Text.RegularExpressions.Regex.IsMatch(stringRepresentation, @"\A\b[0-9a-fA-F]+\b\Z"), Is.True); // Check if it's a valid hex string
    }

    [Test]
    public void CompareTo_SameInstance_ReturnsZero()
    {
        var gusid = Gusid.New();
        Assert.That(gusid.CompareTo(gusid), Is.EqualTo(0));
    }

    [Test]
    public void New_GeneratesManyIds_PerformanceTest()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 5_000_000; i++)
        {
            var gusid = Gusid.New();
        }
        stopwatch.Stop();
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000));
    }

}
