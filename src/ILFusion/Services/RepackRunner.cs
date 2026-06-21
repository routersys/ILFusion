namespace ILFusion.Services;

using System.Text;
using ILFusion.Models;

sealed class RepackRunner(IProcessRunner processRunner, string executablePath = "ILRepack") : IRepackRunner
{
    public async Task<RepackResult> RunAsync(MergeConfiguration config, CancellationToken cancellationToken = default)
    {
        var arguments = BuildArguments(config);
        var (exitCode, output, error) = await processRunner.RunAsync(executablePath, arguments, cancellationToken);
        return new RepackResult(exitCode == 0, output, error);
    }

    private static string BuildArguments(MergeConfiguration config)
    {
        var sb = new StringBuilder();
        sb.Append($"/out:{Quote(config.OutputPath)}");

        if (config.Options.InternalizeAssemblies)
            sb.Append(" /internalize");

        if (config.Options.SuppressDebugInfo)
            sb.Append(" /ndebug");

        if (config.Options.UnionMerge)
            sb.Append(" /union");

        if (config.Options.LibraryPaths is { Count: > 0 } libPaths)
            foreach (var libPath in libPaths)
                sb.Append($" /lib:{Quote(libPath)}");

        sb.Append($" {Quote(config.PrimaryAssembly.Path)}");

        foreach (var assembly in config.SecondaryAssemblies)
            sb.Append($" {Quote(assembly.Path)}");

        return sb.ToString();
    }

    private static string Quote(string path) => $"\"{path.Replace("\"", "\\\"")}\"";
}
