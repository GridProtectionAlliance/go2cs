// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadxcoff implements a XCOFF file reader.
// package loadxcoff -- go2cs converted at 2020 October 08 04:41:32 UTC
// import "cmd/oldlink/internal/loadxcoff" ==> using loadxcoff = go.cmd.oldlink.@internal.loadxcoff_package
// Original source: C:\Go\src\cmd\oldlink\internal\loadxcoff\ldxcoff.go
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.oldlink.@internal.loader_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class loadxcoff_package
    {
        // ldSection is an XCOFF section with its symbols.
        private partial struct ldSection
        {
            public ref xcoff.Section Section => ref Section_val;
            public ptr<sym.Symbol> sym;
        }

        // TODO(brainman): maybe just add ReadAt method to bio.Reader instead of creating xcoffBiobuf

        // xcoffBiobuf makes bio.Reader look like io.ReaderAt.
        private partial struct xcoffBiobuf // : bio.Reader
        {
        }

        private static (long, error) ReadAt(this ptr<xcoffBiobuf> _addr_f, slice<byte> p, long off)
        {
            long _p0 = default;
            error _p0 = default!;
            ref xcoffBiobuf f = ref _addr_f.val;

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

        // Load loads xcoff files with the indexed object files.
        public static (slice<ptr<sym.Symbol>>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
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

        // LoadOld uses the old version of object loading.
        public static (slice<ptr<sym.Symbol>>, error) LoadOld(ptr<sys.Arch> _addr_arch, ptr<sym.Symbols> _addr_syms, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn)
        {
            slice<ptr<sym.Symbol>> textp = default;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbols syms = ref _addr_syms.val;
            ref bio.Reader input = ref _addr_input.val;

            return load(_addr_arch, syms.Lookup, syms.IncVersion(), _addr_input, pkg, length, pn);
        }

        // loads the Xcoff file pn from f.
        // Symbols are written into syms, and a slice of the text symbols is returned.
        private static (slice<ptr<sym.Symbol>>, error) load(ptr<sys.Arch> _addr_arch, Func<@string, long, ptr<sym.Symbol>> lookup, long localSymVersion, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn) => func((defer, _, __) =>
        {
            slice<ptr<sym.Symbol>> textp = default;
            error err = default!;
            ref sys.Arch arch = ref _addr_arch.val;
            ref bio.Reader input = ref _addr_input.val;

            Func<@string, object[], (slice<ptr<sym.Symbol>>, error)> errorf = (str, args) =>
            {
                return (null, error.As(fmt.Errorf("loadxcoff: %v: %v", pn, fmt.Sprintf(str, args)))!);
            }
;

            slice<ptr<ldSection>> ldSections = default;

            var (f, err) = xcoff.NewFile((xcoffBiobuf.val)(input));
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(f.Close());

            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in f.Sections)
                {
                    sect = __sect; 
                    //only text, data and bss section
                    if (sect.Type < xcoff.STYP_TEXT || sect.Type > xcoff.STYP_BSS)
                    {
                        continue;
                    }

                    ptr<ldSection> lds = @new<ldSection>();
                    lds.Section = sect.val;
                    var name = fmt.Sprintf("%s(%s)", pkg, lds.Name);
                    var s = lookup(name, localSymVersion);


                    if (lds.Type == xcoff.STYP_TEXT) 
                        s.Type = sym.STEXT;
                    else if (lds.Type == xcoff.STYP_DATA) 
                        s.Type = sym.SNOPTRDATA;
                    else if (lds.Type == xcoff.STYP_BSS) 
                        s.Type = sym.SNOPTRBSS;
                    else 
                        return errorf("unrecognized section type 0x%x", lds.Type);
                                        s.Size = int64(lds.Size);
                    if (s.Type != sym.SNOPTRBSS)
                    {
                        var (data, err) = lds.Section.Data();
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        s.P = data;

                    }

                    lds.sym = s;
                    ldSections = append(ldSections, lds);

                } 

                // sx = symbol from file
                // s = symbol for syms

                sect = sect__prev1;
            }

            foreach (var (_, sx) in f.Symbols)
            { 
                // get symbol type
                var (stype, errmsg) = getSymbolType(_addr_f, _addr_sx);
                if (errmsg != "")
                {
                    return errorf("error reading symbol %s: %s", sx.Name, errmsg);
                }

                if (stype == sym.Sxxx)
                {
                    continue;
                }

                s = lookup(sx.Name, 0L); 

                // Text symbol
                if (s.Type == sym.STEXT)
                {
                    if (s.Attr.OnList())
                    {
                        return errorf("symbol %s listed multiple times", s.Name);
                    }

                    s.Attr |= sym.AttrOnList;
                    textp = append(textp, s);

                }

            } 

            // Read relocations
            {
                var sect__prev1 = sect;

                foreach (var (_, __sect) in ldSections)
                {
                    sect = __sect; 
                    // TODO(aix): Dwarf section relocation if needed
                    if (sect.Type != xcoff.STYP_TEXT && sect.Type != xcoff.STYP_DATA)
                    {
                        continue;
                    }

                    var rs = make_slice<sym.Reloc>(sect.Nreloc);
                    foreach (var (i, rx) in sect.Relocs)
                    {
                        var r = _addr_rs[i];

                        r.Sym = lookup(rx.Symbol.Name, 0L);
                        if (uint64(int32(rx.VirtualAddress)) != rx.VirtualAddress)
                        {
                            return errorf("virtual address of a relocation is too big: 0x%x", rx.VirtualAddress);
                        }

                        r.Off = int32(rx.VirtualAddress);

                        if (rx.Type == xcoff.R_POS) 
                            // Reloc the address of r.Sym
                            // Length should be 64
                            if (rx.Length != 64L)
                            {
                                return errorf("section %s: relocation R_POS has length different from 64: %d", sect.Name, rx.Length);
                            }

                            r.Siz = 8L;
                            r.Type = objabi.R_CONST;
                            r.Add = int64(rx.Symbol.Value);
                        else if (rx.Type == xcoff.R_RBR) 
                            r.Siz = 4L;
                            r.Type = objabi.R_CALLPOWER;
                            r.Add = 0L; //
                        else 
                            return errorf("section %s: unknown relocation of type 0x%x", sect.Name, rx.Type);
                        
                    }
                    s = sect.sym;
                    s.R = rs;
                    s.R = s.R[..sect.Nreloc];

                }

                sect = sect__prev1;
            }

            return (textp, error.As(null!)!);


        });

        // Convert symbol xcoff type to sym.SymKind
        // Returns nil if this shouldn't be added into syms (like .file or .dw symbols )
        private static (sym.SymKind, @string) getSymbolType(ptr<xcoff.File> _addr_f, ptr<xcoff.Symbol> _addr_s)
        {
            sym.SymKind stype = default;
            @string err = default;
            ref xcoff.File f = ref _addr_f.val;
            ref xcoff.Symbol s = ref _addr_s.val;
 
            // .file symbol
            if (s.SectionNumber == -2L)
            {
                if (s.StorageClass == xcoff.C_FILE)
                {
                    return (sym.Sxxx, "");
                }

                return (sym.Sxxx, "unrecognised StorageClass for sectionNumber = -2");

            } 

            // extern symbols
            // TODO(aix)
            if (s.SectionNumber == 0L)
            {
                return (sym.Sxxx, "");
            }

            var sectType = f.Sections[s.SectionNumber - 1L].SectionHeader.Type;

            if (sectType == xcoff.STYP_DWARF || sectType == xcoff.STYP_DEBUG) 
                return (sym.Sxxx, "");
            else if (sectType == xcoff.STYP_DATA || sectType == xcoff.STYP_BSS || sectType == xcoff.STYP_TEXT)             else 
                return (sym.Sxxx, fmt.Sprintf("getSymbolType for Section type 0x%x not implemented", sectType));
            
            if (s.StorageClass == xcoff.C_HIDEXT || s.StorageClass == xcoff.C_EXT || s.StorageClass == xcoff.C_WEAKEXT) 

                if (s.AuxCSect.StorageMappingClass == xcoff.XMC_PR) 
                    if (sectType == xcoff.STYP_TEXT)
                    {
                        return (sym.STEXT, "");
                    }

                    return (sym.Sxxx, fmt.Sprintf("unrecognised Section Type 0x%x for Storage Class 0x%x with Storage Map XMC_PR", sectType, s.StorageClass)); 

                    // Read/Write Data
                else if (s.AuxCSect.StorageMappingClass == xcoff.XMC_RW) 
                    if (sectType == xcoff.STYP_DATA)
                    {
                        return (sym.SDATA, "");
                    }

                    if (sectType == xcoff.STYP_BSS)
                    {
                        return (sym.SBSS, "");
                    }

                    return (sym.Sxxx, fmt.Sprintf("unrecognised Section Type 0x%x for Storage Class 0x%x with Storage Map XMC_RW", sectType, s.StorageClass)); 

                    // Function descriptor
                else if (s.AuxCSect.StorageMappingClass == xcoff.XMC_DS) 
                    if (sectType == xcoff.STYP_DATA)
                    {
                        return (sym.SDATA, "");
                    }

                    return (sym.Sxxx, fmt.Sprintf("unrecognised Section Type 0x%x for Storage Class 0x%x with Storage Map XMC_DS", sectType, s.StorageClass)); 

                    // TOC anchor and TOC entry
                else if (s.AuxCSect.StorageMappingClass == xcoff.XMC_TC0 || s.AuxCSect.StorageMappingClass == xcoff.XMC_TE) 
                    if (sectType == xcoff.STYP_DATA)
                    {
                        return (sym.SXCOFFTOC, "");
                    }

                    return (sym.Sxxx, fmt.Sprintf("unrecognised Section Type 0x%x for Storage Class 0x%x with Storage Map XMC_DS", sectType, s.StorageClass));
                else 
                    return (sym.Sxxx, fmt.Sprintf("getSymbolType for Storage class 0x%x and Storage Map 0x%x not implemented", s.StorageClass, s.AuxCSect.StorageMappingClass)); 

                    // Program Code
                            else 
                return (sym.Sxxx, fmt.Sprintf("getSymbolType for Storage class 0x%x not implemented", s.StorageClass));
            
        }
    }
}}}}
