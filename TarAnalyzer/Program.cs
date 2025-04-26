using System;
using System.Buffers;
using System.CommandLine;
using System.Formats.Tar;
using System.Numerics;
using System.Text;
using System.IO;
using Spectre.Console;
using System.Collections.Generic;
using System.IO.Compression;

namespace TarAnalyzer;

public class Program
{
    private static void Main(string[] args)
    {
        var rootCommand = new RootCommand("TAR analyzer");

        Option<FileInfo> archiveOption = new(name: "Archive", aliases: ["-a", "--archive"])
        {
            Description = "The tar file to analyze.",
            Arity = ArgumentArity.ExactlyOne
        };

        Option<bool> isCompressedOptions = new(name: "IsCompressed", aliases: ["-c", "--compressed"])
        {
            Description = "Indicates if the file is a GZip file, in which case it will first be decompressed.",
            Arity = ArgumentArity.ExactlyOne,
            DefaultValueFactory = _ => false
        };

        rootCommand.Options.Add(archiveOption);
        rootCommand.Options.Add(isCompressedOptions);

        rootCommand.SetAction((ParseResult result) =>
        {
            TarAnalyzerOptions o = new(
                Archive: result.GetValue(archiveOption) ??
                    throw new ArgumentNullException(nameof(archiveOption), "Archive option is required."),
                IsCompressed: result.GetValue(isCompressedOptions)
            );

            AnalyzeTarFile(o);
        });

        rootCommand.Parse(args).Invoke();
    }

    private static MemoryStream GetTarStream(string filePath, bool isCompressed)
    {
        MemoryStream ms = new MemoryStream();
        using (FileStream fs = File.OpenRead(filePath))
        {
            if (isCompressed)
            {
                using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                {
                    gzipStream.CopyTo(ms);
                }
            }
            else
            {
                fs.CopyTo(ms);
            }
        }
        
        ms.Position = 0;
        return ms;
    }

