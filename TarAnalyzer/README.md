# TAR Analyzer

## Summary

A lightweight console application designed to parse and analyze TAR archives. It processes each entry sequentially, displaying detailed metadata in a well-structured and easy-to-read tabular format.

## Usage

```
~/repos/mytools/TarAnalyzer/bin/Debug/net9.0$ ./TarAnalyzer -h
Description:
  TAR analyzer

Usage:
  TarAnalyzer [options]

Options:
  -?, -h, --help                  Show help and usage information
  --version                       Show version information
  Archive, -a, --archive          The tar file to analyze.
  IsCompressed, -c, --compressed  Indicates if the file is a GZip file, in which case it will first be decompressed. [default: False]
```

## Output example

```
~/repos/mytools/TarAnalyzer/bin/Debug/net9.0$ ./TarAnalyzer -a archive.tar -c false

Analyzing tar file: archive.tar
Do you want to read the next entry? (y/n)
y
Reading entry...
Now reading the raw header bytes...
Finished processing. Now printing the results...
                              Entry #0                               
┌───────────────────────────────────────────────────────────────────┐
│ Header sections                                                   │
├───────────────────────────────────────────────────────────────────┤
│                     Entry #0 basic metadata                       │
│ ┌────────────┬─────────────────────────────┬───────────────────┐  │
│ │ Field Name │ Value from TarReader        │ Value from stream │  │
│ ├────────────┼─────────────────────────────┼───────────────────┤  │
│ │ Format     │ Gnu                         │ Gnu               │  │
│ │ Name       │ blockdev                    │ blockdev          │  │
│ │ Mode       │ 420                         │ 0000644           │  │
│ │ Uid        │ 7913                        │ 0017351           │  │
│ │ Gid        │ 3579                        │ 0006773           │  │
│ │ Size       │ 0                           │ 00000000000       │  │
│ │ MTime      │ 4/14/2022 9:31:19 PM +00:00 │ 14226111247       │  │
│ │ Type       │ BlockDevice                 │ 4                 │  │
│ │ Checksum   │ 5909                        │ 013425            │  │
│ │ LinkName   │                             │                   │  │
│ │ DataOffset │ 512                         │ 512               │  │
│ └────────────┴─────────────────────────────┴───────────────────┘  │
│              Entry #0 POSIX tar entry metadata                    │
│ ┌──────────────┬──────────────────────┬───────────────────┐       │
│ │ Field Name   │ Value from TarReader │ Value from stream │       │
│ ├──────────────┼──────────────────────┼───────────────────┤       │
│ │ Group Name   │ devdiv               │ devdiv            │       │
│ │ User Name    │ dotnet               │ dotnet            │       │
│ │ Device Major │ 71                   │ 0000107           │       │
│ │ Device Minor │ 53                   │ 0000065           │       │
│ └──────────────┴──────────────────────┴───────────────────┘       │
│                  Entry #0 GNU tar entry metadata                  │
│ ┌─────────────┬─────────────────────────────┬───────────────────┐ │
│ │ Field Name  │ Value from TarReader        │ Value from stream │ │
│ ├─────────────┼─────────────────────────────┼───────────────────┤ │
│ │ Access Time │ 1/1/1970 12:00:00 AM +00:00 │                   │ │
│ │ Change Time │ 1/1/1970 12:00:00 AM +00:00 │                   │ │
│ └─────────────┴─────────────────────────────┴───────────────────┘ │
└───────────────────────────────────────────────────────────────────┘
Do you want to read the next entry? (y/n)
n
Exiting...
```

You can find some tar files to test here: [dotnet/runtime-assets/tree/main/src/System.Formats.Tar.TestData/tar](https://github.com/dotnet/runtime-assets/tree/main/src/System.Formats.Tar.TestData/tar)