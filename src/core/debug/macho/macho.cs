// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Mach-O header data structures
// Originally at:
// http://developer.apple.com/mac/library/documentation/DeveloperTools/Conceptual/MachORuntime/Reference/reference.html (since deleted by Apple)
// Archived copy at:
// https://web.archive.org/web/20090819232456/http://developer.apple.com/documentation/DeveloperTools/Conceptual/MachORuntime/index.html
// For cloned PDF see:
// https://github.com/aidansteele/osx-abi-macho-file-format-reference
namespace go.debug;

using strconv = strconv_package;

partial class macho_package {

// A FileHeader represents a Mach-O file header.
[GoType] partial struct FileHeader {
    public uint32 Magic;
    public Cpu Cpu;
    public uint32 SubCpu;
    public Type Type;
    public uint32 Ncmd;
    public uint32 Cmdsz;
    public uint32 Flags;
}

internal static readonly UntypedInt fileHeaderSize32 = /* 7 * 4 */ 28;
internal static readonly UntypedInt fileHeaderSize64 = /* 8 * 4 */ 32;

public const uint32 Magic32 = /* 0xfeedface */ 4277009102;
public const uint32 Magic64 = /* 0xfeedfacf */ 4277009103;
public const uint32 MagicFat = /* 0xcafebabe */ 3405691582;

[GoType("num:uint32")] partial struct Type;

public static readonly Type TypeObj = 1;
public static readonly Type TypeExec = 2;
public static readonly Type TypeDylib = 6;
public static readonly Type TypeBundle = 8;

internal static slice<intName> typeStrings = new intName[]{
    new(((uint32)TypeObj), "Obj"u8),
    new(((uint32)TypeExec), "Exec"u8),
    new(((uint32)TypeDylib), "Dylib"u8),
    new(((uint32)TypeBundle), "Bundle"u8)
}.slice();

public static @string String(this Type t) {
    return stringName(((uint32)t), typeStrings, false);
}

public static @string GoString(this Type t) {
    return stringName(((uint32)t), typeStrings, true);
}

[GoType("num:uint32")] partial struct Cpu;

internal static readonly UntypedInt cpuArch64 = /* 0x01000000 */ 16777216;

public static readonly Cpu Cpu386 = 7;
public static readonly Cpu CpuAmd64 = /* Cpu386 | cpuArch64 */ 16777223;
public static readonly Cpu CpuArm = 12;
public static readonly Cpu CpuArm64 = /* CpuArm | cpuArch64 */ 16777228;
public static readonly Cpu CpuPpc = 18;
public static readonly Cpu CpuPpc64 = /* CpuPpc | cpuArch64 */ 16777234;

internal static slice<intName> cpuStrings = new intName[]{
    new(((uint32)Cpu386), "Cpu386"u8),
    new(((uint32)CpuAmd64), "CpuAmd64"u8),
    new(((uint32)CpuArm), "CpuArm"u8),
    new(((uint32)CpuArm64), "CpuArm64"u8),
    new(((uint32)CpuPpc), "CpuPpc"u8),
    new(((uint32)CpuPpc64), "CpuPpc64"u8)
}.slice();

public static @string String(this Cpu i) {
    return stringName(((uint32)i), cpuStrings, false);
}

public static @string GoString(this Cpu i) {
    return stringName(((uint32)i), cpuStrings, true);
}

[GoType("num:uint32")] partial struct LoadCmd;

public static readonly LoadCmd LoadCmdSegment = /* 0x1 */ 1;
public static readonly LoadCmd LoadCmdSymtab = /* 0x2 */ 2;
public static readonly LoadCmd LoadCmdThread = /* 0x4 */ 4;
public static readonly LoadCmd LoadCmdUnixThread = /* 0x5 */ 5; // thread+stack
public static readonly LoadCmd LoadCmdDysymtab = /* 0xb */ 11;
public static readonly LoadCmd LoadCmdDylib = /* 0xc */ 12;      // load dylib command
public static readonly LoadCmd LoadCmdDylinker = /* 0xf */ 15;   // id dylinker command (not load dylinker command)
public static readonly LoadCmd LoadCmdSegment64 = /* 0x19 */ 25;
public static readonly LoadCmd LoadCmdRpath = /* 0x8000001c */ 2147483676;

internal static slice<intName> cmdStrings = new intName[]{
    new(((uint32)LoadCmdSegment), "LoadCmdSegment"u8),
    new(((uint32)LoadCmdThread), "LoadCmdThread"u8),
    new(((uint32)LoadCmdUnixThread), "LoadCmdUnixThread"u8),
    new(((uint32)LoadCmdDylib), "LoadCmdDylib"u8),
    new(((uint32)LoadCmdSegment64), "LoadCmdSegment64"u8),
    new(((uint32)LoadCmdRpath), "LoadCmdRpath"u8)
}.slice();

public static @string String(this LoadCmd i) {
    return stringName(((uint32)i), cmdStrings, false);
}

public static @string GoString(this LoadCmd i) {
    return stringName(((uint32)i), cmdStrings, true);
}

[GoType] partial struct Segment32 {
    public LoadCmd Cmd;
    public uint32 Len;
    public array<byte> Name = new(16);
    public uint32 Addr;
    public uint32 Memsz;
    public uint32 Offset;
    public uint32 Filesz;
    public uint32 Maxprot;
    public uint32 Prot;
    public uint32 Nsect;
    public uint32 Flag;
}

[GoType] partial struct Segment64 {
    public LoadCmd Cmd;
    public uint32 Len;
    public array<byte> Name = new(16);
    public uint64 Addr;
    public uint64 Memsz;
    public uint64 Offset;
    public uint64 Filesz;
    public uint32 Maxprot;
    public uint32 Prot;
    public uint32 Nsect;
    public uint32 Flag;
}

[GoType] partial struct SymtabCmd {
    public LoadCmd Cmd;
    public uint32 Len;
    public uint32 Symoff;
    public uint32 Nsyms;
    public uint32 Stroff;
    public uint32 Strsize;
}

[GoType] partial struct DysymtabCmd {
    public LoadCmd Cmd;
    public uint32 Len;
    public uint32 Ilocalsym;
    public uint32 Nlocalsym;
    public uint32 Iextdefsym;
    public uint32 Nextdefsym;
    public uint32 Iundefsym;
    public uint32 Nundefsym;
    public uint32 Tocoffset;
    public uint32 Ntoc;
    public uint32 Modtaboff;
    public uint32 Nmodtab;
    public uint32 Extrefsymoff;
    public uint32 Nextrefsyms;
    public uint32 Indirectsymoff;
    public uint32 Nindirectsyms;
    public uint32 Extreloff;
    public uint32 Nextrel;
    public uint32 Locreloff;
    public uint32 Nlocrel;
}

[GoType] partial struct DylibCmd {
    public LoadCmd Cmd;
    public uint32 Len;
    public uint32 Name;
    public uint32 Time;
    public uint32 CurrentVersion;
    public uint32 CompatVersion;
}

[GoType] partial struct RpathCmd {
    public LoadCmd Cmd;
    public uint32 Len;
    public uint32 Path;
}

[GoType] partial struct Thread {
    public LoadCmd Cmd;
    public uint32 Len;
    public uint32 Type;
    public slice<uint32> Data;
}

public const uint32 FlagNoUndefs = /* 0x1 */ 1;
public const uint32 FlagIncrLink = /* 0x2 */ 2;
public const uint32 FlagDyldLink = /* 0x4 */ 4;
public const uint32 FlagBindAtLoad = /* 0x8 */ 8;
public const uint32 FlagPrebound = /* 0x10 */ 16;
public const uint32 FlagSplitSegs = /* 0x20 */ 32;
public const uint32 FlagLazyInit = /* 0x40 */ 64;
public const uint32 FlagTwoLevel = /* 0x80 */ 128;
public const uint32 FlagForceFlat = /* 0x100 */ 256;
public const uint32 FlagNoMultiDefs = /* 0x200 */ 512;
public const uint32 FlagNoFixPrebinding = /* 0x400 */ 1024;
public const uint32 FlagPrebindable = /* 0x800 */ 2048;
public const uint32 FlagAllModsBound = /* 0x1000 */ 4096;
public const uint32 FlagSubsectionsViaSymbols = /* 0x2000 */ 8192;
public const uint32 FlagCanonical = /* 0x4000 */ 16384;
public const uint32 FlagWeakDefines = /* 0x8000 */ 32768;
public const uint32 FlagBindsToWeak = /* 0x10000 */ 65536;
public const uint32 FlagAllowStackExecution = /* 0x20000 */ 131072;
public const uint32 FlagRootSafe = /* 0x40000 */ 262144;
public const uint32 FlagSetuidSafe = /* 0x80000 */ 524288;
public const uint32 FlagNoReexportedDylibs = /* 0x100000 */ 1048576;
public const uint32 FlagPIE = /* 0x200000 */ 2097152;
public const uint32 FlagDeadStrippableDylib = /* 0x400000 */ 4194304;
public const uint32 FlagHasTLVDescriptors = /* 0x800000 */ 8388608;
public const uint32 FlagNoHeapExecution = /* 0x1000000 */ 16777216;
public const uint32 FlagAppExtensionSafe = /* 0x2000000 */ 33554432;

// A Section32 is a 32-bit Mach-O section header.
[GoType] partial struct Section32 {
    public array<byte> Name = new(16);
    public array<byte> Seg = new(16);
    public uint32 Addr;
    public uint32 Size;
    public uint32 Offset;
    public uint32 Align;
    public uint32 Reloff;
    public uint32 Nreloc;
    public uint32 Flags;
    public uint32 Reserve1;
    public uint32 Reserve2;
}

// A Section64 is a 64-bit Mach-O section header.
[GoType] partial struct Section64 {
    public array<byte> Name = new(16);
    public array<byte> Seg = new(16);
    public uint64 Addr;
    public uint64 Size;
    public uint32 Offset;
    public uint32 Align;
    public uint32 Reloff;
    public uint32 Nreloc;
    public uint32 Flags;
    public uint32 Reserve1;
    public uint32 Reserve2;
    public uint32 Reserve3;
}

// An Nlist32 is a Mach-O 32-bit symbol table entry.
[GoType] partial struct Nlist32 {
    public uint32 Name;
    public uint8 Type;
    public uint8 Sect;
    public uint16 Desc;
    public uint32 Value;
}

// An Nlist64 is a Mach-O 64-bit symbol table entry.
[GoType] partial struct Nlist64 {
    public uint32 Name;
    public uint8 Type;
    public uint8 Sect;
    public uint16 Desc;
    public uint64 Value;
}

// Regs386 is the Mach-O 386 register structure.
[GoType] partial struct Regs386 {
    public uint32 AX;
    public uint32 BX;
    public uint32 CX;
    public uint32 DX;
    public uint32 DI;
    public uint32 SI;
    public uint32 BP;
    public uint32 SP;
    public uint32 SS;
    public uint32 FLAGS;
    public uint32 IP;
    public uint32 CS;
    public uint32 DS;
    public uint32 ES;
    public uint32 FS;
    public uint32 GS;
}

// RegsAMD64 is the Mach-O AMD64 register structure.
[GoType] partial struct RegsAMD64 {
    public uint64 AX;
    public uint64 BX;
    public uint64 CX;
    public uint64 DX;
    public uint64 DI;
    public uint64 SI;
    public uint64 BP;
    public uint64 SP;
    public uint64 R8;
    public uint64 R9;
    public uint64 R10;
    public uint64 R11;
    public uint64 R12;
    public uint64 R13;
    public uint64 R14;
    public uint64 R15;
    public uint64 IP;
    public uint64 FLAGS;
    public uint64 CS;
    public uint64 FS;
    public uint64 GS;
}

[GoType] partial struct intName {
    internal uint32 i;
    internal @string s;
}

internal static @string stringName(uint32 i, slice<intName> names, bool goSyntax) {
    foreach (var (_, n) in names) {
        if (n.i == i) {
            if (goSyntax) {
                return "macho."u8 + n.s;
            }
            return n.s;
        }
    }
    return strconv.FormatUint(((uint64)i), 10);
}

} // end macho_package
