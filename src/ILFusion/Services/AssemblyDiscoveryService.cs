namespace ILFusion.Services;

using ILFusion.Models;

sealed class AssemblyDiscoveryService : IAssemblyDiscoveryService
{
    private static readonly string[] Extensions = [".dll", ".exe"];

    public IReadOnlyList<AssemblyEntry> Discover(string directoryPath) =>
        [.. Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => Extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Select(path => new AssemblyEntry(path, Path.GetFileName(path), new FileInfo(path).Length))
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)];
}
