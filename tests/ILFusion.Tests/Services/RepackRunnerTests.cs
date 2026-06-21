namespace ILFusion.Tests.Services;

using ILFusion.Models;
using ILFusion.Services;
using ILFusion.Tests.Fakes;

public sealed class RepackRunnerTests
{
    private static MergeConfiguration BasicConfig(MergeOptions? options = null) => new(
        OutputPath: "/out/merged.dll",
        PrimaryAssembly: new AssemblyEntry("/path/primary.dll", "primary.dll", 100),
        SecondaryAssemblies: [new AssemblyEntry("/path/secondary.dll", "secondary.dll", 200)],
        Options: options ?? new MergeOptions());

    [Fact]
    public async Task RunAsync_ExitCodeZero_ReturnsSuccess()
    {
        var fake = new FakeProcessRunner(exitCode: 0, output: "Merged.");
        var sut = new RepackRunner(fake);

        var result = await sut.RunAsync(BasicConfig());

        Assert.True(result.Success);
        Assert.Equal("Merged.", result.Output);
    }

    [Fact]
    public async Task RunAsync_NonZeroExitCode_ReturnsFailure()
    {
        var fake = new FakeProcessRunner(exitCode: 1, error: "Assembly not found");
        var sut = new RepackRunner(fake);

        var result = await sut.RunAsync(BasicConfig());

        Assert.False(result.Success);
        Assert.Equal("Assembly not found", result.Error);
    }

    [Fact]
    public async Task RunAsync_ArgumentsIncludeOutputPath()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig());

        Assert.Contains("/out:\"/out/merged.dll\"", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_ArgumentsIncludePrimaryAssembly()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig());

        Assert.Contains("\"/path/primary.dll\"", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_ArgumentsIncludeSecondaryAssemblies()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig());

        Assert.Contains("\"/path/secondary.dll\"", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_InternalizeOption_AppendsFlag()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig(new MergeOptions(InternalizeAssemblies: true)));

        Assert.Contains("/internalize", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_SuppressDebugOption_AppendsNdebugFlag()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig(new MergeOptions(SuppressDebugInfo: true)));

        Assert.Contains("/ndebug", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_UnionMergeOption_AppendsFlag()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig(new MergeOptions(UnionMerge: true)));

        Assert.Contains("/union", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_DefaultOptions_OmitsOptionalFlags()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig());

        Assert.DoesNotContain("/internalize", fake.LastArguments);
        Assert.DoesNotContain("/ndebug", fake.LastArguments);
        Assert.DoesNotContain("/union", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_CustomExecutablePath_UsedForProcess()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake, "custom-ilrepack");

        await sut.RunAsync(BasicConfig());

        Assert.Equal("custom-ilrepack", fake.LastFileName);
    }

    [Fact]
    public async Task RunAsync_PathWithSpaces_WrapsInQuotes()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);
        var config = new MergeConfiguration(
            "C:/my output/merged.dll",
            new AssemblyEntry("C:/my path/primary.dll", "primary.dll", 0),
            [],
            new MergeOptions());

        await sut.RunAsync(config);

        Assert.Contains("/out:\"C:/my output/merged.dll\"", fake.LastArguments);
        Assert.Contains("\"C:/my path/primary.dll\"", fake.LastArguments);
    }

    [Fact]
    public async Task RunAsync_PrimaryAssemblyAppearsBeforeSecondaries()
    {
        var fake = new FakeProcessRunner();
        var sut = new RepackRunner(fake);

        await sut.RunAsync(BasicConfig());

        var primaryPos = fake.LastArguments!.IndexOf("primary.dll", StringComparison.Ordinal);
        var secondaryPos = fake.LastArguments.IndexOf("secondary.dll", StringComparison.Ordinal);
        Assert.True(primaryPos < secondaryPos);
    }
}