    private static void AnalyzeTarFile(TarAnalyzerOptions o)
    {
        Console.WriteLine($"Analyzing tar file: {o.Archive.FullName}");

        TarReader? reader = null;
        byte[]? buffer = null;

        try
        {
            using MemoryStream ms = GetTarStream(o.Archive.FullName, o.IsCompressed);

            using (reader = new TarReader(ms))
            {
                buffer = ArrayPool<byte>.Shared.Rent(minimumLength: 512);

                long currentEntryHeaderStartPosition = ms.Position;
                long nextEntryHeaderStartPosition = ms.Position;

                TarEntry? currentEntry;
                int number = 0;
                while ((currentEntry = reader.GetNextEntry()) is not null)
                {
                    Console.WriteLine("Do you want to read the next entry? (y/n)");
                    string? answer = Console.ReadLine();
                    if (answer?.ToLower() == "n")
                    {
                        Console.WriteLine("Exiting...");
                        break;
                    }

                    // After an entry is read, the stream is pointing to the beginning of the next entry, store that
                    nextEntryHeaderStartPosition = ms.Position;

                    Console.WriteLine("Reading entry...");

                    TarEntryFormat format = currentEntry.Format;
                    Stream? stream = currentEntry.DataStream;

                    string name = currentEntry.Name;
                    UnixFileMode mode = currentEntry.Mode;
                    int uid = currentEntry.Uid;
                    int gid = currentEntry.Gid;
                    long size = currentEntry.Length;
                    DateTimeOffset mtime = currentEntry.ModificationTime;
                    TarEntryType type = currentEntry.EntryType;
                    int checksum = currentEntry.Checksum;
                    string linkName = currentEntry.LinkName;
                    long dataOffset = currentEntry.DataOffset;

                    PosixTarEntry? posixTarEntry = currentEntry as PosixTarEntry;

                    string? groupName = posixTarEntry != null ? posixTarEntry.GroupName : null;
                    string? userName = posixTarEntry != null ? posixTarEntry.UserName : null;
                    int deviceMajor = posixTarEntry != null ? posixTarEntry.DeviceMajor : -1;
                    int deviceMinor = posixTarEntry != null ? posixTarEntry.DeviceMinor : -1;

                    PaxGlobalExtendedAttributesTarEntry? globalPaxTarEntry = currentEntry as PaxGlobalExtendedAttributesTarEntry;
                    IReadOnlyDictionary<string, string>? globalExtendedAttributes = globalPaxTarEntry != null ? globalPaxTarEntry.GlobalExtendedAttributes : null;

                    PaxTarEntry? paxTarEntry = currentEntry as PaxTarEntry;
                    IReadOnlyDictionary<string, string>? extendedAttributes = paxTarEntry != null ? paxTarEntry.ExtendedAttributes : null;

                    GnuTarEntry? gnuTarEntry = currentEntry as GnuTarEntry;
                    DateTimeOffset atime = gnuTarEntry != null ? gnuTarEntry.AccessTime : DateTimeOffset.MinValue;
                    DateTimeOffset ctime = gnuTarEntry != null ? gnuTarEntry.ChangeTime : DateTimeOffset.MinValue;

                    // Now rewind the stream to the beginning of the entry and read the actual header bytes of each metadata field, so they can later be compared.
                    ms.Position = currentEntryHeaderStartPosition;

                    Console.WriteLine("Now reading the raw header bytes...");
                    ms.ReadExactly(buffer.AsSpan());

                    ms.Position = currentEntry.DataOffset;
                    BufferTarEntry bufferEntry = BufferTarEntry.Create(buffer, ms, (int)size);

                    Console.WriteLine("Finished processing. Now printing the results...");

                    Table tableBasic = new();
                    tableBasic.Title = new TableTitle($"Entry #{number} basic metadata");

                    tableBasic.AddColumn("Field Name");
                    tableBasic.AddColumn("Value from TarReader");
                    tableBasic.AddColumn("Value from stream");

                    tableBasic.AddRow("Format", format.ToString(), format.ToString());
                    tableBasic.AddRow("Name", name, bufferEntry.Name);
                    tableBasic.AddRow("Mode", ((int)mode).ToString(), bufferEntry.Mode);
                    tableBasic.AddRow("Uid", uid.ToString(), bufferEntry.Uid);
                    tableBasic.AddRow("Gid", gid.ToString(), bufferEntry.Gid);
                    tableBasic.AddRow("Size", size.ToString(), bufferEntry.Size);
                    tableBasic.AddRow("MTime", mtime.ToString(), bufferEntry.MTime);
                    tableBasic.AddRow("Type", type.ToString(), bufferEntry.TypeFlag);
                    tableBasic.AddRow("Checksum", checksum.ToString(), bufferEntry.Checksum);
                    tableBasic.AddRow("LinkName", linkName, bufferEntry.LinkName);
                    tableBasic.AddRow("DataOffset", dataOffset.ToString(), $"{currentEntryHeaderStartPosition + 512}");

                    Table tablePosix = new();

                    Table tableGnu = new();

                    if (posixTarEntry is not null)
                    {
                        tablePosix.Title = new TableTitle($"Entry #{number} POSIX tar entry metadata");

                        tablePosix.AddColumn("Field Name");
                        tablePosix.AddColumn("Value from TarReader");
                        tablePosix.AddColumn("Value from stream");

                        tablePosix.AddRow("Group Name", groupName ?? "N/A", bufferEntry.GName);
                        tablePosix.AddRow("User Name", userName ?? "N/A", bufferEntry.UName);
                        tablePosix.AddRow("Device Major", deviceMajor.ToString(), bufferEntry.DevMajor);
                        tablePosix.AddRow("Device Minor", deviceMinor.ToString(), bufferEntry.DevMinor);

                        if (gnuTarEntry is not null)
                        {
                            tableGnu.Title = new TableTitle($"Entry #{number} GNU tar entry metadata");

                            tableGnu.AddColumn("Field Name");
                            tableGnu.AddColumn("Value from TarReader");
                            tableGnu.AddColumn("Value from stream");

                            tableGnu.AddRow("Access Time", atime.ToString(), bufferEntry.ATime);
                            tableGnu.AddRow("Change Time", ctime.ToString(), bufferEntry.CTime);
                        }
                    }

                    Table mainTable = new Table();
                    mainTable.Title = new TableTitle($"Entry #{number}");
                    mainTable.AddColumn("Header sections");
                    mainTable.AddRow(tableBasic);
                    mainTable.AddRow(tablePosix);
                    mainTable.AddRow(tableGnu);

                    AnsiConsole.Write(mainTable);

                    // Move currentPosition back to where it should be so we can keep reading the next entry
                    currentEntryHeaderStartPosition = nextEntryHeaderStartPosition;
                    ms.Position = nextEntryHeaderStartPosition;
                    number++;
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            if (reader != null)
            {
                reader.Dispose();
            }
        }
    }
}

public record TarAnalyzerOptions(
    FileInfo Archive,
    bool IsCompressed
);

internal class BufferTarEntry
{
    public string Name { get; init; }
    public string Mode { get; init; }
    public string Uid { get; init; }
    public string Gid { get; init; }
    public string Size { get; init; }
    public string MTime { get; init; }
    public string Checksum { get; init; }
    public string TypeFlag { get; init; }
    public string LinkName { get; init; }
    public string UName { get; init; }
    public string GName { get; init; }
    public string DevMajor { get; init; }
    public string DevMinor { get; init; }
    public string ATime { get; init; }
    public string CTime { get; init; }
    public string ExtendedAttributes { get; init; }

    public static BufferTarEntry Create(ReadOnlySpan<byte> buffer, Stream data, int dataSize)
    {
        string extendedAttributes;
        if (dataSize > 0)
        {
            byte[] dataBuffer = new byte[dataSize];
            data.ReadExactly(dataBuffer.AsSpan());
            extendedAttributes = Encoding.UTF8.GetString(dataBuffer);
        }
        else
        {
            extendedAttributes = string.Empty;
        }

        return new BufferTarEntry(buffer, extendedAttributes);
    }

    private BufferTarEntry(ReadOnlySpan<byte> buffer, string extendedAttributes)
    {
        Name = Encoding.UTF8.GetString(buffer.Slice(FieldLocations.Name, FieldLengths.Name));
        Mode = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.Mode, FieldLengths.Mode));
        Uid = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.Uid, FieldLengths.Uid));
        Gid = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.Gid, FieldLengths.Gid));
        Size = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.Size, FieldLengths.Size));
        MTime = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.MTime, FieldLengths.MTime));
        Checksum = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.Checksum, FieldLengths.Checksum));
        TypeFlag = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.TypeFlag, FieldLengths.TypeFlag));
        LinkName = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.LinkName, FieldLengths.LinkName));
        UName = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.UName, FieldLengths.UName));
        GName = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.GName, FieldLengths.GName));
        DevMajor = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.DevMajor, FieldLengths.DevMajor));
        DevMinor = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.DevMinor, FieldLengths.DevMinor));
        ATime = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.ATime, FieldLengths.ATime));
        CTime = Encoding.ASCII.GetString(buffer.Slice(FieldLocations.CTime, FieldLengths.CTime));
        ExtendedAttributes = extendedAttributes;
    }

    /// <summary>Parses a numeric field.</summary>
    private static T ParseNumeric<T>(ReadOnlySpan<byte> buffer) where T : struct, INumber<T>, IBinaryInteger<T>
    {
        // The tar standard specifies that numeric fields are stored using an octal representation.
        // This limits the range of values that can be stored in the fields.
        // To increase the supported range, a GNU extension defines that when the leading byte is
        // '0xff'/'0x80' the remaining bytes are a negative/positive big formatted endian value.
        // Like the 'tar' tool we are permissive when encountering this representation in non GNU formats.
        byte leadingByte = buffer[0];
        if (leadingByte == 0xff)
        {
            return T.ReadBigEndian(buffer, isUnsigned: false);
        }
        else if (leadingByte == 0x80)
        {
            return T.ReadBigEndian(buffer.Slice(1), isUnsigned: true);
        }
        else
        {
            return ParseOctal<T>(buffer);
        }
    }

    /// <summary>Parses a byte span that represents an ASCII string containing a number in octal base.</summary>
    private static T ParseOctal<T>(ReadOnlySpan<byte> buffer) where T : struct, INumber<T>
    {
        buffer = TrimEndingNullsAndSpaces(buffer);
        buffer = TrimLeadingNullsAndSpaces(buffer);

        if (buffer.Length == 0)
        {
            return T.Zero;
        }

        T octalFactor = T.CreateTruncating(8u);
        T value = T.Zero;
        foreach (byte b in buffer)
        {
            uint digit = (uint)(b - '0');
            if (digit >= 8)
            {
                throw new FormatException($"Invalid number: {digit}");
            }

            value = checked((value * octalFactor) + T.CreateTruncating(digit));
        }

        return value;
    }

    private static ReadOnlySpan<byte> TrimEndingNullsAndSpaces(ReadOnlySpan<byte> buffer)
    {
        int trimmedLength = buffer.Length;
        while (trimmedLength > 0 && buffer[trimmedLength - 1] is 0 or 32)
        {
            trimmedLength--;
        }

        return buffer.Slice(0, trimmedLength);
    }

    private static ReadOnlySpan<byte> TrimLeadingNullsAndSpaces(ReadOnlySpan<byte> buffer)
    {
        int newStart = 0;
        while (newStart < buffer.Length && buffer[newStart] is 0 or 32)
        {
            newStart++;
        }

        return buffer.Slice(newStart);
    }
}

