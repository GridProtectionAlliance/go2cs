// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:39:14 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\macho.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using macho = go.debug.macho_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

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

        // MachoPlatformLoad represents a LC_VERSION_MIN_* or
        // LC_BUILD_VERSION load command.
        public partial struct MachoPlatformLoad
        {
            public MachoPlatform platform; // One of PLATFORM_* constants.
            public MachoLoad cmd;
        }

        public partial struct MachoLoad
        {
            public uint type_;
            public slice<uint> data;
        }

        public partial struct MachoPlatform // : long
        {
        }

        /*
         * Total amount of space to reserve at the start of the file
         * for Header, PHeaders, and SHeaders.
         * May waste some.
         */
        public static readonly long INITIAL_MACHO_HEADR = (long)4L * 1024L;


        public static readonly long MACHO_CPU_AMD64 = (long)1L << (int)(24L) | 7L;
        public static readonly long MACHO_CPU_386 = (long)7L;
        public static readonly long MACHO_SUBCPU_X86 = (long)3L;
        public static readonly long MACHO_CPU_ARM = (long)12L;
        public static readonly long MACHO_SUBCPU_ARM = (long)0L;
        public static readonly long MACHO_SUBCPU_ARMV7 = (long)9L;
        public static readonly long MACHO_CPU_ARM64 = (long)1L << (int)(24L) | 12L;
        public static readonly long MACHO_SUBCPU_ARM64_ALL = (long)0L;
        public static readonly long MACHO32SYMSIZE = (long)12L;
        public static readonly long MACHO64SYMSIZE = (long)16L;
        public static readonly long MACHO_X86_64_RELOC_UNSIGNED = (long)0L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED = (long)1L;
        public static readonly long MACHO_X86_64_RELOC_BRANCH = (long)2L;
        public static readonly long MACHO_X86_64_RELOC_GOT_LOAD = (long)3L;
        public static readonly long MACHO_X86_64_RELOC_GOT = (long)4L;
        public static readonly long MACHO_X86_64_RELOC_SUBTRACTOR = (long)5L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_1 = (long)6L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_2 = (long)7L;
        public static readonly long MACHO_X86_64_RELOC_SIGNED_4 = (long)8L;
        public static readonly long MACHO_ARM_RELOC_VANILLA = (long)0L;
        public static readonly long MACHO_ARM_RELOC_PAIR = (long)1L;
        public static readonly long MACHO_ARM_RELOC_SECTDIFF = (long)2L;
        public static readonly long MACHO_ARM_RELOC_BR24 = (long)5L;
        public static readonly long MACHO_ARM64_RELOC_UNSIGNED = (long)0L;
        public static readonly long MACHO_ARM64_RELOC_BRANCH26 = (long)2L;
        public static readonly long MACHO_ARM64_RELOC_PAGE21 = (long)3L;
        public static readonly long MACHO_ARM64_RELOC_PAGEOFF12 = (long)4L;
        public static readonly long MACHO_ARM64_RELOC_ADDEND = (long)10L;
        public static readonly long MACHO_GENERIC_RELOC_VANILLA = (long)0L;
        public static readonly long MACHO_FAKE_GOTPCREL = (long)100L;


        public static readonly ulong MH_MAGIC = (ulong)0xfeedfaceUL;
        public static readonly ulong MH_MAGIC_64 = (ulong)0xfeedfacfUL;

        public static readonly ulong MH_OBJECT = (ulong)0x1UL;
        public static readonly ulong MH_EXECUTE = (ulong)0x2UL;

        public static readonly ulong MH_NOUNDEFS = (ulong)0x1UL;


        public static readonly ulong LC_SEGMENT = (ulong)0x1UL;
        public static readonly ulong LC_SYMTAB = (ulong)0x2UL;
        public static readonly ulong LC_SYMSEG = (ulong)0x3UL;
        public static readonly ulong LC_THREAD = (ulong)0x4UL;
        public static readonly ulong LC_UNIXTHREAD = (ulong)0x5UL;
        public static readonly ulong LC_LOADFVMLIB = (ulong)0x6UL;
        public static readonly ulong LC_IDFVMLIB = (ulong)0x7UL;
        public static readonly ulong LC_IDENT = (ulong)0x8UL;
        public static readonly ulong LC_FVMFILE = (ulong)0x9UL;
        public static readonly ulong LC_PREPAGE = (ulong)0xaUL;
        public static readonly ulong LC_DYSYMTAB = (ulong)0xbUL;
        public static readonly ulong LC_LOAD_DYLIB = (ulong)0xcUL;
        public static readonly ulong LC_ID_DYLIB = (ulong)0xdUL;
        public static readonly ulong LC_LOAD_DYLINKER = (ulong)0xeUL;
        public static readonly ulong LC_ID_DYLINKER = (ulong)0xfUL;
        public static readonly ulong LC_PREBOUND_DYLIB = (ulong)0x10UL;
        public static readonly ulong LC_ROUTINES = (ulong)0x11UL;
        public static readonly ulong LC_SUB_FRAMEWORK = (ulong)0x12UL;
        public static readonly ulong LC_SUB_UMBRELLA = (ulong)0x13UL;
        public static readonly ulong LC_SUB_CLIENT = (ulong)0x14UL;
        public static readonly ulong LC_SUB_LIBRARY = (ulong)0x15UL;
        public static readonly ulong LC_TWOLEVEL_HINTS = (ulong)0x16UL;
        public static readonly ulong LC_PREBIND_CKSUM = (ulong)0x17UL;
        public static readonly ulong LC_LOAD_WEAK_DYLIB = (ulong)0x80000018UL;
        public static readonly ulong LC_SEGMENT_64 = (ulong)0x19UL;
        public static readonly ulong LC_ROUTINES_64 = (ulong)0x1aUL;
        public static readonly ulong LC_UUID = (ulong)0x1bUL;
        public static readonly ulong LC_RPATH = (ulong)0x8000001cUL;
        public static readonly ulong LC_CODE_SIGNATURE = (ulong)0x1dUL;
        public static readonly ulong LC_SEGMENT_SPLIT_INFO = (ulong)0x1eUL;
        public static readonly ulong LC_REEXPORT_DYLIB = (ulong)0x8000001fUL;
        public static readonly ulong LC_LAZY_LOAD_DYLIB = (ulong)0x20UL;
        public static readonly ulong LC_ENCRYPTION_INFO = (ulong)0x21UL;
        public static readonly ulong LC_DYLD_INFO = (ulong)0x22UL;
        public static readonly ulong LC_DYLD_INFO_ONLY = (ulong)0x80000022UL;
        public static readonly ulong LC_LOAD_UPWARD_DYLIB = (ulong)0x80000023UL;
        public static readonly ulong LC_VERSION_MIN_MACOSX = (ulong)0x24UL;
        public static readonly ulong LC_VERSION_MIN_IPHONEOS = (ulong)0x25UL;
        public static readonly ulong LC_FUNCTION_STARTS = (ulong)0x26UL;
        public static readonly ulong LC_DYLD_ENVIRONMENT = (ulong)0x27UL;
        public static readonly ulong LC_MAIN = (ulong)0x80000028UL;
        public static readonly ulong LC_DATA_IN_CODE = (ulong)0x29UL;
        public static readonly ulong LC_SOURCE_VERSION = (ulong)0x2AUL;
        public static readonly ulong LC_DYLIB_CODE_SIGN_DRS = (ulong)0x2BUL;
        public static readonly ulong LC_ENCRYPTION_INFO_64 = (ulong)0x2CUL;
        public static readonly ulong LC_LINKER_OPTION = (ulong)0x2DUL;
        public static readonly ulong LC_LINKER_OPTIMIZATION_HINT = (ulong)0x2EUL;
        public static readonly ulong LC_VERSION_MIN_TVOS = (ulong)0x2FUL;
        public static readonly ulong LC_VERSION_MIN_WATCHOS = (ulong)0x30UL;
        public static readonly ulong LC_VERSION_NOTE = (ulong)0x31UL;
        public static readonly ulong LC_BUILD_VERSION = (ulong)0x32UL;


        public static readonly ulong S_REGULAR = (ulong)0x0UL;
        public static readonly ulong S_ZEROFILL = (ulong)0x1UL;
        public static readonly ulong S_NON_LAZY_SYMBOL_POINTERS = (ulong)0x6UL;
        public static readonly ulong S_SYMBOL_STUBS = (ulong)0x8UL;
        public static readonly ulong S_MOD_INIT_FUNC_POINTERS = (ulong)0x9UL;
        public static readonly ulong S_ATTR_PURE_INSTRUCTIONS = (ulong)0x80000000UL;
        public static readonly ulong S_ATTR_DEBUG = (ulong)0x02000000UL;
        public static readonly ulong S_ATTR_SOME_INSTRUCTIONS = (ulong)0x00000400UL;


        public static readonly MachoPlatform PLATFORM_MACOS = (MachoPlatform)1L;
        public static readonly MachoPlatform PLATFORM_IOS = (MachoPlatform)2L;
        public static readonly MachoPlatform PLATFORM_TVOS = (MachoPlatform)3L;
        public static readonly MachoPlatform PLATFORM_WATCHOS = (MachoPlatform)4L;
        public static readonly MachoPlatform PLATFORM_BRIDGEOS = (MachoPlatform)5L;


        // Mach-O file writing
        // https://developer.apple.com/mac/library/DOCUMENTATION/DeveloperTools/Conceptual/MachORuntime/Reference/reference.html

        private static MachoHdr machohdr = default;

        private static slice<MachoLoad> load = default;

        private static MachoPlatform machoPlatform = default;

        private static array<MachoSeg> seg = new array<MachoSeg>(16L);

        private static long nseg = default;

        private static long ndebug = default;

        private static long nsect = default;

        public static readonly long SymKindLocal = (long)0L + iota;
        public static readonly var SymKindExtdef = (var)0;
        public static readonly var SymKindUndef = (var)1;
        public static readonly var NumSymKind = (var)2;


        private static array<long> nkind = new array<long>(NumSymKind);

        private static slice<loader.Sym> sortsym = default;

        private static long nsortsym = default;

        // Amount of space left for adding load commands
        // that refer to dynamic libraries. Because these have
        // to go in the Mach-O header, we can't just pick a
        // "big enough" header size. The initial header is
        // one page, the non-dynamic library stuff takes
        // up about 1300 bytes; we overestimate that as 2k.
        private static var loadBudget = INITIAL_MACHO_HEADR - 2L * 1024L;

        private static ptr<MachoHdr> getMachoHdr()
        {
            return _addr__addr_machohdr!;
        }

        private static ptr<MachoLoad> newMachoLoad(ptr<sys.Arch> _addr_arch, uint type_, uint ndata)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            if (arch.PtrSize == 8L && (ndata & 1L != 0L))
            {
                ndata++;
            }

            load = append(load, new MachoLoad());
            var l = _addr_load[len(load) - 1L];
            l.type_ = type_;
            l.data = make_slice<uint>(ndata);
            return _addr_l!;

        }

        private static ptr<MachoSeg> newMachoSeg(@string name, long msect)
        {
            if (nseg >= len(seg))
            {
                Exitf("too many segs");
            }

            var s = _addr_seg[nseg];
            nseg++;
            s.name = name;
            s.msect = uint32(msect);
            s.sect = make_slice<MachoSect>(msect);
            return _addr_s!;

        }

        private static ptr<MachoSect> newMachoSect(ptr<MachoSeg> _addr_seg, @string name, @string segname)
        {
            ref MachoSeg seg = ref _addr_seg.val;

            if (seg.nsect >= seg.msect)
            {
                Exitf("too many sects in segment %s", seg.name);
            }

            var s = _addr_seg.sect[seg.nsect];
            seg.nsect++;
            s.name = name;
            s.segname = segname;
            nsect++;
            return _addr_s!;

        }

        // Generic linking code.

        private static slice<@string> dylib = default;

        private static long linkoff = default;

        private static long machowrite(ptr<sys.Arch> _addr_arch, ptr<OutBuf> _addr_@out, LinkMode linkmode)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref OutBuf @out = ref _addr_@out.val;

            var o1 = @out.Offset();

            long loadsize = 4L * 4L * ndebug;
            {
                var i__prev1 = i;

                foreach (var (__i) in load)
                {
                    i = __i;
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
                var i__prev1 = i;

                for (long i = 0L; i < nseg; i++)
                {
                    var s = _addr_seg[i];
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
                            var t = _addr_s.sect[j];
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
                var i__prev1 = i;

                foreach (var (__i) in load)
                {
                    i = __i;
                    var l = _addr_load[i];
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

        private static void domacho(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (FlagD.val)
            {
                return ;
            } 

            // Copy platform load command.
            foreach (var (_, h) in hostobj)
            {
                var (load, err) = hostobjMachoPlatform(_addr_h);
                if (err != null)
                {
                    Exitf("%v", err);
                }

                if (load != null)
                {
                    machoPlatform = load.platform;
                    var ml = newMachoLoad(_addr_ctxt.Arch, load.cmd.type_, uint32(len(load.cmd.data)));
                    copy(ml.data, load.cmd.data);
                    break;
                }

            }
            if (machoPlatform == 0L)
            {

                if (ctxt.Arch.Family == sys.ARM || ctxt.Arch.Family == sys.ARM64) 
                    machoPlatform = PLATFORM_IOS;
                else 
                    machoPlatform = PLATFORM_MACOS;
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
                        //
                        // The version must be at least 10.9; see golang.org/issues/30488.
                        ml = newMachoLoad(_addr_ctxt.Arch, LC_VERSION_MIN_MACOSX, 2L);
                        ml.data[0L] = 10L << (int)(16L) | 9L << (int)(8L) | 0L << (int)(0L); // OS X version 10.9.0
                        ml.data[1L] = 10L << (int)(16L) | 9L << (int)(8L) | 0L << (int)(0L); // SDK 10.9.0
                    }

                            } 

            // empirically, string table must begin with " \x00".
            var s = ctxt.loader.LookupOrCreateSym(".machosymstr", 0L);
            var sb = ctxt.loader.MakeSymbolUpdater(s);

            sb.SetType(sym.SMACHOSYMSTR);
            sb.SetReachable(true);
            sb.AddUint8(' ');
            sb.AddUint8('\x00');

            s = ctxt.loader.LookupOrCreateSym(".machosymtab", 0L);
            sb = ctxt.loader.MakeSymbolUpdater(s);
            sb.SetType(sym.SMACHOSYMTAB);
            sb.SetReachable(true);

            if (ctxt.IsInternal())
            {
                s = ctxt.loader.LookupOrCreateSym(".plt", 0L); // will be __symbol_stub
                sb = ctxt.loader.MakeSymbolUpdater(s);
                sb.SetType(sym.SMACHOPLT);
                sb.SetReachable(true);

                s = ctxt.loader.LookupOrCreateSym(".got", 0L); // will be __nl_symbol_ptr
                sb = ctxt.loader.MakeSymbolUpdater(s);
                sb.SetType(sym.SMACHOGOT);
                sb.SetReachable(true);
                sb.SetAlign(4L);

                s = ctxt.loader.LookupOrCreateSym(".linkedit.plt", 0L); // indirect table for .plt
                sb = ctxt.loader.MakeSymbolUpdater(s);
                sb.SetType(sym.SMACHOINDIRECTPLT);
                sb.SetReachable(true);

                s = ctxt.loader.LookupOrCreateSym(".linkedit.got", 0L); // indirect table for .got
                sb = ctxt.loader.MakeSymbolUpdater(s);
                sb.SetType(sym.SMACHOINDIRECTGOT);
                sb.SetReachable(true);

            } 

            // Add a dummy symbol that will become the __asm marker section.
            if (ctxt.IsExternal())
            {
                s = ctxt.loader.LookupOrCreateSym(".llvmasm", 0L);
                sb = ctxt.loader.MakeSymbolUpdater(s);
                sb.SetType(sym.SMACHO);
                sb.SetReachable(true);
                sb.AddUint8(0L);
            }

        }

        private static void machoadddynlib(@string lib, LinkMode linkmode)
        {
            if (seenlib[lib] || linkmode == LinkExternal)
            {
                return ;
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
                FlagTextAddr.val += 4096L;
                loadBudget += 4096L;
            }

            dylib = append(dylib, lib);

        }

        private static void machoshbits(ptr<Link> _addr_ctxt, ptr<MachoSeg> _addr_mseg, ptr<sym.Section> _addr_sect, @string segname)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref MachoSeg mseg = ref _addr_mseg.val;
            ref sym.Section sect = ref _addr_sect.val;

            @string buf = "__" + strings.Replace(sect.Name[1L..], ".", "_", -1L);

            ptr<MachoSect> msect;
            if (sect.Rwx & 1L == 0L && segname != "__DWARF" && (ctxt.Arch.Family == sys.ARM64 || ctxt.Arch.Family == sys.ARM || (ctxt.Arch.Family == sys.AMD64 && ctxt.BuildMode != BuildModeExe)))
            { 
                // Darwin external linker on arm and arm64, and on amd64 in c-shared/c-archive buildmode
                // complains about absolute relocs in __TEXT, so if the section is not
                // executable, put it in __DATA segment.
                msect = newMachoSect(_addr_mseg, buf, "__DATA");

            }
            else
            {
                msect = newMachoSect(_addr_mseg, buf, segname);
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

            if (sect.Name == ".text")
            {
                msect.flag |= S_ATTR_PURE_INSTRUCTIONS;
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

            // Some platforms such as watchOS and tvOS require binaries with
            // bitcode enabled. The Go toolchain can't output bitcode, so use
            // a marker section in the __LLVM segment, "__asm", to tell the Apple
            // toolchain that the Go text came from assembler and thus has no
            // bitcode. This is not true, but Kotlin/Native, Rust and Flutter
            // are also using this trick.
            if (sect.Name == ".llvmasm")
            {
                msect.name = "__asm";
                msect.segname = "__LLVM";
            }

            if (segname == "__DWARF")
            {
                msect.flag |= S_ATTR_DEBUG;
            }

        }

        public static void Asmbmacho(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            /* apple MACH */
            var va = FlagTextAddr - int64(HEADR).val;

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
                        ptr<MachoSeg> ms;
            if (ctxt.LinkMode == LinkExternal)
            { 
                /* segment for entire file */
                ms = newMachoSeg("", 40L);

                ms.fileoffset = Segtext.Fileoff;
                ms.filesize = Segdwarf.Fileoff + Segdwarf.Filelen - Segtext.Fileoff;
                ms.vsize = Segdwarf.Vaddr + Segdwarf.Length - Segtext.Vaddr;

            } 

            /* segment for zero page */
            if (ctxt.LinkMode != LinkExternal)
            {
                ms = newMachoSeg("__PAGEZERO", 0L);
                ms.vsize = uint64(va);
            } 

            /* text */
            var v = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.val));

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
                    machoshbits(_addr_ctxt, ms, _addr_sect, "__TEXT");
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
                    machoshbits(_addr_ctxt, ms, _addr_sect, "__DATA");
                } 

                /* dwarf */

                sect = sect__prev1;
            }

            if (!FlagW.val)
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
                        machoshbits(_addr_ctxt, ms, _addr_sect, "__DWARF");
                    }

                    sect = sect__prev1;
                }
            }

            if (ctxt.LinkMode != LinkExternal)
            {

                if (ctxt.Arch.Family == sys.ARM) 
                    var ml = newMachoLoad(_addr_ctxt.Arch, LC_UNIXTHREAD, 17L + 2L);
                    ml.data[0L] = 1L; /* thread type */
                    ml.data[1L] = 17L; /* word count */
                    ml.data[2L + 15L] = uint32(Entryvalue(ctxt));                    /* start pc */
                else if (ctxt.Arch.Family == sys.AMD64) 
                    ml = newMachoLoad(_addr_ctxt.Arch, LC_UNIXTHREAD, 42L + 2L);
                    ml.data[0L] = 4L; /* thread type */
                    ml.data[1L] = 42L; /* word count */
                    ml.data[2L + 32L] = uint32(Entryvalue(ctxt)); /* start pc */
                    ml.data[2L + 32L + 1L] = uint32(Entryvalue(ctxt) >> (int)(32L));
                else if (ctxt.Arch.Family == sys.ARM64) 
                    ml = newMachoLoad(_addr_ctxt.Arch, LC_UNIXTHREAD, 68L + 2L);
                    ml.data[0L] = 6L; /* thread type */
                    ml.data[1L] = 68L; /* word count */
                    ml.data[2L + 64L] = uint32(Entryvalue(ctxt)); /* start pc */
                    ml.data[2L + 64L + 1L] = uint32(Entryvalue(ctxt) >> (int)(32L));
                else if (ctxt.Arch.Family == sys.I386) 
                    ml = newMachoLoad(_addr_ctxt.Arch, LC_UNIXTHREAD, 16L + 2L);
                    ml.data[0L] = 1L; /* thread type */
                    ml.data[1L] = 16L; /* word count */
                    ml.data[2L + 10L] = uint32(Entryvalue(ctxt)); /* start pc */
                else 
                    Exitf("unknown macho architecture: %v", ctxt.Arch.Family);
                
            }

            if (!FlagD.val)
            { 
                // must match domacholink below
                var s1 = ctxt.Syms.Lookup(".machosymtab", 0L);
                var s2 = ctxt.Syms.Lookup(".linkedit.plt", 0L);
                var s3 = ctxt.Syms.Lookup(".linkedit.got", 0L);
                var s4 = ctxt.Syms.Lookup(".machosymstr", 0L);

                if (ctxt.LinkMode != LinkExternal)
                {
                    ms = newMachoSeg("__LINKEDIT", 0L);
                    ms.vaddr = uint64(va) + uint64(v) + uint64(Rnd(int64(Segdata.Length), int64(FlagRound.val)));
                    ms.vsize = uint64(s1.Size) + uint64(s2.Size) + uint64(s3.Size) + uint64(s4.Size);
                    ms.fileoffset = uint64(linkoff);
                    ms.filesize = ms.vsize;
                    ms.prot1 = 7L;
                    ms.prot2 = 3L;
                }

                ml = newMachoLoad(_addr_ctxt.Arch, LC_SYMTAB, 4L);
                ml.data[0L] = uint32(linkoff); /* symoff */
                ml.data[1L] = uint32(nsortsym); /* nsyms */
                ml.data[2L] = uint32(linkoff + s1.Size + s2.Size + s3.Size); /* stroff */
                ml.data[3L] = uint32(s4.Size);                /* strsize */

                machodysymtab(_addr_ctxt);

                if (ctxt.LinkMode != LinkExternal)
                {
                    ml = newMachoLoad(_addr_ctxt.Arch, LC_LOAD_DYLINKER, 6L);
                    ml.data[0L] = 12L; /* offset to string */
                    stringtouint32(ml.data[1L..], "/usr/lib/dyld");

                    foreach (var (_, lib) in dylib)
                    {
                        ml = newMachoLoad(_addr_ctxt.Arch, LC_LOAD_DYLIB, 4L + (uint32(len(lib)) + 1L + 7L) / 8L * 2L);
                        ml.data[0L] = 24L; /* offset of string from beginning of load */
                        ml.data[1L] = 0L; /* time stamp */
                        ml.data[2L] = 0L; /* version */
                        ml.data[3L] = 0L; /* compatibility version */
                        stringtouint32(ml.data[4L..], lib);

                    }

                }

            }

            var a = machowrite(_addr_ctxt.Arch, _addr_ctxt.Out, ctxt.LinkMode);
            if (int32(a) > HEADR)
            {
                Exitf("HEADR too small: %d > %d", a, HEADR);
            }

        }

        private static long symkind(ptr<loader.Loader> _addr_ldr, loader.Sym s)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;

            if (ldr.SymType(s) == sym.SDYNIMPORT)
            {
                return SymKindUndef;
            }

            if (ldr.AttrCgoExport(s))
            {
                return SymKindExtdef;
            }

            return SymKindLocal;

        }

        private static void collectmachosyms(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader;

            Action<loader.Sym> addsym = s =>
            {
                sortsym = append(sortsym, s);
                nkind[symkind(_addr_ldr, s)]++;
            } 

            // Add special runtime.text and runtime.etext symbols.
            // We've already included this symbol in Textp on darwin if ctxt.DynlinkingGo().
            // See data.go:/textaddress
; 

            // Add special runtime.text and runtime.etext symbols.
            // We've already included this symbol in Textp on darwin if ctxt.DynlinkingGo().
            // See data.go:/textaddress
            if (!ctxt.DynlinkingGo())
            {
                var s = ldr.Lookup("runtime.text", 0L);
                if (ldr.SymType(s) == sym.STEXT)
                {
                    addsym(s);
                }

                s = ldr.Lookup("runtime.etext", 0L);
                if (ldr.SymType(s) == sym.STEXT)
                {
                    addsym(s);
                }

            } 

            // Add text symbols.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp2)
                {
                    s = __s;
                    addsym(s);
                }

                s = s__prev1;
            }

            Func<loader.Sym, bool> shouldBeInSymbolTable = s =>
            {
                if (ldr.AttrNotInSymbolTable(s))
                {
                    return false;
                }

                var name = ldr.RawSymName(s); // TODO: try not to read the name
                if (name == "" || name[0L] == '.')
                {
                    return false;
                }

                return true;

            } 

            // Add data symbols and external references.
