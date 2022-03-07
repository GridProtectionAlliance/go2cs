// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:21:57 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\macho.go
using bytes = go.bytes_package;
using codesign = go.cmd.@internal.codesign_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using macho = go.debug.macho_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.cmd.link.@internal;

public static partial class ld_package {

public partial struct MachoHdr {
    public uint cpu;
    public uint subcpu;
}

public partial struct MachoSect {
    public @string name;
    public @string segname;
    public ulong addr;
    public ulong size;
    public uint off;
    public uint align;
    public uint reloc;
    public uint nreloc;
    public uint flag;
    public uint res1;
    public uint res2;
}

public partial struct MachoSeg {
    public @string name;
    public ulong vsize;
    public ulong vaddr;
    public ulong fileoffset;
    public ulong filesize;
    public uint prot1;
    public uint prot2;
    public uint nsect;
    public uint msect;
    public slice<MachoSect> sect;
    public uint flag;
}

// MachoPlatformLoad represents a LC_VERSION_MIN_* or
// LC_BUILD_VERSION load command.
public partial struct MachoPlatformLoad {
    public MachoPlatform platform; // One of PLATFORM_* constants.
    public MachoLoad cmd;
}

public partial struct MachoLoad {
    public uint type_;
    public slice<uint> data;
}

public partial struct MachoPlatform { // : nint
}

/*
 * Total amount of space to reserve at the start of the file
 * for Header, PHeaders, and SHeaders.
 * May waste some.
 */
public static readonly nint INITIAL_MACHO_HEADR = 4 * 1024;


public static readonly nint MACHO_CPU_AMD64 = 1 << 24 | 7;
public static readonly nint MACHO_CPU_386 = 7;
public static readonly nint MACHO_SUBCPU_X86 = 3;
public static readonly nint MACHO_CPU_ARM = 12;
public static readonly nint MACHO_SUBCPU_ARM = 0;
public static readonly nint MACHO_SUBCPU_ARMV7 = 9;
public static readonly nint MACHO_CPU_ARM64 = 1 << 24 | 12;
public static readonly nint MACHO_SUBCPU_ARM64_ALL = 0;
public static readonly nint MACHO_SUBCPU_ARM64_V8 = 1;
public static readonly nint MACHO_SUBCPU_ARM64E = 2;
public static readonly nint MACHO32SYMSIZE = 12;
public static readonly nint MACHO64SYMSIZE = 16;
public static readonly nint MACHO_X86_64_RELOC_UNSIGNED = 0;
public static readonly nint MACHO_X86_64_RELOC_SIGNED = 1;
public static readonly nint MACHO_X86_64_RELOC_BRANCH = 2;
public static readonly nint MACHO_X86_64_RELOC_GOT_LOAD = 3;
public static readonly nint MACHO_X86_64_RELOC_GOT = 4;
public static readonly nint MACHO_X86_64_RELOC_SUBTRACTOR = 5;
public static readonly nint MACHO_X86_64_RELOC_SIGNED_1 = 6;
public static readonly nint MACHO_X86_64_RELOC_SIGNED_2 = 7;
public static readonly nint MACHO_X86_64_RELOC_SIGNED_4 = 8;
public static readonly nint MACHO_ARM_RELOC_VANILLA = 0;
public static readonly nint MACHO_ARM_RELOC_PAIR = 1;
public static readonly nint MACHO_ARM_RELOC_SECTDIFF = 2;
public static readonly nint MACHO_ARM_RELOC_BR24 = 5;
public static readonly nint MACHO_ARM64_RELOC_UNSIGNED = 0;
public static readonly nint MACHO_ARM64_RELOC_BRANCH26 = 2;
public static readonly nint MACHO_ARM64_RELOC_PAGE21 = 3;
public static readonly nint MACHO_ARM64_RELOC_PAGEOFF12 = 4;
public static readonly nint MACHO_ARM64_RELOC_GOT_LOAD_PAGE21 = 5;
public static readonly nint MACHO_ARM64_RELOC_GOT_LOAD_PAGEOFF12 = 6;
public static readonly nint MACHO_ARM64_RELOC_ADDEND = 10;
public static readonly nint MACHO_GENERIC_RELOC_VANILLA = 0;
public static readonly nint MACHO_FAKE_GOTPCREL = 100;


public static readonly nuint MH_MAGIC = 0xfeedface;
public static readonly nuint MH_MAGIC_64 = 0xfeedfacf;

public static readonly nuint MH_OBJECT = 0x1;
public static readonly nuint MH_EXECUTE = 0x2;

public static readonly nuint MH_NOUNDEFS = 0x1;
public static readonly nuint MH_DYLDLINK = 0x4;
public static readonly nuint MH_PIE = 0x200000;


public static readonly nuint LC_SEGMENT = 0x1;
public static readonly nuint LC_SYMTAB = 0x2;
public static readonly nuint LC_SYMSEG = 0x3;
public static readonly nuint LC_THREAD = 0x4;
public static readonly nuint LC_UNIXTHREAD = 0x5;
public static readonly nuint LC_LOADFVMLIB = 0x6;
public static readonly nuint LC_IDFVMLIB = 0x7;
public static readonly nuint LC_IDENT = 0x8;
public static readonly nuint LC_FVMFILE = 0x9;
public static readonly nuint LC_PREPAGE = 0xa;
public static readonly nuint LC_DYSYMTAB = 0xb;
public static readonly nuint LC_LOAD_DYLIB = 0xc;
public static readonly nuint LC_ID_DYLIB = 0xd;
public static readonly nuint LC_LOAD_DYLINKER = 0xe;
public static readonly nuint LC_ID_DYLINKER = 0xf;
public static readonly nuint LC_PREBOUND_DYLIB = 0x10;
public static readonly nuint LC_ROUTINES = 0x11;
public static readonly nuint LC_SUB_FRAMEWORK = 0x12;
public static readonly nuint LC_SUB_UMBRELLA = 0x13;
public static readonly nuint LC_SUB_CLIENT = 0x14;
public static readonly nuint LC_SUB_LIBRARY = 0x15;
public static readonly nuint LC_TWOLEVEL_HINTS = 0x16;
public static readonly nuint LC_PREBIND_CKSUM = 0x17;
public static readonly nuint LC_LOAD_WEAK_DYLIB = 0x80000018;
public static readonly nuint LC_SEGMENT_64 = 0x19;
public static readonly nuint LC_ROUTINES_64 = 0x1a;
public static readonly nuint LC_UUID = 0x1b;
public static readonly nuint LC_RPATH = 0x8000001c;
public static readonly nuint LC_CODE_SIGNATURE = 0x1d;
public static readonly nuint LC_SEGMENT_SPLIT_INFO = 0x1e;
public static readonly nuint LC_REEXPORT_DYLIB = 0x8000001f;
public static readonly nuint LC_LAZY_LOAD_DYLIB = 0x20;
public static readonly nuint LC_ENCRYPTION_INFO = 0x21;
public static readonly nuint LC_DYLD_INFO = 0x22;
public static readonly nuint LC_DYLD_INFO_ONLY = 0x80000022;
public static readonly nuint LC_LOAD_UPWARD_DYLIB = 0x80000023;
public static readonly nuint LC_VERSION_MIN_MACOSX = 0x24;
public static readonly nuint LC_VERSION_MIN_IPHONEOS = 0x25;
public static readonly nuint LC_FUNCTION_STARTS = 0x26;
public static readonly nuint LC_DYLD_ENVIRONMENT = 0x27;
public static readonly nuint LC_MAIN = 0x80000028;
public static readonly nuint LC_DATA_IN_CODE = 0x29;
public static readonly nuint LC_SOURCE_VERSION = 0x2A;
public static readonly nuint LC_DYLIB_CODE_SIGN_DRS = 0x2B;
public static readonly nuint LC_ENCRYPTION_INFO_64 = 0x2C;
public static readonly nuint LC_LINKER_OPTION = 0x2D;
public static readonly nuint LC_LINKER_OPTIMIZATION_HINT = 0x2E;
public static readonly nuint LC_VERSION_MIN_TVOS = 0x2F;
public static readonly nuint LC_VERSION_MIN_WATCHOS = 0x30;
public static readonly nuint LC_VERSION_NOTE = 0x31;
public static readonly nuint LC_BUILD_VERSION = 0x32;
public static readonly nuint LC_DYLD_EXPORTS_TRIE = 0x80000033;
public static readonly nuint LC_DYLD_CHAINED_FIXUPS = 0x80000034;


public static readonly nuint S_REGULAR = 0x0;
public static readonly nuint S_ZEROFILL = 0x1;
public static readonly nuint S_NON_LAZY_SYMBOL_POINTERS = 0x6;
public static readonly nuint S_SYMBOL_STUBS = 0x8;
public static readonly nuint S_MOD_INIT_FUNC_POINTERS = 0x9;
public static readonly nuint S_ATTR_PURE_INSTRUCTIONS = 0x80000000;
public static readonly nuint S_ATTR_DEBUG = 0x02000000;
public static readonly nuint S_ATTR_SOME_INSTRUCTIONS = 0x00000400;


public static readonly MachoPlatform PLATFORM_MACOS = 1;
public static readonly MachoPlatform PLATFORM_IOS = 2;
public static readonly MachoPlatform PLATFORM_TVOS = 3;
public static readonly MachoPlatform PLATFORM_WATCHOS = 4;
public static readonly MachoPlatform PLATFORM_BRIDGEOS = 5;


// rebase table opcode
public static readonly nint REBASE_TYPE_POINTER = 1;
public static readonly nint REBASE_TYPE_TEXT_ABSOLUTE32 = 2;
public static readonly nint REBASE_TYPE_TEXT_PCREL32 = 3;

public static readonly nuint REBASE_OPCODE_MASK = 0xF0;
public static readonly nuint REBASE_IMMEDIATE_MASK = 0x0F;
public static readonly nuint REBASE_OPCODE_DONE = 0x00;
public static readonly nuint REBASE_OPCODE_SET_TYPE_IMM = 0x10;
public static readonly nuint REBASE_OPCODE_SET_SEGMENT_AND_OFFSET_ULEB = 0x20;
public static readonly nuint REBASE_OPCODE_ADD_ADDR_ULEB = 0x30;
public static readonly nuint REBASE_OPCODE_ADD_ADDR_IMM_SCALED = 0x40;
public static readonly nuint REBASE_OPCODE_DO_REBASE_IMM_TIMES = 0x50;
public static readonly nuint REBASE_OPCODE_DO_REBASE_ULEB_TIMES = 0x60;
public static readonly nuint REBASE_OPCODE_DO_REBASE_ADD_ADDR_ULEB = 0x70;
public static readonly nuint REBASE_OPCODE_DO_REBASE_ULEB_TIMES_SKIPPING_ULEB = 0x80;


// bind table opcode
public static readonly nint BIND_TYPE_POINTER = 1;
public static readonly nint BIND_TYPE_TEXT_ABSOLUTE32 = 2;
public static readonly nint BIND_TYPE_TEXT_PCREL32 = 3;

public static readonly nint BIND_SPECIAL_DYLIB_SELF = 0;
public static readonly nint BIND_SPECIAL_DYLIB_MAIN_EXECUTABLE = -1;
public static readonly nint BIND_SPECIAL_DYLIB_FLAT_LOOKUP = -2;
public static readonly nint BIND_SPECIAL_DYLIB_WEAK_LOOKUP = -3;

public static readonly nuint BIND_OPCODE_MASK = 0xF0;
public static readonly nuint BIND_IMMEDIATE_MASK = 0x0F;
public static readonly nuint BIND_OPCODE_DONE = 0x00;
public static readonly nuint BIND_OPCODE_SET_DYLIB_ORDINAL_IMM = 0x10;
public static readonly nuint BIND_OPCODE_SET_DYLIB_ORDINAL_ULEB = 0x20;
public static readonly nuint BIND_OPCODE_SET_DYLIB_SPECIAL_IMM = 0x30;
public static readonly nuint BIND_OPCODE_SET_SYMBOL_TRAILING_FLAGS_IMM = 0x40;
public static readonly nuint BIND_OPCODE_SET_TYPE_IMM = 0x50;
public static readonly nuint BIND_OPCODE_SET_ADDEND_SLEB = 0x60;
public static readonly nuint BIND_OPCODE_SET_SEGMENT_AND_OFFSET_ULEB = 0x70;
public static readonly nuint BIND_OPCODE_ADD_ADDR_ULEB = 0x80;
public static readonly nuint BIND_OPCODE_DO_BIND = 0x90;
public static readonly nuint BIND_OPCODE_DO_BIND_ADD_ADDR_ULEB = 0xA0;
public static readonly nuint BIND_OPCODE_DO_BIND_ADD_ADDR_IMM_SCALED = 0xB0;
public static readonly nuint BIND_OPCODE_DO_BIND_ULEB_TIMES_SKIPPING_ULEB = 0xC0;
public static readonly nuint BIND_OPCODE_THREADED = 0xD0;
public static readonly nuint BIND_SUBOPCODE_THREADED_SET_BIND_ORDINAL_TABLE_SIZE_ULEB = 0x00;
public static readonly nuint BIND_SUBOPCODE_THREADED_APPLY = 0x01;


private static readonly nint machoHeaderSize64 = 8 * 4; // size of 64-bit Mach-O header

// Mach-O file writing
// https://developer.apple.com/mac/library/DOCUMENTATION/DeveloperTools/Conceptual/MachORuntime/Reference/reference.html

