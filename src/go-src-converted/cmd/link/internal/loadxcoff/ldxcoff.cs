// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loadxcoff implements a XCOFF file reader.
// package loadxcoff -- go2cs converted at 2020 October 08 04:39:08 UTC
// import "cmd/link/internal/loadxcoff" ==> using loadxcoff = go.cmd.link.@internal.loadxcoff_package
// Original source: C:\Go\src\cmd\link\internal\loadxcoff\ldxcoff.go
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using xcoff = go.@internal.xcoff_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class loadxcoff_package
    {
        // ldSection is an XCOFF section with its symbols.
        private partial struct ldSection
        {
            public ref xcoff.Section Section => ref Section_val;
            public loader.Sym sym;
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

        // loads the Xcoff file pn from f.
        // Symbols are written into loader, and a slice of the text symbols is returned.
        public static (slice<loader.Sym>, error) Load(ptr<loader.Loader> _addr_l, ptr<sys.Arch> _addr_arch, long localSymVersion, ptr<bio.Reader> _addr_input, @string pkg, long length, @string pn) => func((defer, _, __) =>
        {
            slice<loader.Sym> textp = default;
            error err = default!;
            ref loader.Loader l = ref _addr_l.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref bio.Reader input = ref _addr_input.val;

            Func<@string, object[], (slice<loader.Sym>, error)> errorf = (str, args) =>
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
                    var symbol = l.LookupOrCreateSym(name, localSymVersion);
                    var s = l.MakeSymbolUpdater(symbol);


                    if (lds.Type == xcoff.STYP_TEXT) 
                        s.SetType(sym.STEXT);
                    else if (lds.Type == xcoff.STYP_DATA) 
                        s.SetType(sym.SNOPTRDATA);
                    else if (lds.Type == xcoff.STYP_BSS) 
                        s.SetType(sym.SNOPTRBSS);
                    else 
                        return errorf("unrecognized section type 0x%x", lds.Type);
                                        s.SetSize(int64(lds.Size));
                    if (s.Type() != sym.SNOPTRBSS)
                    {
                        var (data, err) = lds.Section.Data();
                        if (err != null)
                        {
                            return (null, error.As(err)!);
                        }

                        s.SetData(data);

                    }

                    lds.sym = symbol;
                    ldSections = append(ldSections, lds);

                } 

                // sx = symbol from file
                // s = symbol for loader

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

                s = l.LookupOrCreateSym(sx.Name, 0L); 

                // Text symbol
                if (l.SymType(s) == sym.STEXT)
                {
                    if (l.AttrOnList(s))
                    {
                        return errorf("symbol %s listed multiple times", l.SymName(s));
                    }

                    l.SetAttrOnList(s, true);
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

                    var sb = l.MakeSymbolUpdater(sect.sym);
                    foreach (var (_, rx) in sect.Relocs)
                    {
                        var rSym = l.LookupOrCreateSym(rx.Symbol.Name, 0L);
                        if (uint64(int32(rx.VirtualAddress)) != rx.VirtualAddress)
                        {
                            return errorf("virtual address of a relocation is too big: 0x%x", rx.VirtualAddress);
                        }

                        var rOff = int32(rx.VirtualAddress);
                        byte rSize = default;
                        objabi.RelocType rType = default;
                        long rAdd = default;

                        if (rx.Type == xcoff.R_POS) 
                            // Reloc the address of r.Sym
                            // Length should be 64
                            if (rx.Length != 64L)
                            {
                                return errorf("section %s: relocation R_POS has length different from 64: %d", sect.Name, rx.Length);
                            }

                            rSize = 8L;
                            rType = objabi.R_CONST;
                            rAdd = int64(rx.Symbol.Value);
                        else if (rx.Type == xcoff.R_RBR) 
                            rSize = 4L;
                            rType = objabi.R_CALLPOWER;
                            rAdd = 0L;
                        else 
                            return errorf("section %s: unknown relocation of type 0x%x", sect.Name, rx.Type);
                                                var (r, _) = sb.AddRel(rType);
                        r.SetOff(rOff);
                        r.SetSiz(rSize);
                        r.SetSym(rSym);
                        r.SetAdd(rAdd);

                    }

                }

                sect = sect__prev1;
            }

            return (textp, error.As(null!)!);


        });

        // Convert symbol xcoff type to sym.SymKind
        // Returns nil if this shouldn't be added into loader (like .file or .dw symbols )
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
