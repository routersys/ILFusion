namespace ILFusion.Services;

using ILFusion.Models;

interface IRepackRunner
{
    Task<RepackResult> RunAsync(MergeConfiguration config, CancellationToken cancellationToken = default);
}