; 

            // Add data symbols and external references.
            {
                var s__prev1 = s;

                for (s = loader.Sym(1L); s < loader.Sym(ldr.NSym()); s++)
                {
                    if (!ldr.AttrReachable(s))
                    {
                        continue;
                    }

                    var t = ldr.SymType(s);
                    if (t >= sym.SELFRXSECT && t < sym.SXREF || t == sym.SCONST)
                    { // data sections handled in dodata
                        if (t == sym.STLSBSS)
                        { 
                            // TLSBSS is not used on darwin. See data.go:allocateDataSections
                            continue;

                        }

                        if (!shouldBeInSymbolTable(s))
                        {
                            continue;
                        }

                        addsym(s);

                    }


                    if (t == sym.SDYNIMPORT || t == sym.SHOSTOBJ || t == sym.SUNDEFEXT) 
                        addsym(s);
                    // Some 64-bit functions have a "$INODE64" or "$INODE64$UNIX2003" suffix.
                    if (t == sym.SDYNIMPORT && ldr.SymDynimplib(s) == "/usr/lib/libSystem.B.dylib")
                    { 
                        // But only on macOS.
                        if (machoPlatform == PLATFORM_MACOS)
                        {
                            {
                                var n = ldr.SymExtname(s);

                                switch (n)
                                {
                                    case "fdopendir": 
                                        switch (objabi.GOARCH)
                                        {
                                            case "amd64": 
                                                ldr.SetSymExtname(s, n + "$INODE64");
                                                break;
                                            case "386": 
                                                ldr.SetSymExtname(s, n + "$INODE64$UNIX2003");
                                                break;
                                        }
                                        break;
                                    case "readdir_r": 

                                    case "getfsstat": 
                                        switch (objabi.GOARCH)
                                        {
                                            case "amd64": 

                                            case "386": 
                                                ldr.SetSymExtname(s, n + "$INODE64");
                                                break;
                                        }
                                        break;
                                }
                            }

                        }

                    }

                }


                s = s__prev1;
            }

            nsortsym = len(sortsym);

        }

        private static void machosymorder(ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ldr = ctxt.loader; 

            // On Mac OS X Mountain Lion, we must sort exported symbols
            // So we sort them here and pre-allocate dynid for them
            // See https://golang.org/issue/4029
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.dynexp2)
                {
                    s = __s;
                    if (!ldr.AttrReachable(s))
                    {
                        panic("dynexp symbol is not reachable");
                    }

                }

                s = s__prev1;
            }

            collectmachosyms(_addr_ctxt);
            sort.Slice(sortsym[..nsortsym], (i, j) =>
            {
                var s1 = sortsym[i];
                var s2 = sortsym[j];
                var k1 = symkind(_addr_ldr, s1);
                var k2 = symkind(_addr_ldr, s2);
                if (k1 != k2)
                {
                    return k1 < k2;
                }

                return ldr.SymExtname(s1) < ldr.SymExtname(s2); // Note: unnamed symbols are not added in collectmachosyms
            });
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in sortsym)
                {
                    i = __i;
                    s = __s;
                    ldr.SetSymDynid(s, int32(i));
                }

                s = s__prev1;
            }
        });

        // machoShouldExport reports whether a symbol needs to be exported.
        //
        // When dynamically linking, all non-local variables and plugin-exported
        // symbols need to be exported.
        private static bool machoShouldExport(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (!ctxt.DynlinkingGo() || s.Attr.Local())
            {
                return false;
            }

            if (ctxt.BuildMode == BuildModePlugin && strings.HasPrefix(s.Extname(), objabi.PathToPrefix(flagPluginPath.val)))
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

            return s.Type >= sym.SFirstWritable; // only writable sections
        }

        private static void machosymtab(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var symtab = ctxt.Syms.Lookup(".machosymtab", 0L);
            var symstr = ctxt.Syms.Lookup(".machosymstr", 0L);

            for (long i = 0L; i < nsortsym; i++)
            {
                var s = ctxt.loader.Syms[sortsym[i]];
                symtab.AddUint32(ctxt.Arch, uint32(symstr.Size));

                var export = machoShouldExport(_addr_ctxt, _addr_s);
                var isGoSymbol = strings.Contains(s.Extname(), "."); 

                // In normal buildmodes, only add _ to C symbols, as
                // Go symbols have dot in the name.
                //
                // Do not export C symbols in plugins, as runtime C
                // symbols like crosscall2 are in pclntab and end up
                // pointing at the host binary, breaking unwinding.
                // See Issue #18190.
                var cexport = !isGoSymbol && (ctxt.BuildMode != BuildModePlugin || onlycsymbol(s.Name));
                if (cexport || export || isGoSymbol)
                {
                    symstr.AddUint8('_');
                } 

                // replace "·" as ".", because DTrace cannot handle it.
                Addstring(symstr, strings.Replace(s.Extname(), "·", ".", -1L));

                if (s.Type == sym.SDYNIMPORT || s.Type == sym.SHOSTOBJ || s.Type == sym.SUNDEFEXT)
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

        private static void machodysymtab(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var ml = newMachoLoad(_addr_ctxt.Arch, LC_DYSYMTAB, 18L);

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

        public static long Domacholink(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            machosymtab(_addr_ctxt); 

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
                linkoff = Rnd(int64(uint64(HEADR) + Segtext.Length), int64(FlagRound.val)) + Rnd(int64(Segdata.Filelen), int64(FlagRound.val)) + Rnd(int64(Segdwarf.Filelen), int64(FlagRound.val));
                ctxt.Out.SeekSet(linkoff);

                ctxt.Out.Write(s1.P[..s1.Size]);
                ctxt.Out.Write(s2.P[..s2.Size]);
                ctxt.Out.Write(s3.P[..s3.Size]);
                ctxt.Out.Write(s4.P[..s4.Size]);
            }

            return Rnd(int64(size), int64(FlagRound.val));

        }

        private static void machorelocsect(ptr<Link> _addr_ctxt, ptr<sym.Section> _addr_sect, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Section sect = ref _addr_sect.val;
 
            // If main section has no bits, nothing to relocate.
            if (sect.Vaddr >= sect.Seg.Vaddr + sect.Seg.Filelen)
            {
                return ;
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

                    foreach (var (ri) in s.R)
                    {
                        var r = _addr_s.R[ri];
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

                        if (!thearch.Machoreloc1(ctxt.Arch, ctxt.Out, s, r, int64(uint64(s.Value + int64(r.Off)) - sect.Vaddr)))
                        {
                            Errorf(s, "unsupported obj reloc %d (%s)/%d to %s", r.Type, sym.RelocName(ctxt.Arch, r.Type), r.Siz, r.Sym.Name);
                        }

                    }

                }

                s = s__prev1;
            }

            sect.Rellen = uint64(ctxt.Out.Offset()) - sect.Reloff;

        }

        public static void Machoemitreloc(ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            while (ctxt.Out.Offset() & 7L != 0L)
            {
                ctxt.Out.Write8(0L);
            }


            machorelocsect(_addr_ctxt, _addr_Segtext.Sections[0L], ctxt.Textp);
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segtext.Sections[1L..])
                {
                    sect = __sect;
                    machorelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                }

                sect = sect__prev1;
            }

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in Segdata.Sections)
                {
                    sect = __sect;
                    machorelocsect(_addr_ctxt, _addr_sect, ctxt.datap);
                }

                sect = sect__prev1;
            }

            for (long i = 0L; i < len(Segdwarf.Sections); i++)
            {
                var sect = Segdwarf.Sections[i];
                var si = dwarfp[i];
                if (si.secSym() != sect.Sym || si.secSym().Sect != sect)
                {
                    panic("inconsistency between dwarfp and Segdwarf");
                }

                machorelocsect(_addr_ctxt, _addr_sect, si.syms);

            }


        });

        // hostobjMachoPlatform returns the first platform load command found
        // in the host object, if any.
        private static (ptr<MachoPlatformLoad>, error) hostobjMachoPlatform(ptr<Hostobj> _addr_h) => func((defer, _, __) =>
        {
            ptr<MachoPlatformLoad> _p0 = default!;
            error _p0 = default!;
            ref Hostobj h = ref _addr_h.val;

            var (f, err) = os.Open(h.file);
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("%s: failed to open host object: %v\n", h.file, err))!);
            }

            defer(f.Close());
            var sr = io.NewSectionReader(f, h.off, h.length);
            var (m, err) = macho.NewFile(sr);
            if (err != null)
            { 
                // Not a valid Mach-O file.
                return (_addr_null!, error.As(null!)!);

            }

            return _addr_peekMachoPlatform(_addr_m)!;

        });

        // peekMachoPlatform returns the first LC_VERSION_MIN_* or LC_BUILD_VERSION
        // load command found in the Mach-O file, if any.
        private static (ptr<MachoPlatformLoad>, error) peekMachoPlatform(ptr<macho.File> _addr_m)
        {
            ptr<MachoPlatformLoad> _p0 = default!;
            error _p0 = default!;
            ref macho.File m = ref _addr_m.val;

            foreach (var (_, cmd) in m.Loads)
            {
                var raw = cmd.Raw();
                MachoLoad ml = new MachoLoad(type_:m.ByteOrder.Uint32(raw),); 
                // Skip the type and command length.
                var data = raw[8L..];
                MachoPlatform p = default;

                if (ml.type_ == LC_VERSION_MIN_IPHONEOS) 
                    p = PLATFORM_IOS;
                else if (ml.type_ == LC_VERSION_MIN_MACOSX) 
                    p = PLATFORM_MACOS;
                else if (ml.type_ == LC_VERSION_MIN_WATCHOS) 
                    p = PLATFORM_WATCHOS;
                else if (ml.type_ == LC_VERSION_MIN_TVOS) 
                    p = PLATFORM_TVOS;
                else if (ml.type_ == LC_BUILD_VERSION) 
                    p = MachoPlatform(m.ByteOrder.Uint32(data));
                else 
                    continue;
                                ml.data = make_slice<uint>(len(data) / 4L);
                var r = bytes.NewReader(data);
                {
                    var err = binary.Read(r, m.ByteOrder, _addr_ml.data);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                }

                return (addr(new MachoPlatformLoad(platform:p,cmd:ml,)), error.As(null!)!);

            }
            return (_addr_null!, error.As(null!)!);

        }
    }
}}}}
