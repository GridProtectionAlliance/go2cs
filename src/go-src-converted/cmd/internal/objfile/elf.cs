// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parsing of ELF executables (Linux, FreeBSD, and so on).

// package objfile -- go2cs converted at 2020 October 08 03:49:41 UTC
// import "cmd/internal/objfile" ==> using objfile = go.cmd.@internal.objfile_package
// Original source: C:\Go\src\cmd\internal\objfile\elf.go
using dwarf = go.debug.dwarf_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objfile_package
    {
        private partial struct elfFile
        {
            public ptr<elf.File> elf;
        }

        private static (rawFile, error) openElf(io.ReaderAt r)
        {
            rawFile _p0 = default;
            error _p0 = default!;

            var (f, err) = elf.NewFile(r);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (addr(new elfFile(f)), error.As(null!)!);

        }

        private static (slice<Sym>, error) symbols(this ptr<elfFile> _addr_f)
        {
            slice<Sym> _p0 = default;
            error _p0 = default!;
            ref elfFile f = ref _addr_f.val;

            var (elfSyms, err) = f.elf.Symbols();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            slice<Sym> syms = default;
            foreach (var (_, s) in elfSyms)
            {
                Sym sym = new Sym(Addr:s.Value,Name:s.Name,Size:int64(s.Size),Code:'?');

                if (s.Section == elf.SHN_UNDEF) 
                    sym.Code = 'U';
                else if (s.Section == elf.SHN_COMMON) 
                    sym.Code = 'B';
                else 
                    var i = int(s.Section);
                    if (i < 0L || i >= len(f.elf.Sections))
                    {
                        break;
                    }

                    var sect = f.elf.Sections[i];

                    if (sect.Flags & (elf.SHF_WRITE | elf.SHF_ALLOC | elf.SHF_EXECINSTR) == elf.SHF_ALLOC | elf.SHF_EXECINSTR) 
                        sym.Code = 'T';
                    else if (sect.Flags & (elf.SHF_WRITE | elf.SHF_ALLOC | elf.SHF_EXECINSTR) == elf.SHF_ALLOC) 
                        sym.Code = 'R';
                    else if (sect.Flags & (elf.SHF_WRITE | elf.SHF_ALLOC | elf.SHF_EXECINSTR) == elf.SHF_ALLOC | elf.SHF_WRITE) 
                        sym.Code = 'D';
                                                    if (elf.ST_BIND(s.Info) == elf.STB_LOCAL)
                {
                    sym.Code += 'a' - 'A';
                }

                syms = append(syms, sym);

            }
            return (syms, error.As(null!)!);

        }

        private static (ulong, slice<byte>, slice<byte>, error) pcln(this ptr<elfFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> symtab = default;
            slice<byte> pclntab = default;
            error err = default!;
            ref elfFile f = ref _addr_f.val;

            {
                var sect__prev1 = sect;

                var sect = f.elf.Section(".text");

                if (sect != null)
                {
                    textStart = sect.Addr;
                }

                sect = sect__prev1;

            }

            {
                var sect__prev1 = sect;

                sect = f.elf.Section(".gosymtab");

                if (sect != null)
                {
                    symtab, err = sect.Data();

                    if (err != null)
                    {
                        return (0L, null, null, error.As(err)!);
                    }

                }

                sect = sect__prev1;

            }

            {
                var sect__prev1 = sect;

                sect = f.elf.Section(".gopclntab");

                if (sect != null)
                {
                    pclntab, err = sect.Data();

                    if (err != null)
                    {
                        return (0L, null, null, error.As(err)!);
                    }

                }

                sect = sect__prev1;

            }

            return (textStart, symtab, pclntab, error.As(null!)!);

        }

        private static (ulong, slice<byte>, error) text(this ptr<elfFile> _addr_f)
        {
            ulong textStart = default;
            slice<byte> text = default;
            error err = default!;
            ref elfFile f = ref _addr_f.val;

            var sect = f.elf.Section(".text");
            if (sect == null)
            {
                return (0L, null, error.As(fmt.Errorf("text section not found"))!);
            }

            textStart = sect.Addr;
            text, err = sect.Data();
            return ;

        }

        private static @string goarch(this ptr<elfFile> _addr_f)
        {
            ref elfFile f = ref _addr_f.val;


            if (f.elf.Machine == elf.EM_386) 
                return "386";
            else if (f.elf.Machine == elf.EM_X86_64) 
                return "amd64";
            else if (f.elf.Machine == elf.EM_ARM) 
                return "arm";
            else if (f.elf.Machine == elf.EM_AARCH64) 
                return "arm64";
            else if (f.elf.Machine == elf.EM_PPC64) 
                if (f.elf.ByteOrder == binary.LittleEndian)
                {
                    return "ppc64le";
                }

                return "ppc64";
            else if (f.elf.Machine == elf.EM_S390) 
                return "s390x";
                        return "";

        }

        private static (ulong, error) loadAddress(this ptr<elfFile> _addr_f)
        {
            ulong _p0 = default;
            error _p0 = default!;
            ref elfFile f = ref _addr_f.val;

            foreach (var (_, p) in f.elf.Progs)
            {
                if (p.Type == elf.PT_LOAD && p.Flags & elf.PF_X != 0L)
                {
                    return (p.Vaddr, error.As(null!)!);
                }

            }
            return (0L, error.As(fmt.Errorf("unknown load address"))!);

        }

        private static (ptr<dwarf.Data>, error) dwarf(this ptr<elfFile> _addr_f)
        {
            ptr<dwarf.Data> _p0 = default!;
            error _p0 = default!;
            ref elfFile f = ref _addr_f.val;

            return _addr_f.elf.DWARF()!;
        }
    }
}}}
