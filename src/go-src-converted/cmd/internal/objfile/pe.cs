// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of PE executables (Microsoft Windows).

// package objfile -- go2cs converted at 2020 August 29 08:46:29 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\pe.go
using dwarf = go.debug.dwarf_package;
using pe = go.debug.pe_package;
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
        private partial struct peFile
        {
            public ptr<pe.File> pe;
        }

        private static (rawFile, error) openPE(io.ReaderAt r)
        {
            var (f, err) = pe.NewFile(r);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new peFile(f), null);
        }

        private static (slice<Sym>, error) symbols(this ref peFile f)
        { 
            // Build sorted list of addresses of all symbols.
            // We infer the size of a symbol by looking at where the next symbol begins.
            slice<ulong> addrs = default;

            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ref pe.OptionalHeader32 oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ref pe.OptionalHeader64 oh:
                    imageBase = oh.ImageBase;
                    break;

            }

            slice<Sym> syms = default;
            foreach (var (_, s) in f.pe.Symbols)
            {
                const long N_UNDEF = 0L; // An undefined (extern) symbol
                const long N_ABS = -1L; // An absolute symbol (e_value is a constant, not an address)
                const long N_DEBUG = -2L; // A debugging symbol
                Sym sym = new Sym(Name:s.Name,Addr:uint64(s.Value),Code:'?');

                if (s.SectionNumber == N_UNDEF) 
                    sym.Code = 'U';
                else if (s.SectionNumber == N_ABS) 
                    sym.Code = 'C';
                else if (s.SectionNumber == N_DEBUG) 
                    sym.Code = '?';
                else 
                    if (s.SectionNumber < 0L || len(f.pe.Sections) < int(s.SectionNumber))
                    {
                        return (null, fmt.Errorf("invalid section number in symbol table"));
                    }
                    var sect = f.pe.Sections[s.SectionNumber - 1L];
                    const ulong text = 0x20UL;
                    const ulong data = 0x40UL;
                    const ulong bss = 0x80UL;
                    const ulong permW = 0x80000000UL;
                    var ch = sect.Characteristics;

                    if (ch & text != 0L) 
                        sym.Code = 'T';
                    else if (ch & data != 0L) 
                        if (ch & permW == 0L)
                        {
                            sym.Code = 'R';
                        }
                        else
                        {
                            sym.Code = 'D';
                        }
                    else if (ch & bss != 0L) 
                        sym.Code = 'B';
                                        sym.Addr += imageBase + uint64(sect.VirtualAddress);
                                syms = append(syms, sym);
                addrs = append(addrs, sym.Addr);
            }
            sort.Sort(uint64s(addrs));
            foreach (var (i) in syms)
            {
                var j = sort.Search(len(addrs), x => addrs[x] > syms[i].Addr);
                if (j < len(addrs))
                {
                    syms[i].Size = int64(addrs[j] - syms[i].Addr);
                }
            }
            return (syms, null);
        }

        private static (ulong, slice<byte>, slice<byte>, error) pcln(this ref peFile f)
        {
            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ref pe.OptionalHeader32 oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ref pe.OptionalHeader64 oh:
                    imageBase = oh.ImageBase;
                    break;
                default:
                {
                    var oh = f.pe.OptionalHeader.type();
                    return (0L, null, null, fmt.Errorf("pe file format not recognized"));
                    break;
                }
            }
            {
                var sect = f.pe.Section(".text");

                if (sect != null)
                {
                    textStart = imageBase + uint64(sect.VirtualAddress);
                }

            }
            pclntab, err = loadPETable(f.pe, "runtime.pclntab", "runtime.epclntab");

            if (err != null)
            { 
                // We didn't find the symbols, so look for the names used in 1.3 and earlier.
                // TODO: Remove code looking for the old symbols when we no longer care about 1.3.
                error err2 = default;
                pclntab, err2 = loadPETable(f.pe, "pclntab", "epclntab");

                if (err2 != null)
                {
                    return (0L, null, null, err);
                }
            }
            symtab, err = loadPETable(f.pe, "runtime.symtab", "runtime.esymtab");

            if (err != null)
            { 
                // Same as above.
                err2 = default;
                symtab, err2 = loadPETable(f.pe, "symtab", "esymtab");

                if (err2 != null)
                {
                    return (0L, null, null, err);
                }
            }
            return (textStart, symtab, pclntab, null);
        }

        private static (ulong, slice<byte>, error) text(this ref peFile f)
        {
            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ref pe.OptionalHeader32 oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ref pe.OptionalHeader64 oh:
                    imageBase = oh.ImageBase;
                    break;
                default:
                {
                    var oh = f.pe.OptionalHeader.type();
                    return (0L, null, fmt.Errorf("pe file format not recognized"));
                    break;
                }
            }
            var sect = f.pe.Section(".text");
            if (sect == null)
            {
                return (0L, null, fmt.Errorf("text section not found"));
            }
            textStart = imageBase + uint64(sect.VirtualAddress);
            text, err = sect.Data();
            return;
        }

        private static (ref pe.Symbol, error) findPESymbol(ref pe.File f, @string name)
        {
            foreach (var (_, s) in f.Symbols)
            {
                if (s.Name != name)
                {
                    continue;
                }
                if (s.SectionNumber <= 0L)
                {
                    return (null, fmt.Errorf("symbol %s: invalid section number %d", name, s.SectionNumber));
                }
                if (len(f.Sections) < int(s.SectionNumber))
                {
                    return (null, fmt.Errorf("symbol %s: section number %d is larger than max %d", name, s.SectionNumber, len(f.Sections)));
                }
                return (s, null);
            }
            return (null, fmt.Errorf("no %s symbol found", name));
        }

        private static (slice<byte>, error) loadPETable(ref pe.File f, @string sname, @string ename)
        {
            var (ssym, err) = findPESymbol(f, sname);
            if (err != null)
            {
                return (null, err);
            }
            var (esym, err) = findPESymbol(f, ename);
            if (err != null)
            {
                return (null, err);
            }
            if (ssym.SectionNumber != esym.SectionNumber)
            {
                return (null, fmt.Errorf("%s and %s symbols must be in the same section", sname, ename));
            }
            var sect = f.Sections[ssym.SectionNumber - 1L];
            var (data, err) = sect.Data();
            if (err != null)
            {
                return (null, err);
            }
            return (data[ssym.Value..esym.Value], null);
        }

        private static @string goarch(this ref peFile f)
        { 
            // Not sure how to get the info we want from PE header.
            // Look in symbol table for telltale rt0 symbol.
            {
                var (_, err) = findPESymbol(f.pe, "_rt0_386_windows");

                if (err == null)
                {
                    return "386";
                }

            }
            {
                (_, err) = findPESymbol(f.pe, "_rt0_amd64_windows");

                if (err == null)
                {
                    return "amd64";
                }

            }
            return "";
        }

        private static (ulong, error) loadAddress(this ref peFile f)
        {
            return (0L, fmt.Errorf("unknown load address"));
        }

        private static (ref dwarf.Data, error) dwarf(this ref peFile f)
        {
            return f.pe.DWARF();
        }
    }
}}}
