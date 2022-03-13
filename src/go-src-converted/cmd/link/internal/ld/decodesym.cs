// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 13 06:33:54 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\decodesym.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using elf = debug.elf_package;
using binary = encoding.binary_package;
using log = log_package;


// Decoding the type.* symbols.     This has to be in sync with
// ../../runtime/type.go, or more specifically, with what
// cmd/compile/internal/reflectdata/reflect.go stuffs in these.

// tflag is documented in reflect/type.go.
//
// tflag values must be kept in sync with copies in:
//    cmd/compile/internal/reflectdata/reflect.go
//    cmd/link/internal/ld/decodesym.go
//    reflect/type.go
//    runtime/type.go

public static partial class ld_package {

private static readonly nint tflagUncommon = 1 << 0;
private static readonly nint tflagExtraStar = 1 << 1;

private static ulong decodeInuxi(ptr<sys.Arch> _addr_arch, slice<byte> p, nint sz) => func((_, panic, _) => {
    ref sys.Arch arch = ref _addr_arch.val;

    switch (sz) {
        case 2: 
            return uint64(arch.ByteOrder.Uint16(p));
            break;
        case 4: 
            return uint64(arch.ByteOrder.Uint32(p));
            break;
        case 8: 
            return arch.ByteOrder.Uint64(p);
            break;
        default: 
            Exitf("dwarf: decode inuxi %d", sz);
            panic("unreachable");
            break;
    }
});

private static nint commonsize(ptr<sys.Arch> _addr_arch) {
    ref sys.Arch arch = ref _addr_arch.val;

    return 4 * arch.PtrSize + 8 + 8;
} // runtime._type
private static nint structfieldSize(ptr<sys.Arch> _addr_arch) {
    ref sys.Arch arch = ref _addr_arch.val;

    return 3 * arch.PtrSize;
} // runtime.structfield
private static nint uncommonSize() {
    return 4 + 2 + 2 + 4 + 4;
} // runtime.uncommontype

// Type.commonType.kind
private static byte decodetypeKind(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return p[2 * arch.PtrSize + 7] & objabi.KindMask; //  0x13 / 0x1f
}

// Type.commonType.kind
private static byte decodetypeUsegcprog(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return p[2 * arch.PtrSize + 7] & objabi.KindGCProg; //  0x13 / 0x1f
}

// Type.commonType.size
private static long decodetypeSize(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return int64(decodeInuxi(_addr_arch, p, arch.PtrSize)); // 0x8 / 0x10
}

// Type.commonType.ptrdata
private static long decodetypePtrdata(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return int64(decodeInuxi(_addr_arch, p[(int)arch.PtrSize..], arch.PtrSize)); // 0x8 / 0x10
}

// Type.commonType.tflag
private static bool decodetypeHasUncommon(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return p[2 * arch.PtrSize + 4] & tflagUncommon != 0;
}

// Type.FuncType.dotdotdot
private static bool decodetypeFuncDotdotdot(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return uint16(decodeInuxi(_addr_arch, p[(int)commonsize(_addr_arch) + 2..], 2)) & (1 << 15) != 0;
}

// Type.FuncType.inCount
private static nint decodetypeFuncInCount(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return int(decodeInuxi(_addr_arch, p[(int)commonsize(_addr_arch)..], 2));
}

private static nint decodetypeFuncOutCount(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return int(uint16(decodeInuxi(_addr_arch, p[(int)commonsize(_addr_arch) + 2..], 2)) & (1 << 15 - 1));
}

// InterfaceType.methods.length
private static long decodetypeIfaceMethodCount(ptr<sys.Arch> _addr_arch, slice<byte> p) {
    ref sys.Arch arch = ref _addr_arch.val;

    return int64(decodeInuxi(_addr_arch, p[(int)commonsize(_addr_arch) + 2 * arch.PtrSize..], arch.PtrSize));
}

// Matches runtime/typekind.go and reflect.Kind.
private static readonly nint kindArray = 17;
private static readonly nint kindChan = 18;
private static readonly nint kindFunc = 19;
private static readonly nint kindInterface = 20;
private static readonly nint kindMap = 21;
private static readonly nint kindPtr = 22;
private static readonly nint kindSlice = 23;
private static readonly nint kindStruct = 25;
private static readonly nint kindMask = (1 << 5) - 1;

private static loader.Reloc decodeReloc(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, int off) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    for (nint j = 0; j < relocs.Count(); j++) {
        var rel = relocs.At(j);
        if (rel.Off() == off) {
            return rel;
        }
    }
    return new loader.Reloc();
}

