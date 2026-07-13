// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildinfo provides access to information embedded in a Go binary
// about how it was built. This includes the Go toolchain version, and the
// set of modules used (for binaries built in module mode).
//
// Build information is available for the currently running binary in
// runtime/debug.ReadBuildInfo.
global using BuildInfo = go.runtime.debug_package.BuildInfo;

namespace go.debug;

using bytes = bytes_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using plan9obj = go.debug.plan9obj_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using xcoff = @internal.xcoff_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using debug = runtime.debug_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using @internal;
using encoding;
using go.debug;
using go.io;
using runtime;

partial class buildinfo_package {

// errUnrecognizedFormat is returned when a given executable file doesn't
// appear to be in a known format, or it breaks the rules of that format,
// or when there are I/O errors reading the file.
internal static error errUnrecognizedFormat = errors.New("unrecognized file format"u8);

// errNotGoExe is returned when a given executable file is valid but does
// not contain Go build information.
//
// errNotGoExe should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/quay/claircore
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNotGoExe
internal static error errNotGoExe = errors.New("not a Go executable"u8);

// The build info blob left by the linker is identified by
// a 16-byte header, consisting of buildInfoMagic (14 bytes),
// the binary's pointer size (1 byte),
// and whether the binary is big endian (1 byte).
internal static slice<byte> buildInfoMagic = slice<byte>(((@string)(new byte[]{0xff, 0x20, 0x47, 0x6f, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x64, 0x69, 0x6e, 0x66, 0x3a})));

// ReadFile returns build information embedded in a Go binary
// file at the given path. Most information is only available for binaries built
// with module support.
public static (ж<BuildInfo> info, error err) ReadFile(@string name) {
    ж<BuildInfo> info = default!;
    heap<error>(out var Ꮡerr);
    func((defer, recover) => {
    ref var err = ref Ꮡerr.ValueSlot;

        defer(() => {
            {
                ref var pathErr = ref heap<ж<fs.PathError>>(out var ᏑpathErr);
                pathErr = (ж<fs.PathError>)(default!); if (errors.As(Ꮡerr.ValueSlot, ᏑpathErr)){
                    Ꮡerr.ValueSlot = fmt.Errorf("could not read Go build info: %w"u8, Ꮡerr.ValueSlot);
                } else 
                if (Ꮡerr.ValueSlot != default!) {
                    Ꮡerr.ValueSlot = fmt.Errorf("could not read Go build info from %s: %w"u8, name, Ꮡerr.ValueSlot);
                }
            }
        });
        (var f, err) = os.Open(name);
        if (err != default!) {
            (info, err) = (default!, err); return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close());
        (info, err) = Read(new os_FileжReaderAt(f));
    });
    return (info, Ꮡerr.ValueSlot);
}

// Read returns build information embedded in a Go binary file
// accessed through the given ReaderAt. Most information is only available for
// binaries built with module support.
public static (ж<BuildInfo>, error) Read(io.ReaderAt r) {
    var (vers, mod, err) = readRawBuildInfo(r);
    if (err != default!) {
        return (default!, err);
    }
    (var bi, err) = debug.ParseBuildInfo(mod);
    if (err != default!) {
        return (default!, err);
    }
    bi.Value.GoVersion = vers;
    return (bi, default!);
}

[GoType] partial interface exe {
    // ReadData reads and returns up to size bytes starting at virtual address addr.
    (slice<byte>, error) ReadData(uint64 addr, uint64 size);
    // DataStart returns the virtual address and size of the segment or section that
    // should contain build information. This is either a specially named section
    // or the first writable non-zero data segment.
    (uint64, uint64) DataStart();
}

// readRawBuildInfo extracts the Go toolchain version and module information
// strings from a Go binary. On success, vers should be non-empty. mod
// is empty if the binary was not built with modules enabled.
internal static (@string vers, @string mod, error err) readRawBuildInfo(io.ReaderAt r) {
    @string vers = default!;
    @string mod = default!;
    error err = default!;

    // Read the first bytes of the file to identify the format, then delegate to
    // a format-specific function to load segment and section headers.
    var ident = new slice<byte>(16);
    {
        var (n, errΔ1) = r.ReadAt(ident, 0); if (n < len(ident) || errΔ1 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
    }
    exe x = default!;
    switch (ᐧ) {
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0x7f, 0x45, 0x4c, 0x46})))): {
        var (f, errΔ3) = elf.NewFile(r);
        if (errΔ3 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new elfExeжexe(Ꮡ(new elfExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>("MZ"u8)): {
        var (f, errΔ4) = pe.NewFile(r);
        if (errΔ4 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new peExeжexe(Ꮡ(new peExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xfe, 0xed, 0xfa})))) || bytes.HasPrefix(ident[1..], slice<byte>(((@string)(new byte[]{0xfa, 0xed, 0xfe})))): {
        var (f, errΔ5) = macho.NewFile(r);
        if (errΔ5 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new machoExeжexe(Ꮡ(new machoExe(f)));
        break;
    }
    case {} when bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xca, 0xfe, 0xba, 0xbe})))) || bytes.HasPrefix(ident, slice<byte>(((@string)(new byte[]{0xca, 0xfe, 0xba, 0xbf})))): {
        var (f, errΔ6) = macho.NewFatFile(r);
        if (errΔ6 != default! || len((~f).Arches) == 0) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new machoExeжexe(Ꮡ(new machoExe((~f).Arches[0].File)));
        break;
    }
    case {} when bytes.HasPrefix(ident, new byte[]{0x01, 0xDF}.slice()) || bytes.HasPrefix(ident, new byte[]{0x01, 0xF7}.slice()): {
        var (f, errΔ7) = xcoff.NewFile(r);
        if (errΔ7 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new xcoffExeжexe(Ꮡ(new xcoffExe(f)));
        break;
    }
    case {} when hasPlan9Magic(ident): {
        var (f, errΔ8) = plan9obj.NewFile(r);
        if (errΔ8 != default!) {
            return ("", "", errUnrecognizedFormat);
        }
        x = new plan9objExeжexe(Ꮡ(new plan9objExe(f)));
        break;
    }
    default: {
        return ("", "", errUnrecognizedFormat);
    }}

    // Read segment or section to find the build info blob.
    // On some platforms, the blob will be in its own section, and DataStart
    // returns the address of that section. On others, it's somewhere in the
    // data segment; the linker puts it near the beginning.
    // See cmd/link/internal/ld.Link.buildinfo.
    var (dataAddr, dataSize) = x.DataStart();
    if (dataSize == 0) {
        return ("", "", errNotGoExe);
    }
    (var data, err) = x.ReadData(dataAddr, dataSize);
    if (err != default!) {
        return ("", "", err);
    }
    UntypedInt buildInfoAlign = 16;
    UntypedInt buildInfoSize = 32;
    while (ᐧ) {
        nint i = bytes.Index(data, buildInfoMagic);
        if (i < 0 || len(data) - i < buildInfoSize) {
            return ("", "", errNotGoExe);
        }
        if (i % (nint)buildInfoAlign == 0 && len(data) - i >= buildInfoSize) {
            data = data[(int)(i)..];
            break;
        }
        data = data[(int)((nint)((i + (nint)buildInfoAlign - 1) & ~(nint)(buildInfoAlign - 1)))..];
    }
    // Decode the blob.
    // The first 14 bytes are buildInfoMagic.
    // The next two bytes indicate pointer size in bytes (4 or 8) and endianness
    // (0 for little, 1 for big).
    // Two virtual addresses to Go strings follow that: runtime.buildVersion,
    // and runtime.modinfo.
    // On 32-bit platforms, the last 8 bytes are unused.
    // If the endianness has the 2 bit set, then the pointers are zero
    // and the 32-byte header is followed by varint-prefixed string data
    // for the two string values we care about.
    nint ptrSize = (nint)data[14];
    if ((byte)(data[15] & 2) != 0){
        (vers, data) = decodeString(data[32..]);
        (mod, data) = decodeString(data);
    } else {
        var bigEndian = data[15] != 0;
        binary.ByteOrder bo = default!;
        if (bigEndian){
            bo = new binary_bigEndianᴠByteOrder(binary.BigEndian);
        } else {
            bo = new binary_littleEndianᴠByteOrder(binary.LittleEndian);
        }
        Func<slice<byte>, uint64> readPtr = default!;
        if (ptrSize == 4){
            var boʗ1 = bo;
            readPtr = (slice<byte> b) => (uint64)boʗ1.Uint32(b);
        } else 
        if (ptrSize == 8){
            var boʗ2 = bo;
                        readPtr = boʗ2.Uint64;
        } else {
            return ("", "", errNotGoExe);
        }
        vers = readString(x, ptrSize, readPtr, readPtr(data[16..]));
        mod = readString(x, ptrSize, readPtr, readPtr(data[(int)(16 + ptrSize)..]));
    }
    if (vers == ""u8) {
        return ("", "", errNotGoExe);
    }
    if (len(mod) >= 33 && mod[len(mod) - 17] == (rune)'\n'){
        // Strip module framing: sentinel strings delimiting the module info.
        // These are cmd/go/internal/modload.infoStart and infoEnd.
        mod = mod[16..(int)(len(mod) - 16)];
    } else {
        mod = ""u8;
    }
    return (vers, mod, default!);
}

