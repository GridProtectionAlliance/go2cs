// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadpe implements a PE/COFF file reader.
// package loadpe -- go2cs converted at 2020 October 09 05:52:09 UTC
// import "cmd/oldlink/internal/loadpe" ==> using loadpe = go.cmd.oldlink.@internal.loadpe_package
// Original source: C:\Go\src\cmd\oldlink\internal\loadpe\ldpe.go
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class loadpe_package
    {
 
        // TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 (same with IMAGE_SYM_DTYPE_POINTER and IMAGE_SYM_DTYPE_FUNCTION)
        public static readonly long IMAGE_SYM_UNDEFINED = (long)0L;
        public static readonly long IMAGE_SYM_ABSOLUTE = (long)-1L;
        public static readonly long IMAGE_SYM_DEBUG = (long)-2L;
        public static readonly long IMAGE_SYM_TYPE_NULL = (long)0L;
        public static readonly long IMAGE_SYM_TYPE_VOID = (long)1L;
        public static readonly long IMAGE_SYM_TYPE_CHAR = (long)2L;
        public static readonly long IMAGE_SYM_TYPE_SHORT = (long)3L;
        public static readonly long IMAGE_SYM_TYPE_INT = (long)4L;
        public static readonly long IMAGE_SYM_TYPE_LONG = (long)5L;
        public static readonly long IMAGE_SYM_TYPE_FLOAT = (long)6L;
        public static readonly long IMAGE_SYM_TYPE_DOUBLE = (long)7L;
        public static readonly long IMAGE_SYM_TYPE_STRUCT = (long)8L;
        public static readonly long IMAGE_SYM_TYPE_UNION = (long)9L;
        public static readonly long IMAGE_SYM_TYPE_ENUM = (long)10L;
        public static readonly long IMAGE_SYM_TYPE_MOE = (long)11L;
        public static readonly long IMAGE_SYM_TYPE_BYTE = (long)12L;
        public static readonly long IMAGE_SYM_TYPE_WORD = (long)13L;
        public static readonly long IMAGE_SYM_TYPE_UINT = (long)14L;
        public static readonly long IMAGE_SYM_TYPE_DWORD = (long)15L;
        public static readonly long IMAGE_SYM_TYPE_PCODE = (long)32768L;
        public static readonly long IMAGE_SYM_DTYPE_NULL = (long)0L;
        public static readonly ulong IMAGE_SYM_DTYPE_POINTER = (ulong)0x10UL;
        public static readonly ulong IMAGE_SYM_DTYPE_FUNCTION = (ulong)0x20UL;
        public static readonly ulong IMAGE_SYM_DTYPE_ARRAY = (ulong)0x30UL;
        public static readonly long IMAGE_SYM_CLASS_END_OF_FUNCTION = (long)-1L;
        public static readonly long IMAGE_SYM_CLASS_NULL = (long)0L;
        public static readonly long IMAGE_SYM_CLASS_AUTOMATIC = (long)1L;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL = (long)2L;
        public static readonly long IMAGE_SYM_CLASS_STATIC = (long)3L;
        public static readonly long IMAGE_SYM_CLASS_REGISTER = (long)4L;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL_DEF = (long)5L;
        public static readonly long IMAGE_SYM_CLASS_LABEL = (long)6L;
        public static readonly long IMAGE_SYM_CLASS_UNDEFINED_LABEL = (long)7L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_STRUCT = (long)8L;
        public static readonly long IMAGE_SYM_CLASS_ARGUMENT = (long)9L;
        public static readonly long IMAGE_SYM_CLASS_STRUCT_TAG = (long)10L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_UNION = (long)11L;
        public static readonly long IMAGE_SYM_CLASS_UNION_TAG = (long)12L;
        public static readonly long IMAGE_SYM_CLASS_TYPE_DEFINITION = (long)13L;
        public static readonly long IMAGE_SYM_CLASS_UNDEFINED_STATIC = (long)14L;
        public static readonly long IMAGE_SYM_CLASS_ENUM_TAG = (long)15L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_ENUM = (long)16L;
        public static readonly long IMAGE_SYM_CLASS_REGISTER_PARAM = (long)17L;
        public static readonly long IMAGE_SYM_CLASS_BIT_FIELD = (long)18L;
        public static readonly long IMAGE_SYM_CLASS_FAR_EXTERNAL = (long)68L; /* Not in PECOFF v8 spec */
        public static readonly long IMAGE_SYM_CLASS_BLOCK = (long)100L;
        public static readonly long IMAGE_SYM_CLASS_FUNCTION = (long)101L;
        public static readonly long IMAGE_SYM_CLASS_END_OF_STRUCT = (long)102L;
        public static readonly long IMAGE_SYM_CLASS_FILE = (long)103L;
        public static readonly long IMAGE_SYM_CLASS_SECTION = (long)104L;
        public static readonly long IMAGE_SYM_CLASS_WEAK_EXTERNAL = (long)105L;
        public static readonly long IMAGE_SYM_CLASS_CLR_TOKEN = (long)107L;
        public static readonly ulong IMAGE_REL_I386_ABSOLUTE = (ulong)0x0000UL;
        public static readonly ulong IMAGE_REL_I386_DIR16 = (ulong)0x0001UL;
        public static readonly ulong IMAGE_REL_I386_REL16 = (ulong)0x0002UL;
        public static readonly ulong IMAGE_REL_I386_DIR32 = (ulong)0x0006UL;
        public static readonly ulong IMAGE_REL_I386_DIR32NB = (ulong)0x0007UL;
        public static readonly ulong IMAGE_REL_I386_SEG12 = (ulong)0x0009UL;
        public static readonly ulong IMAGE_REL_I386_SECTION = (ulong)0x000AUL;
        public static readonly ulong IMAGE_REL_I386_SECREL = (ulong)0x000BUL;
        public static readonly ulong IMAGE_REL_I386_TOKEN = (ulong)0x000CUL;
        public static readonly ulong IMAGE_REL_I386_SECREL7 = (ulong)0x000DUL;
        public static readonly ulong IMAGE_REL_I386_REL32 = (ulong)0x0014UL;
        public static readonly ulong IMAGE_REL_AMD64_ABSOLUTE = (ulong)0x0000UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR64 = (ulong)0x0001UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32 = (ulong)0x0002UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32NB = (ulong)0x0003UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32 = (ulong)0x0004UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_1 = (ulong)0x0005UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_2 = (ulong)0x0006UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_3 = (ulong)0x0007UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_4 = (ulong)0x0008UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_5 = (ulong)0x0009UL;
        public static readonly ulong IMAGE_REL_AMD64_SECTION = (ulong)0x000AUL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL = (ulong)0x000BUL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL7 = (ulong)0x000CUL;
        public static readonly ulong IMAGE_REL_AMD64_TOKEN = (ulong)0x000DUL;
        public static readonly ulong IMAGE_REL_AMD64_SREL32 = (ulong)0x000EUL;
        public static readonly ulong IMAGE_REL_AMD64_PAIR = (ulong)0x000FUL;
        public static readonly ulong IMAGE_REL_AMD64_SSPAN32 = (ulong)0x0010UL;
        public static readonly ulong IMAGE_REL_ARM_ABSOLUTE = (ulong)0x0000UL;
        public static readonly ulong IMAGE_REL_ARM_ADDR32 = (ulong)0x0001UL;
        public static readonly ulong IMAGE_REL_ARM_ADDR32NB = (ulong)0x0002UL;
        public static readonly ulong IMAGE_REL_ARM_BRANCH24 = (ulong)0x0003UL;
        public static readonly ulong IMAGE_REL_ARM_BRANCH11 = (ulong)0x0004UL;
        public static readonly ulong IMAGE_REL_ARM_SECTION = (ulong)0x000EUL;
        public static readonly ulong IMAGE_REL_ARM_SECREL = (ulong)0x000FUL;
        public static readonly ulong IMAGE_REL_ARM_MOV32 = (ulong)0x0010UL;
        public static readonly ulong IMAGE_REL_THUMB_MOV32 = (ulong)0x0011UL;
        public static readonly ulong IMAGE_REL_THUMB_BRANCH20 = (ulong)0x0012UL;
        public static readonly ulong IMAGE_REL_THUMB_BRANCH24 = (ulong)0x0014UL;
        public static readonly ulong IMAGE_REL_THUMB_BLX23 = (ulong)0x0015UL;
        public static readonly ulong IMAGE_REL_ARM_PAIR = (ulong)0x0016UL;


        // TODO(crawshaw): de-duplicate these symbols with cmd/internal/ld, ideally in debug/pe.
        public static readonly ulong IMAGE_SCN_CNT_CODE = (ulong)0x00000020UL;
        public static readonly ulong IMAGE_SCN_CNT_INITIALIZED_DATA = (ulong)0x00000040UL;
        public static readonly ulong IMAGE_SCN_CNT_UNINITIALIZED_DATA = (ulong)0x00000080UL;
        public static readonly ulong IMAGE_SCN_MEM_DISCARDABLE = (ulong)0x02000000UL;
        public static readonly ulong IMAGE_SCN_MEM_EXECUTE = (ulong)0x20000000UL;
        public static readonly ulong IMAGE_SCN_MEM_READ = (ulong)0x40000000UL;
        public static readonly ulong IMAGE_SCN_MEM_WRITE = (ulong)0x80000000UL;


        // TODO(brainman): maybe just add ReadAt method to bio.Reader instead of creating peBiobuf

        // peBiobuf makes bio.Reader look like io.ReaderAt.
        private partial struct peBiobuf // : bio.Reader
        {
        }

        private static (long, error) ReadAt(this ptr<peBiobuf> _addr_f, slice<byte> p, long off)
        {
            long _p0 = default;
            error _p0 = default!;
            ref peBiobuf f = ref _addr_f.val;

            var ret = ((bio.Reader.val)(f)).MustSeek(off, 0L);
            if (ret < 0L)
            {
                return (0L, error.As(errors.New("fail to seek"))!);
            }

            var (n, err) = f.Read(p);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (n, error.As(null!)!);

        }

        public static (slice<ptr<sym.Symbol>>, ptr<sym.Symbol>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
            ptr<sym.Symbol> rsrc = default!;
            error err = default!;
            ref loader.Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader input = ref _addr_input.val;

            Func<@string, long, ptr<sym.Symbol>> lookup = (name, version) =>
            {
                return l.LookupOrCreate(name, version, syms);
            }
;
            return load(_addr_arch, lookup, syms.IncVersion(), _addr_input, pkg, length, pn);

        }

        public static (slice<ptr<sym.Symbol>>, ptr<sym.Symbol>, error) LoadOld(ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
            ptr<sym.Symbol> rsrc = default!;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader input = ref _addr_input.val;

            return load(_addr_arch, syms.Lookup, syms.IncVersion(), _addr_input, pkg, length, pn);
        }

        // load loads the PE file pn from input.
        // Symbols are written into syms, and a slice of the text symbols is returned.
        // If an .rsrc section is found, its symbol is returned as rsrc.
        private static (slice<ptr<sym.Symbol>>, ptr<sym.Symbol>, error) load(ptr<sys.Arch> _addr_arch, Func<@string, long, ptr<sym.Symbol>> lookup, long localSymVersion, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn) => func((defer, _, __) =>
        {
            slice<ptr<sym.Symbol>> textp = default;
            ptr<sym.Symbol> rsrc = default!;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref bio.Reader input = ref _addr_input.val;

            var sectsyms = make_map<ptr<pe.Section>, ptr<sym.Symbol>>();
            var sectdata = make_map<ptr<pe.Section>, slice<byte>>(); 

            // Some input files are archives containing multiple of
            // object files, and pe.NewFile seeks to the start of
            // input file and get confused. Create section reader
            // to stop pe.NewFile looking before current position.
            var sr = io.NewSectionReader((peBiobuf.val)(input), input.Offset(), 1L << (int)(63L) - 1L); 

            // TODO: replace pe.NewFile with pe.Load (grep for "add Load function" in debug/pe for details)
            var (f, err) = pe.NewFile(sr);
            if (err != null)
            {
                return (null, _addr_null!, error.As(err)!);
            }

            defer(f.Close()); 

            // TODO return error if found .cormeta

            // create symbols for mapped sections
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in f.Sections)
                {
                    sect = __sect;
                    if (sect.Characteristics & IMAGE_SCN_MEM_DISCARDABLE != 0L)
                    {
                        continue;
                    }

                    if (sect.Characteristics & (IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_CNT_UNINITIALIZED_DATA) == 0L)
                    { 
                        // This has been seen for .idata sections, which we
                        // want to ignore. See issues 5106 and 5273.
                        continue;

                    }

                    var name = fmt.Sprintf("%s(%s)", pkg, sect.Name);
                    var s = lookup(name, localSymVersion);


                    if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ) //.rdata
                        s.Type = sym.SRODATA;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.bss
                        s.Type = sym.SNOPTRBSS;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.data
                        s.Type = sym.SNOPTRDATA;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ) //.text
                        s.Type = sym.STEXT;
                    else 
                        return (null, _addr_null!, error.As(fmt.Errorf("unexpected flags %#06x for PE section %s", sect.Characteristics, sect.Name))!);
                                        if (s.Type != sym.SNOPTRBSS)
                    {
                        var (data, err) = sect.Data();
                        if (err != null)
                        {
                            return (null, _addr_null!, error.As(err)!);
                        }

                        sectdata[sect] = data;
                        s.P = data;

                    }

                    s.Size = int64(sect.Size);
                    sectsyms[sect] = s;
                    if (sect.Name == ".rsrc")
                    {
                        rsrc = s;
                    }

                } 

                // load relocations

                sect = sect__prev1;
            }

            foreach (var (_, rsect) in f.Sections)
            {
                {
                    var (_, found) = sectsyms[rsect];

                    if (!found)
                    {
                        continue;
                    }

                }

                if (rsect.NumberOfRelocations == 0L)
                {
                    continue;
                }

                if (rsect.Characteristics & IMAGE_SCN_MEM_DISCARDABLE != 0L)
                {
                    continue;
                }

                if (rsect.Characteristics & (IMAGE_SCN_CNT_CODE | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_CNT_UNINITIALIZED_DATA) == 0L)
                { 
                    // This has been seen for .idata sections, which we
                    // want to ignore. See issues 5106 and 5273.
                    continue;

                }

                var rs = make_slice<sym.Reloc>(rsect.NumberOfRelocations);
                foreach (var (j, r) in rsect.Relocs)
                {
                    var rp = _addr_rs[j];
                    if (int(r.SymbolTableIndex) >= len(f.COFFSymbols))
                    {
                        return (null, _addr_null!, error.As(fmt.Errorf("relocation number %d symbol index idx=%d cannot be large then number of symbols %d", j, r.SymbolTableIndex, len(f.COFFSymbols)))!);
                    }

                    var pesym = _addr_f.COFFSymbols[r.SymbolTableIndex];
                    var (gosym, err) = readpesym(_addr_arch, lookup, _addr_f, _addr_pesym, sectsyms, localSymVersion);
                    if (err != null)
                    {
                        return (null, _addr_null!, error.As(err)!);
                    }

                    if (gosym == null)
                    {
                        var (name, err) = pesym.FullName(f.StringTable);
                        if (err != null)
                        {
                            name = string(pesym.Name[..]);
                        }

                        return (null, _addr_null!, error.As(fmt.Errorf("reloc of invalid sym %s idx=%d type=%d", name, r.SymbolTableIndex, pesym.Type))!);

                    }

                    rp.Sym = gosym;
                    rp.Siz = 4L;
                    rp.Off = int32(r.VirtualAddress);

                    if (arch.Family == sys.I386 || arch.Family == sys.AMD64) 

                        if (r.Type == IMAGE_REL_I386_REL32 || r.Type == IMAGE_REL_AMD64_REL32 || r.Type == IMAGE_REL_AMD64_ADDR32 || r.Type == IMAGE_REL_AMD64_ADDR32NB) 
                            rp.Type = objabi.R_PCREL;

                            rp.Add = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][rp.Off..])));
                        else if (r.Type == IMAGE_REL_I386_DIR32NB || r.Type == IMAGE_REL_I386_DIR32) 
                            rp.Type = objabi.R_ADDR; 

                            // load addend from image
                            rp.Add = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][rp.Off..])));
                        else if (r.Type == IMAGE_REL_AMD64_ADDR64) // R_X86_64_64
                            rp.Siz = 8L;

                            rp.Type = objabi.R_ADDR; 

                            // load addend from image
                            rp.Add = int64(binary.LittleEndian.Uint64(sectdata[rsect][rp.Off..]));
                        else 
                            return (null, _addr_null!, error.As(fmt.Errorf("%s: %v: unknown relocation type %v", pn, sectsyms[rsect], r.Type))!);
                                            else if (arch.Family == sys.ARM) 

                        if (r.Type == IMAGE_REL_ARM_SECREL) 
                            rp.Type = objabi.R_PCREL;

                            rp.Add = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][rp.Off..])));
                        else if (r.Type == IMAGE_REL_ARM_ADDR32) 
                            rp.Type = objabi.R_ADDR;

                            rp.Add = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][rp.Off..])));
                        else if (r.Type == IMAGE_REL_ARM_BRANCH24) 
                            rp.Type = objabi.R_CALLARM;

                            rp.Add = int64(int32(binary.LittleEndian.Uint32(sectdata[rsect][rp.Off..])));
                        else 
                            return (null, _addr_null!, error.As(fmt.Errorf("%s: %v: unknown ARM relocation type %v", pn, sectsyms[rsect], r.Type))!);
                                            else 
                        return (null, _addr_null!, error.As(fmt.Errorf("%s: unsupported arch %v", pn, arch.Family))!);
                    // ld -r could generate multiple section symbols for the
                    // same section but with different values, we have to take
                    // that into account
                    if (issect(_addr_pesym))
                    {
                        rp.Add += int64(pesym.Value);
                    }

                }
                sort.Sort(sym.RelocByOff(rs[..rsect.NumberOfRelocations]));

                s = sectsyms[rsect];
                s.R = rs;
                s.R = s.R[..rsect.NumberOfRelocations];

            } 

            // enter sub-symbols into symbol table.
            {
                long i = 0L;
                long numaux = 0L;

                while (i < len(f.COFFSymbols))
                {
                    pesym = _addr_f.COFFSymbols[i];

                    numaux = int(pesym.NumberOfAuxSymbols);

                    (name, err) = pesym.FullName(f.StringTable);
                    if (err != null)
                    {
                        return (null, _addr_null!, error.As(err)!);
                    i += numaux + 1L;
                    }

                    if (name == "")
                    {
                        continue;
                    }

                    if (issect(_addr_pesym))
                    {
                        continue;
                    }

                    if (int(pesym.SectionNumber) > len(f.Sections))
                    {
                        continue;
                    }

                    if (pesym.SectionNumber == IMAGE_SYM_DEBUG)
                    {
                        continue;
                    }

                    ptr<pe.Section> sect;
                    if (pesym.SectionNumber > 0L)
                    {
                        sect = f.Sections[pesym.SectionNumber - 1L];
                        {
                            (_, found) = sectsyms[sect];

                            if (!found)
                            {
                                continue;
                            }

                        }

                    }

                    var (s, err) = readpesym(_addr_arch, lookup, _addr_f, _addr_pesym, sectsyms, localSymVersion);
                    if (err != null)
                    {
                        return (null, _addr_null!, error.As(err)!);
                    }

                    if (pesym.SectionNumber == 0L)
                    { // extern
                        if (s.Type == sym.SDYNIMPORT)
                        {
                            s.SetPlt(-2L); // flag for dynimport in PE object files.
                        }

                        if (s.Type == sym.SXREF && pesym.Value > 0L)
                        { // global data
                            s.Type = sym.SNOPTRDATA;
                            s.Size = int64(pesym.Value);

                        }

                        continue;

                    }
                    else if (pesym.SectionNumber > 0L && int(pesym.SectionNumber) <= len(f.Sections))
                    {
                        sect = f.Sections[pesym.SectionNumber - 1L];
                        {
                            (_, found) = sectsyms[sect];

                            if (!found)
                            {
                                return (null, _addr_null!, error.As(fmt.Errorf("%s: %v: missing sect.sym", pn, s))!);
                            }

                        }

                    }
                    else
                    {
                        return (null, _addr_null!, error.As(fmt.Errorf("%s: %v: sectnum < 0!", pn, s))!);
                    }

                    if (sect == null)
                    {
                        return (null, _addr_rsrc!, error.As(null!)!);
                    }

                    if (s.Outer != null)
                    {
                        if (s.Attr.DuplicateOK())
                        {
                            continue;
                        }

                        return (null, _addr_null!, error.As(fmt.Errorf("%s: duplicate symbol reference: %s in both %s and %s", pn, s.Name, s.Outer.Name, sectsyms[sect].Name))!);

                    }

                    var sectsym = sectsyms[sect];
                    s.Sub = sectsym.Sub;
                    sectsym.Sub = s;
                    s.Type = sectsym.Type;
                    s.Attr |= sym.AttrSubSymbol;
                    s.Value = int64(pesym.Value);
                    s.Size = 4L;
                    s.Outer = sectsym;
                    if (sectsym.Type == sym.STEXT)
                    {
                        if (s.Attr.External() && !s.Attr.DuplicateOK())
                        {
                            return (null, _addr_null!, error.As(fmt.Errorf("%s: duplicate symbol definition", s.Name))!);
                        }

                        s.Attr |= sym.AttrExternal;

                    }

                } 

                // Sort outer lists by address, adding to textp.
                // This keeps textp in increasing address order.

            } 

            // Sort outer lists by address, adding to textp.
            // This keeps textp in increasing address order.
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in f.Sections)
                {
                    sect = __sect;
                    s = sectsyms[sect];
                    if (s == null)
                    {
                        continue;
                    }

                    if (s.Sub != null)
                    {
                        s.Sub = sym.SortSub(s.Sub);
                    }

                    if (s.Type == sym.STEXT)
                    {
                        if (s.Attr.OnList())
                        {
                            return (null, _addr_null!, error.As(fmt.Errorf("symbol %s listed multiple times", s.Name))!);
                        }

                        s.Attr |= sym.AttrOnList;
                        textp = append(textp, s);
                        s = s.Sub;

                        while (s != null)
                        {
                            if (s.Attr.OnList())
                            {
                                return (null, _addr_null!, error.As(fmt.Errorf("symbol %s listed multiple times", s.Name))!);
                            s = s.Sub;
                            }

                            s.Attr |= sym.AttrOnList;
                            textp = append(textp, s);

                        }


                    }

                }

                sect = sect__prev1;
            }

            return (textp, _addr_rsrc!, error.As(null!)!);

        });

        private static bool issect(ptr<pe.COFFSymbol> _addr_s)
        {
            ref pe.COFFSymbol s = ref _addr_s.val;

            return s.StorageClass == IMAGE_SYM_CLASS_STATIC && s.Type == 0L && s.Name[0L] == '.';
        }

        private static (ptr<sym.Symbol>, error) readpesym(ptr<sys.Arch> _addr_arch, Func<@string, long, ptr<sym.Symbol>> lookup, ptr<pe.File> _addr_f, ptr<pe.COFFSymbol> _addr_pesym, map<ptr<pe.Section>, ptr<sym.Symbol>> sectsyms, long localSymVersion)
        {
            ptr<sym.Symbol> _p0 = default!;
            error _p0 = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref pe.File f = ref _addr_f.val;
            ref pe.COFFSymbol pesym = ref _addr_pesym.val;

            var (symname, err) = pesym.FullName(f.StringTable);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            @string name = default;
            if (issect(_addr_pesym))
            {
                name = sectsyms[f.Sections[pesym.SectionNumber - 1L]].Name;
            }
            else
            {
                name = symname;

                if (arch.Family == sys.AMD64) 
                    if (name == "__imp___acrt_iob_func")
                    { 
                        // Do not rename __imp___acrt_iob_func into __acrt_iob_func,
                        // because __imp___acrt_iob_func symbol is real
                        // (see commit b295099 from git://git.code.sf.net/p/mingw-w64/mingw-w64 for details).
                    }
                    else
                    {
                        name = strings.TrimPrefix(name, "__imp_"); // __imp_Name => Name
                    }

                else if (arch.Family == sys.I386) 
                    if (name == "__imp____acrt_iob_func")
                    { 
                        // Do not rename __imp____acrt_iob_func into ___acrt_iob_func,
                        // because __imp____acrt_iob_func symbol is real
                        // (see commit b295099 from git://git.code.sf.net/p/mingw-w64/mingw-w64 for details).
                    }
                    else
                    {
                        name = strings.TrimPrefix(name, "__imp_"); // __imp_Name => Name
                    }

                    if (name[0L] == '_')
                    {
                        name = name[1L..]; // _Name => Name
                    }

                            } 

            // remove last @XXX
            {
                var i = strings.LastIndex(name, "@");

                if (i >= 0L)
                {
                    name = name[..i];
                }

            }


            ptr<sym.Symbol> s;

            if (pesym.Type == IMAGE_SYM_DTYPE_FUNCTION || pesym.Type == IMAGE_SYM_DTYPE_NULL) 

                if (pesym.StorageClass == IMAGE_SYM_CLASS_EXTERNAL) //global
                    s = lookup(name, 0L);
                else if (pesym.StorageClass == IMAGE_SYM_CLASS_NULL || pesym.StorageClass == IMAGE_SYM_CLASS_STATIC || pesym.StorageClass == IMAGE_SYM_CLASS_LABEL) 
                    s = lookup(name, localSymVersion);
                    s.Attr |= sym.AttrDuplicateOK;
                else 
                    return (_addr_null!, error.As(fmt.Errorf("%s: invalid symbol binding %d", symname, pesym.StorageClass))!);
                            else 
                return (_addr_null!, error.As(fmt.Errorf("%s: invalid symbol type %d", symname, pesym.Type))!);
                        if (s != null && s.Type == 0L && (pesym.StorageClass != IMAGE_SYM_CLASS_STATIC || pesym.Value != 0L))
            {
                s.Type = sym.SXREF;
            }

            if (strings.HasPrefix(symname, "__imp_"))
            {
                s.SetGot(-2L); // flag for __imp_
            }

            return (_addr_s!, error.As(null!)!);

        }
    }
}}}}
