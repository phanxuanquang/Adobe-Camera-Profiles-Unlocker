using System.Xml.Linq;
using Unlocker.Infrastructure.Installing;
using Unlocker.Tests.Support;

namespace Unlocker.Tests.Infrastructure;

/// <summary>Runs the real dcpTool.exe.</summary>
[TestClass]
public sealed class DcpToolTests : TempFolderTest
{
    [TestMethod]
    public async Task Round_trip_compiles_fixture()
    {
        var xml = Path.Combine(Root, "in.xml");
        File.Copy(TestFiles.Fixture("sample.dcp.xml"), xml);
        var dcp = Path.Combine(Root, "out.dcp");
        var back = Path.Combine(Root, "back.xml");

        var tool = new DcpTool();
        await tool.CompileAsync(xml, dcp);
        await tool.DecompileAsync(dcp, back);

        Assert.AreEqual("Canon EOS R5", (string?)XDocument.Load(back).Root!.Element("UniqueCameraModelRestriction"));
    }

    [TestMethod]
    public async Task Missing_input_throws()
        => await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new DcpTool().DecompileAsync(Path.Combine(Root, "nope.dcp"), Path.Combine(Root, "x.xml")));
}
