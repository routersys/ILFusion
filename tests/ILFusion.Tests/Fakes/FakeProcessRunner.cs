namespace ILFusion.Tests.Fakes;

using ILFusion.Services;

sealed class FakeProcessRunner(int exitCode = 0, string output = "", string error = "") : IProcessRunner
{
    public string? LastFileName { get; private set; }
    public string? LastArguments { get; private set; }
    public int CallCount { get; private set; }

    public Task<(int ExitCode, string Output, string Error)> RunAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        LastFileName = fileName;
        LastArguments = arguments;
        CallCount++;
        return Task.FromResult((exitCode, output, error));
    }
}