private static loader.Sym decodeRelocSym(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, int off) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    return decodeReloc(_addr_ldr, symIdx, _addr_relocs, off).Sym();
}

// decodetypeName decodes the name from a reflect.name.
private static @string decodetypeName(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, nint off) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    var r = decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(off));
    if (r == 0) {
        return "";
    }
    var data = ldr.Data(r);
    var (nameLen, nameLenLen) = binary.Uvarint(data[(int)1..]);
    return string(data[(int)1 + nameLenLen..(int)1 + nameLenLen + int(nameLen)]);
}

private static loader.Sym decodetypeFuncInType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    var uadd = commonsize(_addr_arch) + 4;
    if (arch.PtrSize == 8) {
        uadd += 4;
    }
    if (decodetypeHasUncommon(_addr_arch, ldr.Data(symIdx))) {
        uadd += uncommonSize();
    }
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(uadd + i * arch.PtrSize));
}

private static loader.Sym decodetypeFuncOutType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Relocs relocs = ref _addr_relocs.val;

    return decodetypeFuncInType(_addr_ldr, _addr_arch, symIdx, _addr_relocs, i + decodetypeFuncInCount(_addr_arch, ldr.Data(symIdx)));
}

private static loader.Sym decodetypeArrayElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
}

private static long decodetypeArrayLen(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var data = ldr.Data(symIdx);
    return int64(decodeInuxi(_addr_arch, data[(int)commonsize(_addr_arch) + 2 * arch.PtrSize..], arch.PtrSize));
}

private static loader.Sym decodetypeChanElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
}

private static loader.Sym decodetypeMapKey(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
}

private static loader.Sym decodetypeMapValue(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch)) + int32(arch.PtrSize)); // 0x20 / 0x38
}

private static loader.Sym decodetypePtrElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
}

private static nint decodetypeStructFieldCount(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var data = ldr.Data(symIdx);
    return int(decodeInuxi(_addr_arch, data[(int)commonsize(_addr_arch) + 2 * arch.PtrSize..], arch.PtrSize));
}

private static nint decodetypeStructFieldArrayOff(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var data = ldr.Data(symIdx);
    var off = commonsize(_addr_arch) + 4 * arch.PtrSize;
    if (decodetypeHasUncommon(_addr_arch, data)) {
        off += uncommonSize();
    }
    off += i * structfieldSize(_addr_arch);
    return off;
}

private static @string decodetypeStructFieldName(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodetypeName(_addr_ldr, symIdx, _addr_relocs, off);
}

private static loader.Sym decodetypeStructFieldType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(off + arch.PtrSize));
}

private static long decodetypeStructFieldOffsAnon(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, nint i) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
    var data = ldr.Data(symIdx);
    return int64(decodeInuxi(_addr_arch, data[(int)off + 2 * arch.PtrSize..], arch.PtrSize));
}

// decodetypeStr returns the contents of an rtype's str field (a nameOff).
private static @string decodetypeStr(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref sys.Arch arch = ref _addr_arch.val;

    ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
    var str = decodetypeName(_addr_ldr, symIdx, _addr_relocs, 4 * arch.PtrSize + 8);
    var data = ldr.Data(symIdx);
    if (data[2 * arch.PtrSize + 4] & tflagExtraStar != 0) {
        return str[(int)1..];
    }
    return str;
}

