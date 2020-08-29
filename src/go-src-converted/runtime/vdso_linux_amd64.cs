// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_linux_amd64.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // ELF64 structure definitions for use by the Linux vDSO loader
        private partial struct elfSym
        {
            public uint st_name;
            public byte st_info;
            public byte st_other;
            public ushort st_shndx;
            public ulong st_value;
            public ulong st_size;
        }

        private partial struct elfVerdef
        {
            public ushort vd_version; /* Version revision */
            public ushort vd_flags; /* Version information */
            public ushort vd_ndx; /* Version Index */
            public ushort vd_cnt; /* Number of associated aux entries */
            public uint vd_hash; /* Version name hash value */
            public uint vd_aux; /* Offset in bytes to verdaux array */
            public uint vd_next; /* Offset in bytes to next verdef entry */
        }

        private partial struct elfEhdr
        {
            public array<byte> e_ident; /* Magic number and other info */
            public ushort e_type; /* Object file type */
            public ushort e_machine; /* Architecture */
            public uint e_version; /* Object file version */
            public ulong e_entry; /* Entry point virtual address */
            public ulong e_phoff; /* Program header table file offset */
            public ulong e_shoff; /* Section header table file offset */
            public uint e_flags; /* Processor-specific flags */
            public ushort e_ehsize; /* ELF header size in bytes */
            public ushort e_phentsize; /* Program header table entry size */
            public ushort e_phnum; /* Program header table entry count */
            public ushort e_shentsize; /* Section header table entry size */
            public ushort e_shnum; /* Section header table entry count */
            public ushort e_shstrndx; /* Section header string table index */
        }

        private partial struct elfPhdr
        {
            public uint p_type; /* Segment type */
            public uint p_flags; /* Segment flags */
            public ulong p_offset; /* Segment file offset */
            public ulong p_vaddr; /* Segment virtual address */
            public ulong p_paddr; /* Segment physical address */
            public ulong p_filesz; /* Segment size in file */
            public ulong p_memsz; /* Segment size in memory */
            public ulong p_align; /* Segment alignment */
        }

        private partial struct elfShdr
        {
            public uint sh_name; /* Section name (string tbl index) */
            public uint sh_type; /* Section type */
            public ulong sh_flags; /* Section flags */
            public ulong sh_addr; /* Section virtual addr at execution */
            public ulong sh_offset; /* Section file offset */
            public ulong sh_size; /* Section size in bytes */
            public uint sh_link; /* Link to another section */
            public uint sh_info; /* Additional section information */
            public ulong sh_addralign; /* Section alignment */
            public ulong sh_entsize; /* Entry size if section holds table */
        }

        private partial struct elfDyn
        {
            public long d_tag; /* Dynamic entry type */
            public ulong d_val; /* Integer value */
        }

        private partial struct elfVerdaux
        {
            public uint vda_name; /* Version or dependency names */
            public uint vda_next; /* Offset in bytes to next verdaux entry */
        }

 
        // vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
        // See cmd/compile/internal/amd64/galign.go arch.MAXWIDTH initialization.
        private static readonly long vdsoArrayMax = 1L << (int)(50L) - 1L;

        private static symbol_key sym_keys = new slice<symbol_key>(new symbol_key[] { {"__vdso_time",0xa33c485,0x821e8e0d,&__vdso_time_sym}, {"__vdso_gettimeofday",0x315ca59,0xb01bca00,&__vdso_gettimeofday_sym}, {"__vdso_clock_gettime",0xd35ec75,0x6e43a318,&__vdso_clock_gettime_sym} });

        // initialize with vsyscall fallbacks
        private static System.UIntPtr __vdso_time_sym = 0xffffffffff600400UL;        private static System.UIntPtr __vdso_gettimeofday_sym = 0xffffffffff600000UL;        private static System.UIntPtr __vdso_clock_gettime_sym = 0L;
    }
}
