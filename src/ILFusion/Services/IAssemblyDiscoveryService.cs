namespace ILFusion.Services;

using ILFusion.Models;

interface IAssemblyDiscoveryService
{
    IReadOnlyList<AssemblyEntry> Discover(string directoryPath);
}
