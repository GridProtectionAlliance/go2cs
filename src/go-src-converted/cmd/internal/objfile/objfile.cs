// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package objfile implements portable access to OS-specific executable files.
// package objfile -- go2cs converted at 2022 March 06 22:32:33 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Program Files\Go\src\cmd\internal\objfile\objfile.go
using archive = go.cmd.@internal.archive_package;
using dwarf = go.debug.dwarf_package;
using gosym = go.debug.gosym_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using System;


namespace go.cmd.@internal;

public static partial class objfile_package {

private partial interface rawFile {
    (ptr<dwarf.Data>, error) symbols();
    (ptr<dwarf.Data>, error) pcln();
    (ptr<dwarf.Data>, error) text();
    (ptr<dwarf.Data>, error) goarch();
    (ptr<dwarf.Data>, error) loadAddress();
    (ptr<dwarf.Data>, error) dwarf();
}

// A File is an opened executable file.
public partial struct File {
    public ptr<os.File> r;
    public slice<ptr<Entry>> entries;
}

public partial struct Entry {
    public @string name;
    public rawFile raw;
}

// A Sym is a symbol defined in an executable file.
public partial struct Sym {
    public @string Name; // symbol name
    public ulong Addr; // virtual address of symbol
    public long Size; // size in bytes
    public int Code; // nm code (T for text, D for data, and so on)
    public @string Type; // XXX?
    public slice<Reloc> Relocs; // in increasing Addr order
}

public partial struct Reloc {
    public ulong Addr; // Address of first byte that reloc applies to.
    public ulong Size; // Number of bytes
    public RelocStringer Stringer;
}

public partial interface RelocStringer {
    @string String(ulong insnOffset);
}

private static Func<io.ReaderAt, (rawFile, error)> openers = new slice<Func<io.ReaderAt, (rawFile, error)>>(new Func<io.ReaderAt, (rawFile, error)>[] { openElf, openMacho, openPE, openPlan9, openXcoff });

// Open opens the named file.
// The caller must call f.Close when the file is no longer needed.
public static (ptr<File>, error) Open(@string name) {
    ptr<File> _p0 = default!;
    error _p0 = default!;

    var (r, err) = os.Open(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    {
        var (f, err) = openGoFile(r);

        if (err == null) {
            return (_addr_f!, error.As(null!)!);
        }        {
            archive.ErrGoObjOtherVersion (_, ok) = err._<archive.ErrGoObjOtherVersion>();


            else if (ok) {
                return (_addr_null!, error.As(fmt.Errorf("open %s: %v", name, err))!);
            }

        }


    }

    foreach (var (_, try) in openers) {
        {
            var (raw, err) = try(r);

            if (err == null) {
                return (addr(new File(r,[]*Entry{{raw:raw}})), error.As(null!)!);
            }

        }

    }    r.Close();
    return (_addr_null!, error.As(fmt.Errorf("open %s: unrecognized object file", name))!);

}

private static error Close(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return error.As(f.r.Close())!;
}

private static slice<ptr<Entry>> Entries(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.entries;
}

private static (slice<Sym>, error) Symbols(this ptr<File> _addr_f) {
    slice<Sym> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return f.entries[0].Symbols();
}

private static (Liner, error) PCLineTable(this ptr<File> _addr_f) {
    Liner _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return f.entries[0].PCLineTable();
}

private static (ulong, slice<byte>, error) Text(this ptr<File> _addr_f) {
    ulong _p0 = default;
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return f.entries[0].Text();
}

private static @string GOARCH(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.entries[0].GOARCH();
}

private static (ulong, error) LoadAddress(this ptr<File> _addr_f) {
    ulong _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return f.entries[0].LoadAddress();
}

private static (ptr<dwarf.Data>, error) DWARF(this ptr<File> _addr_f) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return _addr_f.entries[0].DWARF()!;
}

private static (ptr<Disasm>, error) Disasm(this ptr<File> _addr_f) {
    ptr<Disasm> _p0 = default!;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    return _addr_f.entries[0].Disasm()!;
}

private static @string Name(this ptr<Entry> _addr_e) {
    ref Entry e = ref _addr_e.val;

    return e.name;
}

private static (slice<Sym>, error) Symbols(this ptr<Entry> _addr_e) {
    slice<Sym> _p0 = default;
    error _p0 = default!;
    ref Entry e = ref _addr_e.val;

    var (syms, err) = e.raw.symbols();
    if (err != null) {
        return (null, error.As(err)!);
    }
    sort.Sort(byAddr(syms));
    return (syms, error.As(null!)!);

}

private partial struct byAddr { // : slice<Sym>
}

private static bool Less(this byAddr x, nint i, nint j) {
    return x[i].Addr < x[j].Addr;
}
private static nint Len(this byAddr x) {
    return len(x);
}
private static void Swap(this byAddr x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

private static (Liner, error) PCLineTable(this ptr<Entry> _addr_e) {
    Liner _p0 = default;
    error _p0 = default!;
    ref Entry e = ref _addr_e.val;
 
    // If the raw file implements Liner directly, use that.
    // Currently, only Go intermediate objects and archives (goobj) use this path.
    {
        Liner (pcln, ok) = e.raw._<Liner>();

        if (ok) {
            return (pcln, error.As(null!)!);
        }
    } 
    // Otherwise, read the pcln tables and build a Liner out of that.
    var (textStart, symtab, pclntab, err) = e.raw.pcln();
    if (err != null) {
        return (null, error.As(err)!);
    }
    return gosym.NewTable(symtab, gosym.NewLineTable(pclntab, textStart));

}

private static (ulong, slice<byte>, error) Text(this ptr<Entry> _addr_e) {
    ulong _p0 = default;
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Entry e = ref _addr_e.val;

    return e.raw.text();
}

private static @string GOARCH(this ptr<Entry> _addr_e) {
    ref Entry e = ref _addr_e.val;

    return e.raw.goarch();
}

// LoadAddress returns the expected load address of the file.
// This differs from the actual load address for a position-independent
// executable.
private static (ulong, error) LoadAddress(this ptr<Entry> _addr_e) {
    ulong _p0 = default;
    error _p0 = default!;
    ref Entry e = ref _addr_e.val;

    return e.raw.loadAddress();
}

// DWARF returns DWARF debug data for the file, if any.
// This is for cmd/pprof to locate cgo functions.
private static (ptr<dwarf.Data>, error) DWARF(this ptr<Entry> _addr_e) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref Entry e = ref _addr_e.val;

    return _addr_e.raw.dwarf()!;
}

} // end objfile_package
