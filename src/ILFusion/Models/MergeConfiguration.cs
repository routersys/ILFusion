namespace ILFusion.Models;

sealed record MergeConfiguration(
    string OutputPath,
    AssemblyEntry PrimaryAssembly,
    IReadOnlyList<AssemblyEntry> SecondaryAssemblies,
    MergeOptions Options);
