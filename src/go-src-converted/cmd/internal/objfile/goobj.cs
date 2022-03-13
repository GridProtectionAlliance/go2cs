// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of Go intermediate object files and archives.

// package objfile -- go2cs converted at 2022 March 13 05:43:16 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Program Files\Go\src\cmd\internal\objfile\goobj.go
namespace go.cmd.@internal;

using archive = cmd.@internal.archive_package;
using goobj = cmd.@internal.goobj_package;
using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using dwarf = debug.dwarf_package;
using gosym = debug.gosym_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using System;

public static partial class objfile_package {

private partial struct goobjFile {
    public ptr<archive.GoObj> goobj;
    public ptr<goobj.Reader> r;
    public ptr<os.File> f;
    public ptr<sys.Arch> arch;
}

private static (ptr<File>, error) openGoFile(ptr<os.File> _addr_f) {
    ptr<File> _p0 = default!;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    var (a, err) = archive.Parse(f, false);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var entries = make_slice<ptr<Entry>>(0, len(a.Entries));
L:
    foreach (var (_, e) in a.Entries) {

        if (e.Type == archive.EntryPkgDef) 
            continue;
        else if (e.Type == archive.EntryGoObj) 
            var o = e.Obj;
            var b = make_slice<byte>(o.Size);
            var (_, err) = f.ReadAt(b, o.Offset);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
            var r = goobj.NewReaderFromBytes(b, false);
            ptr<sys.Arch> arch;
            {
                var a__prev2 = a;

                foreach (var (_, __a) in sys.Archs) {
                    a = __a;
                    if (a.Name == e.Obj.Arch) {
                        arch = a;
                        break;
                    }
                }

                a = a__prev2;
            }

            entries = append(entries, addr(new Entry(name:e.Name,raw:&goobjFile{e.Obj,r,f,arch},)));
            continue;
        else if (e.Type == archive.EntryNativeObj) 
            var nr = io.NewSectionReader(f, e.Offset, e.Size);
            foreach (var (_, try) in openers) {
                {
                    var (raw, err) = try(nr);

                    if (err == null) {
                        entries = append(entries, addr(new Entry(name:e.Name,raw:raw,)));
                        _continueL = true;
                        break;
                    }

                }
            }
                return (_addr_null!, error.As(fmt.Errorf("open %s: unrecognized archive member %s", f.Name(), e.Name))!);
    }    return (addr(new File(f,entries)), error.As(null!)!);
}

private static @string goobjName(@string name, nint ver) {
    if (ver == 0) {
        return name;
    }
    return fmt.Sprintf("%s<%d>", name, ver);
}

private partial struct goobjReloc {
    public int Off;
    public byte Size;
    public objabi.RelocType Type;
    public long Add;
    public @string Sym;
}

private static @string String(this goobjReloc r, ulong insnOffset) {
    var delta = int64(r.Off) - int64(insnOffset);
    var s = fmt.Sprintf("[%d:%d]%s", delta, delta + int64(r.Size), r.Type);
    if (r.Sym != "") {
        if (r.Add != 0) {
            return fmt.Sprintf("%s:%s+%d", s, r.Sym, r.Add);
        }
        return fmt.Sprintf("%s:%s", s, r.Sym);
    }
    if (r.Add != 0) {
        return fmt.Sprintf("%s:%d", s, r.Add);
    }
    return s;
}

private static (slice<Sym>, error) symbols(this ptr<goobjFile> _addr_f) => func((_, panic, _) => {
    slice<Sym> _p0 = default;
    error _p0 = default!;
    ref goobjFile f = ref _addr_f.val;

    var r = f.r;
    slice<Sym> syms = default; 

    // Name of referenced indexed symbols.
    var nrefName = r.NRefName();
    var refNames = make_map<goobj.SymRef, @string>(nrefName);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < nrefName; i++) {
            var rn = r.RefName(i);
            refNames[rn.Sym()] = rn.Name(r);
        }

        i = i__prev1;
    }

    Func<ushort, nint> abiToVer = abi => {
        nint ver = default;
        if (abi == goobj.SymABIstatic) { 
            // Static symbol
            ver = 1;
        }
        return ver;
    };

