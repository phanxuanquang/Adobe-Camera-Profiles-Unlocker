using System.Buffers.Binary;
using System.Text;

namespace Unlocker.Infrastructure.Installing;

/// <summary>
/// Parsed DNG Camera Profile (.dcp) = little-endian TIFF with magic "RC" instead of 42 and one IFD (DNG spec, "Camera Profiles").
/// ponytail: only "IIRC" (every Adobe DCP); add big-endian "MMCR" if a DCP from elsewhere must load.
/// Replaces dcpTool: only the string tags change, every other entry is written back byte for byte.
/// Entries are slices of the source buffer (no copies), so keep that buffer alive until <see cref="WriteTo"/>.
/// </summary>
internal sealed class DcpFile
{
    internal sealed record Entry(ushort Tag, ushort Type, uint Count, ReadOnlyMemory<byte> Data);

    /// <summary>String tags the rewrite rules edit, under the shared tag names (CameraProfileRewriter).</summary>
    private static readonly (ushort Tag, string Name)[] StringTags =
    [
        (0xC614, "CameraModelRestriction"),       // UniqueCameraModel (50708)
        (0xC6F4, "ProfileCalibrationSignature"),  // 50932
        (0xC6F8, "Name"),                         // ProfileName (50936)
        (0xC6FE, "Copyright"),                    // ProfileCopyright (50942)
    ];

    private readonly List<Entry> _entries;

    private DcpFile(List<Entry> entries) => _entries = entries;

    public IReadOnlyList<Entry> Entries => _entries;

    /// <summary>Header, IFD right after it, then out-of-line values (word aligned).</summary>
    public int Size
    {
        get
        {
            var size = DataStart(_entries.Count);
            foreach (var e in _entries)
                if (e.Data.Length > 4) size += e.Data.Length + (e.Data.Length & 1);
            return size;
        }
    }

    /// <summary>Not "IIRC" or out of bounds: <see cref="InvalidDataException"/>.</summary>
    public static DcpFile Parse(ReadOnlyMemory<byte> dcp)
    {
        try
        {
            var span = dcp.Span;
            if (!span[..4].SequenceEqual("IIRC"u8)) throw new InvalidDataException("Not a DCP file.");

            var ifd = checked((int)U32(span[4..]));
            var count = U16(span[ifd..]);
            var entries = new List<Entry>(count + StringTags.Length);
            for (var i = 0; i < count; i++)
            {
                var start = ifd + 2 + i * 12;
                var e = span.Slice(start, 12);
                var type = U16(e[2..]);
                var n = U32(e[4..]);
                var size = checked((int)(n * (long)TypeSize(type)));
                var data = size <= 4 ? dcp.Slice(start + 8, size) : dcp.Slice(checked((int)U32(e[8..])), size);
                entries.Add(new Entry(U16(e), type, n, data));
            }
            return new DcpFile(entries);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or OverflowException)
        {
            throw new InvalidDataException("DCP file is truncated or corrupt.", ex);
        }
    }

    public Dictionary<string, string> GetStrings()
    {
        var result = new Dictionary<string, string>();
        foreach (var (tag, name) in StringTags)
            if (_entries.Find(e => e.Tag == tag) is { } e)
                result[name] = Encoding.UTF8.GetString(e.Data.Span).TrimEnd('\0');
        return result;
    }

    /// <summary>Replaces or adds the given string tags. Entries stay sorted by tag (TIFF rule).</summary>
    public void SetStrings(IReadOnlyDictionary<string, string> strings)
    {
        foreach (var (tag, name) in StringTags)
        {
            if (!strings.TryGetValue(name, out var value)) continue;
            _entries.RemoveAll(e => e.Tag == tag);
            // Same as DNG SDK tag_string: ASCII when possible, else UTF-8 as BYTE. NUL-terminated.
            var data = new byte[Encoding.UTF8.GetByteCount(value) + 1];
            Encoding.UTF8.GetBytes(value, data);
            _entries.Add(new Entry(tag, (ushort)(Ascii.IsValid(value) ? 2 : 1), (uint)data.Length, data));
        }
        _entries.Sort((a, b) => a.Tag.CompareTo(b.Tag));
    }

    /// <summary><paramref name="dest"/> must be exactly <see cref="Size"/> bytes.</summary>
    public void WriteTo(Span<byte> dest)
    {
        dest.Clear(); // pooled buffer: padding and next-IFD offset must be 0
        "IIRC"u8.CopyTo(dest);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[4..], 8);
        BinaryPrimitives.WriteUInt16LittleEndian(dest[8..], (ushort)_entries.Count);

        var next = DataStart(_entries.Count);
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            var e = dest.Slice(10 + i * 12, 12);
            BinaryPrimitives.WriteUInt16LittleEndian(e, entry.Tag);
            BinaryPrimitives.WriteUInt16LittleEndian(e[2..], entry.Type);
            BinaryPrimitives.WriteUInt32LittleEndian(e[4..], entry.Count);
            if (entry.Data.Length <= 4)
            {
                entry.Data.Span.CopyTo(e[8..]);
                continue;
            }
            BinaryPrimitives.WriteUInt32LittleEndian(e[8..], (uint)next);
            entry.Data.Span.CopyTo(dest[next..]);
            next += entry.Data.Length + (entry.Data.Length & 1);
        }
    }

    private static int DataStart(int count) => 8 + 2 + count * 12 + 4;

    private static int TypeSize(ushort type) => type switch
    {
        1 or 2 or 6 or 7 => 1,
        3 or 8 => 2,
        4 or 9 or 11 or 13 => 4,
        5 or 10 or 12 => 8,
        _ => throw new InvalidDataException($"Unknown TIFF field type {type}."),
    };

    private static ushort U16(ReadOnlySpan<byte> s) => BinaryPrimitives.ReadUInt16LittleEndian(s);

    private static uint U32(ReadOnlySpan<byte> s) => BinaryPrimitives.ReadUInt32LittleEndian(s);
}