 // size of 64-bit Mach-O header

// Mach-O file writing
// https://developer.apple.com/mac/library/DOCUMENTATION/DeveloperTools/Conceptual/MachORuntime/Reference/reference.html

private static MachoHdr machohdr = default;

private static slice<MachoLoad> load = default;

private static MachoPlatform machoPlatform = default;

private static array<MachoSeg> seg = new array<MachoSeg>(16);

private static nint nseg = default;

private static nint ndebug = default;

private static nint nsect = default;

public static readonly nint SymKindLocal = 0 + iota;
public static readonly var SymKindExtdef = 0;
public static readonly var SymKindUndef = 1;
public static readonly var NumSymKind = 2;


private static array<nint> nkind = new array<nint>(NumSymKind);

private static slice<loader.Sym> sortsym = default;

private static nint nsortsym = default;

// Amount of space left for adding load commands
// that refer to dynamic libraries. Because these have
// to go in the Mach-O header, we can't just pick a
// "big enough" header size. The initial header is
// one page, the non-dynamic library stuff takes
// up about 1300 bytes; we overestimate that as 2k.
private static var loadBudget = INITIAL_MACHO_HEADR - 2 * 1024;

private static ptr<MachoHdr> getMachoHdr() {
    return _addr__addr_machohdr!;
}

private static ptr<MachoLoad> newMachoLoad(ptr<sys.Arch> _addr_arch, uint type_, uint ndata) {
    ref sys.Arch arch = ref _addr_arch.val;

    if (arch.PtrSize == 8 && (ndata & 1 != 0)) {
        ndata++;
    }
    load = append(load, new MachoLoad());
    var l = _addr_load[len(load) - 1];
    l.type_ = type_;
    l.data = make_slice<uint>(ndata);
    return _addr_l!;

}

private static ptr<MachoSeg> newMachoSeg(@string name, nint msect) {
    if (nseg >= len(seg)) {
        Exitf("too many segs");
    }
    var s = _addr_seg[nseg];
    nseg++;
    s.name = name;
    s.msect = uint32(msect);
    s.sect = make_slice<MachoSect>(msect);
    return _addr_s!;

}

private static ptr<MachoSect> newMachoSect(ptr<MachoSeg> _addr_seg, @string name, @string segname) {
    ref MachoSeg seg = ref _addr_seg.val;

    if (seg.nsect >= seg.msect) {
        Exitf("too many sects in segment %s", seg.name);
    }
    var s = _addr_seg.sect[seg.nsect];
    seg.nsect++;
    s.name = name;
    s.segname = segname;
    nsect++;
    return _addr_s!;

}

// Generic linking code.

private static slice<@string> dylib = default;

private static long linkoff = default;

private static nint machowrite(ptr<Link> _addr_ctxt, ptr<sys.Arch> _addr_arch, ptr<OutBuf> _addr_@out, LinkMode linkmode) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref OutBuf @out = ref _addr_@out.val;

    var o1 = @out.Offset();

