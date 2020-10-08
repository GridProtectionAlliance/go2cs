// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build 386 amd64 arm arm64 mips64 mips64le ppc64 ppc64le

// package runtime -- go2cs converted at 2020 October 08 03:24:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // Look up symbols in the Linux vDSO.

        // This code was originally based on the sample Linux vDSO parser at
        // https://git.kernel.org/cgit/linux/kernel/git/torvalds/linux.git/tree/tools/testing/selftests/vDSO/parse_vdso.c

        // This implements the ELF dynamic linking spec at
        // http://sco.com/developers/gabi/latest/ch5.dynamic.html

        // The version section is documented at
        // https://refspecs.linuxfoundation.org/LSB_3.2.0/LSB-Core-generic/LSB-Core-generic/symversion.html
        private static readonly long _AT_SYSINFO_EHDR = (long)33L;

        private static readonly long _PT_LOAD = (long)1L; /* Loadable program segment */
        private static readonly long _PT_DYNAMIC = (long)2L;        /* Dynamic linking information */

        private static readonly long _DT_NULL = (long)0L; /* Marks end of dynamic section */
        private static readonly long _DT_HASH = (long)4L; /* Dynamic symbol hash table */
        private static readonly long _DT_STRTAB = (long)5L; /* Address of string table */
        private static readonly long _DT_SYMTAB = (long)6L; /* Address of symbol table */
        private static readonly ulong _DT_GNU_HASH = (ulong)0x6ffffef5UL; /* GNU-style dynamic symbol hash table */
        private static readonly ulong _DT_VERSYM = (ulong)0x6ffffff0UL;
        private static readonly ulong _DT_VERDEF = (ulong)0x6ffffffcUL;

        private static readonly ulong _VER_FLG_BASE = (ulong)0x1UL;        /* Version definition of file itself */

        private static readonly long _SHN_UNDEF = (long)0L;        /* Undefined section */

        private static readonly long _SHT_DYNSYM = (long)11L;        /* Dynamic linker symbol table */

        private static readonly long _STT_FUNC = (long)2L;        /* Symbol is a code object */

        private static readonly long _STT_NOTYPE = (long)0L;        /* Symbol type is not specified */

        private static readonly long _STB_GLOBAL = (long)1L; /* Global symbol */
        private static readonly long _STB_WEAK = (long)2L;        /* Weak symbol */

        private static readonly long _EI_NIDENT = (long)16L; 

        // Maximum indices for the array types used when traversing the vDSO ELF structures.
        // Computed from architecture-specific max provided by vdso_linux_*.go
        private static readonly var vdsoSymTabSize = (var)vdsoArrayMax / @unsafe.Sizeof(new elfSym());
        private static readonly var vdsoDynSize = (var)vdsoArrayMax / @unsafe.Sizeof(new elfDyn());
        private static readonly var vdsoSymStringsSize = (var)vdsoArrayMax; // byte
        private static readonly var vdsoVerSymSize = (var)vdsoArrayMax / 2L; // uint16
        private static readonly var vdsoHashSize = (var)vdsoArrayMax / 4L; // uint32

        // vdsoBloomSizeScale is a scaling factor for gnuhash tables which are uint32 indexed,
        // but contain uintptrs
        private static readonly var vdsoBloomSizeScale = (var)@unsafe.Sizeof(uintptr(0L)) / 4L; // uint32

        /* How to extract and insert information held in the st_info field.  */
        private static byte _ELF_ST_BIND(byte val)
        {
            return val >> (int)(4L);
        }
        private static byte _ELF_ST_TYPE(byte val)
        {
            return val & 0xfUL;
        }

        private partial struct vdsoSymbolKey
        {
            public @string name;
            public uint symHash;
            public uint gnuHash;
            public ptr<System.UIntPtr> ptr;
        }

        private partial struct vdsoVersionKey
        {
            public @string version;
            public uint verHash;
        }

        private partial struct vdsoInfo
        {
            public bool valid; /* Load information */
            public System.UIntPtr loadAddr;
            public System.UIntPtr loadOffset; /* loadAddr - recorded vaddr */

/* Symbol table */
            public ptr<array<elfSym>> symtab;
            public ptr<array<byte>> symstrings;
            public slice<uint> chain;
            public slice<uint> bucket;
            public uint symOff;
            public bool isGNUHash; /* Version table */
            public ptr<array<ushort>> versym;
            public ptr<elfVerdef> verdef;
        }

        // see vdso_linux_*.go for vdsoSymbolKeys[] and vdso*Sym vars

        private static void vdsoInitFromSysinfoEhdr(ptr<vdsoInfo> _addr_info, ptr<elfEhdr> _addr_hdr)
        {
            ref vdsoInfo info = ref _addr_info.val;
            ref elfEhdr hdr = ref _addr_hdr.val;

            info.valid = false;
            info.loadAddr = uintptr(@unsafe.Pointer(hdr));

            var pt = @unsafe.Pointer(info.loadAddr + uintptr(hdr.e_phoff)); 

            // We need two things from the segment table: the load offset
            // and the dynamic table.
            bool foundVaddr = default;
            ptr<array<elfDyn>> dyn;
            {
                var i__prev1 = i;

                for (var i = uint16(0L); i < hdr.e_phnum; i++)
                {
                    pt = (elfPhdr.val)(add(pt, uintptr(i) * @unsafe.Sizeof(new elfPhdr())));

                    if (pt.p_type == _PT_LOAD) 
                        if (!foundVaddr)
                        {
                            foundVaddr = true;
                            info.loadOffset = info.loadAddr + uintptr(pt.p_offset - pt.p_vaddr);
                        }

                    else if (pt.p_type == _PT_DYNAMIC) 
                        dyn = new ptr<ptr<array<elfDyn>>>(@unsafe.Pointer(info.loadAddr + uintptr(pt.p_offset)));
                    
                }


                i = i__prev1;
            }

            if (!foundVaddr || dyn == null)
            {
                return ; // Failed
            } 

            // Fish out the useful bits of the dynamic table.
            ptr<array<uint>> hash;            ptr<array<uint>> gnuhash;

            info.symstrings = null;
            info.symtab = null;
            info.versym = null;
            info.verdef = null;
            {
                var i__prev1 = i;

                for (i = 0L; dyn[i].d_tag != _DT_NULL; i++)
                {
                    var dt = _addr_dyn[i];
                    var p = info.loadOffset + uintptr(dt.d_val);

                    if (dt.d_tag == _DT_STRTAB) 
                        info.symstrings = new ptr<ptr<array<byte>>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_SYMTAB) 
                        info.symtab = new ptr<ptr<array<elfSym>>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_HASH) 
                        hash = new ptr<ptr<array<uint>>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_GNU_HASH) 
                        gnuhash = new ptr<ptr<array<uint>>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_VERSYM) 
                        info.versym = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_VERDEF) 
                        info.verdef = (elfVerdef.val)(@unsafe.Pointer(p));
                    
                }


                i = i__prev1;
            }

            if (info.symstrings == null || info.symtab == null || (hash == null && gnuhash == null))
            {
                return ; // Failed
            }

            if (info.verdef == null)
            {
                info.versym = null;
            }

            if (gnuhash != null)
            { 
                // Parse the GNU hash table header.
                var nbucket = gnuhash[0L];
                info.symOff = gnuhash[1L];
                var bloomSize = gnuhash[2L];
                info.bucket = gnuhash[4L + bloomSize * uint32(vdsoBloomSizeScale)..][..nbucket];
                info.chain = gnuhash[4L + bloomSize * uint32(vdsoBloomSizeScale) + nbucket..];
                info.isGNUHash = true;

            }
            else
            { 
                // Parse the hash table header.
                nbucket = hash[0L];
                var nchain = hash[1L];
                info.bucket = hash[2L..2L + nbucket];
                info.chain = hash[2L + nbucket..2L + nbucket + nchain];

            } 

            // That's all we need.
            info.valid = true;

        }

        private static int vdsoFindVersion(ptr<vdsoInfo> _addr_info, ptr<vdsoVersionKey> _addr_ver)
        {
            ref vdsoInfo info = ref _addr_info.val;
            ref vdsoVersionKey ver = ref _addr_ver.val;

            if (!info.valid)
            {
                return 0L;
            }

            var def = info.verdef;
            while (true)
            {
                if (def.vd_flags & _VER_FLG_BASE == 0L)
                {
                    var aux = (elfVerdaux.val)(add(@unsafe.Pointer(def), uintptr(def.vd_aux)));
                    if (def.vd_hash == ver.verHash && ver.version == gostringnocopy(_addr_info.symstrings[aux.vda_name]))
                    {
                        return int32(def.vd_ndx & 0x7fffUL);
                    }

                }

                if (def.vd_next == 0L)
                {
                    break;
                }

                def = (elfVerdef.val)(add(@unsafe.Pointer(def), uintptr(def.vd_next)));

            }


            return -1L; // cannot match any version
        }

        private static void vdsoParseSymbols(ptr<vdsoInfo> _addr_info, int version)
        {
            ref vdsoInfo info = ref _addr_info.val;

            if (!info.valid)
            {
                return ;
            }

            Func<uint, vdsoSymbolKey, bool> apply = (symIndex, k) =>
            {
                var sym = _addr_info.symtab[symIndex];
                var typ = _ELF_ST_TYPE(sym.st_info);
                var bind = _ELF_ST_BIND(sym.st_info); 
                // On ppc64x, VDSO functions are of type _STT_NOTYPE.
                if (typ != _STT_FUNC && typ != _STT_NOTYPE || bind != _STB_GLOBAL && bind != _STB_WEAK || sym.st_shndx == _SHN_UNDEF)
                {
                    return false;
                }

                if (k.name != gostringnocopy(_addr_info.symstrings[sym.st_name]))
                {
                    return false;
                } 
                // Check symbol version.
                if (info.versym != null && version != 0L && int32(info.versym[symIndex] & 0x7fffUL) != version)
                {
                    return false;
                }

                k.ptr.val = info.loadOffset + uintptr(sym.st_value);
                return true;

            }
;

            if (!info.isGNUHash)
            { 
                // Old-style DT_HASH table.
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in vdsoSymbolKeys)
                    {
                        k = __k;
                        {
                            var chain = info.bucket[k.symHash % uint32(len(info.bucket))];

                            while (chain != 0L)
                            {
                                if (apply(chain, k))
                                {
                                    break;
                                chain = info.chain[chain];
                                }

                            }

                        }

                    }

                    k = k__prev1;
                }

                return ;

            } 

            // New-style DT_GNU_HASH table.
            {
                var k__prev1 = k;

                foreach (var (_, __k) in vdsoSymbolKeys)
                {
                    k = __k;
                    var symIndex = info.bucket[k.gnuHash % uint32(len(info.bucket))];
                    if (symIndex < info.symOff)
                    {
                        continue;
                    }

                    while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                    {
                        var hash = info.chain[symIndex - info.symOff];
                        if (hash | 1L == k.gnuHash | 1L)
                        { 
                            // Found a hash match.
                            if (apply(symIndex, k))
                            {
                                break;
                        symIndex++;
                            }

                        }

                        if (hash & 1L != 0L)
                        { 
                            // End of chain.
                            break;

                        }

                    }


                }

                k = k__prev1;
            }
        }

        private static void vdsoauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_SYSINFO_EHDR) 
                if (val == 0L)
                { 
                    // Something went wrong
                    return ;

                }

                ref vdsoInfo info = ref heap(out ptr<vdsoInfo> _addr_info); 
                // TODO(rsc): I don't understand why the compiler thinks info escapes
                // when passed to the three functions below.
                var info1 = (vdsoInfo.val)(noescape(@unsafe.Pointer(_addr_info)));
                vdsoInitFromSysinfoEhdr(_addr_info1, _addr_(elfEhdr.val)(@unsafe.Pointer(val)));
                vdsoParseSymbols(_addr_info1, vdsoFindVersion(_addr_info1, _addr_vdsoLinuxVersion));
            
        }

        // vdsoMarker reports whether PC is on the VDSO page.
        //go:nosplit
        private static bool inVDSOPage(System.UIntPtr pc)
        {
            foreach (var (_, k) in vdsoSymbolKeys)
            {
                if (k.ptr != 0L.val)
                {
                    var page = k.ptr & ~(physPageSize - 1L).val;
                    return pc >= page && pc < page + physPageSize;
                }

            }
            return false;

        }
    }
}
