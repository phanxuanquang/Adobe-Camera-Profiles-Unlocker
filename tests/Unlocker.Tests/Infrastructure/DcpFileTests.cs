using System.Buffers.Binary;
using Unlocker.Infrastructure.Installing;

namespace Unlocker.Tests.Infrastructure;

[TestClass]
public sealed class DcpFileTests
{
    /// <summary>Little-endian DCP ("IIRC") like Adobe ships: restriction, name, ColorMatrix1 (9 x SRATIONAL, out of line),
    /// CalibrationIlluminant1 (SHORT, inline), a look table stand-in (6 x FLOAT). Seeds make matrix and look bytes differ between samples.</summary>
    internal static byte[] Sample(string name = "Camera Portrait", string restriction = "Canon EOS R5", byte matrixSeed = 0, byte lookSeed = 0)
    {
        var matrix = new byte[72];
        for (var i = 0; i < matrix.Length; i++) matrix[i] = (byte)(i + matrixSeed);
        var look = new byte[24];
        for (var i = 0; i < look.Length; i++) look[i] = (byte)(i + lookSeed);
        List<DcpFile.Entry> entries = [
            new DcpFile.Entry(0xC614, 2, (uint)restriction.Length + 1, Ascii(restriction)),
            new DcpFile.Entry(0xC621, 10, 9, matrix),
            new DcpFile.Entry(0xC65A, 3, 1, new byte[] { 17, 0 }),
            new DcpFile.Entry(0xC6F4, 2, 10, Ascii("com.adobe")),
            new DcpFile.Entry(0xC6F8, 2, (uint)name.Length + 1, Ascii(name)),
            new DcpFile.Entry(0xC726, 11, 6, look),
        ];
        // Header + IFD + out-of-line data: restriction, matrix, signature, name (inline when <= 4 bytes).
        var bytes = new byte[8 + 2 + entries.Count * 12 + 4];
        "IIRC"u8.CopyTo(bytes);
        bytes[4] = 8;
        bytes[8] = (byte)entries.Count;
        var data = new List<byte>();
        for (var i = 0; i < entries.Count; i++)
        {
            var (e, slot) = (entries[i], 10 + i * 12);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(slot), e.Tag);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(slot + 2), e.Type);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(slot + 4), e.Count);
            if (e.Data.Length <= 4) { e.Data.Span.CopyTo(bytes.AsSpan(slot + 8)); continue; }
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(slot + 8), (uint)(bytes.Length + data.Count));
            data.AddRange(e.Data.ToArray());
            if (data.Count % 2 == 1) data.Add(0);
        }
        return [.. bytes, .. data];
    }

    private static byte[] Ascii(string s) => [.. System.Text.Encoding.ASCII.GetBytes(s), 0];

    /// <summary>In-memory rewrite, same steps as <see cref="DcpProfileInstaller"/>.</summary>
    internal static byte[] Rewrite(byte[] source, Action<Dictionary<string, string>> edit)
    {
        var dcp = DcpFile.Parse(source);
        var strings = dcp.GetStrings();
        edit(strings);
        dcp.SetStrings(strings);
        var output = new byte[dcp.Size];
        dcp.WriteTo(output);
        return output;
    }

    internal static Dictionary<string, string> ReadStrings(byte[] dcp) => DcpFile.Parse(dcp).GetStrings();

    [TestMethod]
    public void Rewrite_replaces_string_tags_and_keeps_other_entries()
    {
        var source = Sample();

        var output = Rewrite(source, s =>
        {
            s["Name"] = "Canon EOS R5: Portrait";
            s["CameraModelRestriction"] = "Sony ILCE-7M4";
        });

        var strings = ReadStrings(output);
        Assert.AreEqual("Canon EOS R5: Portrait", strings["Name"]);
        Assert.AreEqual("Sony ILCE-7M4", strings["CameraModelRestriction"]);
        Assert.AreEqual("com.adobe", strings["ProfileCalibrationSignature"]);
        var before = DcpFile.Parse(source).Entries;
        var after = DcpFile.Parse(output).Entries;
        CollectionAssert.AreEqual("IIRC"u8.ToArray(), output[..4]);
        foreach (var tag in new ushort[] { 0xC621, 0xC65A })
        {
            var (b, a) = (before.Single(e => e.Tag == tag), after.Single(e => e.Tag == tag));
            Assert.AreEqual((b.Type, b.Count), (a.Type, a.Count));
            CollectionAssert.AreEqual(b.Data.ToArray(), a.Data.ToArray());
        }
    }

    [TestMethod]
    public void Rewrite_adds_missing_tag_in_tag_order()
    {
        var output = Rewrite(Sample(), s => s["Copyright"] = "x");

        var tags = DcpFile.Parse(output).Entries.Select(e => e.Tag).ToList();
        CollectionAssert.AreEqual(tags.Order().ToList(), tags);
        Assert.AreEqual("x", ReadStrings(output)["Copyright"]);
    }

    [TestMethod]
    public void Non_ascii_string_is_written_as_utf8_bytes()
    {
        var output = Rewrite(Sample(), s => s["Copyright"] = "© 2024");

        var entry = DcpFile.Parse(output).Entries.Single(e => e.Tag == 0xC6FE);
        Assert.AreEqual((ushort)1, entry.Type);
        Assert.AreEqual("© 2024", ReadStrings(output)["Copyright"]);
    }

    [TestMethod]
    public void Short_string_is_stored_inline()
    {
        var output = Rewrite(Sample(), s => s["Name"] = "abc");

        Assert.AreEqual("abc", ReadStrings(output)["Name"]);
    }

    [TestMethod]
    public void Not_a_dcp_throws()
        => Assert.ThrowsExactly<InvalidDataException>(() => DcpFile.Parse("<?xml version"u8.ToArray()));

    [TestMethod]
    public void Truncated_file_throws()
    {
        var bytes = Sample();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)bytes.Length - 2);
        Assert.ThrowsExactly<InvalidDataException>(() => DcpFile.Parse(bytes));
    }
}