    nint loadsize = 4 * 4 * ndebug;
    {
        var i__prev1 = i;

        foreach (var (__i) in load) {
            i = __i;
            loadsize += 4 * (len(load[i].data) + 2);
        }
        i = i__prev1;
    }

    if (arch.PtrSize == 8) {
        loadsize += 18 * 4 * nseg;
        loadsize += 20 * 4 * nsect;
    }
    else
 {
        loadsize += 14 * 4 * nseg;
        loadsize += 17 * 4 * nsect;
    }
    if (arch.PtrSize == 8) {
        @out.Write32(MH_MAGIC_64);
    }
    else
 {
        @out.Write32(MH_MAGIC);
    }
    @out.Write32(machohdr.cpu);
    @out.Write32(machohdr.subcpu);
    if (linkmode == LinkExternal) {
        @out.Write32(MH_OBJECT); /* file type - mach object */
    }
    else
 {
        @out.Write32(MH_EXECUTE); /* file type - mach executable */
    }
    @out.Write32(uint32(len(load)) + uint32(nseg) + uint32(ndebug));
    @out.Write32(uint32(loadsize));
    var flags = uint32(0);
    if (nkind[SymKindUndef] == 0) {
        flags |= MH_NOUNDEFS;
    }
    if (ctxt.IsPIE() && linkmode == LinkInternal) {
        flags |= MH_PIE | MH_DYLDLINK;
    }
    @out.Write32(flags); /* flags */
    if (arch.PtrSize == 8) {
        @out.Write32(0); /* reserved */
    }
    {
        var i__prev1 = i;

        for (nint i = 0; i < nseg; i++) {
            var s = _addr_seg[i];
            if (arch.PtrSize == 8) {
                @out.Write32(LC_SEGMENT_64);
                @out.Write32(72 + 80 * s.nsect);
                @out.WriteStringN(s.name, 16);
                @out.Write64(s.vaddr);
                @out.Write64(s.vsize);
                @out.Write64(s.fileoffset);
                @out.Write64(s.filesize);
                @out.Write32(s.prot1);
                @out.Write32(s.prot2);
                @out.Write32(s.nsect);
                @out.Write32(s.flag);
            }
            else
 {
                @out.Write32(LC_SEGMENT);
                @out.Write32(56 + 68 * s.nsect);
                @out.WriteStringN(s.name, 16);
                @out.Write32(uint32(s.vaddr));
                @out.Write32(uint32(s.vsize));
                @out.Write32(uint32(s.fileoffset));
                @out.Write32(uint32(s.filesize));
                @out.Write32(s.prot1);
                @out.Write32(s.prot2);
                @out.Write32(s.nsect);
                @out.Write32(s.flag);
            }

            {
                var j__prev2 = j;

                for (var j = uint32(0); j < s.nsect; j++) {
                    var t = _addr_s.sect[j];
                    if (arch.PtrSize == 8) {
                        @out.WriteStringN(t.name, 16);
                        @out.WriteStringN(t.segname, 16);
                        @out.Write64(t.addr);
                        @out.Write64(t.size);
                        @out.Write32(t.off);
                        @out.Write32(t.align);
                        @out.Write32(t.reloc);
                        @out.Write32(t.nreloc);
                        @out.Write32(t.flag);
                        @out.Write32(t.res1); /* reserved */
                        @out.Write32(t.res2); /* reserved */
                        @out.Write32(0); /* reserved */
                    }
                    else
 {
                        @out.WriteStringN(t.name, 16);
                        @out.WriteStringN(t.segname, 16);
                        @out.Write32(uint32(t.addr));
                        @out.Write32(uint32(t.size));
                        @out.Write32(t.off);
                        @out.Write32(t.align);
                        @out.Write32(t.reloc);
                        @out.Write32(t.nreloc);
                        @out.Write32(t.flag);
                        @out.Write32(t.res1); /* reserved */
                        @out.Write32(t.res2); /* reserved */
                    }

                }


                j = j__prev2;
            }

        }

        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in load) {
            i = __i;
            var l = _addr_load[i];
            @out.Write32(l.type_);
            @out.Write32(4 * (uint32(len(l.data)) + 2));
            {
                var j__prev2 = j;

                for (j = 0; j < len(l.data); j++) {
                    @out.Write32(l.data[j]);
                }


                j = j__prev2;
            }

        }
        i = i__prev1;
    }

    return int(@out.Offset() - o1);

}

private static void domacho(this ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (FlagD.val) {
        return ;
    }
    foreach (var (_, h) in hostobj) {
        var (load, err) = hostobjMachoPlatform(_addr_h);
        if (err != null) {
            Exitf("%v", err);
        }
        if (load != null) {
            machoPlatform = load.platform;
            var ml = newMachoLoad(_addr_ctxt.Arch, load.cmd.type_, uint32(len(load.cmd.data)));
            copy(ml.data, load.cmd.data);
            break;
        }
    }    if (machoPlatform == 0) {
        machoPlatform = PLATFORM_MACOS;
        if (buildcfg.GOOS == "ios") {
            machoPlatform = PLATFORM_IOS;
        }
        if (ctxt.LinkMode == LinkInternal && machoPlatform == PLATFORM_MACOS) {
            uint version = default;

            if (ctxt.Arch.Family == sys.AMD64) 
                // The version must be at least 10.9; see golang.org/issues/30488.
                version = 10 << 16 | 9 << 8 | 0 << 0; // 10.9.0
            else if (ctxt.Arch.Family == sys.ARM64) 
                version = 11 << 16 | 0 << 8 | 0 << 0; // 11.0.0
                        ml = newMachoLoad(_addr_ctxt.Arch, LC_BUILD_VERSION, 4);
            ml.data[0] = uint32(machoPlatform);
            ml.data[1] = version; // OS version
            ml.data[2] = version; // SDK version
            ml.data[3] = 0; // ntools
        }
    }
    var s = ctxt.loader.LookupOrCreateSym(".machosymstr", 0);
    var sb = ctxt.loader.MakeSymbolUpdater(s);

    sb.SetType(sym.SMACHOSYMSTR);
    sb.SetReachable(true);
    sb.AddUint8(' ');
    sb.AddUint8('\x00');

    s = ctxt.loader.LookupOrCreateSym(".machosymtab", 0);
    sb = ctxt.loader.MakeSymbolUpdater(s);
    sb.SetType(sym.SMACHOSYMTAB);
    sb.SetReachable(true);

    if (ctxt.IsInternal()) {
        s = ctxt.loader.LookupOrCreateSym(".plt", 0); // will be __symbol_stub
        sb = ctxt.loader.MakeSymbolUpdater(s);
        sb.SetType(sym.SMACHOPLT);
        sb.SetReachable(true);

        s = ctxt.loader.LookupOrCreateSym(".got", 0); // will be __nl_symbol_ptr
        sb = ctxt.loader.MakeSymbolUpdater(s);
        sb.SetType(sym.SMACHOGOT);
        sb.SetReachable(true);
        sb.SetAlign(4);

        s = ctxt.loader.LookupOrCreateSym(".linkedit.plt", 0); // indirect table for .plt
        sb = ctxt.loader.MakeSymbolUpdater(s);
        sb.SetType(sym.SMACHOINDIRECTPLT);
        sb.SetReachable(true);

        s = ctxt.loader.LookupOrCreateSym(".linkedit.got", 0); // indirect table for .got
        sb = ctxt.loader.MakeSymbolUpdater(s);
        sb.SetType(sym.SMACHOINDIRECTGOT);
        sb.SetReachable(true);

    }
    if (ctxt.IsExternal()) {
        s = ctxt.loader.LookupOrCreateSym(".llvmasm", 0);
        sb = ctxt.loader.MakeSymbolUpdater(s);
        sb.SetType(sym.SMACHO);
        sb.SetReachable(true);
        sb.AddUint8(0);
    }
    if (ctxt.BuildMode == BuildModePlugin) {
        foreach (var (_, name) in new slice<@string>(new @string[] { "_cgo_topofstack", "__cgo_topofstack", "_cgo_panic", "crosscall2" })) { 
            // Most of these are data symbols or C
            // symbols, so they have symbol version 0.
            nint ver = 0; 
            // _cgo_panic is a Go function, so it uses ABIInternal.
            if (name == "_cgo_panic") {
                ver = sym.ABIToVersion(obj.ABIInternal);
            }

            s = ctxt.loader.Lookup(name, ver);
            if (s != 0) {
                ctxt.loader.SetAttrCgoExportDynamic(s, false);
            }

        }
    }
}

