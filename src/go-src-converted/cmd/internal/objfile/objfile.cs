// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package objfile implements portable access to OS-specific executable files.
// package objfile -- go2cs converted at 2020 August 29 08:46:27 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\objfile.go
using dwarf = go.debug.dwarf_package;
using gosym = go.debug.gosym_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        private partial interface rawFile
        {
            (ref dwarf.Data, error) symbols();
            (ref dwarf.Data, error) pcln();
            (ref dwarf.Data, error) text();
            (ref dwarf.Data, error) goarch();
            (ref dwarf.Data, error) loadAddress();
            (ref dwarf.Data, error) dwarf();
        }

        // A File is an opened executable file.
        public partial struct File
        {
            public ptr<os.File> r;
            public slice<ref Entry> entries;
        }

        public partial struct Entry
        {
            public @string name;
            public rawFile raw;
        }

        // A Sym is a symbol defined in an executable file.
        public partial struct Sym
        {
            public @string Name; // symbol name
            public ulong Addr; // virtual address of symbol
            public long Size; // size in bytes
            public int Code; // nm code (T for text, D for data, and so on)
            public @string Type; // XXX?
            public slice<Reloc> Relocs; // in increasing Addr order
        }

        public partial struct Reloc
        {
            public ulong Addr; // Address of first byte that reloc applies to.
            public ulong Size; // Number of bytes
            public RelocStringer Stringer;
        }

        public partial interface RelocStringer
        {
            @string String(ulong insnOffset);
        }

        private static Func<io.ReaderAt, (rawFile, error)> openers = new slice<Func<io.ReaderAt, (rawFile, error)>>(new Func<io.ReaderAt, (rawFile, error)>[] { openElf, openMacho, openPE, openPlan9 });

        // Open opens the named file.
        // The caller must call f.Close when the file is no longer needed.
        public static (ref File, error) Open(@string name)
        {
            var (r, err) = os.Open(name);
            if (err != null)
            {
                return (null, err);
            }
            {
                var (f, err) = openGoFile(r);

                if (err == null)
                {
                    return (f, null);
                }

            }
            foreach (var (_, try) in openers)
            {
                {
                    var (raw, err) = try(r);

                    if (err == null)
                    {
                        return (ref new File(r,[]*Entry{&Entry{raw:raw}}), null);
                    }

                }
            }
            r.Close();
            return (null, fmt.Errorf("open %s: unrecognized object file", name));
        }

        private static error Close(this ref File f)
        {
            return error.As(f.r.Close());
        }

        private static slice<ref Entry> Entries(this ref File f)
        {
            return f.entries;
        }

        private static (slice<Sym>, error) Symbols(this ref File f)
        {
            return f.entries[0L].Symbols();
        }

        private static (Liner, error) PCLineTable(this ref File f)
        {
            return f.entries[0L].PCLineTable();
        }

        private static (ulong, slice<byte>, error) Text(this ref File f)
        {
            return f.entries[0L].Text();
        }

        private static @string GOARCH(this ref File f)
        {
            return f.entries[0L].GOARCH();
        }

        private static (ulong, error) LoadAddress(this ref File f)
        {
            return f.entries[0L].LoadAddress();
        }

        private static (ref dwarf.Data, error) DWARF(this ref File f)
        {
            return f.entries[0L].DWARF();
        }

        private static (ref Disasm, error) Disasm(this ref File f)
        {
            return f.entries[0L].Disasm();
        }

        private static @string Name(this ref Entry e)
        {
            return e.name;
        }

        private static (slice<Sym>, error) Symbols(this ref Entry e)
        {
            var (syms, err) = e.raw.symbols();
            if (err != null)
            {
                return (null, err);
            }
            sort.Sort(byAddr(syms));
            return (syms, null);
        }

        private partial struct byAddr // : slice<Sym>
        {
        }

        private static bool Less(this byAddr x, long i, long j)
        {
            return x[i].Addr < x[j].Addr;
        }
        private static long Len(this byAddr x)
        {
            return len(x);
        }
        private static void Swap(this byAddr x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }

        private static (Liner, error) PCLineTable(this ref Entry e)
        { 
            // If the raw file implements Liner directly, use that.
            // Currently, only Go intermediate objects and archives (goobj) use this path.
            {
                Liner (pcln, ok) = e.raw._<Liner>();

                if (ok)
                {
                    return (pcln, null);
                } 
                // Otherwise, read the pcln tables and build a Liner out of that.

            } 
            // Otherwise, read the pcln tables and build a Liner out of that.
            var (textStart, symtab, pclntab, err) = e.raw.pcln();
            if (err != null)
            {
                return (null, err);
            }
            return gosym.NewTable(symtab, gosym.NewLineTable(pclntab, textStart));
        }

        private static (ulong, slice<byte>, error) Text(this ref Entry e)
        {
            return e.raw.text();
        }

        private static @string GOARCH(this ref Entry e)
        {
            return e.raw.goarch();
        }

        // LoadAddress returns the expected load address of the file.
        // This differs from the actual load address for a position-independent
        // executable.
        private static (ulong, error) LoadAddress(this ref Entry e)
        {
            return e.raw.loadAddress();
        }

        // DWARF returns DWARF debug data for the file, if any.
        // This is for cmd/pprof to locate cgo functions.
        private static (ref dwarf.Data, error) DWARF(this ref Entry e)
        {
            return e.raw.dwarf();
        }
    }
}}}