private static slice<byte> decodetypeGcmask(ptr<Link> _addr_ctxt, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.loader.SymType(s) == sym.SDYNIMPORT) {
        var symData = ctxt.loader.Data(s);
        var addr = decodetypeGcprogShlib(_addr_ctxt, symData);
        var ptrdata = decodetypePtrdata(_addr_ctxt.Arch, symData);
        var sect = findShlibSection(_addr_ctxt, ctxt.loader.SymPkg(s), addr);
        if (sect != null) {
            var bits = ptrdata / int64(ctxt.Arch.PtrSize);
            var r = make_slice<byte>((bits + 7) / 8); 
            // ldshlibsyms avoids closing the ELF file so sect.ReadAt works.
            // If we remove this read (and the ones in decodetypeGcprog), we
            // can close the file.
            var (_, err) = sect.ReadAt(r, int64(addr - sect.Addr));
            if (err != null) {
                log.Fatal(err);
            }
            return r;
        }
        Exitf("cannot find gcmask for %s", ctxt.loader.SymName(s));
        return null;
    }
    ref var relocs = ref heap(ctxt.loader.Relocs(s), out ptr<var> _addr_relocs);
    var mask = decodeRelocSym(_addr_ctxt.loader, s, _addr_relocs, 2 * int32(ctxt.Arch.PtrSize) + 8 + 1 * int32(ctxt.Arch.PtrSize));
    return ctxt.loader.Data(mask);
}

// Type.commonType.gc
private static slice<byte> decodetypeGcprog(ptr<Link> _addr_ctxt, loader.Sym s) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.loader.SymType(s) == sym.SDYNIMPORT) {
        var symData = ctxt.loader.Data(s);
        var addr = decodetypeGcprogShlib(_addr_ctxt, symData);
        var sect = findShlibSection(_addr_ctxt, ctxt.loader.SymPkg(s), addr);
        if (sect != null) { 
            // A gcprog is a 4-byte uint32 indicating length, followed by
            // the actual program.
            var progsize = make_slice<byte>(4);
            var (_, err) = sect.ReadAt(progsize, int64(addr - sect.Addr));
            if (err != null) {
                log.Fatal(err);
            }
            var progbytes = make_slice<byte>(ctxt.Arch.ByteOrder.Uint32(progsize));
            _, err = sect.ReadAt(progbytes, int64(addr - sect.Addr + 4));
            if (err != null) {
                log.Fatal(err);
            }
            return append(progsize, progbytes);
        }
        Exitf("cannot find gcmask for %s", ctxt.loader.SymName(s));
        return null;
    }
    ref var relocs = ref heap(ctxt.loader.Relocs(s), out ptr<var> _addr_relocs);
    var rs = decodeRelocSym(_addr_ctxt.loader, s, _addr_relocs, 2 * int32(ctxt.Arch.PtrSize) + 8 + 1 * int32(ctxt.Arch.PtrSize));
    return ctxt.loader.Data(rs);
}

// Find the elf.Section of a given shared library that contains a given address.
private static ptr<elf.Section> findShlibSection(ptr<Link> _addr_ctxt, @string path, ulong addr) {
    ref Link ctxt = ref _addr_ctxt.val;

    foreach (var (_, shlib) in ctxt.Shlibs) {
        if (shlib.Path == path) {
            foreach (var (_, sect) in shlib.File.Sections[(int)1..]) { // skip the NULL section
                if (sect.Addr <= addr && addr < sect.Addr + sect.Size) {
                    return _addr_sect!;
                }
            }
        }
    }    return _addr_null!;
}

private static ulong decodetypeGcprogShlib(ptr<Link> _addr_ctxt, slice<byte> data) {
    ref Link ctxt = ref _addr_ctxt.val;

    return decodeInuxi(_addr_ctxt.Arch, data[(int)2 * int32(ctxt.Arch.PtrSize) + 8 + 1 * int32(ctxt.Arch.PtrSize)..], ctxt.Arch.PtrSize);
}

} // end ld_package
