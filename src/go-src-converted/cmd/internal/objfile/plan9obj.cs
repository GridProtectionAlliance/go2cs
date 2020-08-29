// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of Plan 9 a.out executables.

// package objfile -- go2cs converted at 2020 August 29 08:46:32 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\plan9obj.go
using dwarf = go.debug.dwarf_package;
using plan9obj = go.debug.plan9obj_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        private static map validSymType = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<int, bool>{'T':true,'t':true,'D':true,'d':true,'B':true,'b':true,};

        private partial struct plan9File
        {
            public ptr<plan9obj.File> plan9;
        }

        private static (rawFile, error) openPlan9(io.ReaderAt r)
        {
            var (f, err) = plan9obj.NewFile(r);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new plan9File(f), null);
        }

        private static (slice<Sym>, error) symbols(this ref plan9File f)
        {
            var (plan9Syms, err) = f.plan9.Symbols();
            if (err != null)
            {
                return (null, err);
            } 

            // Build sorted list of addresses of all symbols.
            // We infer the size of a symbol by looking at where the next symbol begins.
            slice<ulong> addrs = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in plan9Syms)
                {
                    s = __s;
                    if (!validSymType[s.Type])
                    {
                        continue;
                    }
                    addrs = append(addrs, s.Value);
                }

                s = s__prev1;
            }

            sort.Sort(uint64s(addrs));

            slice<Sym> syms = default;

            {
                var s__prev1 = s;

                foreach (var (_, __s) in plan9Syms)
                {
                    s = __s;
                    if (!validSymType[s.Type])
                    {
                        continue;
                    }
                    Sym sym = new Sym(Addr:s.Value,Name:s.Name,Code:s.Type);
                    var i = sort.Search(len(addrs), x => addrs[x] > s.Value);
                    if (i < len(addrs))
                    {
                        sym.Size = int64(addrs[i] - s.Value);
                    }
                    syms = append(syms, sym);
                }

                s = s__prev1;
            }

            return (syms, null);
        }

        private static (ulong, slice<byte>, slice<byte>, error) pcln(this ref plan9File f)
        {
            textStart = f.plan9.LoadAddress + f.plan9.HdrSize;
            pclntab, err = loadPlan9Table(f.plan9, "runtime.pclntab", "runtime.epclntab");

            if (err != null)
            { 
                // We didn't find the symbols, so look for the names used in 1.3 and earlier.
                // TODO: Remove code looking for the old symbols when we no longer care about 1.3.
                error err2 = default;
                pclntab, err2 = loadPlan9Table(f.plan9, "pclntab", "epclntab");

                if (err2 != null)
                {
                    return (0L, null, null, err);
                }
            }
            symtab, err = loadPlan9Table(f.plan9, "runtime.symtab", "runtime.esymtab");

            if (err != null)
            { 
                // Same as above.
                err2 = default;
                symtab, err2 = loadPlan9Table(f.plan9, "symtab", "esymtab");

                if (err2 != null)
                {
                    return (0L, null, null, err);
                }
            }
            return (textStart, symtab, pclntab, null);
        }

        private static (ulong, slice<byte>, error) text(this ref plan9File f)
        {
            var sect = f.plan9.Section("text");
            if (sect == null)
            {
                return (0L, null, fmt.Errorf("text section not found"));
            }
            textStart = f.plan9.LoadAddress + f.plan9.HdrSize;
            text, err = sect.Data();
            return;
        }

        private static (ref plan9obj.Sym, error) findPlan9Symbol(ref plan9obj.File f, @string name)
        {
            var (syms, err) = f.Symbols();
            if (err != null)
            {
                return (null, err);
            }
            foreach (var (_, s) in syms)
            {
                if (s.Name != name)
                {
                    continue;
                }
                return (ref s, null);
            }
            return (null, fmt.Errorf("no %s symbol found", name));
        }

        private static (slice<byte>, error) loadPlan9Table(ref plan9obj.File f, @string sname, @string ename)
        {
            var (ssym, err) = findPlan9Symbol(f, sname);
            if (err != null)
            {
                return (null, err);
            }
            var (esym, err) = findPlan9Symbol(f, ename);
            if (err != null)
            {
                return (null, err);
            }
            var sect = f.Section("text");
            if (sect == null)
            {
                return (null, err);
            }
            var (data, err) = sect.Data();
            if (err != null)
            {
                return (null, err);
            }
            var textStart = f.LoadAddress + f.HdrSize;
            return (data[ssym.Value - textStart..esym.Value - textStart], null);
        }

        private static @string goarch(this ref plan9File f)
        {

            if (f.plan9.Magic == plan9obj.Magic386) 
                return "386";
            else if (f.plan9.Magic == plan9obj.MagicAMD64) 
                return "amd64";
            else if (f.plan9.Magic == plan9obj.MagicARM) 
                return "arm";
                        return "";
        }

        private static (ulong, error) loadAddress(this ref plan9File f)
        {
            return (0L, fmt.Errorf("unknown load address"));
        }

        private static (ref dwarf.Data, error) dwarf(this ref plan9File f)
        {
            return (null, errors.New("no DWARF data in Plan 9 file"));
        }
    }
}}}