private static void machoadddynlib(@string lib, LinkMode linkmode) {
    if (seenlib[lib] || linkmode == LinkExternal) {
        return ;
    }
    seenlib[lib] = true; 

    // Will need to store the library name rounded up
    // and 24 bytes of header metadata. If not enough
    // space, grab another page of initial space at the
    // beginning of the output file.
    loadBudget -= (len(lib) + 7) / 8 * 8 + 24;

    if (loadBudget < 0) {
        HEADR += 4096;
        FlagTextAddr.val += 4096;
        loadBudget += 4096;
    }
    dylib = append(dylib, lib);

}

private static void machoshbits(ptr<Link> _addr_ctxt, ptr<MachoSeg> _addr_mseg, ptr<sym.Section> _addr_sect, @string segname) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref MachoSeg mseg = ref _addr_mseg.val;
    ref sym.Section sect = ref _addr_sect.val;

    @string buf = "__" + strings.Replace(sect.Name[(int)1..], ".", "_", -1);

    var msect = newMachoSect(_addr_mseg, buf, segname);

    if (sect.Rellen > 0) {
        msect.reloc = uint32(sect.Reloff);
        msect.nreloc = uint32(sect.Rellen / 8);
    }
    while (1 << (int)(msect.align) < sect.Align) {
        msect.align++;
    }
    msect.addr = sect.Vaddr;
    msect.size = sect.Length;

    if (sect.Vaddr < sect.Seg.Vaddr + sect.Seg.Filelen) { 
        // data in file
        if (sect.Length > sect.Seg.Vaddr + sect.Seg.Filelen - sect.Vaddr) {
            Errorf(null, "macho cannot represent section %s crossing data and bss", sect.Name);
        }
        msect.off = uint32(sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr);

    }
    else
 {
        msect.off = 0;
        msect.flag |= S_ZEROFILL;
    }
    if (sect.Rwx & 1 != 0) {
        msect.flag |= S_ATTR_SOME_INSTRUCTIONS;
    }
    if (sect.Name == ".text") {
        msect.flag |= S_ATTR_PURE_INSTRUCTIONS;
    }
    if (sect.Name == ".plt") {
        msect.name = "__symbol_stub1";
        msect.flag = S_ATTR_PURE_INSTRUCTIONS | S_ATTR_SOME_INSTRUCTIONS | S_SYMBOL_STUBS;
        msect.res1 = 0; //nkind[SymKindLocal];
        msect.res2 = 6;

    }
    if (sect.Name == ".got") {
        msect.name = "__nl_symbol_ptr";
        msect.flag = S_NON_LAZY_SYMBOL_POINTERS;
        msect.res1 = uint32(ctxt.loader.SymSize(ctxt.ArchSyms.LinkEditPLT) / 4); /* offset into indirect symbol table */
    }
    if (sect.Name == ".init_array") {
        msect.name = "__mod_init_func";
        msect.flag = S_MOD_INIT_FUNC_POINTERS;
    }
    if (sect.Name == ".llvmasm") {
        msect.name = "__asm";
        msect.segname = "__LLVM";
    }
    if (segname == "__DWARF") {
        msect.flag |= S_ATTR_DEBUG;
    }
}

private static void asmbMacho(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var machlink = doMachoLink(_addr_ctxt);
    if (!FlagS && ctxt.IsExternal().val) {
        var symo = int64(Segdwarf.Fileoff + uint64(Rnd(int64(Segdwarf.Filelen), int64(FlagRound.val))) + uint64(machlink));
        ctxt.Out.SeekSet(symo);
        machoEmitReloc(_addr_ctxt);
    }
    ctxt.Out.SeekSet(0);

    var ldr = ctxt.loader; 

    /* apple MACH */
    var va = FlagTextAddr - int64(HEADR).val;

    var mh = getMachoHdr();

    if (ctxt.Arch.Family == sys.AMD64) 
        mh.cpu = MACHO_CPU_AMD64;
        mh.subcpu = MACHO_SUBCPU_X86;
    else if (ctxt.Arch.Family == sys.ARM64) 
        mh.cpu = MACHO_CPU_ARM64;
        mh.subcpu = MACHO_SUBCPU_ARM64_ALL;
    else 
        Exitf("unknown macho architecture: %v", ctxt.Arch.Family);
        ptr<MachoSeg> ms;
    if (ctxt.LinkMode == LinkExternal) { 
        /* segment for entire file */
        ms = newMachoSeg("", 40);

        ms.fileoffset = Segtext.Fileoff;
        ms.filesize = Segdwarf.Fileoff + Segdwarf.Filelen - Segtext.Fileoff;
        ms.vsize = Segdwarf.Vaddr + Segdwarf.Length - Segtext.Vaddr;

    }
    if (ctxt.LinkMode != LinkExternal) {
        ms = newMachoSeg("__PAGEZERO", 0);
        ms.vsize = uint64(va);
    }
    var v = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.val));

    if (ctxt.LinkMode != LinkExternal) {
        ms = newMachoSeg("__TEXT", 20);
        ms.vaddr = uint64(va);
        ms.vsize = uint64(v);
        ms.fileoffset = 0;
        ms.filesize = uint64(v);
        ms.prot1 = 7;
        ms.prot2 = 5;
    }
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections) {
            sect = __sect;
            machoshbits(_addr_ctxt, ms, _addr_sect, "__TEXT");
        }
        sect = sect__prev1;
    }

    if (ctxt.LinkMode != LinkExternal && Segrelrodata.Length > 0) {
        ms = newMachoSeg("__DATA_CONST", 20);
        ms.vaddr = Segrelrodata.Vaddr;
        ms.vsize = Segrelrodata.Length;
        ms.fileoffset = Segrelrodata.Fileoff;
        ms.filesize = Segrelrodata.Filelen;
        ms.prot1 = 3;
        ms.prot2 = 3;
        ms.flag = 0x10; // SG_READ_ONLY
    }
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            machoshbits(_addr_ctxt, ms, _addr_sect, "__DATA_CONST");
        }
        sect = sect__prev1;
    }

    if (ctxt.LinkMode != LinkExternal) {
        ms = newMachoSeg("__DATA", 20);
        ms.vaddr = Segdata.Vaddr;
        ms.vsize = Segdata.Length;
        ms.fileoffset = Segdata.Fileoff;
        ms.filesize = Segdata.Filelen;
        ms.prot1 = 3;
        ms.prot2 = 3;
    }
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            machoshbits(_addr_ctxt, ms, _addr_sect, "__DATA");
        }
        sect = sect__prev1;
    }

    if (!FlagW.val) {
        if (ctxt.LinkMode != LinkExternal) {
            ms = newMachoSeg("__DWARF", 20);
            ms.vaddr = Segdwarf.Vaddr;
            ms.vsize = 0;
            ms.fileoffset = Segdwarf.Fileoff;
            ms.filesize = Segdwarf.Filelen;
        }
        {
            var sect__prev1 = sect;

            foreach (var (_, __sect) in Segdwarf.Sections) {
                sect = __sect;
                machoshbits(_addr_ctxt, ms, _addr_sect, "__DWARF");
            }

            sect = sect__prev1;
        }
    }
    if (ctxt.LinkMode != LinkExternal) {

        if (ctxt.Arch.Family == sys.AMD64) 
            var ml = newMachoLoad(_addr_ctxt.Arch, LC_UNIXTHREAD, 42 + 2);
            ml.data[0] = 4; /* thread type */
            ml.data[1] = 42; /* word count */
            ml.data[2 + 32] = uint32(Entryvalue(ctxt)); /* start pc */
            ml.data[2 + 32 + 1] = uint32(Entryvalue(ctxt) >> 32);
        else if (ctxt.Arch.Family == sys.ARM64) 
            ml = newMachoLoad(_addr_ctxt.Arch, LC_MAIN, 4);
            ml.data[0] = uint32(uint64(Entryvalue(ctxt)) - (Segtext.Vaddr - uint64(HEADR)));
            ml.data[1] = uint32((uint64(Entryvalue(ctxt)) - (Segtext.Vaddr - uint64(HEADR))) >> 32);
        else 
            Exitf("unknown macho architecture: %v", ctxt.Arch.Family);
        
    }
    long codesigOff = default;
    if (!FlagD.val) { 
        // must match doMachoLink below
        var s1 = ldr.SymSize(ldr.Lookup(".machorebase", 0));
        var s2 = ldr.SymSize(ldr.Lookup(".machobind", 0));
        var s3 = ldr.SymSize(ldr.Lookup(".machosymtab", 0));
        var s4 = ldr.SymSize(ctxt.ArchSyms.LinkEditPLT);
        var s5 = ldr.SymSize(ctxt.ArchSyms.LinkEditGOT);
        var s6 = ldr.SymSize(ldr.Lookup(".machosymstr", 0));
        var s7 = ldr.SymSize(ldr.Lookup(".machocodesig", 0));

        if (ctxt.LinkMode != LinkExternal) {
            ms = newMachoSeg("__LINKEDIT", 0);
            ms.vaddr = uint64(Rnd(int64(Segdata.Vaddr + Segdata.Length), int64(FlagRound.val)));
            ms.vsize = uint64(s1 + s2 + s3 + s4 + s5 + s6 + s7);
            ms.fileoffset = uint64(linkoff);
            ms.filesize = ms.vsize;
            ms.prot1 = 1;
            ms.prot2 = 1;

            codesigOff = linkoff + s1 + s2 + s3 + s4 + s5 + s6;
        }
        if (ctxt.LinkMode != LinkExternal && ctxt.IsPIE()) {
            ml = newMachoLoad(_addr_ctxt.Arch, LC_DYLD_INFO_ONLY, 10);
            ml.data[0] = uint32(linkoff); // rebase off
            ml.data[1] = uint32(s1); // rebase size
            ml.data[2] = uint32(linkoff + s1); // bind off
            ml.data[3] = uint32(s2); // bind size
            ml.data[4] = 0; // weak bind off
            ml.data[5] = 0; // weak bind size
            ml.data[6] = 0; // lazy bind off
            ml.data[7] = 0; // lazy bind size
            ml.data[8] = 0; // export
            ml.data[9] = 0; // export size
        }
        ml = newMachoLoad(_addr_ctxt.Arch, LC_SYMTAB, 4);
        ml.data[0] = uint32(linkoff + s1 + s2); /* symoff */
        ml.data[1] = uint32(nsortsym); /* nsyms */
        ml.data[2] = uint32(linkoff + s1 + s2 + s3 + s4 + s5); /* stroff */
        ml.data[3] = uint32(s6);        /* strsize */

        machodysymtab(_addr_ctxt, linkoff + s1 + s2);

        if (ctxt.LinkMode != LinkExternal) {
            ml = newMachoLoad(_addr_ctxt.Arch, LC_LOAD_DYLINKER, 6);
            ml.data[0] = 12; /* offset to string */
            stringtouint32(ml.data[(int)1..], "/usr/lib/dyld");

            foreach (var (_, lib) in dylib) {
                ml = newMachoLoad(_addr_ctxt.Arch, LC_LOAD_DYLIB, 4 + (uint32(len(lib)) + 1 + 7) / 8 * 2);
                ml.data[0] = 24; /* offset of string from beginning of load */
                ml.data[1] = 0; /* time stamp */
                ml.data[2] = 0; /* version */
                ml.data[3] = 0; /* compatibility version */
                stringtouint32(ml.data[(int)4..], lib);

            }

        }
        if (ctxt.IsInternal() && ctxt.NeedCodeSign()) {
            ml = newMachoLoad(_addr_ctxt.Arch, LC_CODE_SIGNATURE, 2);
            ml.data[0] = uint32(codesigOff);
            ml.data[1] = uint32(s7);
        }
    }
    var a = machowrite(_addr_ctxt, _addr_ctxt.Arch, _addr_ctxt.Out, ctxt.LinkMode);
    if (int32(a) > HEADR) {
        Exitf("HEADR too small: %d > %d", a, HEADR);
    }
    if (ctxt.IsInternal() && ctxt.NeedCodeSign()) {
        var cs = ldr.Lookup(".machocodesig", 0);
        var data = ctxt.Out.Data();
        if (int64(len(data)) != codesigOff) {
            panic("wrong size");
        }
        codesign.Sign(ldr.Data(cs), bytes.NewReader(data), "a.out", codesigOff, int64(Segtext.Fileoff), int64(Segtext.Filelen), ctxt.IsExe() || ctxt.IsPIE());
        ctxt.Out.SeekSet(codesigOff);
        ctxt.Out.Write(ldr.Data(cs));

    }
});

