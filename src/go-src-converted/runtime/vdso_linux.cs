// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build 386 amd64

// package runtime -- go2cs converted at 2020 August 29 08:21:37 UTC
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
        // http://refspecs.linuxfoundation.org/LSB_3.2.0/LSB-Core-generic/LSB-Core-generic/symversion.html
        private static readonly long _AT_SYSINFO_EHDR = 33L;

        private static readonly long _PT_LOAD = 1L; /* Loadable program segment */
        private static readonly long _PT_DYNAMIC = 2L;        /* Dynamic linking information */

        private static readonly long _DT_NULL = 0L; /* Marks end of dynamic section */
        private static readonly long _DT_HASH = 4L; /* Dynamic symbol hash table */
        private static readonly long _DT_STRTAB = 5L; /* Address of string table */
        private static readonly long _DT_SYMTAB = 6L; /* Address of symbol table */
        private static readonly ulong _DT_GNU_HASH = 0x6ffffef5UL; /* GNU-style dynamic symbol hash table */
        private static readonly ulong _DT_VERSYM = 0x6ffffff0UL;
        private static readonly ulong _DT_VERDEF = 0x6ffffffcUL;

        private static readonly ulong _VER_FLG_BASE = 0x1UL;        /* Version definition of file itself */

        private static readonly long _SHN_UNDEF = 0L;        /* Undefined section */

        private static readonly long _SHT_DYNSYM = 11L;        /* Dynamic linker symbol table */

        private static readonly long _STT_FUNC = 2L;        /* Symbol is a code object */

        private static readonly long _STB_GLOBAL = 1L; /* Global symbol */
        private static readonly long _STB_WEAK = 2L;        /* Weak symbol */

        private static readonly long _EI_NIDENT = 16L; 

        // Maximum indices for the array types used when traversing the vDSO ELF structures.
        // Computed from architecture-specific max provided by vdso_linux_*.go
        private static readonly var vdsoSymTabSize = vdsoArrayMax / @unsafe.Sizeof(new elfSym());
        private static readonly var vdsoDynSize = vdsoArrayMax / @unsafe.Sizeof(new elfDyn());
        private static readonly var vdsoSymStringsSize = vdsoArrayMax; // byte
        private static readonly var vdsoVerSymSize = vdsoArrayMax / 2L; // uint16
        private static readonly var vdsoHashSize = vdsoArrayMax / 4L; // uint32

        // vdsoBloomSizeScale is a scaling factor for gnuhash tables which are uint32 indexed,
        // but contain uintptrs
        private static readonly var vdsoBloomSizeScale = @unsafe.Sizeof(uintptr(0L)) / 4L; // uint32

        /* How to extract and insert information held in the st_info field.  */
        private static byte _ELF_ST_BIND(byte val)
        {
            return val >> (int)(4L);
        }
        private static byte _ELF_ST_TYPE(byte val)
        {
            return val & 0xfUL;
        }

        private partial struct symbol_key
        {
            public @string name;
            public uint sym_hash;
            public uint gnu_hash;
            public ptr<System.UIntPtr> ptr;
        }

        private partial struct version_key
        {
            public @string version;
            public uint ver_hash;
        }

        private partial struct vdso_info
        {
            public bool valid; /* Load information */
            public System.UIntPtr load_addr;
            public System.UIntPtr load_offset; /* load_addr - recorded vaddr */

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

        private static version_key linux26 = new version_key("LINUX_2.6",0x3ae75f6);

        // see vdso_linux_*.go for sym_keys[] and __vdso_* vars

        private static void vdso_init_from_sysinfo_ehdr(ref vdso_info info, ref elfEhdr hdr)
        {
            info.valid = false;
            info.load_addr = uintptr(@unsafe.Pointer(hdr));

            var pt = @unsafe.Pointer(info.load_addr + uintptr(hdr.e_phoff)); 

            // We need two things from the segment table: the load offset
            // and the dynamic table.
            bool found_vaddr = default;
            ref array<elfDyn> dyn = default;
            {
                var i__prev1 = i;

                for (var i = uint16(0L); i < hdr.e_phnum; i++)
                {
                    pt = (elfPhdr.Value)(add(pt, uintptr(i) * @unsafe.Sizeof(new elfPhdr())));

                    if (pt.p_type == _PT_LOAD) 
                        if (!found_vaddr)
                        {
                            found_vaddr = true;
                            info.load_offset = info.load_addr + uintptr(pt.p_offset - pt.p_vaddr);
                        }
                    else if (pt.p_type == _PT_DYNAMIC) 
                        dyn = new ptr<ref array<elfDyn>>(@unsafe.Pointer(info.load_addr + uintptr(pt.p_offset)));
                                    }


                i = i__prev1;
            }

            if (!found_vaddr || dyn == null)
            {
                return; // Failed
            } 

            // Fish out the useful bits of the dynamic table.
            ref array<uint> hash = default;            ref array<uint> gnuhash = default;

            info.symstrings = null;
            info.symtab = null;
            info.versym = null;
            info.verdef = null;
            {
                var i__prev1 = i;

                for (i = 0L; dyn[i].d_tag != _DT_NULL; i++)
                {
                    var dt = ref dyn[i];
                    var p = info.load_offset + uintptr(dt.d_val);

                    if (dt.d_tag == _DT_STRTAB) 
                        info.symstrings = new ptr<ref array<byte>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_SYMTAB) 
                        info.symtab = new ptr<ref array<elfSym>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_HASH) 
                        hash = new ptr<ref array<uint>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_GNU_HASH) 
                        gnuhash = new ptr<ref array<uint>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_VERSYM) 
                        info.versym = new ptr<ref array<ushort>>(@unsafe.Pointer(p));
                    else if (dt.d_tag == _DT_VERDEF) 
                        info.verdef = (elfVerdef.Value)(@unsafe.Pointer(p));
                                    }


                i = i__prev1;
            }

            if (info.symstrings == null || info.symtab == null || (hash == null && gnuhash == null))
            {
                return; // Failed
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

        private static int vdso_find_version(ref vdso_info info, ref version_key ver)
        {
            if (!info.valid)
            {
                return 0L;
            }
            var def = info.verdef;
            while (true)
            {
                if (def.vd_flags & _VER_FLG_BASE == 0L)
                {
                    var aux = (elfVerdaux.Value)(add(@unsafe.Pointer(def), uintptr(def.vd_aux)));
                    if (def.vd_hash == ver.ver_hash && ver.version == gostringnocopy(ref info.symstrings[aux.vda_name]))
                    {
                        return int32(def.vd_ndx & 0x7fffUL);
                    }
                }
                if (def.vd_next == 0L)
                {
                    break;
                }
                def = (elfVerdef.Value)(add(@unsafe.Pointer(def), uintptr(def.vd_next)));
            }


            return -1L; // cannot match any version
        }

        private static void vdso_parse_symbols(ref vdso_info info, int version)
        {
            if (!info.valid)
            {
                return;
            }
            Func<uint, symbol_key, bool> apply = (symIndex, k) =>
            {
                var sym = ref info.symtab[symIndex];
                var typ = _ELF_ST_TYPE(sym.st_info);
                var bind = _ELF_ST_BIND(sym.st_info);
                if (typ != _STT_FUNC || bind != _STB_GLOBAL && bind != _STB_WEAK || sym.st_shndx == _SHN_UNDEF)
                {
                    return false;
                }
                if (k.name != gostringnocopy(ref info.symstrings[sym.st_name]))
                {
                    return false;
                } 

                // Check symbol version.
                if (info.versym != null && version != 0L && int32(info.versym[symIndex] & 0x7fffUL) != version)
                {
                    return false;
                }
                k.ptr.Value = info.load_offset + uintptr(sym.st_value);
                return true;
            }
;

            if (!info.isGNUHash)
            { 
                // Old-style DT_HASH table.
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in sym_keys)
                    {
                        k = __k;
                        {
                            var chain = info.bucket[k.sym_hash % uint32(len(info.bucket))];

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

                return;
            } 

            // New-style DT_GNU_HASH table.
            {
                var k__prev1 = k;

                foreach (var (_, __k) in sym_keys)
                {
                    k = __k;
                    var symIndex = info.bucket[k.gnu_hash % uint32(len(info.bucket))];
                    if (symIndex < info.symOff)
                    {
                        continue;
                    }
                    while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                    {
                        var hash = info.chain[symIndex - info.symOff];
                        if (hash | 1L == k.gnu_hash | 1L)
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

        private static void archauxv(System.UIntPtr tag, System.UIntPtr val)
        {

            if (tag == _AT_SYSINFO_EHDR) 
                if (val == 0L)
                { 
                    // Something went wrong
                    return;
                }
                vdso_info info = default; 
                // TODO(rsc): I don't understand why the compiler thinks info escapes
                // when passed to the three functions below.
                var info1 = (vdso_info.Value)(noescape(@unsafe.Pointer(ref info)));
                vdso_init_from_sysinfo_ehdr(info1, (elfEhdr.Value)(@unsafe.Pointer(val)));
                vdso_parse_symbols(info1, vdso_find_version(info1, ref linux26));
                    }
    }
}