internal static bool hasPlan9Magic(slice<byte> magic) {
    if (len(magic) >= 4) {
        var m = binary.BigEndian.Uint32(magic);
        var exprᴛ1 = m;
        if (exprᴛ1 == plan9obj.Magic386 || exprᴛ1 == plan9obj.MagicAMD64 || exprᴛ1 == plan9obj.MagicARM) {
            return true;
        }

    }
    return false;
}

internal static (@string s, slice<byte> rest) decodeString(slice<byte> data) {
    @string s = default!;
    slice<byte> rest = default!;

    var (u, n) = binary.Uvarint(data);
    if (n <= 0 || u > (uint64)(len(data) - n)) {
        return ("", default!);
    }
    return (((@string)(data[(int)(n)..(int)((uint64)n + u)])), data[(int)((uint64)n + u)..]);
}

// readString returns the string at address addr in the executable x.
internal static @string readString(exe x, nint ptrSize, Func<slice<byte>, uint64> readPtr, uint64 addr) {
    var (hdr, err) = x.ReadData(addr, (uint64)(2 * ptrSize));
    if (err != default! || len(hdr) < 2 * ptrSize) {
        return ""u8;
    }
    var dataAddr = readPtr(hdr);
    var dataLen = readPtr(hdr[(int)(ptrSize)..]);
    (var data, err) = x.ReadData(dataAddr, dataLen);
    if (err != default! || (uint64)len(data) < dataLen) {
        return ""u8;
    }
    return ((@string)data);
}

