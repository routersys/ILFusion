namespace ILFusion.Models;

sealed record MergeOptions(
    bool InternalizeAssemblies = false,
    bool SuppressDebugInfo = false,
    bool UnionMerge = false);
