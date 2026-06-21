namespace ILFusion.Tests.Models;

using ILFusion.Models;

public sealed class AssemblyEntryTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(500, "500 B")]
    [InlineData(1_023, "1023 B")]
    [InlineData(1_024, "1.0 KB")]
    [InlineData(1_536, "1.5 KB")]
    [InlineData(1_048_575, "1024.0 KB")]
    [InlineData(1_048_576, "1.0 MB")]
    [InlineData(2_621_440, "2.5 MB")]
    public void FormattedSize_ReturnsCorrectUnit(long bytes, string expected)
    {
        var entry = new AssemblyEntry("/test.dll", "test.dll", bytes);

        Assert.Equal(expected, entry.FormattedSize);
    }

    [Fact]
    public void Record_EqualityByValue()
    {
        var a = new AssemblyEntry("/path/a.dll", "a.dll", 100);
        var b = new AssemblyEntry("/path/a.dll", "a.dll", 100);

        Assert.Equal(a, b);
    }
}