// Specifies the expected lengths of all the header fields in the supported formats.
internal static class FieldLengths
{
    private const ushort Path = 100;

    // Common attributes

    internal const ushort Name = Path;
    internal const ushort Mode = 8;
    internal const ushort Uid = 8;
    internal const ushort Gid = 8;
    internal const ushort Size = 12;
    internal const ushort MTime = 12;
    internal const ushort Checksum = 8;
    internal const ushort TypeFlag = 1;
    internal const ushort LinkName = Path;

    // POSIX and GNU shared attributes

    internal const ushort Magic = 6;
    internal const ushort Version = 2;
    internal const ushort UName = 32;
    internal const ushort GName = 32;
    internal const ushort DevMajor = 8;
    internal const ushort DevMinor = 8;

    // POSIX attributes

    internal const ushort Prefix = 155;

    // GNU attributes

    internal const ushort ATime = 12;
    internal const ushort CTime = 12;
    internal const ushort Offset = 12;
    internal const ushort LongNames = 4;
    internal const ushort Unused = 1;
    internal const ushort Sparse = 4 * (12 + 12);
    internal const ushort IsExtended = 1;
    internal const ushort RealSize = 12;

    // Padding lengths depending on format

    internal const ushort V7Padding = 255;
    internal const ushort PosixPadding = 12;