    Func<goobj.SymRef, @string> resolveSymRef = s => {
        i = default;
        {
            var p = s.PkgIdx;


            if (p == goobj.PkgIdxInvalid) 
                if (s.SymIdx != 0) {
                    panic("bad sym ref");
                }
                return "";
            else if (p == goobj.PkgIdxHashed64) 
                i = s.SymIdx + uint32(r.NSym());
            else if (p == goobj.PkgIdxHashed) 
                i = s.SymIdx + uint32(r.NSym() + r.NHashed64def());
            else if (p == goobj.PkgIdxNone) 
                i = s.SymIdx + uint32(r.NSym() + r.NHashed64def() + r.NHasheddef());
            else if (p == goobj.PkgIdxBuiltin) 
                var (name, abi) = goobj.BuiltinName(int(s.SymIdx));
                return goobjName(name, abi);
            else if (p == goobj.PkgIdxSelf) 
                i = s.SymIdx;
            else 
                return refNames[s];

        }
        var sym = r.Sym(i);
        return goobjName(sym.Name(r), abiToVer(sym.ABI()));
    }; 

    // Defined symbols
    var ndef = uint32(r.NSym() + r.NHashed64def() + r.NHasheddef() + r.NNonpkgdef());
    {
        nint i__prev1 = i;

        for (i = uint32(0); i < ndef; i++) {
            var osym = r.Sym(i);
            if (osym.Name(r) == "") {
                continue; // not a real symbol
            }
            var name = osym.Name(r);
            ver = osym.ABI();
            name = goobjName(name, abiToVer(ver));
            var typ = objabi.SymKind(osym.Type());
            int code = '?';

            if (typ == objabi.STEXT) 
                code = 'T';
            else if (typ == objabi.SRODATA) 
                code = 'R';
            else if (typ == objabi.SNOPTRDATA || typ == objabi.SDATA) 
                code = 'D';
            else if (typ == objabi.SBSS || typ == objabi.SNOPTRBSS || typ == objabi.STLSBSS) 
                code = 'B';
                        if (ver >= goobj.SymABIstatic) {
                code += 'a' - 'A';
            }
            sym = new Sym(Name:name,Addr:uint64(r.DataOff(i)),Size:int64(osym.Siz()),Code:code,);

            var relocs = r.Relocs(i);
            sym.Relocs = make_slice<Reloc>(len(relocs));
            foreach (var (j) in relocs) {
                var rel = _addr_relocs[j];
                sym.Relocs[j] = new Reloc(Addr:uint64(r.DataOff(i))+uint64(rel.Off()),Size:uint64(rel.Siz()),Stringer:goobjReloc{Off:rel.Off(),Size:rel.Siz(),Type:objabi.RelocType(rel.Type()),Add:rel.Add(),Sym:resolveSymRef(rel.Sym()),},);
            }
            syms = append(syms, sym);
        }

        i = i__prev1;
    } 

    // Referenced symbols
    var n = ndef + uint32(r.NNonpkgref());
    {
        nint i__prev1 = i;

        for (i = ndef; i < n; i++) {
            osym = r.Sym(i);
            sym = new Sym(Name:osym.Name(r),Code:'U');
            syms = append(syms, sym);
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < nrefName; i++) {
            rn = r.RefName(i);
            sym = new Sym(Name:rn.Name(r),Code:'U');
            syms = append(syms, sym);
        }

        i = i__prev1;
    }

    return (syms, error.As(null!)!);
});

private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<goobjFile> _addr_f) {
    ulong textStart = default;
    slice<byte> symtab = default;
    slice<byte> pclntab = default;
    error err = default!;
    ref goobjFile f = ref _addr_f.val;
 
    // Should never be called. We implement Liner below, callers
    // should use that instead.
    return (0, null, null, error.As(fmt.Errorf("pcln not available in go object file"))!);
}

