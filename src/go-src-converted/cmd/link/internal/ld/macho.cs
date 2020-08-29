// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:04:16 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\macho.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        public partial struct MachoHdr
        {
            public uint cpu;
            public uint subcpu;
        }

        public partial struct MachoSect
        {
            public @string name;
            public @string segname;
            public ulong addr;
            public ulong size;
            public uint off;
            public uint align;
            public uint reloc;
            public uint nreloc;
            public uint flag;
            public uint res1;
            public uint res2;
        }

        public partial struct MachoSeg
        {
            public @string name;
            public ulong vsize;
            public ulong vaddr;
            public ulong fileoffset;
            public ulong filesize;
            public uint prot1;
            public uint prot2;
            public uint nsect;
            public uint msect;
            public slice<MachoSect> sect;
            public uint flag;
        }

        public partial struct MachoLoad
        {
            public uint type_;
            public slice<uint> data;
        }

        /*
         * Total amount of space to reserve at the start of the file
         * for Header, PHeaders, and SHeaders.
         * May waste some.
         */
        public static readonly long INITIAL_MACHO_HEADR = 4L * 1024L;

        public static readonly long MACHO_CPU_AMD64 = 1L << (int)(24L) | 7L;
        public static readonly long MACHO_CPU_386 = 7L;
        public static readonly long MACHO_SUBCPU_X86 = 3L;
        public static readonly long MACHO_CPU_ARM = 12L;
        public static readonly long MACHO_SUBCPU_ARM = 0L;
        public static readonly long MACHO_SUBCPU_ARMV7 = 9L;
        public static readonly long MACHO_CPU_ARM64 = 1L << (int)(24L) | 12L;
        public static readonly long MACHO_SUBCPU_ARM64_ALL = 0L;
        public static readonly long MACHO32SYMSIZE = 12L;
        public static readonly long MACHO64SYMSIZE = 16L;
        public static readonly long MACHO_X86_64_RELOC_UNSIGNED = 0L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED = 1L;
        public static readonly long MACHO_X86_64_RELOC_BRANCH = 2L;
        public static readonly long MACHO_X86_64_RELOC_GOT_LOAD = 3L;
        public static readonly long MACHO_X86_64_RELOC_GOT = 4L;
        public static readonly long MACHO_X86_64_RELOC_SUBTRACTOR = 5L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_1 = 6L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_2 = 7L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_4 = 8L;
        public static readonly long MACHO_ARM_RELOC_VANILLA = 0L;
        public static readonly long MACHO_ARM_RELOC_PAIR = 1L;
        public static readonly long MACHO_ARM_RELOC_SECTDIFF = 2L;
        public static readonly long MACHO_ARM_RELOC_BR24 = 5L;
        public static readonly long MACHO_ARM64_RELOC_UNSIGNED = 0L;
        public static readonly long MACHO_ARM64_RELOC_BRANCH26 = 2L;
        public static readonly long MACHO_ARM64_RELOC_PAGE21 = 3L;
        public static readonly long MACHO_ARM64_RELOC_PAGEOFF12 = 4L;
        public static readonly long MACHO_ARM64_RELOC_ADDEND = 10L;
        public static readonly long MACHO_GENERIC_RELOC_VANILLA = 0L;
        public static readonly long MACHO_FAKE_GOTPCREL = 100L;

        public static readonly ulong MH_MAGIC = 0xfeedfaceUL;
        public static readonly ulong MH_MAGIC_64 = 0xfeedfacfUL;

        public static readonly ulong MH_OBJECT = 0x1UL;
        public static readonly ulong MH_EXECUTE = 0x2UL;

        public static readonly ulong MH_NOUNDEFS = 0x1UL;

        public static readonly ulong LC_SEGMENT = 0x1UL;
        public static readonly ulong LC_SYMTAB = 0x2UL;
        public static readonly ulong LC_UNIXTHREAD = 0x5UL;
        public static readonly ulong LC_DYSYMTAB = 0xbUL;
        public static readonly ulong LC_LOAD_DYLIB = 0xcUL;
        public static readonly ulong LC_ID_DYLIB = 0xdUL;
        public static readonly ulong LC_LOAD_DYLINKER = 0xeUL;
        public static readonly ulong LC_PREBOUND_DYLIB = 0x10UL;
        public static readonly ulong LC_LOAD_WEAK_DYLIB = 0x18UL;
        public static readonly ulong LC_SEGMENT_64 = 0x19UL;
        public static readonly ulong LC_UUID = 0x1bUL;
        public static readonly ulong LC_RPATH = 0x8000001cUL;
        public static readonly ulong LC_CODE_SIGNATURE = 0x1dUL;
        public static readonly ulong LC_SEGMENT_SPLIT_INFO = 0x1eUL;
        public static readonly ulong LC_REEXPORT_DYLIB = 0x8000001fUL;
        public static readonly ulong LC_ENCRYPTION_INFO = 0x21UL;
        public static readonly ulong LC_DYLD_INFO = 0x22UL;
        public static readonly ulong LC_DYLD_INFO_ONLY = 0x80000022UL;
        public static readonly ulong LC_VERSION_MIN_MACOSX = 0x24UL;
        public static readonly ulong LC_VERSION_MIN_IPHONEOS = 0x25UL;
        public static readonly ulong LC_FUNCTION_STARTS = 0x26UL;
        public static readonly ulong LC_MAIN = 0x80000028UL;
        public static readonly ulong LC_DATA_IN_CODE = 0x29UL;
        public static readonly ulong LC_SOURCE_VERSION = 0x2AUL;
        public static readonly ulong LC_DYLIB_CODE_SIGN_DRS = 0x2BUL;
        public static readonly ulong LC_ENCRYPTION_INFO_64 = 0x2CUL;

        public static readonly ulong S_REGULAR = 0x0UL;
        public static readonly ulong S_ZEROFILL = 0x1UL;
        public static readonly ulong S_NON_LAZY_SYMBOL_POINTERS = 0x6UL;
        public static readonly ulong S_SYMBOL_STUBS = 0x8UL;
        public static readonly ulong S_MOD_INIT_FUNC_POINTERS = 0x9UL;
        public static readonly ulong S_ATTR_PURE_INSTRUCTIONS = 0x80000000UL;
        public static readonly ulong S_ATTR_DEBUG = 0x02000000UL;
        public static readonly ulong S_ATTR_SOME_INSTRUCTIONS = 0x00000400UL;

        // Copyright 2009 The Go Authors. All rights reserved.
        // Use of this source code is governed by a BSD-style
        // license that can be found in the LICENSE file.

        // Mach-O file writing
        // http://developer.apple.com/mac/library/DOCUMENTATION/DeveloperTools/Conceptual/MachORuntime/Reference/reference.html

        private static MachoHdr machohdr = default;

        private static slice<MachoLoad> load = default;

        private static array<MachoSeg> seg = new array<MachoSeg>(16L);

        private static long nseg = default;

        private static long ndebug = default;

        private static long nsect = default;

        public static readonly long SymKindLocal = 0L + iota;
        public static readonly var SymKindExtdef = 0;
        public static readonly var SymKindUndef = 1;
        public static readonly var NumSymKind = 2;

        private static array<long> nkind = new array<long>(NumSymKind);

        private static slice<ref sym.Symbol> sortsym = default;

        private static long nsortsym = default;

        // Amount of space left for adding load commands
        // that refer to dynamic libraries. Because these have
        // to go in the Mach-O header, we can't just pick a
        // "big enough" header size. The initial header is
        // one page, the non-dynamic library stuff takes
        // up about 1300 bytes; we overestimate that as 2k.
        private static var loadBudget = INITIAL_MACHO_HEADR - 2L * 1024L;

        private static ref MachoHdr getMachoHdr()
        {
            return ref machohdr;
        }

        private static ref MachoLoad newMachoLoad(ref sys.Arch arch, uint type_, uint ndata)
        {
            if (arch.PtrSize == 8L && (ndata & 1L != 0L))
            {
                ndata++;
            }
            load = append(load, new MachoLoad());
            var l = ref load[len(load) - 1L];
            l.type_ = type_;
            l.data = make_slice<uint>(ndata);
            return l;
        }

        private static ref MachoSeg newMachoSeg(@string name, long msect)
        {
            if (nseg >= len(seg))
            {
                Exitf("too many segs");
            }
            var s = ref seg[nseg];
            nseg++;
            s.name = name;
            s.msect = uint32(msect);
            s.sect = make_slice<MachoSect>(msect);
            return s;
        }

        private static ref MachoSect newMachoSect(ref MachoSeg seg, @string name, @string segname)
        {
            if (seg.nsect >= seg.msect)
            {
                Exitf("too many sects in segment %s", seg.name);
            }
            var s = ref seg.sect[seg.nsect];
            seg.nsect++;
            s.name = name;
            s.segname = segname;
            nsect++;
            return s;
        }

        // Generic linking code.

        private static slice<@string> dylib = default;

        private static long linkoff = default;

        private static long machowrite(ref sys.Arch arch, ref OutBuf @out, LinkMode linkmode)
        {
            var o1 = @out.Offset();

            long loadsize = 4L * 4L * ndebug;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(load); i++)
                {
                    loadsize += 4L * (len(load[i].data) + 2L);
                }


                i = i__prev1;
            }
            if (arch.PtrSize == 8L)
            {
                loadsize += 18L * 4L * nseg;
                loadsize += 20L * 4L * nsect;
            }
            else
            {
                loadsize += 14L * 4L * nseg;
                loadsize += 17L * 4L * nsect;
            }
            if (arch.PtrSize == 8L)
            {
                @out.Write32(MH_MAGIC_64);
            }
            else
            {
                @out.Write32(MH_MAGIC);
            }
            @out.Write32(machohdr.cpu);
            @out.Write32(machohdr.subcpu);
            if (linkmode == LinkExternal)
            {
                @out.Write32(MH_OBJECT); /* file type - mach object */
            }
            else
            {
                @out.Write32(MH_EXECUTE); /* file type - mach executable */
            }
            @out.Write32(uint32(len(load)) + uint32(nseg) + uint32(ndebug));
            @out.Write32(uint32(loadsize));
            if (nkind[SymKindUndef] == 0L)
            {
                @out.Write32(MH_NOUNDEFS); /* flags - no undefines */
            }
            else
            {
                @out.Write32(0L); /* flags */
            }
            if (arch.PtrSize == 8L)
            {
                @out.Write32(0L); /* reserved */
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < nseg; i++)
                {
                    var s = ref seg[i];
                    if (arch.PtrSize == 8L)
                    {
                        @out.Write32(LC_SEGMENT_64);
                        @out.Write32(72L + 80L * s.nsect);
                        @out.WriteStringN(s.name, 16L);
                        @out.Write64(s.vaddr);
                        @out.Write64(s.vsize);
                        @out.Write64(s.fileoffset);
                        @out.Write64(s.filesize);
                        @out.Write32(s.prot1);
                        @out.Write32(s.prot2);
                        @out.Write32(s.nsect);
                        @out.Write32(s.flag);
                    }
                    else
                    {
                        @out.Write32(LC_SEGMENT);
                        @out.Write32(56L + 68L * s.nsect);
                        @out.WriteStringN(s.name, 16L);
                        @out.Write32(uint32(s.vaddr));
                        @out.Write32(uint32(s.vsize));
                        @out.Write32(uint32(s.fileoffset));
                        @out.Write32(uint32(s.filesize));
                        @out.Write32(s.prot1);
                        @out.Write32(s.prot2);
                        @out.Write32(s.nsect);
                        @out.Write32(s.flag);
                    }
                    {
                        var j__prev2 = j;

                        for (var j = uint32(0L); j < s.nsect; j++)
                        {
                            var t = ref s.sect[j];
                            if (arch.PtrSize == 8L)
                            {
                                @out.WriteStringN(t.name, 16L);
                                @out.WriteStringN(t.segname, 16L);
                                @out.Write64(t.addr);
                                @out.Write64(t.size);
                                @out.Write32(t.off);
                                @out.Write32(t.align);
                                @out.Write32(t.reloc);
                                @out.Write32(t.nreloc);
                                @out.Write32(t.flag);
                                @out.Write32(t.res1); /* reserved */
                                @out.Write32(t.res2); /* reserved */
                                @out.Write32(0L); /* reserved */
                            }
                            else
                            {
                                @out.WriteStringN(t.name, 16L);
                                @out.WriteStringN(t.segname, 16L);
                                @out.Write32(uint32(t.addr));
                                @out.Write32(uint32(t.size));
                                @out.Write32(t.off);
                                @out.Write32(t.align);
                                @out.Write32(t.reloc);
                                @out.Write32(t.nreloc);
                                @out.Write32(t.flag);
                                @out.Write32(t.res1); /* reserved */
                                @out.Write32(t.res2); /* reserved */
                            }
                        }


                        j = j__prev2;
                    }
                }


                i = i__prev1;
            }

            {
                long i__prev1 = i;

                for (i = 0L; i < len(load); i++)
                {
                    var l = ref load[i];
                    @out.Write32(l.type_);
                    @out.Write32(4L * (uint32(len(l.data)) + 2L));
                    {
                        var j__prev2 = j;

                        for (j = 0L; j < len(l.data); j++)
                        {
                            @out.Write32(l.data[j]);
                        }


                        j = j__prev2;
                    }
                }


                i = i__prev1;
            }

            return int(@out.Offset() - o1);
        }

        private static void domacho(this ref Link ctxt)
        {
            if (FlagD.Value)
            {
                return;
            } 

            // empirically, string table must begin with " \x00".
            var s = ctxt.Syms.Lookup(".machosymstr", 0L);

            s.Type = sym.SMACHOSYMSTR;
            s.Attr |= sym.AttrReachable;
            s.AddUint8(' ');
            s.AddUint8('\x00');

            s = ctxt.Syms.Lookup(".machosymtab", 0L);
            s.Type = sym.SMACHOSYMTAB;
            s.Attr |= sym.AttrReachable;

            if (ctxt.LinkMode != LinkExternal)
            {
                s = ctxt.Syms.Lookup(".plt", 0L); // will be __symbol_stub
                s.Type = sym.SMACHOPLT;
                s.Attr |= sym.AttrReachable;

                s = ctxt.Syms.Lookup(".got", 0L); // will be __nl_symbol_ptr
                s.Type = sym.SMACHOGOT;
                s.Attr |= sym.AttrReachable;
                s.Align = 4L;

                s = ctxt.Syms.Lookup(".linkedit.plt", 0L); // indirect table for .plt
                s.Type = sym.SMACHOINDIRECTPLT;
                s.Attr |= sym.AttrReachable;

                s = ctxt.Syms.Lookup(".linkedit.got", 0L); // indirect table for .got
                s.Type = sym.SMACHOINDIRECTGOT;
                s.Attr |= sym.AttrReachable;
            }
        }

        private static void machoadddynlib(@string lib, LinkMode linkmode)
        {
            if (seenlib[lib] || linkmode == LinkExternal)
            {
                return;
            }
            seenlib[lib] = true; 

            // Will need to store the library name rounded up
            // and 24 bytes of header metadata. If not enough
            // space, grab another page of initial space at the
            // beginning of the output file.
            loadBudget -= (len(lib) + 7L) / 8L * 8L + 24L;

            if (loadBudget < 0L)
            {
                HEADR += 4096L;
                FlagTextAddr.Value += 4096L;
                loadBudget += 4096L;
            }
            dylib = append(dylib, lib);
        }

        private static void machoshbits(ref Link ctxt, ref MachoSeg mseg, ref sym.Section sect, @string segname)
        {
            @string buf = "__" + strings.Replace(sect.Name[1L..], ".", "_", -1L);

            ref MachoSect msect = default;
            if (sect.Rwx & 1L == 0L && segname != "__DWARF" && (ctxt.Arch.Family == sys.ARM64 || (ctxt.Arch.Family == sys.AMD64 && ctxt.BuildMode != BuildModeExe) || (ctxt.Arch.Family == sys.ARM && ctxt.BuildMode != BuildModeExe)))
            { 
                // Darwin external linker on arm64 and on amd64 and arm in c-shared/c-archive buildmode
                // complains about absolute relocs in __TEXT, so if the section is not
                // executable, put it in __DATA segment.
                msect = newMachoSect(mseg, buf, "__DATA");
            }
            else
            {
                msect = newMachoSect(mseg, buf, segname);
            }
            if (sect.Rellen > 0L)
            {
                msect.reloc = uint32(sect.Reloff);
                msect.nreloc = uint32(sect.Rellen / 8L);
            }
            while (1L << (int)(msect.align) < sect.Align)
            {
                msect.align++;
            }

            msect.addr = sect.Vaddr;
            msect.size = sect.Length;

            if (sect.Vaddr < sect.Seg.Vaddr + sect.Seg.Filelen)
            { 
                // data in file
                if (sect.Length > sect.Seg.Vaddr + sect.Seg.Filelen - sect.Vaddr)
                {
                    Errorf(null, "macho cannot represent section %s crossing data and bss", sect.Name);
                }
                msect.off = uint32(sect.Seg.Fileoff + sect.Vaddr - sect.Seg.Vaddr);
            }
            else
            {
                msect.off = 0L;
                msect.flag |= S_ZEROFILL;
            }
            if (sect.Rwx & 1L != 0L)
            {
                msect.flag |= S_ATTR_SOME_INSTRUCTIONS;
            }
            if (sect.Name == ".plt")
            {
                msect.name = "__symbol_stub1";
                msect.flag = S_ATTR_PURE_INSTRUCTIONS | S_ATTR_SOME_INSTRUCTIONS | S_SYMBOL_STUBS;
                msect.res1 = 0L; //nkind[SymKindLocal];
                msect.res2 = 6L;
            }
            if (sect.Name == ".got")
            {
                msect.name = "__nl_symbol_ptr";
                msect.flag = S_NON_LAZY_SYMBOL_POINTERS;
                msect.res1 = uint32(ctxt.Syms.Lookup(".linkedit.plt", 0L).Size / 4L); /* offset into indirect symbol table */
            }
            if (sect.Name == ".init_array")
            {
                msect.name = "__mod_init_func";
                msect.flag = S_MOD_INIT_FUNC_POINTERS;
            }
            if (segname == "__DWARF")
            {
                msect.flag |= S_ATTR_DEBUG;
            }
        }

        public static void Asmbmacho(ref Link ctxt)
        { 
            /* apple MACH */
            var va = FlagTextAddr - int64(HEADR).Value;

            var mh = getMachoHdr();

            if (ctxt.Arch.Family == sys.ARM) 
                mh.cpu = MACHO_CPU_ARM;
                mh.subcpu = MACHO_SUBCPU_ARMV7;
            else if (ctxt.Arch.Family == sys.AMD64) 
                mh.cpu = MACHO_CPU_AMD64;
                mh.subcpu = MACHO_SUBCPU_X86;
            else if (ctxt.Arch.Family == sys.ARM64) 
                mh.cpu = MACHO_CPU_ARM64;
                mh.subcpu = MACHO_SUBCPU_ARM64_ALL;
            else if (ctxt.Arch.Family == sys.I386) 
                mh.cpu = MACHO_CPU_386;
                mh.subcpu = MACHO_SUBCPU_X86;
            else 
                Exitf("unknown macho architecture: %v", ctxt.Arch.Family);
                        ref MachoSeg ms = default;
            if (ctxt.LinkMode == LinkExternal)
            { 
                /* segment for entire file */
                ms = newMachoSeg("", 40L);

                ms.fileoffset = Segtext.Fileoff;
                if (ctxt.Arch.Family == sys.ARM || ctxt.BuildMode == BuildModeCArchive)
                {
                    ms.filesize = Segdata.Fileoff + Segdata.Filelen - Segtext.Fileoff;
                }
                else
                {
                    ms.filesize = Segdwarf.Fileoff + Segdwarf.Filelen - Segtext.Fileoff;
                    ms.vsize = Segdwarf.Vaddr + Segdwarf.Length - Segtext.Vaddr;
                }
            } 

            /* segment for zero page */
            if (ctxt.LinkMode != LinkExternal)
            {
                ms = newMachoSeg("__PAGEZERO", 0L);
                ms.vsize = uint64(va);
            } 

            /* text */
            var v = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.Value));

            if (ctxt.LinkMode != LinkExternal)
            {
                ms = newMachoSeg("__TEXT", 20L);
                ms.vaddr = uint64(va);
                ms.vsize = uint64(v);
                ms.fileoffset = 0L;
                ms.filesize = uint64(v);
                ms.prot1 = 7L;
                ms.prot2 = 5L;
            }
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections)
                {
                    sect = __sect;
                    machoshbits(ctxt, ms, sect, "__TEXT");
                } 

                /* data */

                sect = sect__prev1;
            }

            if (ctxt.LinkMode != LinkExternal)
            {
                var w = int64(Segdata.Length);
                ms = newMachoSeg("__DATA", 20L);
                ms.vaddr = uint64(va) + uint64(v);
                ms.vsize = uint64(w);
                ms.fileoffset = uint64(v);
                ms.filesize = Segdata.Filelen;
                ms.prot1 = 3L;
                ms.prot2 = 3L;
            }
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    machoshbits(ctxt, ms, sect, "__DATA");
                } 

                /* dwarf */

                sect = sect__prev1;
            }

            if (!FlagW.Value)
            {
                if (ctxt.LinkMode != LinkExternal)
                {
                    ms = newMachoSeg("__DWARF", 20L);
                    ms.vaddr = Segdwarf.Vaddr;
                    ms.vsize = 0L;
                    ms.fileoffset = Segdwarf.Fileoff;
                    ms.filesize = Segdwarf.Filelen;
                }
                {
                    var sect__prev1 = sect;

                    foreach (var (_, __sect) in Segdwarf.Sections)
                    {
                        sect = __sect;
                        machoshbits(ctxt, ms, sect, "__DWARF");
                    }

                    sect = sect__prev1;
                }

            }
            if (ctxt.LinkMode != LinkExternal)
            {

                if (ctxt.Arch.Family == sys.ARM) 
                    var ml = newMachoLoad(ctxt.Arch, LC_UNIXTHREAD, 17L + 2L);
                    ml.data[0L] = 1L; /* thread type */
                    ml.data[1L] = 17L; /* word count */
                    ml.data[2L + 15L] = uint32(Entryvalue(ctxt));                    /* start pc */
                else if (ctxt.Arch.Family == sys.AMD64) 
                    ml = newMachoLoad(ctxt.Arch, LC_UNIXTHREAD, 42L + 2L);
                    ml.data[0L] = 4L; /* thread type */
                    ml.data[1L] = 42L; /* word count */
                    ml.data[2L + 32L] = uint32(Entryvalue(ctxt)); /* start pc */
                    ml.data[2L + 32L + 1L] = uint32(Entryvalue(ctxt) >> (int)(32L));
                else if (ctxt.Arch.Family == sys.ARM64) 
                    ml = newMachoLoad(ctxt.Arch, LC_UNIXTHREAD, 68L + 2L);
                    ml.data[0L] = 6L; /* thread type */
                    ml.data[1L] = 68L; /* word count */
                    ml.data[2L + 64L] = uint32(Entryvalue(ctxt)); /* start pc */
                    ml.data[2L + 64L + 1L] = uint32(Entryvalue(ctxt) >> (int)(32L));
                else if (ctxt.Arch.Family == sys.I386) 
                    ml = newMachoLoad(ctxt.Arch, LC_UNIXTHREAD, 16L + 2L);
                    ml.data[0L] = 1L; /* thread type */
                    ml.data[1L] = 16L; /* word count */
                    ml.data[2L + 10L] = uint32(Entryvalue(ctxt)); /* start pc */
                else 
                    Exitf("unknown macho architecture: %v", ctxt.Arch.Family);
                            }
            if (!FlagD.Value)
            { 
                // must match domacholink below
                var s1 = ctxt.Syms.Lookup(".machosymtab", 0L);
                var s2 = ctxt.Syms.Lookup(".linkedit.plt", 0L);
                var s3 = ctxt.Syms.Lookup(".linkedit.got", 0L);
                var s4 = ctxt.Syms.Lookup(".machosymstr", 0L);

                if (ctxt.LinkMode != LinkExternal)
                {
                    ms = newMachoSeg("__LINKEDIT", 0L);
                    ms.vaddr = uint64(va) + uint64(v) + uint64(Rnd(int64(Segdata.Length), int64(FlagRound.Value)));
                    ms.vsize = uint64(s1.Size) + uint64(s2.Size) + uint64(s3.Size) + uint64(s4.Size);
                    ms.fileoffset = uint64(linkoff);
                    ms.filesize = ms.vsize;
                    ms.prot1 = 7L;
                    ms.prot2 = 3L;
                }
                ml = newMachoLoad(ctxt.Arch, LC_SYMTAB, 4L);
                ml.data[0L] = uint32(linkoff); /* symoff */
                ml.data[1L] = uint32(nsortsym); /* nsyms */
                ml.data[2L] = uint32(linkoff + s1.Size + s2.Size + s3.Size); /* stroff */
                ml.data[3L] = uint32(s4.Size);                /* strsize */

                machodysymtab(ctxt);

                if (ctxt.LinkMode != LinkExternal)
                {
                    ml = newMachoLoad(ctxt.Arch, LC_LOAD_DYLINKER, 6L);
                    ml.data[0L] = 12L; /* offset to string */
                    stringtouint32(ml.data[1L..], "/usr/lib/dyld");

                    for (long i = 0L; i < len(dylib); i++)
                    {
                        ml = newMachoLoad(ctxt.Arch, LC_LOAD_DYLIB, 4L + (uint32(len(dylib[i])) + 1L + 7L) / 8L * 2L);
                        ml.data[0L] = 24L; /* offset of string from beginning of load */
                        ml.data[1L] = 0L; /* time stamp */
                        ml.data[2L] = 0L; /* version */
                        ml.data[3L] = 0L; /* compatibility version */
                        stringtouint32(ml.data[4L..], dylib[i]);
                    }

                }
            }
            if (ctxt.LinkMode == LinkInternal)
            { 
                // For lldb, must say LC_VERSION_MIN_MACOSX or else
                // it won't know that this Mach-O binary is from OS X
                // (could be iOS or WatchOS instead).
                // Go on iOS uses linkmode=external, and linkmode=external
                // adds this itself. So we only need this code for linkmode=internal
                // and we can assume OS X.
                //
                // See golang.org/issues/12941.
                ml = newMachoLoad(ctxt.Arch, LC_VERSION_MIN_MACOSX, 2L);
                ml.data[0L] = 10L << (int)(16L) | 7L << (int)(8L) | 0L << (int)(0L); // OS X version 10.7.0
                ml.data[1L] = 10L << (int)(16L) | 7L << (int)(8L) | 0L << (int)(0L); // SDK 10.7.0
            }
            var a = machowrite(ctxt.Arch, ctxt.Out, ctxt.LinkMode);
            if (int32(a) > HEADR)
            {
                Exitf("HEADR too small: %d > %d", a, HEADR);
            }
        }

        private static long symkind(ref sym.Symbol s)
        {
            if (s.Type == sym.SDYNIMPORT)
            {
                return SymKindUndef;
            }
            if (s.Attr.CgoExport())
            {
                return SymKindExtdef;
            }
            return SymKindLocal;
        }

        private static void addsym(ref Link ctxt, ref sym.Symbol s, @string name, SymbolType type_, long addr, ref sym.Symbol gotype)
        {
            if (s == null)
            {
                return;
            }

            if (type_ == DataSym || type_ == BSSSym || type_ == TextSym) 
                break;
            else 
                return;
                        if (sortsym != null)
            {
                sortsym[nsortsym] = s;
                nkind[symkind(s)]++;
            }
            nsortsym++;
        }

        private partial struct machoscmp // : slice<ref sym.Symbol>
        {
        }

        private static long Len(this machoscmp x)
        {
            return len(x);
        }

        private static void Swap(this machoscmp x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];
        }

        private static bool Less(this machoscmp x, long i, long j)
        {
            var s1 = x[i];
            var s2 = x[j];

            var k1 = symkind(s1);
            var k2 = symkind(s2);
            if (k1 != k2)
            {
                return k1 < k2;
            }
            return s1.Extname < s2.Extname;
        }

        private static void machogenasmsym(ref Link ctxt)
        {
            genasmsym(ctxt, addsym);
            foreach (var (_, s) in ctxt.Syms.Allsym)
            {
                if (s.Type == sym.SDYNIMPORT || s.Type == sym.SHOSTOBJ)
                {
                    if (s.Attr.Reachable())
                    {
                        addsym(ctxt, s, "", DataSym, 0L, null);
                    }
                }
            }
        }

        private static void machosymorder(ref Link ctxt)
        { 
            // On Mac OS X Mountain Lion, we must sort exported symbols
            // So we sort them here and pre-allocate dynid for them
            // See https://golang.org/issue/4029
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(dynexp); i++)
                {
                    dynexp[i].Attr |= sym.AttrReachable;
                }


                i = i__prev1;
            }
            machogenasmsym(ctxt);
            sortsym = make_slice<ref sym.Symbol>(nsortsym);
            nsortsym = 0L;
            machogenasmsym(ctxt);
            sort.Sort(machoscmp(sortsym[..nsortsym]));
            {
                long i__prev1 = i;

                for (i = 0L; i < nsortsym; i++)
                {
                    sortsym[i].Dynid = int32(i);
                }


                i = i__prev1;
            }
        }

        // machoShouldExport reports whether a symbol needs to be exported.
        //
        // When dynamically linking, all non-local variables and plugin-exported
        // symbols need to be exported.
        private static bool machoShouldExport(ref Link ctxt, ref sym.Symbol s)
        {
            if (!ctxt.DynlinkingGo() || s.Attr.Local())
            {
                return false;
            }
            if (ctxt.BuildMode == BuildModePlugin && strings.HasPrefix(s.Extname, objabi.PathToPrefix(flagPluginPath.Value)))
            {
                return true;
            }
            if (strings.HasPrefix(s.Name, "go.itab."))
            {
                return true;
            }
            if (strings.HasPrefix(s.Name, "type.") && !strings.HasPrefix(s.Name, "type.."))
            { 
                // reduce runtime typemap pressure, but do not
                // export alg functions (type..*), as these
                // appear in pclntable.
                return true;
            }
            if (strings.HasPrefix(s.Name, "go.link.pkghash"))
            {
                return true;
            }
            return s.Type >= sym.SELFSECT; // only writable sections
        }

        private static void machosymtab(ref Link ctxt)
        {
            var symtab = ctxt.Syms.Lookup(".machosymtab", 0L);
            var symstr = ctxt.Syms.Lookup(".machosymstr", 0L);

            for (long i = 0L; i < nsortsym; i++)
            {
                var s = sortsym[i];
                symtab.AddUint32(ctxt.Arch, uint32(symstr.Size));

                var export = machoShouldExport(ctxt, s); 

                // In normal buildmodes, only add _ to C symbols, as
                // Go symbols have dot in the name.
                //
                // Do not export C symbols in plugins, as runtime C
                // symbols like crosscall2 are in pclntab and end up
                // pointing at the host binary, breaking unwinding.
                // See Issue #18190.
                var cexport = !strings.Contains(s.Extname, ".") && (ctxt.BuildMode != BuildModePlugin || onlycsymbol(s));
                if (cexport || export)
                {
                    symstr.AddUint8('_');
                } 

                // replace "·" as ".", because DTrace cannot handle it.
                Addstring(symstr, strings.Replace(s.Extname, "·", ".", -1L));

                if (s.Type == sym.SDYNIMPORT || s.Type == sym.SHOSTOBJ)
                {
                    symtab.AddUint8(0x01UL); // type N_EXT, external symbol
                    symtab.AddUint8(0L); // no section
                    symtab.AddUint16(ctxt.Arch, 0L); // desc
                    symtab.AddUintXX(ctxt.Arch, 0L, ctxt.Arch.PtrSize); // no value
                }
                else
                {
                    if (s.Attr.CgoExport() || export)
                    {
                        symtab.AddUint8(0x0fUL);
                    }
                    else
                    {
                        symtab.AddUint8(0x0eUL);
                    }
                    var o = s;
                    while (o.Outer != null)
                    {
                        o = o.Outer;
                    }

                    if (o.Sect == null)
                    {
                        Errorf(s, "missing section for symbol");
                        symtab.AddUint8(0L);
                    }
                    else
                    {
                        symtab.AddUint8(uint8(o.Sect.Extnum));
                    }
                    symtab.AddUint16(ctxt.Arch, 0L); // desc
                    symtab.AddUintXX(ctxt.Arch, uint64(Symaddr(s)), ctxt.Arch.PtrSize);
                }
            }

        }

        private static void machodysymtab(ref Link ctxt)
        {
            var ml = newMachoLoad(ctxt.Arch, LC_DYSYMTAB, 18L);

            long n = 0L;
            ml.data[0L] = uint32(n); /* ilocalsym */
            ml.data[1L] = uint32(nkind[SymKindLocal]); /* nlocalsym */
            n += nkind[SymKindLocal];

            ml.data[2L] = uint32(n); /* iextdefsym */
            ml.data[3L] = uint32(nkind[SymKindExtdef]); /* nextdefsym */
            n += nkind[SymKindExtdef];

            ml.data[4L] = uint32(n); /* iundefsym */
            ml.data[5L] = uint32(nkind[SymKindUndef]);            /* nundefsym */

            ml.data[6L] = 0L; /* tocoffset */
            ml.data[7L] = 0L; /* ntoc */
            ml.data[8L] = 0L; /* modtaboff */
            ml.data[9L] = 0L; /* nmodtab */
            ml.data[10L] = 0L; /* extrefsymoff */
            ml.data[11L] = 0L;            /* nextrefsyms */

            // must match domacholink below
            var s1 = ctxt.Syms.Lookup(".machosymtab", 0L);

            var s2 = ctxt.Syms.Lookup(".linkedit.plt", 0L);
            var s3 = ctxt.Syms.Lookup(".linkedit.got", 0L);
            ml.data[12L] = uint32(linkoff + s1.Size); /* indirectsymoff */
            ml.data[13L] = uint32((s2.Size + s3.Size) / 4L);            /* nindirectsyms */

            ml.data[14L] = 0L; /* extreloff */
            ml.data[15L] = 0L; /* nextrel */
            ml.data[16L] = 0L; /* locreloff */
            ml.data[17L] = 0L; /* nlocrel */
        }

        public static long Domacholink(ref Link ctxt)
        {
            machosymtab(ctxt); 

            // write data that will be linkedit section
            var s1 = ctxt.Syms.Lookup(".machosymtab", 0L);

            var s2 = ctxt.Syms.Lookup(".linkedit.plt", 0L);
            var s3 = ctxt.Syms.Lookup(".linkedit.got", 0L);
            var s4 = ctxt.Syms.Lookup(".machosymstr", 0L); 

            // Force the linkedit section to end on a 16-byte
            // boundary. This allows pure (non-cgo) Go binaries
            // to be code signed correctly.
            //
            // Apple's codesign_allocate (a helper utility for
            // the codesign utility) can do this fine itself if
            // it is run on a dynamic Mach-O binary. However,
            // when it is run on a pure (non-cgo) Go binary, where
            // the linkedit section is mostly empty, it fails to
            // account for the extra padding that it itself adds
            // when adding the LC_CODE_SIGNATURE load command
            // (which must be aligned on a 16-byte boundary).
            //
            // By forcing the linkedit section to end on a 16-byte
            // boundary, codesign_allocate will not need to apply
            // any alignment padding itself, working around the
            // issue.
            while (s4.Size % 16L != 0L)
            {
                s4.AddUint8(0L);
            }


            var size = int(s1.Size + s2.Size + s3.Size + s4.Size);

            if (size > 0L)
            {
                linkoff = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.Value)) + Rnd(int64(Segdata.Filelen), int64(FlagRound.Value)) + Rnd(int64(Segdwarf.Filelen), int64(FlagRound.Value));
                ctxt.Out.SeekSet(linkoff);

                ctxt.Out.Write(s1.P[..s1.Size]);
                ctxt.Out.Write(s2.P[..s2.Size]);
                ctxt.Out.Write(s3.P[..s3.Size]);
                ctxt.Out.Write(s4.P[..s4.Size]);
            }
            return Rnd(int64(size), int64(FlagRound.Value));
        }

        private static void machorelocsect(ref Link ctxt, ref sym.Section sect, slice<ref sym.Symbol> syms)
        { 
            // If main section has no bits, nothing to relocate.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return;
            }
            sect.Reloff = uint64(ctxt.Out.Offset());
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in syms)
                {
                    i = __i;
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }
                    if (uint64(s.Value) >= sect.Vaddr)
                    {
                        syms = syms[i..];
                        break;
                    }
                }

                s = s__prev1;
            }

            var eaddr = int32(sect.Vaddr + sect.Length);
            {
                var s__prev1 = s;

                foreach (var (_, __s) in syms)
                {
                    s = __s;
                    if (!s.Attr.Reachable())
                    {
                        continue;
                    }
                    if (s.Value >= int64(eaddr))
                    {
                        break;
                    }
                    for (long ri = 0L; ri < len(s.R); ri++)
                    {
                        var r = ref s.R[ri];
                        if (r.Done)
                        {
                            continue;
                        }
                        if (r.Xsym == null)
                        {
                            Errorf(s, "missing xsym in relocation");
                            continue;
                        }
                        if (!r.Xsym.Attr.Reachable())
                        {
                            Errorf(s, "unreachable reloc %d (%s) target %v", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Xsym.Name);
                        }
                        if (!Thearch.Machoreloc1(ctxt.Arch, ctxt.Out, s, r, int64(uint64(s.Value + int64(r.Off)) - sect.Vaddr)))
                        {
                            Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Siz, r.Sym.Name);
                        }
                    }

                }

                s = s__prev1;
            }

            sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;
        }

        public static void Machoemitreloc(ref Link ctxt)
        {
            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            }


            machorelocsect(ctxt, Segtext.Sections[0L], ctxt.Textp);
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections[1L..])
                {
                    sect = __sect;
                    machorelocsect(ctxt, sect, datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    machorelocsect(ctxt, sect, datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdwarf.Sections)
                {
                    sect = __sect;
                    machorelocsect(ctxt, sect, dwarfp);
                }

                sect = sect__prev1;
            }

        }
    }
}}}}