// elfExe is the ELF implementation of the exe interface.
[GoType] partial struct elfExe {
    internal ж<elf.File> f;
}

[GoRecv] internal static (slice<byte>, error) ReadData(this ref elfExe x, uint64 addr, uint64 size) {
    foreach (var (_, prog) in (~x.f).Progs) {
        if ((~prog).Vaddr <= addr && addr <= (~prog).Vaddr + (~prog).Filesz - 1) {
            var n = (~prog).Vaddr + (~prog).Filesz - addr;
            if (n > size) {
                n = size;
            }
            return saferio.ReadDataAt(new elf_ProgжReaderAt(prog), n, (int64)(addr - (~prog).Vaddr));
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref elfExe x) {
    foreach (var (_, s) in (~x.f).Sections) {
        if ((~s).Name == ".go.buildinfo"u8) {
            return ((~s).Addr, (~s).Size);
        }
    }
    foreach (var (_, p) in (~x.f).Progs) {
        if ((~p).Type == elf.PT_LOAD && (elf.ProgFlag)((~p).Flags & ((elf.ProgFlag)(elf.PF_X | elf.PF_W))) == elf.PF_W) {
            return ((~p).Vaddr, (~p).Memsz);
        }
    }
    return (0, 0);
}

// peExe is the PE (Windows Portable Executable) implementation of the exe interface.
[GoType] partial struct peExe {
    internal ж<pe.File> f;
}

[GoRecv] internal static uint64 imageBase(this ref peExe x) {
    switch ((~x.f).OptionalHeader.type()) {
    case ж<pe.OptionalHeader32> oh: {
        return (uint64)(~oh).ImageBase;
    }
    case ж<pe.OptionalHeader64> oh: {
        return (~oh).ImageBase;
    }}
    return 0;
}

[GoRecv] internal static (slice<byte>, error) ReadData(this ref peExe x, uint64 addr, uint64 size) {
    addr -= x.imageBase();
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((uint64)(~sect).VirtualAddress <= addr && addr <= (uint64)((~sect).VirtualAddress + (~sect).Size - 1)) {
            var n = (uint64)((~sect).VirtualAddress + (~sect).Size) - addr;
            if (n > size) {
                n = size;
            }
            return saferio.ReadDataAt(new pe_ΔSectionжReaderAt(sect), n, (int64)(addr - (uint64)(~sect).VirtualAddress));
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref peExe x) {
    // Assume data is first writable section.
    UntypedInt IMAGE_SCN_CNT_CODE = 0x00000020;
    
    UntypedInt IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
    
    UntypedInt IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;
    
    UntypedInt IMAGE_SCN_MEM_EXECUTE = 0x20000000;
    
    UntypedInt IMAGE_SCN_MEM_READ = 0x40000000;
    
    UntypedInt IMAGE_SCN_MEM_WRITE = 0x80000000;
    
    UntypedInt IMAGE_SCN_MEM_DISCARDABLE = 0x2000000;
    
    UntypedInt IMAGE_SCN_LNK_NRELOC_OVFL = 0x1000000;
    
    UntypedInt IMAGE_SCN_ALIGN_32BYTES = 0x600000;
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((~sect).VirtualAddress != 0 && (~sect).Size != 0 && (uint32)((~sect).Characteristics & ~(uint32)IMAGE_SCN_ALIGN_32BYTES) == (uint32)((UntypedInt)(IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ) | (uint32)IMAGE_SCN_MEM_WRITE)) {
            return ((uint64)(~sect).VirtualAddress + x.imageBase(), (uint64)(~sect).VirtualSize);
        }
    }
    return (0, 0);
}

// machoExe is the Mach-O (Apple macOS/iOS) implementation of the exe interface.
[GoType] partial struct machoExe {
    internal ж<macho.File> f;
}

[GoRecv] internal static (slice<byte>, error) ReadData(this ref machoExe x, uint64 addr, uint64 size) {
    foreach (var (_, load) in (~x.f).Loads) {
        var (seg, ok) = load._<ж<machoꓸSegment>>(ᐧ);
        if (!ok) {
            continue;
        }
        if ((~seg).Addr <= addr && addr <= (~seg).Addr + (~seg).Filesz - 1) {
            if ((~seg).Name == "__PAGEZERO"u8) {
                continue;
            }
            var n = (~seg).Addr + (~seg).Filesz - addr;
            if (n > size) {
                n = size;
            }
            return saferio.ReadDataAt(new macho_ΔSegmentжReaderAt(seg), n, (int64)(addr - (~seg).Addr));
        }
    }
    return (default!, errUnrecognizedFormat);
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref machoExe x) {
    // Look for section named "__go_buildinfo".
    foreach (var (_, sec) in (~x.f).Sections) {
        if ((~sec).Name == "__go_buildinfo"u8) {
            return ((~sec).Addr, (~sec).Size);
        }
    }
    // Try the first non-empty writable segment.
    UntypedInt RW = 3;
    foreach (var (_, load) in (~x.f).Loads) {
        var (seg, ok) = load._<ж<machoꓸSegment>>(ᐧ);
        if (ok && (~seg).Addr != 0 && (~seg).Filesz != 0 && (~seg).Prot == RW && (~seg).Maxprot == RW) {
            return ((~seg).Addr, (~seg).Memsz);
        }
    }
    return (0, 0);
}

// xcoffExe is the XCOFF (AIX eXtended COFF) implementation of the exe interface.
[GoType] partial struct xcoffExe {
    internal ж<xcoff.File> f;
}

[GoRecv] internal static (slice<byte>, error) ReadData(this ref xcoffExe x, uint64 addr, uint64 size) {
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((~sect).VirtualAddress <= addr && addr <= (~sect).VirtualAddress + (~sect).Size - 1) {
            var n = (~sect).VirtualAddress + (~sect).Size - addr;
            if (n > size) {
                n = size;
            }
            return saferio.ReadDataAt(new xcoff_ΔSectionжReaderAt(sect), n, (int64)(addr - (~sect).VirtualAddress));
        }
    }
    return (default!, errors.New("address not mapped"u8));
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref xcoffExe x) {
    {
        var s = x.f.SectionByType(xcoff.STYP_DATA); if (s != nil) {
            return ((~s).VirtualAddress, (~s).Size);
        }
    }
    return (0, 0);
}

// plan9objExe is the Plan 9 a.out implementation of the exe interface.
[GoType] partial struct plan9objExe {
    internal ж<plan9obj.File> f;
}

[GoRecv] internal static (uint64, uint64) DataStart(this ref plan9objExe x) {
    {
        var s = x.f.Section("data"u8); if (s != nil) {
            return ((uint64)(~s).Offset, (uint64)(~s).Size);
        }
    }
    return (0, 0);
}

[GoRecv] internal static (slice<byte>, error) ReadData(this ref plan9objExe x, uint64 addr, uint64 size) {
    foreach (var (_, sect) in (~x.f).Sections) {
        if ((uint64)(~sect).Offset <= addr && addr <= (uint64)((~sect).Offset + (~sect).Size - 1)) {
            var n = (uint64)((~sect).Offset + (~sect).Size) - addr;
            if (n > size) {
                n = size;
            }
            return saferio.ReadDataAt(new plan9obj_ΔSectionжReaderAt(sect), n, (int64)(addr - (uint64)(~sect).Offset));
        }
    }
    return (default!, errors.New("address not mapped"u8));
}

} // end buildinfo_package