private static nint symkind(ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (ldr.SymType(s) == sym.SDYNIMPORT) {
        return SymKindUndef;
    }
    if (ldr.AttrCgoExport(s)) {
        return SymKindExtdef;
    }
    return SymKindLocal;

}

private static void collectmachosyms(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;

    Action<loader.Sym> addsym = s => {
        sortsym = append(sortsym, s);
        nkind[symkind(_addr_ldr, s)]++;
    }; 

    // Add special runtime.text and runtime.etext symbols.
    // We've already included this symbol in Textp on darwin if ctxt.DynlinkingGo().
    // See data.go:/textaddress
    if (!ctxt.DynlinkingGo()) {
        var s = ldr.Lookup("runtime.text", 0);
        if (ldr.SymType(s) == sym.STEXT) {
            addsym(s);
        }
        s = ldr.Lookup("runtime.etext", 0);
        if (ldr.SymType(s) == sym.STEXT) {
            addsym(s);
        }
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.Textp) {
            s = __s;
            addsym(s);
        }
        s = s__prev1;
    }

    Func<loader.Sym, bool> shouldBeInSymbolTable = s => {
        if (ldr.AttrNotInSymbolTable(s)) {
            return false;
        }
        var name = ldr.RawSymName(s); // TODO: try not to read the name
        if (name == "" || name[0] == '.') {
            return false;
        }
        return true;

    }; 

    // Add data symbols and external references.
    {
        var s__prev1 = s;

        for (s = loader.Sym(1); s < loader.Sym(ldr.NSym()); s++) {
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            var t = ldr.SymType(s);
            if (t >= sym.SELFRXSECT && t < sym.SXREF) { // data sections handled in dodata
                if (t == sym.STLSBSS) { 
                    // TLSBSS is not used on darwin. See data.go:allocateDataSections
                    continue;

                }

                if (!shouldBeInSymbolTable(s)) {
                    continue;
                }

                addsym(s);

            }


            if (t == sym.SDYNIMPORT || t == sym.SHOSTOBJ || t == sym.SUNDEFEXT) 
                addsym(s);
            // Some 64-bit functions have a "$INODE64" or "$INODE64$UNIX2003" suffix.
            if (t == sym.SDYNIMPORT && ldr.SymDynimplib(s) == "/usr/lib/libSystem.B.dylib") { 
                // But only on macOS.
                if (machoPlatform == PLATFORM_MACOS) {
                    {
                        var n = ldr.SymExtname(s);

                        switch (n) {
                            case "fdopendir": 
                                switch (buildcfg.GOARCH) {
                                    case "amd64": 
                                        ldr.SetSymExtname(s, n + "$INODE64");
                                        break;
                                }

                                break;
                            case "readdir_r": 

                            case "getfsstat": 
                                switch (buildcfg.GOARCH) {
                                    case "amd64": 
                                        ldr.SetSymExtname(s, n + "$INODE64");
                                        break;
                                }

                                break;
                        }
                    }

                }

            }

        }

        s = s__prev1;
    }

    nsortsym = len(sortsym);

}

private static void machosymorder(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader; 

    // On Mac OS X Mountain Lion, we must sort exported symbols
    // So we sort them here and pre-allocate dynid for them
    // See https://golang.org/issue/4029
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.dynexp) {
            s = __s;
            if (!ldr.AttrReachable(s)) {
                panic("dynexp symbol is not reachable");
            }
        }
        s = s__prev1;
    }

    collectmachosyms(_addr_ctxt);
    sort.Slice(sortsym[..(int)nsortsym], (i, j) => {
        var s1 = sortsym[i];
        var s2 = sortsym[j];
        var k1 = symkind(_addr_ldr, s1);
        var k2 = symkind(_addr_ldr, s2);
        if (k1 != k2) {
            return k1 < k2;
        }
        return ldr.SymExtname(s1) < ldr.SymExtname(s2); // Note: unnamed symbols are not added in collectmachosyms
    });
    {
        var s__prev1 = s;

        foreach (var (__i, __s) in sortsym) {
            i = __i;
            s = __s;
            ldr.SetSymDynid(s, int32(i));
        }
        s = s__prev1;
    }
});

