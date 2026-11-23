using Unlocker.Infrastructure.Installing;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Fakes;

/// <summary>Decompile returns the fixture XML. Compile copies the XML, so tests can read the "DCP" back.</summary>
internal sealed class FakeDcpTool : IDcpTool
{
    public Task DecompileAsync(string dcpPath, string xmlPath, CancellationToken ct = default)
    {
        File.Copy(TestFiles.Fixture("sample.dcp.xml"), xmlPath);
        return Task.CompletedTask;
    }

    public Task CompileAsync(string xmlPath, string dcpPath, CancellationToken ct = default)
    {
        File.Copy(xmlPath, dcpPath, overwrite: true);
        return Task.CompletedTask;
    }
}