    internal const int AllGnuUnused = Offset + LongNames + Unused + Sparse + IsExtended + RealSize;

    internal const ushort GnuPadding = 17;
}

// Specifies the position of the first byte of each header field.
internal static class FieldLocations
{
    // Common attributes

    internal const ushort Name = 0;
    internal const ushort Mode = FieldLengths.Name;
    internal const ushort Uid = Mode + FieldLengths.Mode;
    internal const ushort Gid = Uid + FieldLengths.Uid;
    internal const ushort Size = Gid + FieldLengths.Gid;
    internal const ushort MTime = Size + FieldLengths.Size;
    internal const ushort Checksum = MTime + FieldLengths.MTime;
    internal const ushort TypeFlag = Checksum + FieldLengths.Checksum;
    internal const ushort LinkName = TypeFlag + FieldLengths.TypeFlag;

    // POSIX and GNU shared attributes

    internal const ushort Magic = LinkName + FieldLengths.LinkName;
    internal const ushort Version = Magic + FieldLengths.Magic;
    internal const ushort UName = Version + FieldLengths.Version;
    internal const ushort GName = UName + FieldLengths.UName;
    internal const ushort DevMajor = GName + FieldLengths.GName;
    internal const ushort DevMinor = DevMajor + FieldLengths.DevMajor;

    // POSIX attributes

    internal const ushort Prefix = DevMinor + FieldLengths.DevMinor;

    // GNU attributes

    internal const ushort ATime = DevMinor + FieldLengths.DevMinor;
    internal const ushort CTime = ATime + FieldLengths.ATime;
    internal const ushort Offset = CTime + FieldLengths.CTime;
    internal const ushort LongNames = Offset + FieldLengths.Offset;
    internal const ushort Unused = LongNames + FieldLengths.LongNames;
    internal const ushort Sparse = Unused + FieldLengths.Unused;
    internal const ushort IsExtended = Sparse + FieldLengths.Sparse;
    internal const ushort RealSize = IsExtended + FieldLengths.IsExtended;

    internal const ushort GnuUnused = CTime + FieldLengths.CTime;

    // Padding lengths depending on format

    internal const ushort V7Padding = LinkName + FieldLengths.LinkName;
    internal const ushort PosixPadding = Prefix + FieldLengths.Prefix;
    internal const ushort GnuPadding = RealSize + FieldLengths.RealSize;

    internal const ushort V7Data = V7Padding + FieldLengths.V7Padding;
    internal const ushort PosixData = PosixPadding + FieldLengths.PosixPadding;
    internal const ushort GnuData = GnuPadding + FieldLengths.GnuPadding;
}