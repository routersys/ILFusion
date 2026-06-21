namespace ILFusion.Tests.Services;

using ILFusion.Services;

public sealed class AssemblyDiscoveryServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AssemblyDiscoveryService _sut;

    public AssemblyDiscoveryServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _sut = new AssemblyDiscoveryService();
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void Discover_ReturnsDllsAndExes()
    {
        CreateFile("a.dll");
        CreateFile("b.exe");
        CreateFile("c.txt");

        var result = _sut.Discover(_tempDir);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Name == "a.dll");
        Assert.Contains(result, a => a.Name == "b.exe");
    }

    [Fact]
    public void Discover_ExcludesNonAssemblyFiles()
    {
        CreateFile("config.json");
        CreateFile("readme.md");
        CreateFile("lib.pdb");

        var result = _sut.Discover(_tempDir);

        Assert.Empty(result);
    }

    [Fact]
    public void Discover_ReturnsSortedByNameCaseInsensitive()
    {
        CreateFile("Zeta.dll");
        CreateFile("alpha.dll");
        CreateFile("Middle.dll");

        var result = _sut.Discover(_tempDir);

        Assert.Equal(["alpha.dll", "Middle.dll", "Zeta.dll"], result.Select(a => a.Name));
    }

    [Fact]
    public void Discover_ReturnsCorrectFileSize()
    {
        File.WriteAllBytes(Path.Combine(_tempDir, "test.dll"), new byte[1024]);

        var result = _sut.Discover(_tempDir);

        Assert.Single(result);
        Assert.Equal(1024, result[0].SizeBytes);
    }

    [Fact]
    public void Discover_EmptyDirectory_ReturnsEmpty()
    {
        var result = _sut.Discover(_tempDir);

        Assert.Empty(result);
    }

    [Fact]
    public void Discover_CaseInsensitiveExtensions()
    {
        CreateFile("A.DLL");
        CreateFile("B.EXE");

        var result = _sut.Discover(_tempDir);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Discover_ReturnsAbsolutePaths()
    {
        CreateFile("test.dll");

        var result = _sut.Discover(_tempDir);

        Assert.All(result, a => Assert.True(Path.IsPathRooted(a.Path)));
    }

    [Fact]
    public void Discover_DoesNotRecurseIntoSubdirectories()
    {
        CreateFile("root.dll");
        var subDir = Directory.CreateDirectory(Path.Combine(_tempDir, "sub"));
        File.WriteAllBytes(Path.Combine(subDir.FullName, "nested.dll"), []);

        var result = _sut.Discover(_tempDir);

        Assert.Single(result);
        Assert.Equal("root.dll", result[0].Name);
    }

    private void CreateFile(string name, int sizeBytes = 0) =>
        File.WriteAllBytes(Path.Combine(_tempDir, name), new byte[sizeBytes]);
}