// AddMachoSym adds s to Mach-O symbol table, used in GenSymLate.
// Currently only used on ARM64 when external linking.
public static void AddMachoSym(ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;

    ldr.SetSymDynid(s, int32(nsortsym));
    sortsym = append(sortsym, s);
    nsortsym++;
    nkind[symkind(_addr_ldr, s)]++;
}

// machoShouldExport reports whether a symbol needs to be exported.
//
// When dynamically linking, all non-local variables and plugin-exported
// symbols need to be exported.
private static bool machoShouldExport(ptr<Link> _addr_ctxt, ptr<loader.Loader> _addr_ldr, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.Loader ldr = ref _addr_ldr.val;

    if (!ctxt.DynlinkingGo() || ldr.AttrLocal(s)) {
        return false;
    }
    if (ctxt.BuildMode == BuildModePlugin && strings.HasPrefix(ldr.SymExtname(s), objabi.PathToPrefix(flagPluginPath.val))) {
        return true;
    }
    var name = ldr.RawSymName(s);
    if (strings.HasPrefix(name, "go.itab.")) {
        return true;
    }
    if (strings.HasPrefix(name, "type.") && !strings.HasPrefix(name, "type..")) { 
        // reduce runtime typemap pressure, but do not
        // export alg functions (type..*), as these
        // appear in pclntable.
        return true;

    }
    if (strings.HasPrefix(name, "go.link.pkghash")) {
        return true;
    }
    return ldr.SymType(s) >= sym.SFirstWritable; // only writable sections
}

private static void machosymtab(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var symtab = ldr.CreateSymForUpdate(".machosymtab", 0);
    var symstr = ldr.CreateSymForUpdate(".machosymstr", 0);

    foreach (var (_, s) in sortsym[..(int)nsortsym]) {
        symtab.AddUint32(ctxt.Arch, uint32(symstr.Size()));

        var export = machoShouldExport(_addr_ctxt, _addr_ldr, s); 

        // Prefix symbol names with "_" to match the system toolchain.
        // (We used to only prefix C symbols, which is all required for the build.
        // But some tools don't recognize Go symbols as symbols, so we prefix them
        // as well.)
        symstr.AddUint8('_'); 

        // replace "·" as ".", because DTrace cannot handle it.
        var name = strings.Replace(ldr.SymExtname(s), "·", ".", -1);

        name = mangleABIName(ctxt, ldr, s, name);
        symstr.Addstring(name);

        {
            var t = ldr.SymType(s);

            if (t == sym.SDYNIMPORT || t == sym.SHOSTOBJ || t == sym.SUNDEFEXT) {
                symtab.AddUint8(0x01); // type N_EXT, external symbol
                symtab.AddUint8(0); // no section
                symtab.AddUint16(ctxt.Arch, 0); // desc
                symtab.AddUintXX(ctxt.Arch, 0, ctxt.Arch.PtrSize); // no value
            }
            else
 {
                if (export || ldr.AttrCgoExportDynamic(s)) {
                    symtab.AddUint8(0x0f); // N_SECT | N_EXT
                }
                else if (ldr.AttrCgoExportStatic(s)) { 
                    // Only export statically, not dynamically. (N_PEXT is like hidden visibility)
                    symtab.AddUint8(0x1f); // N_SECT | N_EXT | N_PEXT
                }
                else
 {
                    symtab.AddUint8(0x0e); // N_SECT
                }

                var o = s;
                {
                    var outer = ldr.OuterSym(o);

                    if (outer != 0) {
                        o = outer;
                    }

                }

                if (ldr.SymSect(o) == null) {
                    ldr.Errorf(s, "missing section for symbol");
                    symtab.AddUint8(0);
                }
                else
 {
                    symtab.AddUint8(uint8(ldr.SymSect(o).Extnum));
                }

                symtab.AddUint16(ctxt.Arch, 0); // desc
                symtab.AddUintXX(ctxt.Arch, uint64(ldr.SymAddr(s)), ctxt.Arch.PtrSize);

            }

        }

    }
}

private static void machodysymtab(ptr<Link> _addr_ctxt, long @base) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ml = newMachoLoad(_addr_ctxt.Arch, LC_DYSYMTAB, 18);

    nint n = 0;
    ml.data[0] = uint32(n); /* ilocalsym */
    ml.data[1] = uint32(nkind[SymKindLocal]); /* nlocalsym */
    n += nkind[SymKindLocal];

    ml.data[2] = uint32(n); /* iextdefsym */
    ml.data[3] = uint32(nkind[SymKindExtdef]); /* nextdefsym */
    n += nkind[SymKindExtdef];

    ml.data[4] = uint32(n); /* iundefsym */
    ml.data[5] = uint32(nkind[SymKindUndef]);    /* nundefsym */

    ml.data[6] = 0; /* tocoffset */
    ml.data[7] = 0; /* ntoc */
    ml.data[8] = 0; /* modtaboff */
    ml.data[9] = 0; /* nmodtab */
    ml.data[10] = 0; /* extrefsymoff */
    ml.data[11] = 0;    /* nextrefsyms */

    var ldr = ctxt.loader; 

    // must match domacholink below
    var s1 = ldr.SymSize(ldr.Lookup(".machosymtab", 0));
    var s2 = ldr.SymSize(ctxt.ArchSyms.LinkEditPLT);
    var s3 = ldr.SymSize(ctxt.ArchSyms.LinkEditGOT);
    ml.data[12] = uint32(base + s1); /* indirectsymoff */
    ml.data[13] = uint32((s2 + s3) / 4);    /* nindirectsyms */

    ml.data[14] = 0; /* extreloff */
    ml.data[15] = 0; /* nextrel */
    ml.data[16] = 0; /* locreloff */
    ml.data[17] = 0; /* nlocrel */
}

private static long doMachoLink(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    machosymtab(_addr_ctxt);
    machoDyldInfo(_addr_ctxt);

    var ldr = ctxt.loader; 

    // write data that will be linkedit section
    var s1 = ldr.Lookup(".machorebase", 0);
    var s2 = ldr.Lookup(".machobind", 0);
    var s3 = ldr.Lookup(".machosymtab", 0);
    var s4 = ctxt.ArchSyms.LinkEditPLT;
    var s5 = ctxt.ArchSyms.LinkEditGOT;
    var s6 = ldr.Lookup(".machosymstr", 0);

    var size = ldr.SymSize(s1) + ldr.SymSize(s2) + ldr.SymSize(s3) + ldr.SymSize(s4) + ldr.SymSize(s5) + ldr.SymSize(s6); 

    // Force the linkedit section to end on a 16-byte
    // boundary. This allows pure (non-cgo) Go binaries
    // to be code signed correctly.
    //
    // Apple's codesign_allocate (a helper utility for
    // the codesign utility) can do this fine itself if
    // it is run on a dynamic Mach-O binary. However,
    // when it is run on a pure (non-cgo) Go binary, where
    // the linkedit section is mostly empty, it fails to
    // account for the extra padding that it itself adds
    // when adding the LC_CODE_SIGNATURE load command
    // (which must be aligned on a 16-byte boundary).
    //
    // By forcing the linkedit section to end on a 16-byte
    // boundary, codesign_allocate will not need to apply
    // any alignment padding itself, working around the
    // issue.
    if (size % 16 != 0) {
        nint n = 16 - size % 16;
        var s6b = ldr.MakeSymbolUpdater(s6);
        s6b.Grow(s6b.Size() + n);
        s6b.SetSize(s6b.Size() + n);
        size += n;
    }
    if (size > 0) {
        linkoff = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.val)) + Rnd(int64(Segrelrodata.Filelen), int64(FlagRound.val)) + Rnd(int64(Segdata.Filelen), int64(FlagRound.val)) + Rnd(int64(Segdwarf.Filelen), int64(FlagRound.val));
        ctxt.Out.SeekSet(linkoff);

        ctxt.Out.Write(ldr.Data(s1));
        ctxt.Out.Write(ldr.Data(s2));
        ctxt.Out.Write(ldr.Data(s3));
        ctxt.Out.Write(ldr.Data(s4));
        ctxt.Out.Write(ldr.Data(s5));
        ctxt.Out.Write(ldr.Data(s6)); 

        // Add code signature if necessary. This must be the last.
        var s7 = machoCodeSigSym(_addr_ctxt, linkoff + size);
        size += ldr.SymSize(s7);

    }
    return Rnd(size, int64(FlagRound.val));

}