// Find returns the file name, line, and function data for the given pc.
// Returns "",0,nil if unknown.
// This function implements the Liner interface in preference to pcln() above.
private static (@string, nint, ptr<gosym.Func>) PCToLine(this ptr<goobjFile> _addr_f, ulong pc) => func((_, panic, _) => {
    @string _p0 = default;
    nint _p0 = default;
    ptr<gosym.Func> _p0 = default!;
    ref goobjFile f = ref _addr_f.val;

    var r = f.r;
    if (f.arch == null) {
        return ("", 0, _addr_null!);
    }
    Func<goobj.SymRef, slice<byte>> getSymData = s => {
        if (s.PkgIdx != goobj.PkgIdxHashed) { 
            // We don't need the data for non-hashed symbols, yet.
            panic("not supported");
        }
        var i = uint32(s.SymIdx + uint32(r.NSym() + r.NHashed64def()));
        return r.BytesAt(r.DataOff(i), r.DataSize(i));
    };

    var ndef = uint32(r.NSym() + r.NHashed64def() + r.NHasheddef() + r.NNonpkgdef());
    {
        var i__prev1 = i;

        for (i = uint32(0); i < ndef; i++) {
            var osym = r.Sym(i);
            var addr = uint64(r.DataOff(i));
            if (pc < addr || pc >= addr + uint64(osym.Siz())) {
                continue;
            }
            var isym = ~uint32(0);
            var auxs = r.Auxs(i);
            foreach (var (j) in auxs) {
                var a = _addr_auxs[j];
                if (a.Type() != goobj.AuxFuncInfo) {
                    continue;
                }
                if (a.Sym().PkgIdx != goobj.PkgIdxSelf) {
                    panic("funcinfo symbol not defined in current package");
                }
                isym = a.Sym().SymIdx;
            }
            if (isym == ~uint32(0)) {
                continue;
            }
            var b = r.BytesAt(r.DataOff(isym), r.DataSize(isym));
            ptr<goobj.FuncInfo> info;
            var pcline = getSymData(info.ReadPcline(b));
            var line = int(pcValue(pcline, pc - addr, _addr_f.arch));
            var pcfile = getSymData(info.ReadPcfile(b));
            var fileID = pcValue(pcfile, pc - addr, _addr_f.arch);
            var fileName = r.File(int(fileID)); 
            // Note: we provide only the name in the Func structure.
            // We could provide more if needed.
            return (fileName, line, addr(new gosym.Func(Sym:&gosym.Sym{Name:osym.Name(r)})));
        }

        i = i__prev1;
    }
    return ("", 0, _addr_null!);
});

// pcValue looks up the given PC in a pc value table. target is the
// offset of the pc from the entry point.
private static int pcValue(slice<byte> tab, ulong target, ptr<sys.Arch> _addr_arch) {
    ref sys.Arch arch = ref _addr_arch.val;

    ref var val = ref heap(int32(-1), out ptr<var> _addr_val);
    ref ulong pc = ref heap(out ptr<ulong> _addr_pc);
    while (step(_addr_tab, _addr_pc, _addr_val, pc == 0, _addr_arch)) {
        if (target < pc) {
            return val;
        }
    }
    return -1;
}

// step advances to the next pc, value pair in the encoded table.
private static bool step(ptr<slice<byte>> _addr_p, ptr<ulong> _addr_pc, ptr<int> _addr_val, bool first, ptr<sys.Arch> _addr_arch) {
    ref slice<byte> p = ref _addr_p.val;
    ref ulong pc = ref _addr_pc.val;
    ref int val = ref _addr_val.val;
    ref sys.Arch arch = ref _addr_arch.val;

    var uvdelta = readvarint(_addr_p);
    if (uvdelta == 0 && !first) {
        return false;
    }
    if (uvdelta & 1 != 0) {
        uvdelta = ~(uvdelta >> 1);
    }
    else
 {
        uvdelta>>=1;
    }
    var vdelta = int32(uvdelta);
    var pcdelta = readvarint(_addr_p) * uint32(arch.MinLC);
    pc += uint64(pcdelta);
    val += vdelta;
    return true;
}

// readvarint reads, removes, and returns a varint from *p.
private static uint readvarint(ptr<slice<byte>> _addr_p) {
    ref slice<byte> p = ref _addr_p.val;

    uint v = default;    uint shift = default;

    slice<byte> s = p;
    shift = 0;

    while () {
        var b = s[0];
        s = s[(int)1..];
        v |= (uint32(b) & 0x7F) << (int)(shift);
        if (b & 0x80 == 0) {
            break;
        shift += 7;
        }
    }
    p = s;
    return v;
}

// We treat the whole object file as the text section.
private static (ulong, slice<byte>, error) text(this ptr<goobjFile> _addr_f) {
    ulong textStart = default;
    slice<byte> text = default;
    error err = default!;
    ref goobjFile f = ref _addr_f.val;

    text = make_slice<byte>(f.goobj.Size);
    _, err = f.f.ReadAt(text, int64(f.goobj.Offset));
    return ;
}

private static @string goarch(this ptr<goobjFile> _addr_f) {
    ref goobjFile f = ref _addr_f.val;

    return f.goobj.Arch;
}

private static (ulong, error) loadAddress(this ptr<goobjFile> _addr_f) {
    ulong _p0 = default;
    error _p0 = default!;
    ref goobjFile f = ref _addr_f.val;

    return (0, error.As(fmt.Errorf("unknown load address"))!);
}

private static (ptr<dwarf.Data>, error) dwarf(this ptr<goobjFile> _addr_f) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref goobjFile f = ref _addr_f.val;

    return (_addr_null!, error.As(errors.New("no DWARF data in go object file"))!);
}

} // end objfile_package
