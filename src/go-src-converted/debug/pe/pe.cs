// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

partial class pe_package {

[GoType] partial struct FileHeader {
    public uint16 Machine;
    public uint16 NumberOfSections;
    public uint32 TimeDateStamp;
    public uint32 PointerToSymbolTable;
    public uint32 NumberOfSymbols;
    public uint16 SizeOfOptionalHeader;
    public uint16 Characteristics;
}

[GoType] partial struct DataDirectory {
    public uint32 VirtualAddress;
    public uint32 Size;
}

[GoType] partial struct OptionalHeader32 {
    public uint16 Magic;
    public uint8 MajorLinkerVersion;
    public uint8 MinorLinkerVersion;
    public uint32 SizeOfCode;
    public uint32 SizeOfInitializedData;
    public uint32 SizeOfUninitializedData;
    public uint32 AddressOfEntryPoint;
    public uint32 BaseOfCode;
    public uint32 BaseOfData;
    public uint32 ImageBase;
    public uint32 SectionAlignment;
    public uint32 FileAlignment;
    public uint16 MajorOperatingSystemVersion;
    public uint16 MinorOperatingSystemVersion;
    public uint16 MajorImageVersion;
    public uint16 MinorImageVersion;
    public uint16 MajorSubsystemVersion;
    public uint16 MinorSubsystemVersion;
    public uint32 Win32VersionValue;
    public uint32 SizeOfImage;
    public uint32 SizeOfHeaders;
    public uint32 CheckSum;
    public uint16 Subsystem;
    public uint16 DllCharacteristics;
    public uint32 SizeOfStackReserve;
    public uint32 SizeOfStackCommit;
    public uint32 SizeOfHeapReserve;
    public uint32 SizeOfHeapCommit;
    public uint32 LoaderFlags;
    public uint32 NumberOfRvaAndSizes;
    public array<DataDirectory> DataDirectory = new(16);
}

[GoType] partial struct OptionalHeader64 {
    public uint16 Magic;
    public uint8 MajorLinkerVersion;
    public uint8 MinorLinkerVersion;
    public uint32 SizeOfCode;
    public uint32 SizeOfInitializedData;
    public uint32 SizeOfUninitializedData;
    public uint32 AddressOfEntryPoint;
    public uint32 BaseOfCode;
    public uint64 ImageBase;
    public uint32 SectionAlignment;
    public uint32 FileAlignment;
    public uint16 MajorOperatingSystemVersion;
    public uint16 MinorOperatingSystemVersion;
    public uint16 MajorImageVersion;
    public uint16 MinorImageVersion;
    public uint16 MajorSubsystemVersion;
    public uint16 MinorSubsystemVersion;
    public uint32 Win32VersionValue;
    public uint32 SizeOfImage;
    public uint32 SizeOfHeaders;
    public uint32 CheckSum;
    public uint16 Subsystem;
    public uint16 DllCharacteristics;
    public uint64 SizeOfStackReserve;
    public uint64 SizeOfStackCommit;
    public uint64 SizeOfHeapReserve;
    public uint64 SizeOfHeapCommit;
    public uint32 LoaderFlags;
    public uint32 NumberOfRvaAndSizes;
    public array<DataDirectory> DataDirectory = new(16);
}

public static readonly UntypedInt IMAGE_FILE_MACHINE_UNKNOWN = /* 0x0 */ 0;
public static readonly UntypedInt IMAGE_FILE_MACHINE_AM33 = /* 0x1d3 */ 467;
public static readonly UntypedInt IMAGE_FILE_MACHINE_AMD64 = /* 0x8664 */ 34404;
public static readonly UntypedInt IMAGE_FILE_MACHINE_ARM = /* 0x1c0 */ 448;
public static readonly UntypedInt IMAGE_FILE_MACHINE_ARMNT = /* 0x1c4 */ 452;
public static readonly UntypedInt IMAGE_FILE_MACHINE_ARM64 = /* 0xaa64 */ 43620;
public static readonly UntypedInt IMAGE_FILE_MACHINE_EBC = /* 0xebc */ 3772;
public static readonly UntypedInt IMAGE_FILE_MACHINE_I386 = /* 0x14c */ 332;
public static readonly UntypedInt IMAGE_FILE_MACHINE_IA64 = /* 0x200 */ 512;
public static readonly UntypedInt IMAGE_FILE_MACHINE_LOONGARCH32 = /* 0x6232 */ 25138;
public static readonly UntypedInt IMAGE_FILE_MACHINE_LOONGARCH64 = /* 0x6264 */ 25188;
public static readonly UntypedInt IMAGE_FILE_MACHINE_M32R = /* 0x9041 */ 36929;
public static readonly UntypedInt IMAGE_FILE_MACHINE_MIPS16 = /* 0x266 */ 614;
public static readonly UntypedInt IMAGE_FILE_MACHINE_MIPSFPU = /* 0x366 */ 870;
public static readonly UntypedInt IMAGE_FILE_MACHINE_MIPSFPU16 = /* 0x466 */ 1126;
public static readonly UntypedInt IMAGE_FILE_MACHINE_POWERPC = /* 0x1f0 */ 496;
public static readonly UntypedInt IMAGE_FILE_MACHINE_POWERPCFP = /* 0x1f1 */ 497;
public static readonly UntypedInt IMAGE_FILE_MACHINE_R4000 = /* 0x166 */ 358;
public static readonly UntypedInt IMAGE_FILE_MACHINE_SH3 = /* 0x1a2 */ 418;
public static readonly UntypedInt IMAGE_FILE_MACHINE_SH3DSP = /* 0x1a3 */ 419;
public static readonly UntypedInt IMAGE_FILE_MACHINE_SH4 = /* 0x1a6 */ 422;
public static readonly UntypedInt IMAGE_FILE_MACHINE_SH5 = /* 0x1a8 */ 424;
public static readonly UntypedInt IMAGE_FILE_MACHINE_THUMB = /* 0x1c2 */ 450;
public static readonly UntypedInt IMAGE_FILE_MACHINE_WCEMIPSV2 = /* 0x169 */ 361;
public static readonly UntypedInt IMAGE_FILE_MACHINE_RISCV32 = /* 0x5032 */ 20530;
public static readonly UntypedInt IMAGE_FILE_MACHINE_RISCV64 = /* 0x5064 */ 20580;
public static readonly UntypedInt IMAGE_FILE_MACHINE_RISCV128 = /* 0x5128 */ 20776;

// IMAGE_DIRECTORY_ENTRY constants
public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_EXPORT = 0;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_IMPORT = 1;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_SECURITY = 4;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_BASERELOC = 5;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_DEBUG = 6;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_TLS = 9;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_IAT = 12;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13;

public static readonly UntypedInt IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14;

// Values of IMAGE_FILE_HEADER.Characteristics. These can be combined together.
public static readonly UntypedInt IMAGE_FILE_RELOCS_STRIPPED = /* 0x0001 */ 1;

public static readonly UntypedInt IMAGE_FILE_EXECUTABLE_IMAGE = /* 0x0002 */ 2;

public static readonly UntypedInt IMAGE_FILE_LINE_NUMS_STRIPPED = /* 0x0004 */ 4;

public static readonly UntypedInt IMAGE_FILE_LOCAL_SYMS_STRIPPED = /* 0x0008 */ 8;

public static readonly UntypedInt IMAGE_FILE_AGGRESIVE_WS_TRIM = /* 0x0010 */ 16;

public static readonly UntypedInt IMAGE_FILE_LARGE_ADDRESS_AWARE = /* 0x0020 */ 32;

public static readonly UntypedInt IMAGE_FILE_BYTES_REVERSED_LO = /* 0x0080 */ 128;

public static readonly UntypedInt IMAGE_FILE_32BIT_MACHINE = /* 0x0100 */ 256;

public static readonly UntypedInt IMAGE_FILE_DEBUG_STRIPPED = /* 0x0200 */ 512;

public static readonly UntypedInt IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = /* 0x0400 */ 1024;

public static readonly UntypedInt IMAGE_FILE_NET_RUN_FROM_SWAP = /* 0x0800 */ 2048;

public static readonly UntypedInt IMAGE_FILE_SYSTEM = /* 0x1000 */ 4096;

public static readonly UntypedInt IMAGE_FILE_DLL = /* 0x2000 */ 8192;

public static readonly UntypedInt IMAGE_FILE_UP_SYSTEM_ONLY = /* 0x4000 */ 16384;

public static readonly UntypedInt IMAGE_FILE_BYTES_REVERSED_HI = /* 0x8000 */ 32768;

// OptionalHeader64.Subsystem and OptionalHeader32.Subsystem values.
public static readonly UntypedInt IMAGE_SUBSYSTEM_UNKNOWN = 0;

public static readonly UntypedInt IMAGE_SUBSYSTEM_NATIVE = 1;

public static readonly UntypedInt IMAGE_SUBSYSTEM_WINDOWS_GUI = 2;

public static readonly UntypedInt IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;

public static readonly UntypedInt IMAGE_SUBSYSTEM_OS2_CUI = 5;

public static readonly UntypedInt IMAGE_SUBSYSTEM_POSIX_CUI = 7;

public static readonly UntypedInt IMAGE_SUBSYSTEM_NATIVE_WINDOWS = 8;

public static readonly UntypedInt IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9;

public static readonly UntypedInt IMAGE_SUBSYSTEM_EFI_APPLICATION = 10;

public static readonly UntypedInt IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11;

public static readonly UntypedInt IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12;

public static readonly UntypedInt IMAGE_SUBSYSTEM_EFI_ROM = 13;

public static readonly UntypedInt IMAGE_SUBSYSTEM_XBOX = 14;

public static readonly UntypedInt IMAGE_SUBSYSTEM_WINDOWS_BOOT_APPLICATION = 16;

// OptionalHeader64.DllCharacteristics and OptionalHeader32.DllCharacteristics
// values. These can be combined together.
public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA = /* 0x0020 */ 32;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE = /* 0x0040 */ 64;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_FORCE_INTEGRITY = /* 0x0080 */ 128;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_NX_COMPAT = /* 0x0100 */ 256;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = /* 0x0200 */ 512;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_NO_SEH = /* 0x0400 */ 1024;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_NO_BIND = /* 0x0800 */ 2048;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_APPCONTAINER = /* 0x1000 */ 4096;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = /* 0x2000 */ 8192;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_GUARD_CF = /* 0x4000 */ 16384;

public static readonly UntypedInt IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = /* 0x8000 */ 32768;

} // end pe_package