private static void machorelocsect(ptr<Link> _addr_ctxt, ptr<OutBuf> _addr_@out, ptr<sym.Section> _addr_sect, slice<loader.Sym> syms) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
    ref OutBuf @out = ref _addr_@out.val;
    ref sym.Section sect = ref _addr_sect.val;
 
    // If main section has no bits, nothing to relocate.
    if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen) {
        return ;
    }
    var ldr = ctxt.loader;

    {
        var s__prev1 = s;

        foreach (var (__i, __s) in syms) {
            i = __i;
            s = __s;
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            if (uint64(ldr.SymValue(s)) >= sect.Vaddr) {
                syms = syms[(int)i..];
                break;
            }
        }
        s = s__prev1;
    }

    var eaddr = sect.Vaddr + sect.Length;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in syms) {
            s = __s;
            if (!ldr.AttrReachable(s)) {
                continue;
            }
            if (ldr.SymValue(s) >= int64(eaddr)) {
                break;
            } 

            // Compute external relocations on the go, and pass to Machoreloc1
            // to stream out.
            var relocs = ldr.Relocs(s);
            for (nint ri = 0; ri < relocs.Count(); ri++) {
                var r = relocs.At(ri);
                var (rr, ok) = extreloc(ctxt, ldr, s, r);
                if (!ok) {
                    continue;
                }
                if (rr.Xsym == 0) {
                    ldr.Errorf(s, "missing xsym in relocation");
                    continue;
                }
                if (!ldr.AttrReachable(rr.Xsym)) {
                    ldr.Errorf(s, "unreachable reloc %d (%s) target %v", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), ldr.SymName(rr.Xsym));
                }
                if (!thearch.Machoreloc1(ctxt.Arch, out, ldr, s, rr, int64(uint64(ldr.SymValue(s) + int64(r.Off())) - sect.Vaddr))) {
                    ldr.Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type(), sym.RelocName(ctxt.Arch, r.Type()), r.Siz(), ldr.SymName(r.Sym()));
                }
            }


        }
        s = s__prev1;
    }

    if (uint64(@out.Offset()) != sect.Reloff + sect.Rellen) {
        panic("machorelocsect: size mismatch");
    }
});

private static void machoEmitReloc(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    while (ctxt.Out.Offset() & 7 != 0) {
        ctxt.Out.Write8(0);
    }

    sizeExtRelocs(ctxt, thearch.MachorelocSize);
    var (relocSect, wg) = relocSectFn(ctxt, machorelocsect);

    relocSect(ctxt, Segtext.Sections[0], ctxt.Textp);
    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segtext.Sections[(int)1..]) {
            sect = __sect;
            if (sect.Name == ".text") {
                relocSect(ctxt, sect, ctxt.Textp);
            }
            else
 {
                relocSect(ctxt, sect, ctxt.datap);
            }

        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segrelrodata.Sections) {
            sect = __sect;
            relocSect(ctxt, sect, ctxt.datap);
        }
        sect = sect__prev1;
    }

    {
        var sect__prev1 = sect;

        foreach (var (_, __sect) in Segdata.Sections) {
            sect = __sect;
            relocSect(ctxt, sect, ctxt.datap);
        }
        sect = sect__prev1;
    }

    for (nint i = 0; i < len(Segdwarf.Sections); i++) {
        var sect = Segdwarf.Sections[i];
        var si = dwarfp[i];
        if (si.secSym() != loader.Sym(sect.Sym) || ctxt.loader.SymSect(si.secSym()) != sect) {
            panic("inconsistency between dwarfp and Segdwarf");
        }
        relocSect(ctxt, sect, si.syms);

    }
    wg.Wait();

});

// hostobjMachoPlatform returns the first platform load command found
// in the host object, if any.
private static (ptr<MachoPlatformLoad>, error) hostobjMachoPlatform(ptr<Hostobj> _addr_h) => func((defer, _, _) => {
    ptr<MachoPlatformLoad> _p0 = default!;
    error _p0 = default!;
    ref Hostobj h = ref _addr_h.val;

    var (f, err) = os.Open(h.file);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("%s: failed to open host object: %v\n", h.file, err))!);
    }
    defer(f.Close());
    var sr = io.NewSectionReader(f, h.off, h.length);
    var (m, err) = macho.NewFile(sr);
    if (err != null) { 
        // Not a valid Mach-O file.
        return (_addr_null!, error.As(null!)!);

    }
    return _addr_peekMachoPlatform(_addr_m)!;

});

// peekMachoPlatform returns the first LC_VERSION_MIN_* or LC_BUILD_VERSION
// load command found in the Mach-O file, if any.
private static (ptr<MachoPlatformLoad>, error) peekMachoPlatform(ptr<macho.File> _addr_m) {
    ptr<MachoPlatformLoad> _p0 = default!;
    error _p0 = default!;
    ref macho.File m = ref _addr_m.val;

    foreach (var (_, cmd) in m.Loads) {
        var raw = cmd.Raw();
        MachoLoad ml = new MachoLoad(type_:m.ByteOrder.Uint32(raw),); 
        // Skip the type and command length.
        var data = raw[(int)8..];
        MachoPlatform p = default;

        if (ml.type_ == LC_VERSION_MIN_IPHONEOS) 
            p = PLATFORM_IOS;
        else if (ml.type_ == LC_VERSION_MIN_MACOSX) 
            p = PLATFORM_MACOS;
        else if (ml.type_ == LC_VERSION_MIN_WATCHOS) 
            p = PLATFORM_WATCHOS;
        else if (ml.type_ == LC_VERSION_MIN_TVOS) 
            p = PLATFORM_TVOS;
        else if (ml.type_ == LC_BUILD_VERSION) 
            p = MachoPlatform(m.ByteOrder.Uint32(data));
        else 
            continue;
                ml.data = make_slice<uint>(len(data) / 4);
        var r = bytes.NewReader(data);
        {
            var err = binary.Read(r, m.ByteOrder, _addr_ml.data);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

        }

        return (addr(new MachoPlatformLoad(platform:p,cmd:ml,)), error.As(null!)!);

    }    return (_addr_null!, error.As(null!)!);

}

// A rebase entry tells the dynamic linker the data at sym+off needs to be
// relocated when the in-memory image moves. (This is somewhat like, say,
// ELF R_X86_64_RELATIVE).
// For now, the only kind of entry we support is that the data is an absolute
// address. That seems all we need.
// In the binary it uses a compact stateful bytecode encoding. So we record
// entries as we go and build the table at the end.
private partial struct machoRebaseRecord {
    public loader.Sym sym;
    public long off;
}

private static slice<machoRebaseRecord> machorebase = default;

public static void MachoAddRebase(loader.Sym s, long off) {
    machorebase = append(machorebase, new machoRebaseRecord(s,off));
}

// A bind entry tells the dynamic linker the data at GOT+off should be bound
// to the address of the target symbol, which is a dynamic import.
// For now, the only kind of entry we support is that the data is an absolute
// address, and the source symbol is always the GOT. That seems all we need.
// In the binary it uses a compact stateful bytecode encoding. So we record
// entries as we go and build the table at the end.
private partial struct machoBindRecord {
    public long off;
    public loader.Sym targ;
}

private static slice<machoBindRecord> machobind = default;

public static void MachoAddBind(long off, loader.Sym targ) {
    machobind = append(machobind, new machoBindRecord(off,targ));
}

