// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of Mach-O executables (OS X).

// package objfile -- go2cs converted at 2022 March 13 05:43:29 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Program Files\Go\src\cmd\internal\objfile\macho.go
namespace go.cmd.@internal;

using dwarf = debug.dwarf_package;
using macho = debug.macho_package;
using fmt = fmt_package;
using io = io_package;
using sort = sort_package;
using System;

public static partial class objfile_package {

private static readonly nuint stabTypeMask = 0xe0;



private partial struct machoFile {
    public ptr<macho.File> macho;
}

private static (rawFile, error) openMacho(io.ReaderAt r) {
    rawFile _p0 = default;
    error _p0 = default!;

    var (f, err) = macho.NewFile(r);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (addr(new machoFile(f)), error.As(null!)!);
}

private static (slice<Sym>, error) symbols(this ptr<machoFile> _addr_f) {
    slice<Sym> _p0 = default;
    error _p0 = default!;
    ref machoFile f = ref _addr_f.val;

    if (f.macho.Symtab == null) {
        return (null, error.As(null!)!);
    }
    slice<ulong> addrs = default;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in f.macho.Symtab.Syms) {
            s = __s; 
            // Skip stab debug info.
            if (s.Type & stabTypeMask == 0) {
                addrs = append(addrs, s.Value);
            }
        }
        s = s__prev1;
    }

    sort.Sort(uint64s(addrs));

    slice<Sym> syms = default;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in f.macho.Symtab.Syms) {
            s = __s;
            if (s.Type & stabTypeMask != 0) { 
                // Skip stab debug info.
                continue;
            }
            Sym sym = new Sym(Name:s.Name,Addr:s.Value,Code:'?');
            var i = sort.Search(len(addrs), x => addrs[x] > s.Value);
            if (i < len(addrs)) {
                sym.Size = int64(addrs[i] - s.Value);
            }
            if (s.Sect == 0) {
                sym.Code = 'U';
            }
            else if (int(s.Sect) <= len(f.macho.Sections)) {
                var sect = f.macho.Sections[s.Sect - 1];
                switch (sect.Seg) {
                    case "__TEXT": 

                    case "__DATA_CONST": 
                        sym.Code = 'R';
                        break;
                    case "__DATA": 
                        sym.Code = 'D';
                        break;
                }
                switch (sect.Seg + " " + sect.Name) {
                    case "__TEXT __text": 
                        sym.Code = 'T';
                        break;
                    case "__DATA __bss": 

                    case "__DATA __noptrbss": 
                        sym.Code = 'B';
                        break;
                }
            }
            syms = append(syms, sym);
        }
        s = s__prev1;
    }

    return (syms, error.As(null!)!);
}

private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<machoFile> _addr_f) {
    ulong textStart = default;
    slice<byte> symtab = default;
    slice<byte> pclntab = default;
    error err = default!;
    ref machoFile f = ref _addr_f.val;

    {
        var sect__prev1 = sect;

        var sect = f.macho.Section("__text");

        if (sect != null) {
            textStart = sect.Addr;
        }
        sect = sect__prev1;

    }
    {
        var sect__prev1 = sect;

        sect = f.macho.Section("__gosymtab");

        if (sect != null) {
            symtab, err = sect.Data();

            if (err != null) {
                return (0, null, null, error.As(err)!);
            }
        }
        sect = sect__prev1;

    }
    {
        var sect__prev1 = sect;

        sect = f.macho.Section("__gopclntab");

        if (sect != null) {
            pclntab, err = sect.Data();

            if (err != null) {
                return (0, null, null, error.As(err)!);
            }
        }
        sect = sect__prev1;

    }
    return (textStart, symtab, pclntab, error.As(null!)!);
}

private static (ulong, slice<byte>, error) text(this ptr<machoFile> _addr_f) {
    ulong textStart = default;
    slice<byte> text = default;
    error err = default!;
    ref machoFile f = ref _addr_f.val;

    var sect = f.macho.Section("__text");
    if (sect == null) {
        return (0, null, error.As(fmt.Errorf("text section not found"))!);
    }
    textStart = sect.Addr;
    text, err = sect.Data();
    return ;
}

private static @string goarch(this ptr<machoFile> _addr_f) {
    ref machoFile f = ref _addr_f.val;


    if (f.macho.Cpu == macho.Cpu386) 
        return "386";
    else if (f.macho.Cpu == macho.CpuAmd64) 
        return "amd64";
    else if (f.macho.Cpu == macho.CpuArm) 
        return "arm";
    else if (f.macho.Cpu == macho.CpuArm64) 
        return "arm64";
    else if (f.macho.Cpu == macho.CpuPpc64) 
        return "ppc64";
        return "";
}

private partial struct uint64s { // : slice<ulong>
}

private static nint Len(this uint64s x) {
    return len(x);
}
private static void Swap(this uint64s x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}
private static bool Less(this uint64s x, nint i, nint j) {
    return x[i] < x[j];
}

private static (ulong, error) loadAddress(this ptr<machoFile> _addr_f) {
    ulong _p0 = default;
    error _p0 = default!;
    ref machoFile f = ref _addr_f.val;

    return (0, error.As(fmt.Errorf("unknown load address"))!);
}

private static (ptr<dwarf.Data>, error) dwarf(this ptr<machoFile> _addr_f) {
    ptr<dwarf.Data> _p0 = default!;
    error _p0 = default!;
    ref machoFile f = ref _addr_f.val;

    return _addr_f.macho.DWARF()!;
}

} // end objfile_package
