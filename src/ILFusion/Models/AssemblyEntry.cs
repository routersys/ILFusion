namespace ILFusion.Models;

sealed record AssemblyEntry(string Path, string Name, long SizeBytes)
{
    public string FormattedSize => SizeBytes switch
    {
        >= 1_048_576 => $"{SizeBytes / 1_048_576.0:F1} MB",
        >= 1_024 => $"{SizeBytes / 1_024.0:F1} KB",
        _ => $"{SizeBytes} B"
    };
}
