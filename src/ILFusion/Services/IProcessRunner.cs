namespace ILFusion.Services;

interface IProcessRunner
{
    Task<(int ExitCode, string Output, string Error)> RunAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken = default);
}
