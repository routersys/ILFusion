namespace ILFusion;

using ILFusion.Models;
using ILFusion.Services;
using ILFusion.UI;

sealed class Application(
    IAssemblyDiscoveryService discoveryService,
    IRepackRunner repackRunner,
    MultiSelector selector,
    IConsoleIO console)
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        WriteBanner();

        var inputPath = args is [var first, ..] ? first : PromptPath();

        if (!TryResolvePaths(inputPath, out var directory, out var primaryAssembly))
        {
            WriteError($"パスが見つかりません: {inputPath}");
            return ExitCodes.InvalidPath;
        }

        var assemblies = discoveryService.Discover(directory);

        if (assemblies.Count == 0)
        {
            WriteError("対象ディレクトリにアセンブリが見つかりませんでした");
            return ExitCodes.NoAssembliesFound;
        }

        console.WriteLine();
        WriteInfo($"ディレクトリ: {directory}  ({assemblies.Count} 件検出)");
        console.WriteLine();

        var selected = selector.Select(assemblies, primaryAssembly);

        console.WriteLine();

        if (selected.Count < 2)
        {
            WriteError("結合するには2つ以上のアセンブリを選択してください");
            return ExitCodes.InsufficientSelection;
        }

        var outputPath = PromptOutput(directory, selected[0]);
        var options = PromptOptions();

        var config = new MergeConfiguration(
            outputPath,
            selected[0],
            [.. selected.Skip(1)],
            options);

        console.WriteLine();
        WriteInfo("ILRepack を実行中...");

        var result = await repackRunner.RunAsync(config, cancellationToken);

        console.WriteLine();

        if (result.Success)
        {
            WriteSuccess($"結合が完了しました → {outputPath}");
            if (!string.IsNullOrWhiteSpace(result.Output))
                console.WriteLine(result.Output);
            return ExitCodes.Success;
        }

        WriteError("結合に失敗しました");
        if (!string.IsNullOrWhiteSpace(result.Error))
            console.WriteLine(result.Error);
        if (!string.IsNullOrWhiteSpace(result.Output))
            console.WriteLine(result.Output);
        return ExitCodes.RepackFailed;
    }

    private void WriteBanner()
    {
        console.ForegroundColor = ConsoleColor.Cyan;
        console.WriteLine("╔══════════════════════════════════════════╗");
        console.WriteLine("║           ILFusion  v1.0                 ║");
        console.WriteLine("║     .NET アセンブリ結合ツール            ║");
        console.WriteLine("╚══════════════════════════════════════════╝");
        console.ResetColor();
        console.WriteLine();
    }

    private string PromptPath()
    {
        WriteInfo("ディレクトリまたはアセンブリのパスを入力してください:");
        console.Write("> ");
        return console.ReadLine() ?? string.Empty;
    }

    private string PromptOutput(string directory, AssemblyEntry primary)
    {
        var stem = Path.GetFileNameWithoutExtension(primary.Name);
        var ext = Path.GetExtension(primary.Name);
        var defaultOutput = Path.Combine(directory, $"{stem}.merged{ext}");
        console.WriteLine();
        WriteInfo("出力ファイルのパスを入力してください:");
        console.ForegroundColor = ConsoleColor.DarkGray;
        console.WriteLine($"  (未入力の場合: {defaultOutput})");
        console.ResetColor();
        console.Write("> ");
        var input = console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? defaultOutput : input;
    }

    private MergeOptions PromptOptions()
    {
        console.WriteLine();
        WriteInfo("オプション設定 (y/N):");
        var internalize = PromptYesNo("  内部化する (/internalize)");
        var suppressDebug = PromptYesNo("  デバッグ情報を除去する (/ndebug)");
        var union = PromptYesNo("  ユニオンマージ (/union)");
        return new MergeOptions(internalize, suppressDebug, union);
    }

    private bool PromptYesNo(string question)
    {
        console.Write($"{question} [y/N]: ");
        var input = console.ReadLine()?.Trim();
        return string.Equals(input, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolvePaths(
        string inputPath,
        out string directory,
        out AssemblyEntry? primaryAssembly)
    {
        var fullPath = Path.GetFullPath(inputPath);

        if (File.Exists(fullPath))
        {
            directory = Path.GetDirectoryName(fullPath)!;
            var info = new FileInfo(fullPath);
            primaryAssembly = new AssemblyEntry(info.FullName, info.Name, info.Length);
            return true;
        }

        if (Directory.Exists(fullPath))
        {
            directory = fullPath;
            primaryAssembly = null;
            return true;
        }

        directory = string.Empty;
        primaryAssembly = null;
        return false;
    }

    private void WriteInfo(string message)
    {
        console.ForegroundColor = ConsoleColor.White;
        console.WriteLine(message);
        console.ResetColor();
    }

    private void WriteError(string message)
    {
        console.ForegroundColor = ConsoleColor.Red;
        console.WriteLine($"エラー: {message}");
        console.ResetColor();
    }

    private void WriteSuccess(string message)
    {
        console.ForegroundColor = ConsoleColor.Green;
        console.WriteLine($"✓ {message}");
        console.ResetColor();
    }
}
