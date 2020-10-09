// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package version -- go2cs converted at 2020 October 09 05:45:23 UTC
// import "cmd/go/internal/version" ==> using version = go.cmd.go.@internal.version_package
// Original source: C:\Go\src\cmd\go\internal\version\exe.go
using bytes = go.bytes_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class version_package
    {
        // An exe is a generic interface to an OS executable (ELF, Mach-O, PE, XCOFF).
        private partial interface exe
        {
            ulong Close(); // ReadData reads and returns up to size byte starting at virtual address addr.
            ulong ReadData(ulong addr, ulong size); // DataStart returns the writable data segment start address.
            ulong DataStart();
        }

        // openExe opens file and returns it as an exe.
        private static (exe, error) openExe(@string file)
        {
            exe _p0 = default;
            error _p0 = default!;

            var (f, err) = os.Open(file);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var data = make_slice<byte>(16L);
            {
                var (_, err) = io.ReadFull(f, data);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            f.Seek(0L, 0L);
            if (bytes.HasPrefix(data, (slice<byte>)"\x7FELF"))
            {
                var (e, err) = elf.NewFile(f);
                if (err != null)
                {
                    f.Close();
                    return (null, error.As(err)!);
                }

                return (addr(new elfExe(f,e)), error.As(null!)!);

            }

            if (bytes.HasPrefix(data, (slice<byte>)"MZ"))
            {
                (e, err) = pe.NewFile(f);
                if (err != null)
                {
                    f.Close();
                    return (null, error.As(err)!);
                }

                return (addr(new peExe(f,e)), error.As(null!)!);

            }

            if (bytes.HasPrefix(data, (slice<byte>)"\xFE\xED\xFA") || bytes.HasPrefix(data[1L..], (slice<byte>)"\xFA\xED\xFE"))
            {
                (e, err) = macho.NewFile(f);
                if (err != null)
                {
                    f.Close();
                    return (null, error.As(err)!);
                }

                return (addr(new machoExe(f,e)), error.As(null!)!);

            }

            if (bytes.HasPrefix(data, new slice<byte>(new byte[] { 0x01, 0xDF })) || bytes.HasPrefix(data, new slice<byte>(new byte[] { 0x01, 0xF7 })))
            {
                (e, err) = xcoff.NewFile(f);
                if (err != null)
                {
                    f.Close();
                    return (null, error.As(err)!);
                }

                return (addr(new xcoffExe(f,e)), error.As(null!)!);


            }

            return (null, error.As(fmt.Errorf("unrecognized executable format"))!);

        }

        // elfExe is the ELF implementation of the exe interface.
        private partial struct elfExe
        {
            public ptr<os.File> os;
            public ptr<elf.File> f;
        }

        private static error Close(this ptr<elfExe> _addr_x)
        {
            ref elfExe x = ref _addr_x.val;

            return error.As(x.os.Close())!;
        }

        private static (slice<byte>, error) ReadData(this ptr<elfExe> _addr_x, ulong addr, ulong size)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref elfExe x = ref _addr_x.val;

            foreach (var (_, prog) in x.f.Progs)
            {
                if (prog.Vaddr <= addr && addr <= prog.Vaddr + prog.Filesz - 1L)
                {
                    var n = prog.Vaddr + prog.Filesz - addr;
                    if (n > size)
                    {
                        n = size;
                    }

                    var data = make_slice<byte>(n);
                    var (_, err) = prog.ReadAt(data, int64(addr - prog.Vaddr));
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    return (data, error.As(null!)!);

                }

            }
            return (null, error.As(fmt.Errorf("address not mapped"))!);

        }

        private static ulong DataStart(this ptr<elfExe> _addr_x)
        {
            ref elfExe x = ref _addr_x.val;

            foreach (var (_, s) in x.f.Sections)
            {
                if (s.Name == ".go.buildinfo")
                {
                    return s.Addr;
                }

            }
            foreach (var (_, p) in x.f.Progs)
            {
                if (p.Type == elf.PT_LOAD && p.Flags & (elf.PF_X | elf.PF_W) == elf.PF_W)
                {
                    return p.Vaddr;
                }

            }
            return 0L;

        }

        // peExe is the PE (Windows Portable Executable) implementation of the exe interface.
        private partial struct peExe
        {
            public ptr<os.File> os;
            public ptr<pe.File> f;
        }

        private static error Close(this ptr<peExe> _addr_x)
        {
            ref peExe x = ref _addr_x.val;

            return error.As(x.os.Close())!;
        }

        private static ulong imageBase(this ptr<peExe> _addr_x)
        {
            ref peExe x = ref _addr_x.val;

            switch (x.f.OptionalHeader.type())
            {
                case ptr<pe.OptionalHeader32> oh:
                    return uint64(oh.ImageBase);
                    break;
                case ptr<pe.OptionalHeader64> oh:
                    return oh.ImageBase;
                    break;
            }
            return 0L;

        }

        private static (slice<byte>, error) ReadData(this ptr<peExe> _addr_x, ulong addr, ulong size)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref peExe x = ref _addr_x.val;

            addr -= x.imageBase();
            foreach (var (_, sect) in x.f.Sections)
            {
                if (uint64(sect.VirtualAddress) <= addr && addr <= uint64(sect.VirtualAddress + sect.Size - 1L))
                {
                    var n = uint64(sect.VirtualAddress + sect.Size) - addr;
                    if (n > size)
                    {
                        n = size;
                    }

                    var data = make_slice<byte>(n);
                    var (_, err) = sect.ReadAt(data, int64(addr - uint64(sect.VirtualAddress)));
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    return (data, error.As(null!)!);

                }

            }
            return (null, error.As(fmt.Errorf("address not mapped"))!);

        }

        private static ulong DataStart(this ptr<peExe> _addr_x)
        {
            ref peExe x = ref _addr_x.val;
 
            // Assume data is first writable section.
            const ulong IMAGE_SCN_CNT_CODE = (ulong)0x00000020UL;
            const ulong IMAGE_SCN_CNT_INITIALIZED_DATA = (ulong)0x00000040UL;
            const ulong IMAGE_SCN_CNT_UNINITIALIZED_DATA = (ulong)0x00000080UL;
            const ulong IMAGE_SCN_MEM_EXECUTE = (ulong)0x20000000UL;
            const ulong IMAGE_SCN_MEM_READ = (ulong)0x40000000UL;
            const ulong IMAGE_SCN_MEM_WRITE = (ulong)0x80000000UL;
            const ulong IMAGE_SCN_MEM_DISCARDABLE = (ulong)0x2000000UL;
            const ulong IMAGE_SCN_LNK_NRELOC_OVFL = (ulong)0x1000000UL;
            const ulong IMAGE_SCN_ALIGN_32BYTES = (ulong)0x600000UL;

            foreach (var (_, sect) in x.f.Sections)
            {
                if (sect.VirtualAddress != 0L && sect.Size != 0L && sect.Characteristics & ~IMAGE_SCN_ALIGN_32BYTES == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE)
                {
                    return uint64(sect.VirtualAddress) + x.imageBase();
                }

            }
            return 0L;

        }

        // machoExe is the Mach-O (Apple macOS/iOS) implementation of the exe interface.
        private partial struct machoExe
        {
            public ptr<os.File> os;
            public ptr<macho.File> f;
        }

        private static error Close(this ptr<machoExe> _addr_x)
        {
            ref machoExe x = ref _addr_x.val;

            return error.As(x.os.Close())!;
        }

        private static (slice<byte>, error) ReadData(this ptr<machoExe> _addr_x, ulong addr, ulong size)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref machoExe x = ref _addr_x.val;

            foreach (var (_, load) in x.f.Loads)
            {
                ptr<macho.Segment> (seg, ok) = load._<ptr<macho.Segment>>();
                if (!ok)
                {
                    continue;
                }

                if (seg.Addr <= addr && addr <= seg.Addr + seg.Filesz - 1L)
                {
                    if (seg.Name == "__PAGEZERO")
                    {
                        continue;
                    }

                    var n = seg.Addr + seg.Filesz - addr;
                    if (n > size)
                    {
                        n = size;
                    }

                    var data = make_slice<byte>(n);
                    var (_, err) = seg.ReadAt(data, int64(addr - seg.Addr));
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    return (data, error.As(null!)!);

                }

            }
            return (null, error.As(fmt.Errorf("address not mapped"))!);

        }

        private static ulong DataStart(this ptr<machoExe> _addr_x)
        {
            ref machoExe x = ref _addr_x.val;
 
            // Look for section named "__go_buildinfo".
            foreach (var (_, sec) in x.f.Sections)
            {
                if (sec.Name == "__go_buildinfo")
                {
                    return sec.Addr;
                }

            } 
            // Try the first non-empty writable segment.
            const long RW = (long)3L;

            foreach (var (_, load) in x.f.Loads)
            {
                ptr<macho.Segment> (seg, ok) = load._<ptr<macho.Segment>>();
                if (ok && seg.Addr != 0L && seg.Filesz != 0L && seg.Prot == RW && seg.Maxprot == RW)
                {
                    return seg.Addr;
                }

            }
            return 0L;

        }

        // xcoffExe is the XCOFF (AIX eXtended COFF) implementation of the exe interface.
        private partial struct xcoffExe
        {
            public ptr<os.File> os;
            public ptr<xcoff.File> f;
        }

        private static error Close(this ptr<xcoffExe> _addr_x)
        {
            ref xcoffExe x = ref _addr_x.val;

            return error.As(x.os.Close())!;
        }

        private static (slice<byte>, error) ReadData(this ptr<xcoffExe> _addr_x, ulong addr, ulong size)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref xcoffExe x = ref _addr_x.val;

            foreach (var (_, sect) in x.f.Sections)
            {
                if (uint64(sect.VirtualAddress) <= addr && addr <= uint64(sect.VirtualAddress + sect.Size - 1L))
                {
                    var n = uint64(sect.VirtualAddress + sect.Size) - addr;
                    if (n > size)
                    {
                        n = size;
                    }

                    var data = make_slice<byte>(n);
                    var (_, err) = sect.ReadAt(data, int64(addr - uint64(sect.VirtualAddress)));
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    return (data, error.As(null!)!);

                }

            }
            return (null, error.As(fmt.Errorf("address not mapped"))!);

        }

        private static ulong DataStart(this ptr<xcoffExe> _addr_x)
        {
            ref xcoffExe x = ref _addr_x.val;

            return x.f.SectionByType(xcoff.STYP_DATA).VirtualAddress;
        }
    }
}}}}