// Generate data for the dynamic linker, used in LC_DYLD_INFO_ONLY load command.
// See mach-o/loader.h, struct dyld_info_command, for the encoding.
// e.g. https://opensource.apple.com/source/xnu/xnu-6153.81.5/EXTERNAL_HEADERS/mach-o/loader.h
private static void machoDyldInfo(ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var rebase = ldr.CreateSymForUpdate(".machorebase", 0);
    var bind = ldr.CreateSymForUpdate(".machobind", 0);

    if (!(ctxt.IsPIE() && ctxt.IsInternal())) {
        return ;
    }
    Func<ptr<sym.Segment>, byte> segId = seg => {

        if (seg == _addr_Segtext) 
            return 1;
        else if (seg == _addr_Segrelrodata) 
            return 2;
        else if (seg == _addr_Segdata) 
            if (Segrelrodata.Length > 0) {
                return 3;
            }
            return 2;
                panic("unknown segment");

    };

    Func<loader.Sym, nint> dylibId = s => {
        var slib = ldr.SymDynimplib(s);
        foreach (var (i, lib) in dylib) {
            if (lib == slib) {
                return i + 1;
            }
        }        return BIND_SPECIAL_DYLIB_FLAT_LOOKUP; // don't know where it is from
    }; 

    // Rebase table.
    // TODO: use more compact encoding. The encoding is stateful, and
    // we can use delta encoding.
    rebase.AddUint8(REBASE_OPCODE_SET_TYPE_IMM | REBASE_TYPE_POINTER);
    {
        var r__prev1 = r;

        foreach (var (_, __r) in machorebase) {
            r = __r;
            var seg = ldr.SymSect(r.sym).Seg;
            var off = uint64(ldr.SymValue(r.sym) + r.off) - seg.Vaddr;
            rebase.AddUint8(REBASE_OPCODE_SET_SEGMENT_AND_OFFSET_ULEB | segId(seg));
            rebase.AddUleb(off);

            rebase.AddUint8(REBASE_OPCODE_DO_REBASE_IMM_TIMES | 1);
        }
        r = r__prev1;
    }

    rebase.AddUint8(REBASE_OPCODE_DONE);
    var sz = Rnd(rebase.Size(), 8);
    rebase.Grow(sz);
    rebase.SetSize(sz); 

    // Bind table.
    // TODO: compact encoding, as above.
    // TODO: lazy binding?
    var got = ctxt.GOT;
    seg = ldr.SymSect(got).Seg;
    var gotAddr = ldr.SymValue(got);
    bind.AddUint8(BIND_OPCODE_SET_TYPE_IMM | BIND_TYPE_POINTER);
    {
        var r__prev1 = r;

        foreach (var (_, __r) in machobind) {
            r = __r;
            off = uint64(gotAddr + r.off) - seg.Vaddr;
            bind.AddUint8(BIND_OPCODE_SET_SEGMENT_AND_OFFSET_ULEB | segId(seg));
            bind.AddUleb(off);

            var d = dylibId(r.targ);
            if (d > 0 && d < 128) {
                bind.AddUint8(BIND_OPCODE_SET_DYLIB_ORDINAL_IMM | uint8(d) & 0xf);
            }
            else if (d >= 128) {
                bind.AddUint8(BIND_OPCODE_SET_DYLIB_ORDINAL_ULEB);
                bind.AddUleb(uint64(d));
            }
            else
 { // d <= 0
                bind.AddUint8(BIND_OPCODE_SET_DYLIB_SPECIAL_IMM | uint8(d) & 0xf);

            }

            bind.AddUint8(BIND_OPCODE_SET_SYMBOL_TRAILING_FLAGS_IMM); 
            // target symbol name as a C string, with _ prefix
            bind.AddUint8('_');
            bind.Addstring(ldr.SymExtname(r.targ));

            bind.AddUint8(BIND_OPCODE_DO_BIND);

        }
        r = r__prev1;
    }

    bind.AddUint8(BIND_OPCODE_DONE);
    sz = Rnd(bind.Size(), 16); // make it 16-byte aligned, see the comment in doMachoLink
    bind.Grow(sz);
    bind.SetSize(sz); 

    // TODO: export table.
    // The symbols names are encoded as a trie. I'm really too lazy to do that
    // for now.
    // Without it, the symbols are not dynamically exported, so they cannot be
    // e.g. dlsym'd. But internal linking is not the default in that case, so
    // it is fine.
});

// machoCodeSigSym creates and returns a symbol for code signature.
// The symbol context is left as zeros, which will be generated at the end
// (as it depends on the rest of the file).
private static loader.Sym machoCodeSigSym(ptr<Link> _addr_ctxt, long codeSize) {
    ref Link ctxt = ref _addr_ctxt.val;

    var ldr = ctxt.loader;
    var cs = ldr.CreateSymForUpdate(".machocodesig", 0);
    if (!ctxt.NeedCodeSign() || ctxt.IsExternal()) {
        return cs.Sym();
    }
    var sz = codesign.Size(codeSize, "a.out");
    cs.Grow(sz);
    cs.SetSize(sz);
    return cs.Sym();

}

// machoCodeSign code-signs Mach-O file fname with an ad-hoc signature.
// This is used for updating an external linker generated binary.
private static error machoCodeSign(ptr<Link> _addr_ctxt, @string fname) => func((defer, _, _) => {
    ref Link ctxt = ref _addr_ctxt.val;

    var (f, err) = os.OpenFile(fname, os.O_RDWR, 0);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());

    var (mf, err) = macho.NewFile(f);
    if (err != null) {
        return error.As(err)!;
    }
    if (mf.Magic != macho.Magic64) {
        Exitf("not 64-bit Mach-O file: %s", fname);
    }
    long sigOff = default;    long sigSz = default;    long csCmdOff = default;    long linkeditOff = default;

    ptr<macho.Segment> linkeditSeg;    ptr<macho.Segment> textSeg;

    var loadOff = int64(machoHeaderSize64);
    var get32 = mf.ByteOrder.Uint32;
    foreach (var (_, l) in mf.Loads) {
        var data = l.Raw();
        var cmd = get32(data);
        var sz = get32(data[(int)4..]);
        if (cmd == LC_CODE_SIGNATURE) {
            sigOff = int64(get32(data[(int)8..]));
            sigSz = int64(get32(data[(int)12..]));
            csCmdOff = loadOff;
        }
        {
            ptr<macho.Segment> (seg, ok) = l._<ptr<macho.Segment>>();

            if (ok) {
                switch (seg.Name) {
                    case "__LINKEDIT": 
                        linkeditSeg = seg;
                        linkeditOff = loadOff;
                        break;
                    case "__TEXT": 
                        textSeg = seg;
                        break;
                }

            }

        }

        loadOff += int64(sz);

    }    if (sigOff == 0) { 
        // The C linker doesn't generate a signed binary, for some reason.
        // Skip.
        return error.As(null!)!;

    }
    var (fi, err) = f.Stat();
    if (err != null) {
        return error.As(err)!;
    }
    if (sigOff + sigSz != fi.Size()) { 
        // We don't expect anything after the signature (this will invalidate
        // the signature anyway.)
        return error.As(fmt.Errorf("unexpected content after code signature"))!;

    }
    sz = codesign.Size(sigOff, "a.out");
    if (sz != sigSz) { 
        // Update the load command,
        array<byte> tmp = new array<byte>(8);
        mf.ByteOrder.PutUint32(tmp[..(int)4], uint32(sz));
        _, err = f.WriteAt(tmp[..(int)4], csCmdOff + 12);
        if (err != null) {
            return error.As(err)!;
        }
        var segSz = sigOff + sz - int64(linkeditSeg.Offset);
        mf.ByteOrder.PutUint64(tmp[..(int)8], uint64(segSz));
        _, err = f.WriteAt(tmp[..(int)8], int64(linkeditOff) + int64(@unsafe.Offsetof(new macho.Segment64().Memsz)));
        if (err != null) {
            return error.As(err)!;
        }
        _, err = f.WriteAt(tmp[..(int)8], int64(linkeditOff) + int64(@unsafe.Offsetof(new macho.Segment64().Filesz)));
        if (err != null) {
            return error.As(err)!;
        }
    }
    var cs = make_slice<byte>(sz);
    codesign.Sign(cs, f, "a.out", sigOff, int64(textSeg.Offset), int64(textSeg.Filesz), ctxt.IsExe() || ctxt.IsPIE());
    _, err = f.WriteAt(cs, sigOff);
    if (err != null) {
        return error.As(err)!;
    }
    err = f.Truncate(sigOff + sz);
    return error.As(err)!;

});

} // end ld_package
