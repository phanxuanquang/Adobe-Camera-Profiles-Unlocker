using System.Diagnostics;
using Unlocker.Infrastructure.Platform;

namespace Unlocker.Infrastructure.Installing;

internal sealed class DcpTool : IDcpTool
{
    public Task DecompileAsync(string dcpPath, string xmlPath, CancellationToken ct = default)
        => RunAsync(["-d", dcpPath, xmlPath], ct);

    public Task CompileAsync(string xmlPath, string dcpPath, CancellationToken ct = default)
        => RunAsync(["-c", xmlPath, dcpPath], ct);

    private static async Task RunAsync(string[] args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(AppPaths.DcpToolExe)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("dcpTool did not start.");
        var stderr = process.StandardError.ReadToEndAsync(ct);
        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        // Missing or unreadable input exits with 1 and writes no output file.
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dcpTool exited with {process.ExitCode}: {(await stderr).Trim()}");
    }
}
