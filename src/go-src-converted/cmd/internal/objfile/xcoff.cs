// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of XCOFF executable (AIX)

// package objfile -- go2cs converted at 2022 March 13 05:52:05 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Program Files\Go\src\cmd\internal\objfile\xcoff.go
namespace go.cmd.@internal;

using dwarf = debug.dwarf_package;
using fmt = fmt_package;
using xcoff = @internal.xcoff_package;
using io = io_package;
using unicode = unicode_package;

public static partial class objfile_package {

private partial struct xcoffFile {
    public ptr<xcoff.File> xcoff;
}

private static (rawFile, error) openXcoff(io.ReaderAt r) {
    rawFile _p0 = default;
    error _p0 = default!;

    var (f, err) = xcoff.NewFile(r);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (addr(new xcoffFile(f)), error.As(null!)!);
}

private static (slice<Sym>, error) symbols(this ptr<xcoffFile> _addr_f) {
    slice<Sym> _p0 = default;
    error _p0 = default!;
    ref xcoffFile f = ref _addr_f.val;

    slice<Sym> syms = default;
    foreach (var (_, s) in f.xcoff.Symbols) {
        const nint N_UNDEF = 0; // An undefined (extern) symbol
        const nint N_ABS = -1; // An absolute symbol (e_value is a constant, not an address)
        const nint N_DEBUG = -2; // A debugging symbol
        Sym sym = new Sym(Name:s.Name,Addr:s.Value,Code:'?');


        if (s.SectionNumber == N_UNDEF) 
            sym.Code = 'U';
        else if (s.SectionNumber == N_ABS) 
            sym.Code = 'C';
        else if (s.SectionNumber == N_DEBUG) 
            sym.Code = '?';
        else 
            if (s.SectionNumber < 0 || len(f.xcoff.Sections) < int(s.SectionNumber)) {
                return (null, error.As(fmt.Errorf("invalid section number in symbol table"))!);
            }
            var sect = f.xcoff.Sections[s.SectionNumber - 1]; 

            // debug/xcoff returns an offset in the section not the actual address
            sym.Addr += sect.VirtualAddress;

            if (s.AuxCSect.SymbolType & 0x3 == xcoff.XTY_LD) { 
                // The size of a function is contained in the
                // AUX_FCN entry
                sym.Size = s.AuxFcn.Size;
            }
            else
 {
                sym.Size = s.AuxCSect.Length;
            }
            sym.Size = s.AuxCSect.Length;


            if (sect.Type == xcoff.STYP_TEXT) 
                if (s.AuxCSect.StorageMappingClass == xcoff.XMC_RO) {
                    sym.Code = 'R';
                }
                else
 {
                    sym.Code = 'T';
                }
            else if (sect.Type == xcoff.STYP_DATA) 
                sym.Code = 'D';
            else if (sect.Type == xcoff.STYP_BSS) 
                sym.Code = 'B';
                        if (s.StorageClass == xcoff.C_HIDEXT) { 
                // Local symbol
                sym.Code = unicode.ToLower(sym.Code);
            }
                syms = append(syms, sym);
    }    return (syms, error.As(null!)!);
}

private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<xcoffFile> _addr_f) {
    ulong textStart = default;
    slice<byte> symtab = default;
    slice<byte> pclntab = default;
    error err = default!;
    ref xcoffFile f = ref _addr_f.val;

    {
        var sect = f.xcoff.Section(".text");

        if (sect != null) {
            textStart = sect.VirtualAddress;
        }
    }
    pclntab, err = loadXCOFFTable(_addr_f.xcoff, "runtime.pclntab", "runtime.epclntab");

    if (err != null) {
        return (0, null, null, error.As(err)!);
    }
    symtab, _ = loadXCOFFTable(_addr_f.xcoff, "runtime.symtab", "runtime.esymtab"); // ignore error, this symbol is not useful anyway
    return (textStart, symtab, pclntab, error.As(null!)!);
}

private static (ulong, slice<byte>, error) text(this ptr<xcoffFile> _addr_f) {
    ulong textStart = default;
    slice<byte> text = default;
    error err = default!;
    ref xcoffFile f = ref _addr_f.val;

    var sect = f.xcoff.Section(".text");
    if (sect == null) {
        return (0, null, error.As(fmt.Errorf("text section not found"))!);
    }
    textStart = sect.VirtualAddress;
    text, err = sect.Data();
    return ;
}

private static (ptr<xcoff.Symbol>, error) findXCOFFSymbol(ptr<xcoff.File> _addr_f, @string name) {
    ptr<xcoff.Symbol> _p0 = default!;
    error _p0 = default!;
    ref xcoff.File f = ref _addr_f.val;

    foreach (var (_, s) in f.Symbols) {
        if (s.Name != name) {
            continue;
        }
        if (s.SectionNumber <= 0) {
            return (_addr_null!, error.As(fmt.Errorf("symbol %s: invalid section number %d", name, s.SectionNumber))!);
        }
        if (len(f.Sections) < int(s.SectionNumber)) {
            return (_addr_null!, error.As(fmt.Errorf("symbol %s: section number %d is larger than max %d", name, s.SectionNumber, len(f.Sections)))!);
        }
        return (_addr_s!, error.As(null!)!);
    }    return (_addr_null!, error.As(fmt.Errorf("no %s symbol found", name))!);
}

private static (slice<byte>, error) loadXCOFFTable(ptr<xcoff.File> _addr_f, @string sname, @string ename) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref xcoff.File f = ref _addr_f.val;

    var (ssym, err) = findXCOFFSymbol(_addr_f, sname);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (esym, err) = findXCOFFSymbol(_addr_f, ename);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (ssym.SectionNumber != esym.SectionNumber) {
        return (null, error.As(fmt.Errorf("%s and %s symbols must be in the same section", sname, ename))!);
    }
    var sect = f.Sections[ssym.SectionNumber - 1];
    var (data, err) = sect.Data();
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (data[(int)ssym.Value..(int)esym.Value], error.As(null!)!);
}

private static @string goarch(this ptr<xcoffFile> _addr_f) {
    ref xcoffFile f = ref _addr_f.val;


    if (f.xcoff.TargetMachine == xcoff.U802TOCMAGIC) 
        return "ppc";
    else if (f.xcoff.TargetMachine == xcoff.U64_TOCMAGIC) 
        return "ppc64";
        return "";
}

private static (ulong, error) loadAddress(this ptr<xcoffFile> _addr_f) {
    ulong _p0 = default;
    error _p0 = default!;
    ref xcoffFile f = ref _addr_f.val;

    return (0, error.As(fmt.Errorf("unknown load address"))!);
}

private static (ptr<dwarf.Data>, error) dwarf(this ptr<xcoffFile> _addr_f) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref xcoffFile f = ref _addr_f.val;

    return _addr_f.xcoff.DWARF()!;
}

} // end objfile_package
