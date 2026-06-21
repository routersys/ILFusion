using System.Text;
using ILFusion;
using ILFusion.Services;
using ILFusion.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var console = new ConsoleIO();
var discoveryService = new AssemblyDiscoveryService();
var processRunner = new ProcessRunner();
var repackRunner = new RepackRunner(processRunner, FindILRepack());
var singleSelector = new SingleSelector(console);
var multiSelector = new MultiSelector(console);
var app = new Application(discoveryService, repackRunner, singleSelector, multiSelector, console);
return await app.RunAsync(args);

static string FindILRepack()
{
    var localCandidates = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "ILRepack.exe"),
        Path.Combine(AppContext.BaseDirectory, "ilrepack.exe"),
        Path.Combine(AppContext.BaseDirectory, "ILRepack"),
        Path.Combine(AppContext.BaseDirectory, "ilrepack"),
    };

    foreach (var candidate in localCandidates)
    {
        if (File.Exists(candidate))
            return candidate;
    }

    return "ILRepack";
}
