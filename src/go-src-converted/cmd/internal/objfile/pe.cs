// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of PE executables (Microsoft Windows).

// package objfile -- go2cs converted at 2020 October 09 05:09:00 UTC
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
            rawFile _p0 = default;
            error _p0 = default!;

            var (f, err) = pe.NewFile(r);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (addr(new peFile(f)), error.As(null!)!);

        }

        private static (slice<Sym>, error) symbols(this ptr<peFile> _addr_f)
        {
            slice<Sym> _p0 = default;
            error _p0 = default!;
            ref peFile f = ref _addr_f.val;
 
            // Build sorted list of addresses of all symbols.
            // We infer the size of a symbol by looking at where the next symbol begins.
            slice<ulong> addrs = default;

            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ptr<pe.OptionalHeader32> oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ptr<pe.OptionalHeader64> oh:
                    imageBase = oh.ImageBase;
                    break;

            }

            slice<Sym> syms = default;
            foreach (var (_, s) in f.pe.Symbols)
            {
                const long N_UNDEF = (long)0L; // An undefined (extern) symbol
                const long N_ABS = (long)-1L; // An absolute symbol (e_value is a constant, not an address)
                const long N_DEBUG = (long)-2L; // A debugging symbol
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
                        return (null, error.As(fmt.Errorf("invalid section number in symbol table"))!);
                    }

                    var sect = f.pe.Sections[s.SectionNumber - 1L];
                    const ulong text = (ulong)0x20UL;
                    const ulong data = (ulong)0x40UL;
                    const ulong bss = (ulong)0x80UL;
                    const ulong permW = (ulong)0x80000000UL;

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
            return (syms, error.As(null!)!);

        }

        private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<peFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> symtab = default;
            slice<byte> pclntab = default;
            error err = default!;
            ref peFile f = ref _addr_f.val;

            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ptr<pe.OptionalHeader32> oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ptr<pe.OptionalHeader64> oh:
                    imageBase = oh.ImageBase;
                    break;
                default:
                {
                    var oh = f.pe.OptionalHeader.type();
                    return (0L, null, null, error.As(fmt.Errorf("pe file format not recognized"))!);
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

            pclntab, err = loadPETable(_addr_f.pe, "runtime.pclntab", "runtime.epclntab");

            if (err != null)
            { 
                // We didn't find the symbols, so look for the names used in 1.3 and earlier.
                // TODO: Remove code looking for the old symbols when we no longer care about 1.3.
                error err2 = default!;
                pclntab, err2 = loadPETable(_addr_f.pe, "pclntab", "epclntab");

                if (err2 != null)
                {
                    return (0L, null, null, error.As(err)!);
                }

            }

            symtab, err = loadPETable(_addr_f.pe, "runtime.symtab", "runtime.esymtab");

            if (err != null)
            { 
                // Same as above.
                err2 = default!;
                symtab, err2 = loadPETable(_addr_f.pe, "symtab", "esymtab");

                if (err2 != null)
                {
                    return (0L, null, null, error.As(err)!);
                }

            }

            return (textStart, symtab, pclntab, error.As(null!)!);

        }

        private static (ulong, slice<byte>, error) text(this ptr<peFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> text = default;
            error err = default!;
            ref peFile f = ref _addr_f.val;

            ulong imageBase = default;
            switch (f.pe.OptionalHeader.type())
            {
                case ptr<pe.OptionalHeader32> oh:
                    imageBase = uint64(oh.ImageBase);
                    break;
                case ptr<pe.OptionalHeader64> oh:
                    imageBase = oh.ImageBase;
                    break;
                default:
                {
                    var oh = f.pe.OptionalHeader.type();
                    return (0L, null, error.As(fmt.Errorf("pe file format not recognized"))!);
                    break;
                }
            }
            var sect = f.pe.Section(".text");
            if (sect == null)
            {
                return (0L, null, error.As(fmt.Errorf("text section not found"))!);
            }

            textStart = imageBase + uint64(sect.VirtualAddress);
            text, err = sect.Data();
            return ;

        }

        private static (ptr<pe.Symbol>, error) findPESymbol(ptr<pe.File> _addr_f, @string name)
        {
            ptr<pe.Symbol> _p0 = default!;
            error _p0 = default!;
            ref pe.File f = ref _addr_f.val;

            foreach (var (_, s) in f.Symbols)
            {
                if (s.Name != name)
                {
                    continue;
                }

                if (s.SectionNumber <= 0L)
                {
                    return (_addr_null!, error.As(fmt.Errorf("symbol %s: invalid section number %d", name, s.SectionNumber))!);
                }

                if (len(f.Sections) < int(s.SectionNumber))
                {
                    return (_addr_null!, error.As(fmt.Errorf("symbol %s: section number %d is larger than max %d", name, s.SectionNumber, len(f.Sections)))!);
                }

                return (_addr_s!, error.As(null!)!);

            }
            return (_addr_null!, error.As(fmt.Errorf("no %s symbol found", name))!);

        }

        private static (slice<byte>, error) loadPETable(ptr<pe.File> _addr_f, @string sname, @string ename)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref pe.File f = ref _addr_f.val;

            var (ssym, err) = findPESymbol(_addr_f, sname);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (esym, err) = findPESymbol(_addr_f, ename);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (ssym.SectionNumber != esym.SectionNumber)
            {
                return (null, error.As(fmt.Errorf("%s and %s symbols must be in the same section", sname, ename))!);
            }

            var sect = f.Sections[ssym.SectionNumber - 1L];
            var (data, err) = sect.Data();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (data[ssym.Value..esym.Value], error.As(null!)!);

        }

        private static @string goarch(this ptr<peFile> _addr_f)
        {
            ref peFile f = ref _addr_f.val;
 
            // Not sure how to get the info we want from PE header.
            // Look in symbol table for telltale rt0 symbol.
            {
                var (_, err) = findPESymbol(_addr_f.pe, "_rt0_386_windows");

                if (err == null)
                {
                    return "386";
                }

            }

            {
                (_, err) = findPESymbol(_addr_f.pe, "_rt0_amd64_windows");

                if (err == null)
                {
                    return "amd64";
                }

            }

            {
                (_, err) = findPESymbol(_addr_f.pe, "_rt0_arm_windows");

                if (err == null)
                {
                    return "arm";
                }

            }

            return "";

        }

        private static (ulong, error) loadAddress(this ptr<peFile> _addr_f)
        {
            ulong _p0 = default;
            error _p0 = default!;
            ref peFile f = ref _addr_f.val;

            return (0L, error.As(fmt.Errorf("unknown load address"))!);
        }

        private static (ptr<dwarf.Data>, error) dwarf(this ptr<peFile> _addr_f)
        {
            ptr<dwarf.Data> _p0 = default!;
            error _p0 = default!;
            ref peFile f = ref _addr_f.val;

            return _addr_f.pe.DWARF()!;
        }
    }
}}}
