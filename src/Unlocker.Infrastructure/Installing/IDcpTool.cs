namespace Unlocker.Infrastructure.Installing;

/// <summary>Assets\dcpTool.exe (DCP to XML and back). Internal seam so tests can skip the real exe.</summary>
internal interface IDcpTool
{
    Task DecompileAsync(string dcpPath, string xmlPath, CancellationToken ct = default);

    Task CompileAsync(string xmlPath, string dcpPath, CancellationToken ct = default);
}
