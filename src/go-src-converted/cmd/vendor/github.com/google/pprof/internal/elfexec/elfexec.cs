// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Package elfexec provides utility routines to examine ELF binaries.
// package elfexec -- go2cs converted at 2020 October 08 04:43:09 UTC
// import "cmd/vendor/github.com/google/pprof/internal/elfexec" ==> using elfexec = go.cmd.vendor.github.com.google.pprof.@internal.elfexec_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\elfexec\elfexec.go
using bufio = go.bufio_package;
using elf = go.debug.elf_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class elfexec_package
    {
        private static readonly long maxNoteSize = (long)1L << (int)(20L); // in bytes
        private static readonly long noteTypeGNUBuildID = (long)3L;


        // elfNote is the payload of a Note Section in an ELF file.
        private partial struct elfNote
        {
            public @string Name; // Contents of the "name" field, omitting the trailing zero byte.
            public slice<byte> Desc; // Contents of the "desc" field.
            public uint Type; // Contents of the "type" field.
        }

        // parseNotes returns the notes from a SHT_NOTE section or PT_NOTE segment.
        private static (slice<elfNote>, error) parseNotes(io.Reader reader, long alignment, binary.ByteOrder order)
        {
            slice<elfNote> _p0 = default;
            error _p0 = default!;

            var r = bufio.NewReader(reader); 

            // padding returns the number of bytes required to pad the given size to an
            // alignment boundary.
            Func<long, long> padding = size =>
            {
                return ((size + (alignment - 1L)) & ~(alignment - 1L)) - size;
            }
;

            slice<elfNote> notes = default;
            while (true)
            {
                var noteHeader = make_slice<byte>(12L); // 3 4-byte words
                {
                    var (_, err) = io.ReadFull(r, noteHeader);

                    if (err == io.EOF)
                    {
                        break;
                    }
                    else if (err != null)
                    {
                        return (null, error.As(err)!);
                    }


                }

                var namesz = order.Uint32(noteHeader[0L..4L]);
                var descsz = order.Uint32(noteHeader[4L..8L]);
                var typ = order.Uint32(noteHeader[8L..12L]);

                if (uint64(namesz) > uint64(maxNoteSize))
                {
                    return (null, error.As(fmt.Errorf("note name too long (%d bytes)", namesz))!);
                }

                @string name = default;
                if (namesz > 0L)
                { 
                    // Documentation differs as to whether namesz is meant to include the
                    // trailing zero, but everyone agrees that name is null-terminated.
                    // So we'll just determine the actual length after the fact.
                    error err = default!;
                    name, err = r.ReadString('\x00');
                    if (err == io.EOF)
                    {
                        return (null, error.As(fmt.Errorf("missing note name (want %d bytes)", namesz))!);
                    }
                    else if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    namesz = uint32(len(name));
                    name = name[..len(name) - 1L];

                } 

                // Drop padding bytes until the desc field.
                {
                    var n__prev2 = n;

                    for (var n = padding(len(noteHeader) + int(namesz)); n > 0L; n--)
                    {
                        {
                            (_, err) = r.ReadByte();

                            if (err == io.EOF)
                            {
                                return (null, error.As(fmt.Errorf("missing %d bytes of padding after note name", n))!);
                            }
                            else if (err != null)
                            {
                                return (null, error.As(err)!);
                            }


                        }

                    }


                    n = n__prev2;
                }

                if (uint64(descsz) > uint64(maxNoteSize))
                {
                    return (null, error.As(fmt.Errorf("note desc too long (%d bytes)", descsz))!);
                }

                var desc = make_slice<byte>(int(descsz));
                {
                    (_, err) = io.ReadFull(r, desc);

                    if (err == io.EOF)
                    {
                        return (null, error.As(fmt.Errorf("missing desc (want %d bytes)", len(desc)))!);
                    }
                    else if (err != null)
                    {
                        return (null, error.As(err)!);
                    }


                }


                notes = append(notes, new elfNote(Name:name,Desc:desc,Type:typ)); 

                // Drop padding bytes until the next note or the end of the section,
                // whichever comes first.
                {
                    var n__prev2 = n;

                    for (n = padding(len(desc)); n > 0L; n--)
                    {
                        {
                            (_, err) = r.ReadByte();

                            if (err == io.EOF)
                            { 
                                // We hit the end of the section before an alignment boundary.
                                // This can happen if this section is at the end of the file or the next
                                // section has a smaller alignment requirement.
                                break;

                            }
                            else if (err != null)
                            {
                                return (null, error.As(err)!);
                            }


                        }

                    }


                    n = n__prev2;
                }

            }

            return (notes, error.As(null!)!);

        }

        // GetBuildID returns the GNU build-ID for an ELF binary.
        //
        // If no build-ID was found but the binary was read without error, it returns
        // (nil, nil).
        public static (slice<byte>, error) GetBuildID(io.ReaderAt binary)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (f, err) = elf.NewFile(binary);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            Func<slice<elfNote>, (slice<byte>, error)> findBuildID = notes =>
            {
                slice<byte> buildID = default;
                foreach (var (_, note) in notes)
                {
                    if (note.Name == "GNU" && note.Type == noteTypeGNUBuildID)
                    {
                        if (buildID == null)
                        {
                            buildID = note.Desc;
                        }
                        else
                        {
                            return (null, error.As(fmt.Errorf("multiple build ids found, don't know which to use"))!);
                        }

                    }

                }
                return (buildID, error.As(null!)!);

            }
;

            foreach (var (_, p) in f.Progs)
            {
                if (p.Type != elf.PT_NOTE)
                {
                    continue;
                }

                var (notes, err) = parseNotes(p.Open(), int(p.Align), f.ByteOrder);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                {
                    var b__prev1 = b;

                    var (b, err) = findBuildID(notes);

                    if (b != null || err != null)
                    {
                        return (b, error.As(err)!);
                    }

                    b = b__prev1;

                }

            }
            foreach (var (_, s) in f.Sections)
            {
                if (s.Type != elf.SHT_NOTE)
                {
                    continue;
                }

                (notes, err) = parseNotes(s.Open(), int(s.Addralign), f.ByteOrder);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                {
                    var b__prev1 = b;

                    (b, err) = findBuildID(notes);

                    if (b != null || err != null)
                    {
                        return (b, error.As(err)!);
                    }

                    b = b__prev1;

                }

            }
            return (null, error.As(null!)!);

        }

        // GetBase determines the base address to subtract from virtual
        // address to get symbol table address. For an executable, the base
        // is 0. Otherwise, it's a shared library, and the base is the
        // address where the mapping starts. The kernel is special, and may
        // use the address of the _stext symbol as the mmap start. _stext
        // offset can be obtained with `nm vmlinux | grep _stext`
        public static (ulong, error) GetBase(ptr<elf.FileHeader> _addr_fh, ptr<elf.ProgHeader> _addr_loadSegment, ptr<ulong> _addr_stextOffset, ulong start, ulong limit, ulong offset)
        {
            ulong _p0 = default;
            error _p0 = default!;
            ref elf.FileHeader fh = ref _addr_fh.val;
            ref elf.ProgHeader loadSegment = ref _addr_loadSegment.val;
            ref ulong stextOffset = ref _addr_stextOffset.val;

            const long pageSize = (long)4096L; 
            // PAGE_OFFSET for PowerPC64, see arch/powerpc/Kconfig in the kernel sources.
            const ulong pageOffsetPpc64 = (ulong)0xc000000000000000UL;


            if (start == 0L && offset == 0L && (limit == ~uint64(0L) || limit == 0L))
            { 
                // Some tools may introduce a fake mapping that spans the entire
                // address space. Assume that the address has already been
                // adjusted, so no additional base adjustment is necessary.
                return (0L, error.As(null!)!);

            }


            if (fh.Type == elf.ET_EXEC) 
                if (loadSegment == null)
                { 
                    // Assume fixed-address executable and so no adjustment.
                    return (0L, error.As(null!)!);

                }

                if (stextOffset == null && start > 0L && start < 0x8000000000000000UL)
                { 
                    // A regular user-mode executable. Compute the base offset using same
                    // arithmetics as in ET_DYN case below, see the explanation there.
                    // Ideally, the condition would just be "stextOffset == nil" as that
                    // represents the address of _stext symbol in the vmlinux image. Alas,
                    // the caller may skip reading it from the binary (it's expensive to scan
                    // all the symbols) and so it may be nil even for the kernel executable.
                    // So additionally check that the start is within the user-mode half of
                    // the 64-bit address space.
                    return (start - offset + loadSegment.Off - loadSegment.Vaddr, error.As(null!)!);

                } 
                // Various kernel heuristics and cases follow.
                if (loadSegment.Vaddr == start - offset)
                {
                    return (offset, error.As(null!)!);
                }

                if (start == 0L && limit != 0L)
                { 
                    // ChromeOS remaps its kernel to 0. Nothing else should come
                    // down this path. Empirical values:
                    //       VADDR=0xffffffff80200000
                    // stextOffset=0xffffffff80200198
                    if (stextOffset != null)
                    {
                        return (-stextOffset, error.As(null!)!);
                    }

                    return (-loadSegment.Vaddr, error.As(null!)!);

                }

                if (start >= loadSegment.Vaddr && limit > start && (offset == 0L || offset == pageOffsetPpc64 || offset == start))
                { 
                    // Some kernels look like:
                    //       VADDR=0xffffffff80200000
                    // stextOffset=0xffffffff80200198
                    //       Start=0xffffffff83200000
                    //       Limit=0xffffffff84200000
                    //      Offset=0 (0xc000000000000000 for PowerPC64) (== Start for ASLR kernel)
                    // So the base should be:
                    if (stextOffset != null && (start % pageSize) == (stextOffset % pageSize.val))
                    { 
                        // perf uses the address of _stext as start. Some tools may
                        // adjust for this before calling GetBase, in which case the page
                        // alignment should be different from that of stextOffset.
                        return (start - stextOffset, error.As(null!)!);

                    }

                    return (start - loadSegment.Vaddr, error.As(null!)!);

                }
                else if (start % pageSize != 0L && stextOffset != null && stextOffset % pageSize == start % pageSize.val)
                { 
                    // ChromeOS remaps its kernel to 0 + start%pageSize. Nothing
                    // else should come down this path. Empirical values:
                    //       start=0x198 limit=0x2f9fffff offset=0
                    //       VADDR=0xffffffff81000000
                    // stextOffset=0xffffffff81000198
                    return (start - stextOffset, error.As(null!)!);

                }

                return (0L, error.As(fmt.Errorf("don't know how to handle EXEC segment: %v start=0x%x limit=0x%x offset=0x%x", loadSegment, start, limit, offset))!);
            else if (fh.Type == elf.ET_REL) 
                if (offset != 0L)
                {
                    return (0L, error.As(fmt.Errorf("don't know how to handle mapping.Offset"))!);
                }

                return (start, error.As(null!)!);
            else if (fh.Type == elf.ET_DYN) 
                // The process mapping information, start = start of virtual address range,
                // and offset = offset in the executable file of the start address, tells us
                // that a runtime virtual address x maps to a file offset
                // fx = x - start + offset.
                if (loadSegment == null)
                {
                    return (start - offset, error.As(null!)!);
                } 
                // The program header, if not nil, indicates the offset in the file where
                // the executable segment is located (loadSegment.Off), and the base virtual
                // address where the first byte of the segment is loaded
                // (loadSegment.Vaddr). A file offset fx maps to a virtual (symbol) address
                // sx = fx - loadSegment.Off + loadSegment.Vaddr.
                //
                // Thus, a runtime virtual address x maps to a symbol address
                // sx = x - start + offset - loadSegment.Off + loadSegment.Vaddr.
                return (start - offset + loadSegment.Off - loadSegment.Vaddr, error.As(null!)!);
                        return (0L, error.As(fmt.Errorf("don't know how to handle FileHeader.Type %v", fh.Type))!);

        }

        // FindTextProgHeader finds the program segment header containing the .text
        // section or nil if the segment cannot be found.
        public static ptr<elf.ProgHeader> FindTextProgHeader(ptr<elf.File> _addr_f)
        {
            ref elf.File f = ref _addr_f.val;

            foreach (var (_, s) in f.Sections)
            {
                if (s.Name == ".text")
                { 
                    // Find the LOAD segment containing the .text section.
                    foreach (var (_, p) in f.Progs)
                    {
                        if (p.Type == elf.PT_LOAD && p.Flags & elf.PF_X != 0L && s.Addr >= p.Vaddr && s.Addr < p.Vaddr + p.Memsz)
                        {
                            return _addr__addr_p.ProgHeader!;
                        }

                    }

                }

            }
            return _addr_null!;

        }
    }
}}}}}}}
