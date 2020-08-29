// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadpe implements a PE/COFF file reader.
// package loadpe -- go2cs converted at 2020 August 29 10:04:06 UTC
// import "cmd/link/internal/loadpe" ==> using loadpe = go.cmd.link.@internal.loadpe_package
// Original source: C:\Go\src\cmd\link\internal\loadpe\ldpe.go
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using pe = go.debug.pe_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class loadpe_package
    {
 
        // TODO: the Microsoft doco says IMAGE_SYM_DTYPE_ARRAY is 3 (same with IMAGE_SYM_DTYPE_POINTER and IMAGE_SYM_DTYPE_FUNCTION)
        public static readonly long IMAGE_SYM_UNDEFINED = 0L;
        public static readonly long IMAGE_SYM_ABSOLUTE = -1L;
        public static readonly long IMAGE_SYM_DEBUG = -2L;
        public static readonly long IMAGE_SYM_TYPE_NULL = 0L;
        public static readonly long IMAGE_SYM_TYPE_VOID = 1L;
        public static readonly long IMAGE_SYM_TYPE_CHAR = 2L;
        public static readonly long IMAGE_SYM_TYPE_SHORT = 3L;
        public static readonly long IMAGE_SYM_TYPE_INT = 4L;
        public static readonly long IMAGE_SYM_TYPE_LONG = 5L;
        public static readonly long IMAGE_SYM_TYPE_FLOAT = 6L;
        public static readonly long IMAGE_SYM_TYPE_DOUBLE = 7L;
        public static readonly long IMAGE_SYM_TYPE_STRUCT = 8L;
        public static readonly long IMAGE_SYM_TYPE_UNION = 9L;
        public static readonly long IMAGE_SYM_TYPE_ENUM = 10L;
        public static readonly long IMAGE_SYM_TYPE_MOE = 11L;
        public static readonly long IMAGE_SYM_TYPE_BYTE = 12L;
        public static readonly long IMAGE_SYM_TYPE_WORD = 13L;
        public static readonly long IMAGE_SYM_TYPE_UINT = 14L;
        public static readonly long IMAGE_SYM_TYPE_DWORD = 15L;
        public static readonly long IMAGE_SYM_TYPE_PCODE = 32768L;
        public static readonly long IMAGE_SYM_DTYPE_NULL = 0L;
        public static readonly ulong IMAGE_SYM_DTYPE_POINTER = 0x10UL;
        public static readonly ulong IMAGE_SYM_DTYPE_FUNCTION = 0x20UL;
        public static readonly ulong IMAGE_SYM_DTYPE_ARRAY = 0x30UL;
        public static readonly long IMAGE_SYM_CLASS_END_OF_FUNCTION = -1L;
        public static readonly long IMAGE_SYM_CLASS_NULL = 0L;
        public static readonly long IMAGE_SYM_CLASS_AUTOMATIC = 1L;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL = 2L;
        public static readonly long IMAGE_SYM_CLASS_STATIC = 3L;
        public static readonly long IMAGE_SYM_CLASS_REGISTER = 4L;
        public static readonly long IMAGE_SYM_CLASS_EXTERNAL_DEF = 5L;
        public static readonly long IMAGE_SYM_CLASS_LABEL = 6L;
        public static readonly long IMAGE_SYM_CLASS_UNDEFINED_LABEL = 7L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_STRUCT = 8L;
        public static readonly long IMAGE_SYM_CLASS_ARGUMENT = 9L;
        public static readonly long IMAGE_SYM_CLASS_STRUCT_TAG = 10L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_UNION = 11L;
        public static readonly long IMAGE_SYM_CLASS_UNION_TAG = 12L;
        public static readonly long IMAGE_SYM_CLASS_TYPE_DEFINITION = 13L;
        public static readonly long IMAGE_SYM_CLASS_UNDEFINED_STATIC = 14L;
        public static readonly long IMAGE_SYM_CLASS_ENUM_TAG = 15L;
        public static readonly long IMAGE_SYM_CLASS_MEMBER_OF_ENUM = 16L;
        public static readonly long IMAGE_SYM_CLASS_REGISTER_PARAM = 17L;
        public static readonly long IMAGE_SYM_CLASS_BIT_FIELD = 18L;
        public static readonly long IMAGE_SYM_CLASS_FAR_EXTERNAL = 68L; /* Not in PECOFF v8 spec */
        public static readonly long IMAGE_SYM_CLASS_BLOCK = 100L;
        public static readonly long IMAGE_SYM_CLASS_FUNCTION = 101L;
        public static readonly long IMAGE_SYM_CLASS_END_OF_STRUCT = 102L;
        public static readonly long IMAGE_SYM_CLASS_FILE = 103L;
        public static readonly long IMAGE_SYM_CLASS_SECTION = 104L;
        public static readonly long IMAGE_SYM_CLASS_WEAK_EXTERNAL = 105L;
        public static readonly long IMAGE_SYM_CLASS_CLR_TOKEN = 107L;
        public static readonly ulong IMAGE_REL_I386_ABSOLUTE = 0x0000UL;
        public static readonly ulong IMAGE_REL_I386_DIR16 = 0x0001UL;
        public static readonly ulong IMAGE_REL_I386_REL16 = 0x0002UL;
        public static readonly ulong IMAGE_REL_I386_DIR32 = 0x0006UL;
        public static readonly ulong IMAGE_REL_I386_DIR32NB = 0x0007UL;
        public static readonly ulong IMAGE_REL_I386_SEG12 = 0x0009UL;
        public static readonly ulong IMAGE_REL_I386_SECTION = 0x000AUL;
        public static readonly ulong IMAGE_REL_I386_SECREL = 0x000BUL;
        public static readonly ulong IMAGE_REL_I386_TOKEN = 0x000CUL;
        public static readonly ulong IMAGE_REL_I386_SECREL7 = 0x000DUL;
        public static readonly ulong IMAGE_REL_I386_REL32 = 0x0014UL;
        public static readonly ulong IMAGE_REL_AMD64_ABSOLUTE = 0x0000UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR64 = 0x0001UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32 = 0x0002UL;
        public static readonly ulong IMAGE_REL_AMD64_ADDR32NB = 0x0003UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32 = 0x0004UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_1 = 0x0005UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_2 = 0x0006UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_3 = 0x0007UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_4 = 0x0008UL;
        public static readonly ulong IMAGE_REL_AMD64_REL32_5 = 0x0009UL;
        public static readonly ulong IMAGE_REL_AMD64_SECTION = 0x000AUL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL = 0x000BUL;
        public static readonly ulong IMAGE_REL_AMD64_SECREL7 = 0x000CUL;
        public static readonly ulong IMAGE_REL_AMD64_TOKEN = 0x000DUL;
        public static readonly ulong IMAGE_REL_AMD64_SREL32 = 0x000EUL;
        public static readonly ulong IMAGE_REL_AMD64_PAIR = 0x000FUL;
        public static readonly ulong IMAGE_REL_AMD64_SSPAN32 = 0x0010UL;

        // TODO(crawshaw): de-duplicate these symbols with cmd/internal/ld, ideally in debug/pe.
        public static readonly ulong IMAGE_SCN_CNT_CODE = 0x00000020UL;
        public static readonly ulong IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040UL;
        public static readonly ulong IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080UL;
        public static readonly ulong IMAGE_SCN_MEM_DISCARDABLE = 0x02000000UL;
        public static readonly ulong IMAGE_SCN_MEM_EXECUTE = 0x20000000UL;
        public static readonly ulong IMAGE_SCN_MEM_READ = 0x40000000UL;
        public static readonly ulong IMAGE_SCN_MEM_WRITE = 0x80000000UL;

        // TODO(brainman): maybe just add ReadAt method to bio.Reader instead of creating peBiobuf

        // peBiobuf makes bio.Reader look like io.ReaderAt.
        private partial struct peBiobuf // : bio.Reader
        {
        }

        private static (long, error) ReadAt(this ref peBiobuf f, slice<byte> p, long off)
        {
            var ret = ((bio.Reader.Value)(f)).Seek(off, 0L);
            if (ret < 0L)
            {
                return (0L, errors.New("fail to seek"));
            }
            var (n, err) = f.Read(p);
            if (err != null)
            {
                return (0L, err);
            }
            return (n, null);
        }

        // Load loads the PE file pn from input.
        // Symbols are written into syms, and a slice of the text symbols is returned.
        // If an .rsrc section is found, its symbol is returned as rsrc.
        public static (slice<ref sym.Symbol>, ref sym.Symbol, error) Load(ref sys.Arch _arch, ref sym.Symbols _syms, ref bio.Reader _input, @string pkg, long length, @string pn) => func(_arch, _syms, _input, (ref sys.Arch arch, ref sym.Symbols syms, ref bio.Reader input, Defer defer, Panic _, Recover __) =>
        {
            var localSymVersion = syms.IncVersion();

            var sectsyms = make_map<ref pe.Section, ref sym.Symbol>();
            var sectdata = make_map<ref pe.Section, slice<byte>>(); 

            // Some input files are archives containing multiple of
            // object files, and pe.NewFile seeks to the start of
            // input file and get confused. Create section reader
            // to stop pe.NewFile looking before current position.
            var sr = io.NewSectionReader((peBiobuf.Value)(input), input.Offset(), 1L << (int)(63L) - 1L); 

            // TODO: replace pe.NewFile with pe.Load (grep for "add Load function" in debug/pe for details)
            var (f, err) = pe.NewFile(sr);
            if (err != null)
            {
                return (null, null, err);
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
                    var s = syms.Lookup(name, localSymVersion);


                    if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ) //.rdata
                        s.Type = sym.SRODATA;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.bss
                        s.Type = sym.SNOPTRBSS;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE) //.data
                        s.Type = sym.SNOPTRDATA;
                    else if (sect.Characteristics & (IMAGE_SCN_CNT_UNINITIALIZED_DATA | IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ | IMAGE_SCN_MEM_WRITE | IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE) == IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_MEM_READ) //.text
                        s.Type = sym.STEXT;
                    else 
                        return (null, null, fmt.Errorf("unexpected flags %#06x for PE section %s", sect.Characteristics, sect.Name));
                                        if (s.Type != sym.SNOPTRBSS)
                    {
                        var (data, err) = sect.Data();
                        if (err != null)
                        {
                            return (null, null, err);
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
                    var rp = ref rs[j];
                    if (int(r.SymbolTableIndex) >= len(f.COFFSymbols))
                    {
                        return (null, null, fmt.Errorf("relocation number %d symbol index idx=%d cannot be large then number of symbols %d", j, r.SymbolTableIndex, len(f.COFFSymbols)));
                    }
                    var pesym = ref f.COFFSymbols[r.SymbolTableIndex];
                    var (gosym, err) = readpesym(arch, syms, f, pesym, sectsyms, localSymVersion);
                    if (err != null)
                    {
                        return (null, null, err);
                    }
                    if (gosym == null)
                    {
                        var (name, err) = pesym.FullName(f.StringTable);
                        if (err != null)
                        {
                            name = string(pesym.Name[..]);
                        }
                        return (null, null, fmt.Errorf("reloc of invalid sym %s idx=%d type=%d", name, r.SymbolTableIndex, pesym.Type));
                    }
                    rp.Sym = gosym;
                    rp.Siz = 4L;
                    rp.Off = int32(r.VirtualAddress);

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
                        return (null, null, fmt.Errorf("%s: %v: unknown relocation type %v", pn, sectsyms[rsect], r.Type));
                    // ld -r could generate multiple section symbols for the
                    // same section but with different values, we have to take
                    // that into account
                    if (issect(pesym))
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
                    pesym = ref f.COFFSymbols[i];

                    numaux = int(pesym.NumberOfAuxSymbols);

                    (name, err) = pesym.FullName(f.StringTable);
                    if (err != null)
                    {
                        return (null, null, err);
                    i += numaux + 1L;
                    }
                    if (name == "")
                    {
                        continue;
                    }
                    if (issect(pesym))
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
                    ref pe.Section sect = default;
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
                    var (s, err) = readpesym(arch, syms, f, pesym, sectsyms, localSymVersion);
                    if (err != null)
                    {
                        return (null, null, err);
                    }
                    if (pesym.SectionNumber == 0L)
                    { // extern
                        if (s.Type == sym.SDYNIMPORT)
                        {
                            s.Plt = -2L; // flag for dynimport in PE object files.
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
                                return (null, null, fmt.Errorf("%s: %v: missing sect.sym", pn, s));
                            }

                        }
                    }
                    else
                    {
                        return (null, null, fmt.Errorf("%s: %v: sectnum < 0!", pn, s));
                    }
                    if (sect == null)
                    {
                        return (null, rsrc, null);
                    }
                    if (s.Outer != null)
                    {
                        if (s.Attr.DuplicateOK())
                        {
                            continue;
                        }
                        return (null, null, fmt.Errorf("%s: duplicate symbol reference: %s in both %s and %s", pn, s.Name, s.Outer.Name, sectsyms[sect].Name));
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
                            return (null, null, fmt.Errorf("%s: duplicate symbol definition", s.Name));
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
                            return (null, null, fmt.Errorf("symbol %s listed multiple times", s.Name));
                        }
                        s.Attr |= sym.AttrOnList;
                        textp = append(textp, s);
                        s = s.Sub;

                        while (s != null)
                        {
                            if (s.Attr.OnList())
                            {
                                return (null, null, fmt.Errorf("symbol %s listed multiple times", s.Name));
                            s = s.Sub;
                            }
                            s.Attr |= sym.AttrOnList;
                            textp = append(textp, s);
                        }

                    }
                }

                sect = sect__prev1;
            }

            return (textp, rsrc, null);
        });

        private static bool issect(ref pe.COFFSymbol s)
        {
            return s.StorageClass == IMAGE_SYM_CLASS_STATIC && s.Type == 0L && s.Name[0L] == '.';
        }

        private static (ref sym.Symbol, error) readpesym(ref sys.Arch arch, ref sym.Symbols syms, ref pe.File f, ref pe.COFFSymbol pesym, map<ref pe.Section, ref sym.Symbol> sectsyms, long localSymVersion)
        {
            var (symname, err) = pesym.FullName(f.StringTable);
            if (err != null)
            {
                return (null, err);
            }
            @string name = default;
            if (issect(pesym))
            {
                name = sectsyms[f.Sections[pesym.SectionNumber - 1L]].Name;
            }
            else
            {
                name = symname;
                if (strings.HasPrefix(name, "__imp_"))
                {
                    name = name[6L..]; // __imp_Name => Name
                }
                if (arch.Family == sys.I386 && name[0L] == '_')
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

            ref sym.Symbol s = default;

            if (pesym.Type == IMAGE_SYM_DTYPE_FUNCTION || pesym.Type == IMAGE_SYM_DTYPE_NULL) 

                if (pesym.StorageClass == IMAGE_SYM_CLASS_EXTERNAL) //global
                    s = syms.Lookup(name, 0L);
                else if (pesym.StorageClass == IMAGE_SYM_CLASS_NULL || pesym.StorageClass == IMAGE_SYM_CLASS_STATIC || pesym.StorageClass == IMAGE_SYM_CLASS_LABEL) 
                    s = syms.Lookup(name, localSymVersion);
                    s.Attr |= sym.AttrDuplicateOK;
                else 
                    return (null, fmt.Errorf("%s: invalid symbol binding %d", symname, pesym.StorageClass));
                            else 
                return (null, fmt.Errorf("%s: invalid symbol type %d", symname, pesym.Type));
                        if (s != null && s.Type == 0L && (pesym.StorageClass != IMAGE_SYM_CLASS_STATIC || pesym.Value != 0L))
            {
                s.Type = sym.SXREF;
            }
            if (strings.HasPrefix(symname, "__imp_"))
            {
                s.Got = -2L; // flag for __imp_
            }
            return (s, null);
        }
    }
}}}}
