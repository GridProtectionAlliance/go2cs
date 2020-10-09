// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO/NICETOHAVE:
//   - eliminate DW_CLS_ if not used
//   - package info in compilation units
//   - assign types to their packages
//   - gdb uses c syntax, meaning clumsy quoting is needed for go identifiers. eg
//     ptype struct '[]uint8' and qualifiers need to be quoted away
//   - file:line info for variables
//   - make strings a typedef so prettyprinters can see the underlying string type

// package ld -- go2cs converted at 2020 October 09 05:49:26 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\dwarf.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
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
        // dwctxt2 is a wrapper intended to satisfy the method set of
        // dwarf.Context, so that functions like dwarf.PutAttrs will work with
        // DIEs that use loader.Sym as opposed to *sym.Symbol. It is also
        // being used as a place to store tables/maps that are useful as part
        // of type conversion (this is just a convenience; it would be easy to
        // split these things out into another type if need be).
        private partial struct dwctxt2
        {
            public ptr<Link> linkctxt;
            public ptr<loader.Loader> ldr;
            public ptr<sys.Arch> arch; // This maps type name string (e.g. "uintptr") to loader symbol for
// the DWARF DIE for that type (e.g. "go.info.type.uintptr")
            public map<@string, loader.Sym> tmap; // This maps loader symbol for the DWARF DIE symbol generated for
// a type (e.g. "go.info.uintptr") to the type symbol itself
// ("type.uintptr").
// FIXME: try converting this map (and the next one) to a single
// array indexed by loader.Sym -- this may perform better.
            public map<loader.Sym, loader.Sym> rtmap; // This maps Go type symbol (e.g. "type.XXX") to loader symbol for
// the typedef DIE for that type (e.g. "go.info.XXX..def")
            public map<loader.Sym, loader.Sym> tdmap; // Cache these type symbols, so as to avoid repeatedly looking them up
            public loader.Sym typeRuntimeEface;
            public loader.Sym typeRuntimeIface;
            public loader.Sym uintptrInfoSym;
        }

        private static dwctxt2 newdwctxt2(ptr<Link> _addr_linkctxt, bool forTypeGen)
        {
            ref Link linkctxt = ref _addr_linkctxt.val;

            dwctxt2 d = new dwctxt2(linkctxt:linkctxt,ldr:linkctxt.loader,arch:linkctxt.Arch,tmap:make(map[string]loader.Sym),tdmap:make(map[loader.Sym]loader.Sym),rtmap:make(map[loader.Sym]loader.Sym),);
            d.typeRuntimeEface = d.lookupOrDiag("type.runtime.eface");
            d.typeRuntimeIface = d.lookupOrDiag("type.runtime.iface");
            return d;
        }

        // dwSym wraps a loader.Sym; this type is meant to obey the interface
        // rules for dwarf.Sym from the cmd/internal/dwarf package. DwDie and
        // DwAttr objects contain references to symbols via this type.
        private partial struct dwSym // : loader.Sym
        {
        }

        private static long Length(this dwSym s, object dwarfContext)
        {
            dwctxt2 l = dwarfContext._<dwctxt2>().ldr;
            return int64(len(l.Data(loader.Sym(s))));
        }

        private static long PtrSize(this dwctxt2 c)
        {
            return c.arch.PtrSize;
        }

        private static void AddInt(this dwctxt2 c, dwarf.Sym s, long size, long i)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            dsu.AddUintXX(c.arch, uint64(i), size);
        }

        private static void AddBytes(this dwctxt2 c, dwarf.Sym s, slice<byte> b)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            dsu.AddBytes(b);
        }

        private static void AddString(this dwctxt2 c, dwarf.Sym s, @string v)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            dsu.Addstring(v);
        }

        private static void AddAddress(this dwctxt2 c, dwarf.Sym s, object data, long value)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            if (value != 0L)
            {
                value -= dsu.Value();
            }

            var tgtds = loader.Sym(data._<dwSym>());
            dsu.AddAddrPlus(c.arch, tgtds, value);

        }

        private static void AddCURelativeAddress(this dwctxt2 c, dwarf.Sym s, object data, long value)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            if (value != 0L)
            {
                value -= dsu.Value();
            }

            var tgtds = loader.Sym(data._<dwSym>());
            dsu.AddCURelativeAddrPlus(c.arch, tgtds, value);

        }

        private static void AddSectionOffset(this dwctxt2 c, dwarf.Sym s, long size, object t, long ofs)
        {
            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            var tds = loader.Sym(t._<dwSym>());

            if (size == c.arch.PtrSize || size == 4L)             else 
                c.linkctxt.Errorf(ds, "invalid size %d in adddwarfref\n", size);
                        dsu.AddSymRef(c.arch, tds, ofs, objabi.R_ADDROFF, size);

        }

        private static void AddDWARFAddrSectionOffset(this dwctxt2 c, dwarf.Sym s, object t, long ofs)
        {
            long size = 4L;
            if (isDwarf64(c.linkctxt))
            {
                size = 8L;
            }

            var ds = loader.Sym(s._<dwSym>());
            var dsu = c.ldr.MakeSymbolUpdater(ds);
            var tds = loader.Sym(t._<dwSym>());

            if (size == c.arch.PtrSize || size == 4L)             else 
                c.linkctxt.Errorf(ds, "invalid size %d in adddwarfref\n", size);
                        dsu.AddSymRef(c.arch, tds, ofs, objabi.R_DWARFSECREF, size);

        }

        private static void Logf(this dwctxt2 c, @string format, params object[] args)
        {
            args = args.Clone();

            c.linkctxt.Logf(format, args);
        }

        // At the moment these interfaces are only used in the compiler.

        private static void AddFileRef(this dwctxt2 c, dwarf.Sym s, object f) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static long CurrentOffset(this dwctxt2 c, dwarf.Sym s) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static void RecordDclReference(this dwctxt2 c, dwarf.Sym s, dwarf.Sym t, long dclIdx, long inlIndex) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static void RecordChildDieOffsets(this dwctxt2 c, dwarf.Sym s, slice<ptr<dwarf.Var>> vars, slice<int> offsets) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static @string gdbscript = default;

        // dwarfSecInfo holds information about a DWARF output section,
        // specifically a section symbol and a list of symbols contained in
        // that section. On the syms list, the first symbol will always be the
        // section symbol, then any remaining symbols (if any) will be
        // sub-symbols in that section. Note that for some sections (eg:
        // .debug_abbrev), the section symbol is all there is (all content is
        // contained in it). For other sections (eg: .debug_info), the section
        // symbol is empty and all the content is in the sub-symbols. Finally
        // there are some sections (eg: .debug_ranges) where it is a mix (both
        // the section symbol and the sub-symbols have content)
        private partial struct dwarfSecInfo
        {
            public slice<loader.Sym> syms;
        }

        // secSym returns the section symbol for the section.
        private static loader.Sym secSym(this ptr<dwarfSecInfo> _addr_dsi)
        {
            ref dwarfSecInfo dsi = ref _addr_dsi.val;

            if (len(dsi.syms) == 0L)
            {
                return 0L;
            }

            return dsi.syms[0L];

        }

        // subSyms returns a list of sub-symbols for the section.
        private static slice<loader.Sym> subSyms(this ptr<dwarfSecInfo> _addr_dsi)
        {
            ref dwarfSecInfo dsi = ref _addr_dsi.val;

            if (len(dsi.syms) == 0L)
            {
                return new slice<loader.Sym>(new loader.Sym[] {  });
            }

            return dsi.syms[1L..];

        }

        // dwarfp2 stores the collected DWARF symbols created during
        // dwarf generation.
        private static slice<dwarfSecInfo> dwarfp2 = default;

        private static dwarfSecInfo writeabbrev(this ptr<dwctxt2> _addr_d)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var abrvs = d.ldr.LookupOrCreateSym(".debug_abbrev", 0L);
            var u = d.ldr.MakeSymbolUpdater(abrvs);
            u.SetType(sym.SDWARFSECT);
            u.AddBytes(dwarf.GetAbbrev());
            return new dwarfSecInfo(syms:[]loader.Sym{abrvs});
        }

        private static dwarf.DWDie dwtypes = default;

        // newattr attaches a new attribute to the specified DIE.
        //
        // FIXME: at the moment attributes are stored in a linked list in a
        // fairly space-inefficient way -- it might be better to instead look
        // up all attrs in a single large table, then store indices into the
        // table in the DIE. This would allow us to common up storage for
        // attributes that are shared by many DIEs (ex: byte size of N).
        private static ptr<dwarf.DWAttr> newattr(ptr<dwarf.DWDie> _addr_die, ushort attr, long cls, long value, object data)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            ptr<dwarf.DWAttr> a = @new<dwarf.DWAttr>();
            a.Link = die.Attr;
            die.Attr = a;
            a.Atr = attr;
            a.Cls = uint8(cls);
            a.Value = value;
            a.Data = data;
            return _addr_a!;
        }

        // Each DIE (except the root ones) has at least 1 attribute: its
        // name. getattr moves the desired one to the front so
        // frequently searched ones are found faster.
        private static ptr<dwarf.DWAttr> getattr(ptr<dwarf.DWDie> _addr_die, ushort attr)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            if (die.Attr.Atr == attr)
            {
                return _addr_die.Attr!;
            }

            var a = die.Attr;
            var b = a.Link;
            while (b != null)
            {
                if (b.Atr == attr)
                {
                    a.Link = b.Link;
                    b.Link = die.Attr;
                    die.Attr = b;
                    return _addr_b!;
                }

                a = b;
                b = b.Link;

            }


            return _addr_null!;

        }

        // Every DIE manufactured by the linker has at least an AT_name
        // attribute (but it will only be written out if it is listed in the abbrev).
        // The compiler does create nameless DWARF DIEs (ex: concrete subprogram
        // instance).
        // FIXME: it would be more efficient to bulk-allocate DIEs.
        private static ptr<dwarf.DWDie> newdie(this ptr<dwctxt2> _addr_d, ptr<dwarf.DWDie> _addr_parent, long abbrev, @string name, long version)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie parent = ref _addr_parent.val;

            ptr<dwarf.DWDie> die = @new<dwarf.DWDie>();
            die.Abbrev = abbrev;
            die.Link = parent.Child;
            parent.Child = die;

            newattr(die, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len(name)), name);

            if (name != "" && (abbrev <= dwarf.DW_ABRV_VARIABLE || abbrev >= dwarf.DW_ABRV_NULLTYPE))
            { 
                // Q: do we need version here? My understanding is that all these
                // symbols should be version 0.
                if (abbrev != dwarf.DW_ABRV_VARIABLE || version == 0L)
                {
                    if (abbrev == dwarf.DW_ABRV_COMPUNIT)
                    { 
                        // Avoid collisions with "real" symbol names.
                        name = fmt.Sprintf(".pkg.%s.%d", name, len(d.linkctxt.compUnits));

                    }

                    var ds = d.ldr.LookupOrCreateSym(dwarf.InfoPrefix + name, version);
                    var dsu = d.ldr.MakeSymbolUpdater(ds);
                    dsu.SetType(sym.SDWARFINFO);
                    d.ldr.SetAttrNotInSymbolTable(ds, true);
                    d.ldr.SetAttrReachable(ds, true);
                    die.Sym = dwSym(ds);
                    if (abbrev >= dwarf.DW_ABRV_NULLTYPE && abbrev <= dwarf.DW_ABRV_TYPEDECL)
                    {
                        d.tmap[name] = ds;
                    }

                }

            }

            return _addr_die!;

        }

        private static ptr<dwarf.DWDie> walktypedef(ptr<dwarf.DWDie> _addr_die)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            if (die == null)
            {
                return _addr_null!;
            } 
            // Resolve typedef if present.
            if (die.Abbrev == dwarf.DW_ABRV_TYPEDECL)
            {
                {
                    var attr = die.Attr;

                    while (attr != null)
                    {
                        if (attr.Atr == dwarf.DW_AT_type && attr.Cls == dwarf.DW_CLS_REFERENCE && attr.Data != null)
                        {
                            return attr.Data._<ptr<dwarf.DWDie>>();
                        attr = attr.Link;
                        }

                    }

                }

            }

            return _addr_die!;

        }

        private static loader.Sym walksymtypedef(this ptr<dwctxt2> _addr_d, loader.Sym symIdx)
        {
            ref dwctxt2 d = ref _addr_d.val;

            // We're being given the loader symbol for the type DIE, e.g.
            // "go.info.type.uintptr". Map that first to the type symbol (e.g.
            // "type.uintptr") and then to the typedef DIE for the type.
            // FIXME: this seems clunky, maybe there is a better way to do this.

            {
                var (ts, ok) = d.rtmap[symIdx];

                if (ok)
                {
                    {
                        var (def, ok) = d.tdmap[ts];

                        if (ok)
                        {
                            return def;
                        }

                    }

                    d.linkctxt.Errorf(ts, "internal error: no entry for sym %d in tdmap\n", ts);
                    return 0L;

                }

            }

            d.linkctxt.Errorf(symIdx, "internal error: no entry for sym %d in rtmap\n", symIdx);
            return 0L;

        }

        // Find child by AT_name using hashtable if available or linear scan
        // if not.
        private static ptr<dwarf.DWDie> findchild(ptr<dwarf.DWDie> _addr_die, @string name)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            ptr<dwarf.DWDie> prev;
            while (die != prev)
            {
                {
                    var a = die.Child;

                    while (a != null)
                    {
                        if (name == getattr(_addr_a, dwarf.DW_AT_name).Data)
                        {
                            return _addr_a!;
                        a = a.Link;
                        }

                prev = die;
            die = walktypedef(_addr_die);
                    }

                }
                continue;

            }

            return _addr_null!;

        }

        // Used to avoid string allocation when looking up dwarf symbols
        private static slice<byte> prefixBuf = (slice<byte>)dwarf.InfoPrefix;

        // find looks up the loader symbol for the DWARF DIE generated for the
        // type with the specified name.
        private static loader.Sym find(this ptr<dwctxt2> _addr_d, @string name)
        {
            ref dwctxt2 d = ref _addr_d.val;

            return d.tmap[name];
        }

        private static loader.Sym mustFind(this ptr<dwctxt2> _addr_d, @string name)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var r = d.find(name);
            if (r == 0L)
            {
                Exitf("dwarf find: cannot find %s", name);
            }

            return r;

        }

        private static long adddwarfref(this ptr<dwctxt2> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, loader.Sym t, long size)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref loader.SymbolBuilder sb = ref _addr_sb.val;

            long result = default;

            if (size == d.arch.PtrSize || size == 4L)             else 
                d.linkctxt.Errorf(sb.Sym(), "invalid size %d in adddwarfref\n", size);
                        result = sb.AddSymRef(d.arch, t, 0L, objabi.R_DWARFSECREF, size);
            return result;

        }

        private static ptr<dwarf.DWAttr> newrefattr(this ptr<dwctxt2> _addr_d, ptr<dwarf.DWDie> _addr_die, ushort attr, loader.Sym @ref)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            if (ref == 0L)
            {
                return _addr_null!;
            }

            return _addr_newattr(_addr_die, attr, dwarf.DW_CLS_REFERENCE, 0L, dwSym(ref))!;

        }

        private static loader.Sym dtolsym(this ptr<dwctxt2> _addr_d, dwarf.Sym s)
        {
            ref dwctxt2 d = ref _addr_d.val;

            if (s == null)
            {
                return 0L;
            }

            var dws = loader.Sym(s._<dwSym>());
            return dws;

        }

        private static slice<loader.Sym> putdie(this ptr<dwctxt2> _addr_d, slice<loader.Sym> syms, ptr<dwarf.DWDie> _addr_die)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var s = d.dtolsym(die.Sym);
            if (s == 0L)
            {
                s = syms[len(syms) - 1L];
            }
            else
            {
                if (d.ldr.AttrOnList(s))
                {
                    log.Fatalf("symbol %s listed multiple times", d.ldr.SymName(s));
                }

                d.ldr.SetAttrOnList(s, true);
                syms = append(syms, s);

            }

            var sDwsym = dwSym(s);
            dwarf.Uleb128put(d, sDwsym, int64(die.Abbrev));
            dwarf.PutAttrs(d, sDwsym, die.Abbrev, die.Attr);
            if (dwarf.HasChildren(die))
            {
                {
                    var die = die.Child;

                    while (die != null)
                    {
                        syms = d.putdie(syms, die);
                        die = die.Link;
                    }

                }
                var dsu = d.ldr.MakeSymbolUpdater(syms[len(syms) - 1L]);
                dsu.AddUint8(0L);

            }

            return syms;

        }

        private static void reverselist(ptr<ptr<dwarf.DWDie>> _addr_list)
        {
            ref ptr<dwarf.DWDie> list = ref _addr_list.val;

            ptr<ptr<dwarf.DWDie>> curr = list.val;
            ptr<dwarf.DWDie> prev;
            while (curr != null)
            {
                var next = curr.Link;
                curr.Link = prev;
                prev = addr(curr);
                curr = next;
            }


            list.val = addr(prev);

        }

        private static void reversetree(ptr<ptr<dwarf.DWDie>> _addr_list)
        {
            ref ptr<dwarf.DWDie> list = ref _addr_list.val;

            reverselist(_addr_list);
            {
                ptr<ptr<dwarf.DWDie>> die = list.val;

                while (die != null)
                {
                    if (dwarf.HasChildren(die))
                    {
                        reversetree(_addr_die.Child);
                    die = die.Link;
                    }

                }

            }

        }

        private static void newmemberoffsetattr(ptr<dwarf.DWDie> _addr_die, int offs)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            newattr(_addr_die, dwarf.DW_AT_data_member_location, dwarf.DW_CLS_CONSTANT, int64(offs), null);
        }

        // GDB doesn't like FORM_addr for AT_location, so emit a
        // location expression that evals to a const.
        private static void newabslocexprattr(this ptr<dwctxt2> _addr_d, ptr<dwarf.DWDie> _addr_die, long addr, loader.Sym symIdx)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            newattr(_addr_die, dwarf.DW_AT_location, dwarf.DW_CLS_ADDRESS, addr, dwSym(symIdx));
        }

        private static loader.Sym lookupOrDiag(this ptr<dwctxt2> _addr_d, @string n)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var symIdx = d.ldr.Lookup(n, 0L);
            if (symIdx == 0L)
            {
                Exitf("dwarf: missing type: %s", n);
            }

            if (len(d.ldr.Data(symIdx)) == 0L)
            {
                Exitf("dwarf: missing type (no data): %s", n);
            }

            return symIdx;

        }

        private static ptr<dwarf.DWDie> dotypedef(this ptr<dwctxt2> _addr_d, ptr<dwarf.DWDie> _addr_parent, loader.Sym gotype, @string name, ptr<dwarf.DWDie> _addr_def)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie parent = ref _addr_parent.val;
            ref dwarf.DWDie def = ref _addr_def.val;
 
            // Only emit typedefs for real names.
            if (strings.HasPrefix(name, "map["))
            {
                return _addr_null!;
            }

            if (strings.HasPrefix(name, "struct {"))
            {
                return _addr_null!;
            }

            if (strings.HasPrefix(name, "chan "))
            {
                return _addr_null!;
            }

            if (name[0L] == '[' || name[0L] == '*')
            {
                return _addr_null!;
            }

            if (def == null)
            {
                Errorf(null, "dwarf: bad def in dotypedef");
            } 

            // Create a new loader symbol for the typedef. We no longer
            // do lookups of typedef symbols by name, so this is going
            // to be an anonymous symbol (we want this for perf reasons).
            var tds = d.ldr.CreateExtSym("", 0L);
            var tdsu = d.ldr.MakeSymbolUpdater(tds);
            tdsu.SetType(sym.SDWARFINFO);
            def.Sym = dwSym(tds);
            d.ldr.SetAttrNotInSymbolTable(tds, true);
            d.ldr.SetAttrReachable(tds, true); 

            // The typedef entry must be created after the def,
            // so that future lookups will find the typedef instead
            // of the real definition. This hooks the typedef into any
            // circular definition loops, so that gdb can understand them.
            var die = d.newdie(parent, dwarf.DW_ABRV_TYPEDECL, name, 0L);

            d.newrefattr(die, dwarf.DW_AT_type, tds);

            return _addr_die!;

        }

        // Define gotype, for composite ones recurse into constituents.
        private static loader.Sym defgotype(this ptr<dwctxt2> _addr_d, loader.Sym gotype)
        {
            ref dwctxt2 d = ref _addr_d.val;

            if (gotype == 0L)
            {
                return d.mustFind("<unspecified>");
            } 

            // If we already have a tdmap entry for the gotype, return it.
            {
                var (ds, ok) = d.tdmap[gotype];

                if (ok)
                {
                    return ds;
                }

            }


            var sn = d.ldr.SymName(gotype);
            if (!strings.HasPrefix(sn, "type."))
            {
                d.linkctxt.Errorf(gotype, "dwarf: type name doesn't start with \"type.\"");
                return d.mustFind("<unspecified>");
            }

            var name = sn[5L..]; // could also decode from Type.string

            var sdie = d.find(name);
            if (sdie != 0L)
            {
                return sdie;
            }

            var gtdwSym = d.newtype(gotype);
            d.tdmap[gotype] = loader.Sym(gtdwSym.Sym._<dwSym>());
            return loader.Sym(gtdwSym.Sym._<dwSym>());

        }

        private static ptr<dwarf.DWDie> newtype(this ptr<dwctxt2> _addr_d, loader.Sym gotype)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var sn = d.ldr.SymName(gotype);
            var name = sn[5L..]; // could also decode from Type.string
            var tdata = d.ldr.Data(gotype);
            var kind = decodetypeKind(d.arch, tdata);
            var bytesize = decodetypeSize(d.arch, tdata);

            ptr<dwarf.DWDie> die;            ptr<dwarf.DWDie> typedefdie;


            if (kind == objabi.KindBool) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_boolean, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindInt || kind == objabi.KindInt8 || kind == objabi.KindInt16 || kind == objabi.KindInt32 || kind == objabi.KindInt64) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_signed, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindUint || kind == objabi.KindUint8 || kind == objabi.KindUint16 || kind == objabi.KindUint32 || kind == objabi.KindUint64 || kind == objabi.KindUintptr) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindFloat32 || kind == objabi.KindFloat64) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindComplex64 || kind == objabi.KindComplex128) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_complex_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindArray) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_ARRAYTYPE, name, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                var s = decodetypeArrayElem(d.ldr, d.arch, gotype);
                d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
                var fld = d.newdie(die, dwarf.DW_ABRV_ARRAYRANGE, "range", 0L); 

                // use actual length not upper bound; correct for 0-length arrays.
                newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, decodetypeArrayLen(d.ldr, d.arch, gotype), 0L);

                d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
            else if (kind == objabi.KindChan) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_CHANTYPE, name, 0L);
                s = decodetypeChanElem(d.ldr, d.arch, gotype);
                d.newrefattr(die, dwarf.DW_AT_go_elem, d.defgotype(s)); 
                // Save elem type for synthesizechantypes. We could synthesize here
                // but that would change the order of DIEs we output.
                d.newrefattr(die, dwarf.DW_AT_type, s);
            else if (kind == objabi.KindFunc) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_FUNCTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                var data = d.ldr.Data(gotype); 
                // FIXME: add caching or reuse reloc slice.
                ref var relocs = ref heap(d.ldr.Relocs(gotype), out ptr<var> _addr_relocs);
                var nfields = decodetypeFuncInCount(d.arch, data);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncInType(d.ldr, d.arch, gotype, _addr_relocs, i);
                        sn = d.ldr.SymName(s);
                        fld = d.newdie(die, dwarf.DW_ABRV_FUNCTYPEPARAM, sn[5L..], 0L);
                        d.newrefattr(fld, dwarf.DW_AT_type, d.defgotype(s));
                    }


                    i = i__prev1;
                }

                if (decodetypeFuncDotdotdot(d.arch, data))
                {
                    d.newdie(die, dwarf.DW_ABRV_DOTDOTDOT, "...", 0L);
                }

                nfields = decodetypeFuncOutCount(d.arch, data);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncOutType(d.ldr, d.arch, gotype, _addr_relocs, i);
                        sn = d.ldr.SymName(s);
                        fld = d.newdie(die, dwarf.DW_ABRV_FUNCTYPEPARAM, sn[5L..], 0L);
                        d.newrefattr(fld, dwarf.DW_AT_type, d.defptrto(d.defgotype(s)));
                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindInterface) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_IFACETYPE, name, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                data = d.ldr.Data(gotype);
                nfields = int(decodetypeIfaceMethodCount(d.arch, data));
                s = default;
                if (nfields == 0L)
                {
                    s = d.typeRuntimeEface;
                }
                else
                {
                    s = d.typeRuntimeIface;
                }

                d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
            else if (kind == objabi.KindMap) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_MAPTYPE, name, 0L);
                s = decodetypeMapKey(d.ldr, d.arch, gotype);
                d.newrefattr(die, dwarf.DW_AT_go_key, d.defgotype(s));
                s = decodetypeMapValue(d.ldr, d.arch, gotype);
                d.newrefattr(die, dwarf.DW_AT_go_elem, d.defgotype(s)); 
                // Save gotype for use in synthesizemaptypes. We could synthesize here,
                // but that would change the order of the DIEs.
                d.newrefattr(die, dwarf.DW_AT_type, gotype);
            else if (kind == objabi.KindPtr) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, name, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                s = decodetypePtrElem(d.ldr, d.arch, gotype);
                d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
            else if (kind == objabi.KindSlice) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_SLICETYPE, name, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                s = decodetypeArrayElem(d.ldr, d.arch, gotype);
                var elem = d.defgotype(s);
                d.newrefattr(die, dwarf.DW_AT_go_elem, elem);
            else if (kind == objabi.KindString) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_STRINGTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindStruct) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_STRUCTTYPE, name, 0L);
                typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                nfields = decodetypeStructFieldCount(d.ldr, d.arch, gotype);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < nfields; i++)
                    {
                        var f = decodetypeStructFieldName(d.ldr, d.arch, gotype, i);
                        s = decodetypeStructFieldType(d.ldr, d.arch, gotype, i);
                        if (f == "")
                        {
                            sn = d.ldr.SymName(s);
                            f = sn[5L..]; // skip "type."
                        }

                        fld = d.newdie(die, dwarf.DW_ABRV_STRUCTFIELD, f, 0L);
                        d.newrefattr(fld, dwarf.DW_AT_type, d.defgotype(s));
                        var offsetAnon = decodetypeStructFieldOffsAnon(d.ldr, d.arch, gotype, i);
                        newmemberoffsetattr(_addr_fld, int32(offsetAnon >> (int)(1L)));
                        if (offsetAnon & 1L != 0L)
                        { // is embedded field
                            newattr(_addr_fld, dwarf.DW_AT_go_embedded_field, dwarf.DW_CLS_FLAG, 1L, 0L);

                        }

                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindUnsafePointer) 
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, name, 0L);
            else 
                d.linkctxt.Errorf(gotype, "dwarf: definition of unknown kind %d", kind);
                die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_TYPEDECL, name, 0L);
                d.newrefattr(die, dwarf.DW_AT_type, d.mustFind("<unspecified>"));
                        newattr(die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, int64(kind), 0L);

            if (d.ldr.AttrReachable(gotype))
            {
                newattr(die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0L, dwSym(gotype));
            } 

            // Sanity check.
            {
                var (_, ok) = d.rtmap[gotype];

                if (ok)
                {
                    log.Fatalf("internal error: rtmap entry already installed\n");
                }

            }


            var ds = loader.Sym(die.Sym._<dwSym>());
            if (typedefdie != null)
            {
                ds = loader.Sym(typedefdie.Sym._<dwSym>());
            }

            d.rtmap[ds] = gotype;

            {
                (_, ok) = prototypedies[sn];

                if (ok)
                {
                    prototypedies[sn] = die;
                }

            }


            if (typedefdie != null)
            {
                return _addr_typedefdie!;
            }

            return _addr_die!;

        }

        private static @string nameFromDIESym(this ptr<dwctxt2> _addr_d, loader.Sym dwtypeDIESym)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var sn = d.ldr.SymName(dwtypeDIESym);
            return sn[len(dwarf.InfoPrefix)..];
        }

        private static loader.Sym defptrto(this ptr<dwctxt2> _addr_d, loader.Sym dwtype)
        {
            ref dwctxt2 d = ref _addr_d.val;

            // FIXME: it would be nice if the compiler attached an aux symbol
            // ref from the element type to the pointer type -- it would be
            // more efficient to do it this way as opposed to via name lookups.

            @string ptrname = "*" + d.nameFromDIESym(dwtype);
            {
                var die = d.find(ptrname);

                if (die != 0L)
                {
                    return die;
                }

            }


            var pdie = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, ptrname, 0L);
            d.newrefattr(pdie, dwarf.DW_AT_type, dwtype); 

            // The DWARF info synthesizes pointer types that don't exist at the
            // language level, like *hash<...> and *bucket<...>, and the data
            // pointers of slices. Link to the ones we can find.
            var gts = d.ldr.Lookup("type." + ptrname, 0L);
            if (gts != 0L && d.ldr.AttrReachable(gts))
            {
                newattr(_addr_pdie, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0L, dwSym(gts));
            }

            if (gts != 0L)
            {
                var ds = loader.Sym(pdie.Sym._<dwSym>());
                d.rtmap[ds] = gts;
                d.tdmap[gts] = ds;
            }

            return d.dtolsym(pdie.Sym);

        }

        // Copies src's children into dst. Copies attributes by value.
        // DWAttr.data is copied as pointer only. If except is one of
        // the top-level children, it will not be copied.
        private static void copychildrenexcept(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src, ptr<dwarf.DWDie> _addr_except)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie dst = ref _addr_dst.val;
            ref dwarf.DWDie src = ref _addr_src.val;
            ref dwarf.DWDie except = ref _addr_except.val;

            src = src.Child;

            while (src != null)
            {
                if (src == except)
                {
                    continue;
                src = src.Link;
                }

                var c = d.newdie(dst, src.Abbrev, getattr(_addr_src, dwarf.DW_AT_name).Data._<@string>(), 0L);
                {
                    var a = src.Attr;

                    while (a != null)
                    {
                        newattr(_addr_c, a.Atr, int(a.Cls), a.Value, a.Data);
                        a = a.Link;
                    }

                }
                d.copychildrenexcept(ctxt, c, src, null);

            }


            reverselist(_addr_dst.Child);

        }

        private static void copychildren(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie dst = ref _addr_dst.val;
            ref dwarf.DWDie src = ref _addr_src.val;

            d.copychildrenexcept(ctxt, dst, src, null);
        }

        // Search children (assumed to have TAG_member) for the one named
        // field and set its AT_type to dwtype
        private static void substitutetype(this ptr<dwctxt2> _addr_d, ptr<dwarf.DWDie> _addr_structdie, @string field, loader.Sym dwtype)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref dwarf.DWDie structdie = ref _addr_structdie.val;

            var child = findchild(_addr_structdie, field);
            if (child == null)
            {
                Exitf("dwarf substitutetype: %s does not have member %s", getattr(_addr_structdie, dwarf.DW_AT_name).Data, field);
                return ;
            }

            var a = getattr(_addr_child, dwarf.DW_AT_type);
            if (a != null)
            {
                a.Data = dwSym(dwtype);
            }
            else
            {
                d.newrefattr(child, dwarf.DW_AT_type, dwtype);
            }

        }

        private static ptr<dwarf.DWDie> findprotodie(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, @string name)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            var (die, ok) = prototypedies[name];
            if (ok && die == null)
            {
                d.defgotype(d.lookupOrDiag(name));
                die = prototypedies[name];
            }

            if (die == null)
            {
                log.Fatalf("internal error: DIE generation failed for %s\n", name);
            }

            return _addr_die!;

        }

        private static void synthesizestringtypes(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var prototype = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.stringStructDWARF"));
            if (prototype == null)
            {
                return ;
            }

            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_STRINGTYPE)
                {
                    continue;
                die = die.Link;
                }

                d.copychildren(ctxt, die, prototype);

            }


        }

        private static void synthesizeslicetypes(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var prototype = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.slice"));
            if (prototype == null)
            {
                return ;
            }

            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_SLICETYPE)
                {
                    continue;
                die = die.Link;
                }

                d.copychildren(ctxt, die, prototype);
                var elem = loader.Sym(getattr(_addr_die, dwarf.DW_AT_go_elem).Data._<dwSym>());
                d.substitutetype(die, "array", d.defptrto(elem));

            }


        }

        private static @string mkinternaltypename(@string @base, @string arg1, @string arg2)
        {
            if (arg2 == "")
            {
                return fmt.Sprintf("%s<%s>", base, arg1);
            }

            return fmt.Sprintf("%s<%s,%s>", base, arg1, arg2);

        }

        // synthesizemaptypes is way too closely married to runtime/hashmap.c
        public static readonly long MaxKeySize = (long)128L;
        public static readonly long MaxValSize = (long)128L;
        public static readonly long BucketSize = (long)8L;


        private static loader.Sym mkinternaltype(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, long abbrev, @string typename, @string keyname, @string valname, Action<ptr<dwarf.DWDie>> f)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            var name = mkinternaltypename(typename, keyname, valname);
            var symname = dwarf.InfoPrefix + name;
            var s = d.ldr.Lookup(symname, 0L);
            if (s != 0L && d.ldr.SymType(s) == sym.SDWARFINFO)
            {
                return s;
            }

            var die = d.newdie(_addr_dwtypes, abbrev, name, 0L);
            f(die);
            return d.dtolsym(die.Sym);

        }

        private static void synthesizemaptypes(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var hash = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.hmap"));
            var bucket = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.bmap"));

            if (hash == null)
            {
                return ;
            }

            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_MAPTYPE)
                {
                    continue;
                die = die.Link;
                }

                var gotype = loader.Sym(getattr(_addr_die, dwarf.DW_AT_type).Data._<dwSym>());
                var keytype = decodetypeMapKey(d.ldr, d.arch, gotype);
                var valtype = decodetypeMapValue(d.ldr, d.arch, gotype);
                var keydata = d.ldr.Data(keytype);
                var valdata = d.ldr.Data(valtype);
                var keysize = decodetypeSize(d.arch, keydata);
                var valsize = decodetypeSize(d.arch, valdata);
                keytype = d.walksymtypedef(d.defgotype(keytype));
                valtype = d.walksymtypedef(d.defgotype(valtype)); 

                // compute size info like hashmap.c does.
                var indirectKey = false;
                var indirectVal = false;
                if (keysize > MaxKeySize)
                {
                    keysize = int64(d.arch.PtrSize);
                    indirectKey = true;
                }

                if (valsize > MaxValSize)
                {
                    valsize = int64(d.arch.PtrSize);
                    indirectVal = true;
                } 

                // Construct type to represent an array of BucketSize keys
                var keyname = d.nameFromDIESym(keytype);
                var dwhks = d.mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]key", keyname, "", dwhk =>
                {
                    newattr(_addr_dwhk, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * keysize, 0L);
                    var t = keytype;
                    if (indirectKey)
                    {
                        t = d.defptrto(keytype);
                    }

                    d.newrefattr(dwhk, dwarf.DW_AT_type, t);
                    var fld = d.newdie(dwhk, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);

                }); 

                // Construct type to represent an array of BucketSize values
                var valname = d.nameFromDIESym(valtype);
                var dwhvs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]val", valname, "", dwhv =>
                {
                    newattr(_addr_dwhv, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * valsize, 0L);
                    t = valtype;
                    if (indirectVal)
                    {
                        t = d.defptrto(valtype);
                    }

                    d.newrefattr(dwhv, dwarf.DW_AT_type, t);
                    fld = d.newdie(dwhv, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);

                }); 

                // Construct bucket<K,V>
                var dwhbs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "bucket", keyname, valname, dwhb =>
                { 
                    // Copy over all fields except the field "data" from the generic
                    // bucket. "data" will be replaced with keys/values below.
                    d.copychildrenexcept(ctxt, dwhb, bucket, findchild(_addr_bucket, "data"));

                    fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "keys", 0L);
                    d.newrefattr(fld, dwarf.DW_AT_type, dwhks);
                    newmemberoffsetattr(_addr_fld, BucketSize);
                    fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "values", 0L);
                    d.newrefattr(fld, dwarf.DW_AT_type, dwhvs);
                    newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * int32(keysize));
                    fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "overflow", 0L);
                    d.newrefattr(fld, dwarf.DW_AT_type, d.defptrto(d.dtolsym(dwhb.Sym)));
                    newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)));
                    if (d.arch.RegSize > d.arch.PtrSize)
                    {
                        fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "pad", 0L);
                        d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
                        newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)) + int32(d.arch.PtrSize));
                    }

                    newattr(_addr_dwhb, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize + BucketSize * keysize + BucketSize * valsize + int64(d.arch.RegSize), 0L);

                }); 

                // Construct hash<K,V>
                var dwhs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hash", keyname, valname, dwh =>
                {
                    d.copychildren(ctxt, dwh, hash);
                    d.substitutetype(dwh, "buckets", d.defptrto(dwhbs));
                    d.substitutetype(dwh, "oldbuckets", d.defptrto(dwhbs));
                    newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hash, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // make map type a pointer to hash<K,V>
                d.newrefattr(die, dwarf.DW_AT_type, d.defptrto(dwhs));

            }


        }

        private static void synthesizechantypes(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var sudog = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.sudog"));
            var waitq = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.waitq"));
            var hchan = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.hchan"));
            if (sudog == null || waitq == null || hchan == null)
            {
                return ;
            }

            var sudogsize = int(getattr(_addr_sudog, dwarf.DW_AT_byte_size).Value);

            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_CHANTYPE)
                {
                    continue;
                die = die.Link;
                }

                var elemgotype = loader.Sym(getattr(_addr_die, dwarf.DW_AT_type).Data._<dwSym>());
                var tname = d.ldr.SymName(elemgotype);
                var elemname = tname[5L..];
                var elemtype = d.walksymtypedef(d.defgotype(d.lookupOrDiag(tname))); 

                // sudog<T>
                var dwss = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "sudog", elemname, "", dws =>
                {
                    d.copychildren(ctxt, dws, sudog);
                    d.substitutetype(dws, "elem", d.defptrto(elemtype));
                    newattr(_addr_dws, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(sudogsize), null);
                }); 

                // waitq<T>
                var dwws = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "waitq", elemname, "", dww =>
                {
                    d.copychildren(ctxt, dww, waitq);
                    d.substitutetype(dww, "first", d.defptrto(dwss));
                    d.substitutetype(dww, "last", d.defptrto(dwss));
                    newattr(_addr_dww, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_waitq, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // hchan<T>
                var dwhs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hchan", elemname, "", dwh =>
                {
                    d.copychildren(ctxt, dwh, hchan);
                    d.substitutetype(dwh, "recvq", dwws);
                    d.substitutetype(dwh, "sendq", dwws);
                    newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hchan, dwarf.DW_AT_byte_size).Value, null);
                });

                d.newrefattr(die, dwarf.DW_AT_type, d.defptrto(dwhs));

            }


        }

        private static void dwarfDefineGlobal(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, loader.Sym symIdx, @string str, long v, loader.Sym gotype)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Find a suitable CU DIE to include the global.
            // One would think it's as simple as just looking at the unit, but that might
            // not have any reachable code. So, we go to the runtime's CU if our unit
            // isn't otherwise reachable.
            var unit = d.ldr.SymUnit(symIdx);
            if (unit == null)
            {
                unit = ctxt.runtimeCU;
            }

            var ver = d.ldr.SymVersion(symIdx);
            var dv = d.newdie(unit.DWInfo, dwarf.DW_ABRV_VARIABLE, str, int(ver));
            d.newabslocexprattr(dv, v, symIdx);
            if (d.ldr.SymVersion(symIdx) < sym.SymVerStatic)
            {
                newattr(_addr_dv, dwarf.DW_AT_external, dwarf.DW_CLS_FLAG, 1L, 0L);
            }

            var dt = d.defgotype(gotype);
            d.newrefattr(dv, dwarf.DW_AT_type, dt);

        }

        // createUnitLength creates the initial length field with value v and update
        // offset of unit_length if needed.
        private static void createUnitLength(this ptr<dwctxt2> _addr_d, ptr<loader.SymbolBuilder> _addr_su, ulong v)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref loader.SymbolBuilder su = ref _addr_su.val;

            if (isDwarf64(d.linkctxt))
            {
                su.AddUint32(d.arch, 0xFFFFFFFFUL);
            }

            d.addDwarfAddrField(su, v);

        }

        // addDwarfAddrField adds a DWARF field in DWARF 64bits or 32bits.
        private static void addDwarfAddrField(this ptr<dwctxt2> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, ulong v)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref loader.SymbolBuilder sb = ref _addr_sb.val;

            if (isDwarf64(d.linkctxt))
            {
                sb.AddUint(d.arch, v);
            }
            else
            {
                sb.AddUint32(d.arch, uint32(v));
            }

        }

        // addDwarfAddrRef adds a DWARF pointer in DWARF 64bits or 32bits.
        private static void addDwarfAddrRef(this ptr<dwctxt2> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, loader.Sym t)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref loader.SymbolBuilder sb = ref _addr_sb.val;

            if (isDwarf64(d.linkctxt))
            {
                d.adddwarfref(sb, t, 8L);
            }
            else
            {
                d.adddwarfref(sb, t, 4L);
            }

        }

        // calcCompUnitRanges calculates the PC ranges of the compilation units.
        private static void calcCompUnitRanges(this ptr<dwctxt2> _addr_d)
        {
            ref dwctxt2 d = ref _addr_d.val;

            ptr<sym.CompilationUnit> prevUnit;
            foreach (var (_, s) in d.linkctxt.Textp2)
            {
                var sym = loader.Sym(s);

                var fi = d.ldr.FuncInfo(sym);
                if (!fi.Valid())
                {
                    continue;
                } 

                // Skip linker-created functions (ex: runtime.addmoduledata), since they
                // don't have DWARF to begin with.
                var unit = d.ldr.SymUnit(sym);
                if (unit == null)
                {
                    continue;
                } 

                // Update PC ranges.
                //
                // We don't simply compare the end of the previous
                // symbol with the start of the next because there's
                // often a little padding between them. Instead, we
                // only create boundaries between symbols from
                // different units.
                var sval = d.ldr.SymValue(sym);
                var u0val = d.ldr.SymValue(loader.Sym(unit.Textp2[0L]));
                if (prevUnit != unit)
                {
                    unit.PCs = append(unit.PCs, new dwarf.Range(Start:sval-u0val));
                    prevUnit = unit;
                }

                unit.PCs[len(unit.PCs) - 1L].End = sval - u0val + int64(len(d.ldr.Data(sym)));

            }

        }

        private static void movetomodule(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_parent)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie parent = ref _addr_parent.val;

            var die = ctxt.runtimeCU.DWInfo.Child;
            if (die == null)
            {
                ctxt.runtimeCU.DWInfo.Child = parent.Child;
                return ;
            }

            while (die.Link != null)
            {
                die = die.Link;
            }

            die.Link = parent.Child;

        }

        /*
         * Generate a sequence of opcodes that is as short as possible.
         * See section 6.2.5
         */
        public static readonly long LINE_BASE = (long)-4L;
        public static readonly long LINE_RANGE = (long)10L;
        public static readonly long PC_RANGE = (long)(255L - OPCODE_BASE) / LINE_RANGE;
        public static readonly long OPCODE_BASE = (long)11L;


        /*
         * Walk prog table, emit line program and build DIE tree.
         */

        private static @string getCompilationDir()
        { 
            // OSX requires this be set to something, but it's not easy to choose
            // a value. Linking takes place in a temporary directory, so there's
            // no point including it here. Paths in the file table are usually
            // absolute, in which case debuggers will ignore this value. -trimpath
            // produces relative paths, but we don't know where they start, so
            // all we can do here is try not to make things worse.
            return ".";

        }

        private static void importInfoSymbol(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, loader.Sym dsym)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            d.ldr.SetAttrReachable(dsym, true);
            d.ldr.SetAttrNotInSymbolTable(dsym, true);
            if (d.ldr.SymType(dsym) != sym.SDWARFINFO)
            {
                log.Fatalf("error: DWARF info sym %d/%s with incorrect type %s", dsym, d.ldr.SymName(dsym), d.ldr.SymType(dsym).String());
            }

            var relocs = d.ldr.Relocs(dsym);
            for (long i = 0L; i < relocs.Count(); i++)
            {
                var r = relocs.At2(i);
                if (r.Type() != objabi.R_DWARFSECREF)
                {
                    continue;
                }

                var rsym = r.Sym(); 
                // If there is an entry for the symbol in our rtmap, then it
                // means we've processed the type already, and can skip this one.
                {
                    var (_, ok) = d.rtmap[rsym];

                    if (ok)
                    { 
                        // type already generated
                        continue;

                    } 
                    // FIXME: is there a way we could avoid materializing the
                    // symbol name here?

                } 
                // FIXME: is there a way we could avoid materializing the
                // symbol name here?
                var sn = d.ldr.SymName(rsym);
                var tn = sn[len(dwarf.InfoPrefix)..];
                var ts = d.ldr.Lookup("type." + tn, 0L);
                d.defgotype(ts);

            }


        }

        private static @string expandFile(@string fname)
        {
            if (strings.HasPrefix(fname, src.FileSymPrefix))
            {
                fname = fname[len(src.FileSymPrefix)..];
            }

            return expandGoroot(fname);

        }

        private static @string expandFileSym(ptr<loader.Loader> _addr_l, loader.Sym fsym)
        {
            ref loader.Loader l = ref _addr_l.val;

            return expandFile(l.SymName(fsym));
        }

        private static void writelines(this ptr<dwctxt2> _addr_d, ptr<sym.CompilationUnit> _addr_unit, loader.Sym ls)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var is_stmt = uint8(1L); // initially = recommended default_is_stmt = 1, tracks is_stmt toggles.

            var unitstart = int64(-1L);
            var headerstart = int64(-1L);
            var headerend = int64(-1L);

            var lsu = d.ldr.MakeSymbolUpdater(ls);
            newattr(_addr_unit.DWInfo, dwarf.DW_AT_stmt_list, dwarf.DW_CLS_PTR, lsu.Size(), dwSym(ls));

            var internalExec = d.linkctxt.BuildMode == BuildModeExe && d.linkctxt.IsInternal();
            var addAddrPlus = loader.GenAddAddrPlusFunc(internalExec); 

            // Write .debug_line Line Number Program Header (sec 6.2.4)
            // Fields marked with (*) must be changed for 64-bit dwarf
            var unitLengthOffset = lsu.Size();
            d.createUnitLength(lsu, 0L); // unit_length (*), filled in at end

            unitstart = lsu.Size();
            lsu.AddUint16(d.arch, 2L); // dwarf version (appendix F) -- version 3 is incompatible w/ XCode 9.0's dsymutil, latest supported on OSX 10.12 as of 2018-05
            var headerLengthOffset = lsu.Size();
            d.addDwarfAddrField(lsu, 0L); // header_length (*), filled in at end
            headerstart = lsu.Size(); 

            // cpos == unitstart + 4 + 2 + 4
            lsu.AddUint8(1L); // minimum_instruction_length
            lsu.AddUint8(is_stmt); // default_is_stmt
            lsu.AddUint8(LINE_BASE & 0xFFUL); // line_base
            lsu.AddUint8(LINE_RANGE); // line_range
            lsu.AddUint8(OPCODE_BASE); // opcode_base
            lsu.AddUint8(0L); // standard_opcode_lengths[1]
            lsu.AddUint8(1L); // standard_opcode_lengths[2]
            lsu.AddUint8(1L); // standard_opcode_lengths[3]
            lsu.AddUint8(1L); // standard_opcode_lengths[4]
            lsu.AddUint8(1L); // standard_opcode_lengths[5]
            lsu.AddUint8(0L); // standard_opcode_lengths[6]
            lsu.AddUint8(0L); // standard_opcode_lengths[7]
            lsu.AddUint8(0L); // standard_opcode_lengths[8]
            lsu.AddUint8(1L); // standard_opcode_lengths[9]
            lsu.AddUint8(0L); // standard_opcode_lengths[10]
            lsu.AddUint8(0L); // include_directories  (empty)

            // Copy over the file table.
            var fileNums = make_map<@string, long>();
            var lsDwsym = dwSym(ls);
            {
                var i__prev1 = i;
                var name__prev1 = name;

                foreach (var (__i, __name) in unit.DWARFFileTable)
                {
                    i = __i;
                    name = __name;
                    var name = expandFile(name);
                    if (len(name) == 0L)
                    { 
                        // Can't have empty filenames, and having a unique
                        // filename is quite useful for debugging.
                        name = fmt.Sprintf("<missing>_%d", i);

                    }

                    fileNums[name] = i + 1L;
                    d.AddString(lsDwsym, name);
                    lsu.AddUint8(0L);
                    lsu.AddUint8(0L);
                    lsu.AddUint8(0L);
                    if (gdbscript == "")
                    { 
                        // We can't use something that may be dead-code
                        // eliminated from a binary here. proc.go contains
                        // main and the scheduler, so it's not going anywhere.
                        {
                            var i__prev2 = i;

                            var i = strings.Index(name, "runtime/proc.go");

                            if (i >= 0L)
                            {
                                var k = strings.Index(name, "runtime/proc.go");
                                gdbscript = name[..k] + "runtime/runtime-gdb.py";
                            }

                            i = i__prev2;

                        }

                    }

                } 

                // 4 zeros: the string termination + 3 fields.

                i = i__prev1;
                name = name__prev1;
            }

            lsu.AddUint8(0L); 
            // terminate file_names.
            headerend = lsu.Size(); 

            // Output the state machine for each function remaining.
            long lastAddr = default;
            foreach (var (_, s) in unit.Textp2)
            {
                var fnSym = loader.Sym(s); 

                // Set the PC.
                lsu.AddUint8(0L);
                dwarf.Uleb128put(d, lsDwsym, 1L + int64(d.arch.PtrSize));
                lsu.AddUint8(dwarf.DW_LNE_set_address);
                var addr = addAddrPlus(lsu, d.arch, fnSym, 0L); 
                // Make sure the units are sorted.
                if (addr < lastAddr)
                {
                    d.linkctxt.Errorf(fnSym, "address wasn't increasing %x < %x", addr, lastAddr);
                }

                lastAddr = addr; 

                // Output the line table.
                // TODO: Now that we have all the debug information in separate
                // symbols, it would make sense to use a rope, and concatenate them all
                // together rather then the append() below. This would allow us to have
                // the compiler emit the DW_LNE_set_address and a rope data structure
                // to concat them all together in the output.
                var (_, _, _, lines) = d.ldr.GetFuncDwarfAuxSyms(fnSym);
                if (lines != 0L)
                {
                    lsu.AddBytes(d.ldr.Data(lines));
                }

            } 

            // Issue 38192: the DWARF standard specifies that when you issue
            // an end-sequence op, the PC value should be one past the last
            // text address in the translation unit, so apply a delta to the
            // text address before the end sequence op. If this isn't done,
            // GDB will assign a line number of zero the last row in the line
            // table, which we don't want. The 1 + ptrsize amount is somewhat
            // arbitrary, this is chosen to be consistent with the way LLVM
            // emits its end sequence ops.
            lsu.AddUint8(dwarf.DW_LNS_advance_pc);
            dwarf.Uleb128put(d, lsDwsym, int64(1L + d.arch.PtrSize)); 

            // Emit an end-sequence at the end of the unit.
            lsu.AddUint8(0L); // start extended opcode
            dwarf.Uleb128put(d, lsDwsym, 1L);
            lsu.AddUint8(dwarf.DW_LNE_end_sequence);

            if (d.linkctxt.HeadType == objabi.Haix)
            {
                saveDwsectCUSize(".debug_line", unit.Lib.Pkg, uint64(lsu.Size() - unitLengthOffset));
            }

            if (isDwarf64(d.linkctxt))
            {
                lsu.SetUint(d.arch, unitLengthOffset + 4L, uint64(lsu.Size() - unitstart)); // +4 because of 0xFFFFFFFF
                lsu.SetUint(d.arch, headerLengthOffset, uint64(headerend - headerstart));

            }
            else
            {
                lsu.SetUint32(d.arch, unitLengthOffset, uint32(lsu.Size() - unitstart));
                lsu.SetUint32(d.arch, headerLengthOffset, uint32(headerend - headerstart));
            }

        }

        // writepcranges generates the DW_AT_ranges table for compilation unit cu.
        private static void writepcranges(this ptr<dwctxt2> _addr_d, ptr<sym.CompilationUnit> _addr_unit, loader.Sym @base, slice<dwarf.Range> pcs, loader.Sym ranges)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;

            var rsu = d.ldr.MakeSymbolUpdater(ranges);
            var rDwSym = dwSym(ranges);

            var unitLengthOffset = rsu.Size(); 

            // Create PC ranges for this CU.
            newattr(_addr_unit.DWInfo, dwarf.DW_AT_ranges, dwarf.DW_CLS_PTR, rsu.Size(), rDwSym);
            newattr(_addr_unit.DWInfo, dwarf.DW_AT_low_pc, dwarf.DW_CLS_ADDRESS, 0L, dwSym(base));
            dwarf.PutBasedRanges(d, rDwSym, pcs);

            if (d.linkctxt.HeadType == objabi.Haix)
            {
                addDwsectCUSize(".debug_ranges", unit.Lib.Pkg, uint64(rsu.Size() - unitLengthOffset));
            }

        }

        /*
         *  Emit .debug_frame
         */
        private static readonly long dataAlignmentFactor = (long)-4L;


        // appendPCDeltaCFA appends per-PC CFA deltas to b and returns the final slice.
        private static slice<byte> appendPCDeltaCFA(ptr<sys.Arch> _addr_arch, slice<byte> b, long deltapc, long cfa)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            b = append(b, dwarf.DW_CFA_def_cfa_offset_sf);
            b = dwarf.AppendSleb128(b, cfa / dataAlignmentFactor);


            if (deltapc < 0x40UL) 
                b = append(b, uint8(dwarf.DW_CFA_advance_loc + deltapc));
            else if (deltapc < 0x100UL) 
                b = append(b, dwarf.DW_CFA_advance_loc1);
                b = append(b, uint8(deltapc));
            else if (deltapc < 0x10000UL) 
                b = append(b, dwarf.DW_CFA_advance_loc2, 0L, 0L);
                arch.ByteOrder.PutUint16(b[len(b) - 2L..], uint16(deltapc));
            else 
                b = append(b, dwarf.DW_CFA_advance_loc4, 0L, 0L, 0L, 0L);
                arch.ByteOrder.PutUint32(b[len(b) - 4L..], uint32(deltapc));
                        return b;

        }

        private static dwarfSecInfo writeframes(this ptr<dwctxt2> _addr_d)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var fs = d.ldr.LookupOrCreateSym(".debug_frame", 0L);
            var fsd = dwSym(fs);
            var fsu = d.ldr.MakeSymbolUpdater(fs);
            fsu.SetType(sym.SDWARFSECT);
            var isdw64 = isDwarf64(d.linkctxt);
            var haslr = haslinkregister(d.linkctxt); 

            // Length field is 4 bytes on Dwarf32 and 12 bytes on Dwarf64
            var lengthFieldSize = int64(4L);
            if (isdw64)
            {
                lengthFieldSize += 8L;
            } 

            // Emit the CIE, Section 6.4.1
            var cieReserve = uint32(16L);
            if (haslr)
            {
                cieReserve = 32L;
            }

            if (isdw64)
            {
                cieReserve += 4L; // 4 bytes added for cid
            }

            d.createUnitLength(fsu, uint64(cieReserve)); // initial length, must be multiple of thearch.ptrsize
            d.addDwarfAddrField(fsu, ~uint64(0L)); // cid
            fsu.AddUint8(3L); // dwarf version (appendix F)
            fsu.AddUint8(0L); // augmentation ""
            dwarf.Uleb128put(d, fsd, 1L); // code_alignment_factor
            dwarf.Sleb128put(d, fsd, dataAlignmentFactor); // all CFI offset calculations include multiplication with this factor
            dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr)); // return_address_register

            fsu.AddUint8(dwarf.DW_CFA_def_cfa); // Set the current frame address..
            dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfregsp)); // ...to use the value in the platform's SP register (defined in l.go)...
            if (haslr)
            {
                dwarf.Uleb128put(d, fsd, int64(0L)); // ...plus a 0 offset.

                fsu.AddUint8(dwarf.DW_CFA_same_value); // The platform's link register is unchanged during the prologue.
                dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr));

                fsu.AddUint8(dwarf.DW_CFA_val_offset); // The previous value...
                dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfregsp)); // ...of the platform's SP register...
                dwarf.Uleb128put(d, fsd, int64(0L)); // ...is CFA+0.
            }
            else
            {
                dwarf.Uleb128put(d, fsd, int64(d.arch.PtrSize)); // ...plus the word size (because the call instruction implicitly adds one word to the frame).

                fsu.AddUint8(dwarf.DW_CFA_offset_extended); // The previous value...
                dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr)); // ...of the return address...
                dwarf.Uleb128put(d, fsd, int64(-d.arch.PtrSize) / dataAlignmentFactor); // ...is saved at [CFA - (PtrSize/4)].
            }

            var pad = int64(cieReserve) + lengthFieldSize - int64(len(d.ldr.Data(fs)));

            if (pad < 0L)
            {
                Exitf("dwarf: cieReserve too small by %d bytes.", -pad);
            }

            var internalExec = d.linkctxt.BuildMode == BuildModeExe && d.linkctxt.IsInternal();
            var addAddrPlus = loader.GenAddAddrPlusFunc(internalExec);

            fsu.AddBytes(zeros[..pad]);

            slice<byte> deltaBuf = default;
            var pcsp = obj.NewPCIter(uint32(d.arch.MinLC));
            foreach (var (_, s) in d.linkctxt.Textp2)
            {
                var fn = loader.Sym(s);
                var fi = d.ldr.FuncInfo(fn);
                if (!fi.Valid())
                {
                    continue;
                }

                var fpcsp = fi.Pcsp(); 

                // Emit a FDE, Section 6.4.1.
                // First build the section contents into a byte buffer.
                deltaBuf = deltaBuf[..0L];
                if (haslr && d.ldr.AttrTopFrame(fn))
                { 
                    // Mark the link register as having an undefined value.
                    // This stops call stack unwinders progressing any further.
                    // TODO: similar mark on non-LR architectures.
                    deltaBuf = append(deltaBuf, dwarf.DW_CFA_undefined);
                    deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(thearch.Dwarfreglr));

                }

                pcsp.Init(fpcsp);

                while (!pcsp.Done)
                {
                    var nextpc = pcsp.NextPC; 

                    // pciterinit goes up to the end of the function,
                    // but DWARF expects us to stop just before the end.
                    if (int64(nextpc) == int64(len(d.ldr.Data(fn))))
                    {
                        nextpc--;
                        if (nextpc < pcsp.PC)
                        {
                            continue;
                    pcsp.Next();
                        }

                    }

                    var spdelta = int64(pcsp.Value);
                    if (!haslr)
                    { 
                        // Return address has been pushed onto stack.
                        spdelta += int64(d.arch.PtrSize);

                    }

                    if (haslr && !d.ldr.AttrTopFrame(fn))
                    { 
                        // TODO(bryanpkc): This is imprecise. In general, the instruction
                        // that stores the return address to the stack frame is not the
                        // same one that allocates the frame.
                        if (pcsp.Value > 0L)
                        { 
                            // The return address is preserved at (CFA-frame_size)
                            // after a stack frame has been allocated.
                            deltaBuf = append(deltaBuf, dwarf.DW_CFA_offset_extended_sf);
                            deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(thearch.Dwarfreglr));
                            deltaBuf = dwarf.AppendSleb128(deltaBuf, -spdelta / dataAlignmentFactor);

                        }
                        else
                        { 
                            // The return address is restored into the link register
                            // when a stack frame has been de-allocated.
                            deltaBuf = append(deltaBuf, dwarf.DW_CFA_same_value);
                            deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(thearch.Dwarfreglr));

                        }

                    }

                    deltaBuf = appendPCDeltaCFA(_addr_d.arch, deltaBuf, int64(nextpc) - int64(pcsp.PC), spdelta);

                }

                pad = int(Rnd(int64(len(deltaBuf)), int64(d.arch.PtrSize))) - len(deltaBuf);
                deltaBuf = append(deltaBuf, zeros[..pad]); 

                // Emit the FDE header, Section 6.4.1.
                //    4 bytes: length, must be multiple of thearch.ptrsize
                //    4/8 bytes: Pointer to the CIE above, at offset 0
                //    ptrsize: initial location
                //    ptrsize: address range

                var fdeLength = uint64(4L + 2L * d.arch.PtrSize + len(deltaBuf));
                if (isdw64)
                {
                    fdeLength += 4L; // 4 bytes added for CIE pointer
                }

                d.createUnitLength(fsu, fdeLength);

                if (d.linkctxt.LinkMode == LinkExternal)
                {
                    d.addDwarfAddrRef(fsu, fs);
                }
                else
                {
                    d.addDwarfAddrField(fsu, 0L); // CIE offset
                }

                addAddrPlus(fsu, d.arch, s, 0L);
                fsu.AddUintXX(d.arch, uint64(len(d.ldr.Data(fn))), d.arch.PtrSize); // address range
                fsu.AddBytes(deltaBuf);

                if (d.linkctxt.HeadType == objabi.Haix)
                {
                    addDwsectCUSize(".debug_frame", d.ldr.SymPkg(fn), fdeLength + uint64(lengthFieldSize));
                }

            }
            return new dwarfSecInfo(syms:[]loader.Sym{fs});

        }

        /*
         *  Walk DWarfDebugInfoEntries, and emit .debug_info
         */

        public static readonly long COMPUNITHEADERSIZE = (long)4L + 2L + 4L + 1L;


        // appendSyms appends the syms from 'src' into 'syms' and returns the
        // result. This can go away once we do away with sym.LoaderSym
        // entirely.
        private static slice<loader.Sym> appendSyms(slice<loader.Sym> syms, slice<sym.LoaderSym> src)
        {
            foreach (var (_, s) in src)
            {
                syms = append(syms, loader.Sym(s));
            }
            return syms;

        }

        private static dwarfSecInfo writeinfo(this ptr<dwctxt2> _addr_d, slice<ptr<sym.CompilationUnit>> units, loader.Sym abbrevsym, ptr<pubWriter2> _addr_pubNames, ptr<pubWriter2> _addr_pubTypes)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref pubWriter2 pubNames = ref _addr_pubNames.val;
            ref pubWriter2 pubTypes = ref _addr_pubTypes.val;

            var infosec = d.ldr.LookupOrCreateSym(".debug_info", 0L);
            var disu = d.ldr.MakeSymbolUpdater(infosec);
            disu.SetType(sym.SDWARFINFO);
            d.ldr.SetAttrReachable(infosec, true);
            loader.Sym syms = new slice<loader.Sym>(new loader.Sym[] { infosec });

            foreach (var (_, u) in units)
            {
                var compunit = u.DWInfo;
                var s = d.dtolsym(compunit.Sym);
                var su = d.ldr.MakeSymbolUpdater(s);

                if (len(u.Textp2) == 0L && u.DWInfo.Child == null)
                {
                    continue;
                }

                pubNames.beginCompUnit(compunit);
                pubTypes.beginCompUnit(compunit); 

                // Write .debug_info Compilation Unit Header (sec 7.5.1)
                // Fields marked with (*) must be changed for 64-bit dwarf
                // This must match COMPUNITHEADERSIZE above.
                d.createUnitLength(su, 0L); // unit_length (*), will be filled in later.
                su.AddUint16(d.arch, 4L); // dwarf version (appendix F)

                // debug_abbrev_offset (*)
                d.addDwarfAddrRef(su, abbrevsym);

                su.AddUint8(uint8(d.arch.PtrSize)); // address_size

                var ds = dwSym(s);
                dwarf.Uleb128put(d, ds, int64(compunit.Abbrev));
                dwarf.PutAttrs(d, ds, compunit.Abbrev, compunit.Attr);

                loader.Sym cu = new slice<loader.Sym>(new loader.Sym[] { s });
                cu = appendSyms(cu, u.AbsFnDIEs2);
                cu = appendSyms(cu, u.FuncDIEs2);
                if (u.Consts2 != 0L)
                {
                    cu = append(cu, loader.Sym(u.Consts2));
                }

                long cusize = default;
                {
                    var child__prev2 = child;

                    foreach (var (_, __child) in cu)
                    {
                        child = __child;
                        cusize += int64(len(d.ldr.Data(child)));
                    }

                    child = child__prev2;
                }

                {
                    var die = compunit.Child;

                    while (die != null)
                    {
                        var l = len(cu);
                        var lastSymSz = int64(len(d.ldr.Data(cu[l - 1L])));
                        cu = d.putdie(cu, die);
                        if (ispubname(_addr_die))
                        {
                            pubNames.add(die, cusize);
                        die = die.Link;
                        }

                        if (ispubtype(_addr_die))
                        {
                            pubTypes.add(die, cusize);
                        }

                        if (lastSymSz != int64(len(d.ldr.Data(cu[l - 1L]))))
                        { 
                            // putdie will sometimes append directly to the last symbol of the list
                            cusize = cusize - lastSymSz + int64(len(d.ldr.Data(cu[l - 1L])));

                        }

                        {
                            var child__prev3 = child;

                            foreach (var (_, __child) in cu[l..])
                            {
                                child = __child;
                                cusize += int64(len(d.ldr.Data(child)));
                            }

                            child = child__prev3;
                        }
                    }

                }

                var culu = d.ldr.MakeSymbolUpdater(cu[len(cu) - 1L]);
                culu.AddUint8(0L); // closes compilation unit DIE
                cusize++; 

                // Save size for AIX symbol table.
                if (d.linkctxt.HeadType == objabi.Haix)
                {
                    saveDwsectCUSize(".debug_info", d.getPkgFromCUSym(s), uint64(cusize));
                }

                if (isDwarf64(d.linkctxt))
                {
                    cusize -= 12L; // exclude the length field.
                    su.SetUint(d.arch, 4L, uint64(cusize)); // 4 because of 0XFFFFFFFF
                }
                else
                {
                    cusize -= 4L; // exclude the length field.
                    su.SetUint32(d.arch, 0L, uint32(cusize));

                }

                pubNames.endCompUnit(compunit, uint32(cusize) + 4L);
                pubTypes.endCompUnit(compunit, uint32(cusize) + 4L);
                syms = append(syms, cu);

            }
            return new dwarfSecInfo(syms:syms);

        }

        /*
         *  Emit .debug_pubnames/_types.  _info must have been written before,
         *  because we need die->offs and infoo/infosize;
         */

        private partial struct pubWriter2
        {
            public ptr<dwctxt2> d;
            public loader.Sym s;
            public ptr<loader.SymbolBuilder> su;
            public @string sname;
            public long sectionstart;
            public long culengthOff;
        }

        private static ptr<pubWriter2> newPubWriter2(ptr<dwctxt2> _addr_d, @string sname)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var s = d.ldr.LookupOrCreateSym(sname, 0L);
            var u = d.ldr.MakeSymbolUpdater(s);
            u.SetType(sym.SDWARFSECT);
            return addr(new pubWriter2(d:d,s:s,su:u,sname:sname));
        }

        private static void beginCompUnit(this ptr<pubWriter2> _addr_pw, ptr<dwarf.DWDie> _addr_compunit)
        {
            ref pubWriter2 pw = ref _addr_pw.val;
            ref dwarf.DWDie compunit = ref _addr_compunit.val;

            pw.sectionstart = pw.su.Size(); 

            // Write .debug_pubnames/types    Header (sec 6.1.1)
            pw.d.createUnitLength(pw.su, 0L); // unit_length (*), will be filled in later.
            pw.su.AddUint16(pw.d.arch, 2L); // dwarf version (appendix F)
            pw.d.addDwarfAddrRef(pw.su, pw.d.dtolsym(compunit.Sym)); // debug_info_offset (of the Comp unit Header)
            pw.culengthOff = pw.su.Size();
            pw.d.addDwarfAddrField(pw.su, uint64(0L)); // debug_info_length, will be filled in later.
        }

        private static void add(this ptr<pubWriter2> _addr_pw, ptr<dwarf.DWDie> _addr_die, long offset)
        {
            ref pubWriter2 pw = ref _addr_pw.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var dwa = getattr(_addr_die, dwarf.DW_AT_name);
            @string name = dwa.Data._<@string>();
            if (pw.d.dtolsym(die.Sym) == 0L)
            {
                fmt.Println("Missing sym for ", name);
            }

            pw.d.addDwarfAddrField(pw.su, uint64(offset));
            pw.su.Addstring(name);

        }

        private static void endCompUnit(this ptr<pubWriter2> _addr_pw, ptr<dwarf.DWDie> _addr_compunit, uint culength)
        {
            ref pubWriter2 pw = ref _addr_pw.val;
            ref dwarf.DWDie compunit = ref _addr_compunit.val;

            pw.d.addDwarfAddrField(pw.su, 0L); // Null offset

            // On AIX, save the current size of this compilation unit.
            if (pw.d.linkctxt.HeadType == objabi.Haix)
            {
                saveDwsectCUSize(pw.sname, pw.d.getPkgFromCUSym(pw.d.dtolsym(compunit.Sym)), uint64(pw.su.Size() - pw.sectionstart));
            }

            if (isDwarf64(pw.d.linkctxt))
            {
                pw.su.SetUint(pw.d.arch, pw.sectionstart + 4L, uint64(pw.su.Size() - pw.sectionstart) - 12L); // exclude the length field.
                pw.su.SetUint(pw.d.arch, pw.culengthOff, uint64(culength));

            }
            else
            {
                pw.su.SetUint32(pw.d.arch, pw.sectionstart, uint32(pw.su.Size() - pw.sectionstart) - 4L); // exclude the length field.
                pw.su.SetUint32(pw.d.arch, pw.culengthOff, culength);

            }

        }

        private static bool ispubname(ptr<dwarf.DWDie> _addr_die)
        {
            ref dwarf.DWDie die = ref _addr_die.val;


            if (die.Abbrev == dwarf.DW_ABRV_FUNCTION || die.Abbrev == dwarf.DW_ABRV_VARIABLE) 
                var a = getattr(_addr_die, dwarf.DW_AT_external);
                return a != null && a.Value != 0L;
                        return false;

        }

        private static bool ispubtype(ptr<dwarf.DWDie> _addr_die)
        {
            ref dwarf.DWDie die = ref _addr_die.val;

            return die.Abbrev >= dwarf.DW_ABRV_NULLTYPE;
        }

        private static dwarfSecInfo writegdbscript(this ptr<dwctxt2> _addr_d)
        {
            ref dwctxt2 d = ref _addr_d.val;
 
            // TODO (aix): make it available
            if (d.linkctxt.HeadType == objabi.Haix)
            {
                return new dwarfSecInfo();
            }

            if (d.linkctxt.LinkMode == LinkExternal && d.linkctxt.HeadType == objabi.Hwindows && d.linkctxt.BuildMode == BuildModeCArchive)
            { 
                // gcc on Windows places .debug_gdb_scripts in the wrong location, which
                // causes the program not to run. See https://golang.org/issue/20183
                // Non c-archives can avoid this issue via a linker script
                // (see fix near writeGDBLinkerScript).
                // c-archive users would need to specify the linker script manually.
                // For UX it's better not to deal with this.
                return new dwarfSecInfo();

            }

            if (gdbscript == "")
            {
                return new dwarfSecInfo();
            }

            var gs = d.ldr.LookupOrCreateSym(".debug_gdb_scripts", 0L);
            var u = d.ldr.MakeSymbolUpdater(gs);
            u.SetType(sym.SDWARFSECT);

            u.AddUint8(1L); // magic 1 byte?
            u.Addstring(gdbscript);
            return new dwarfSecInfo(syms:[]loader.Sym{gs});

        }

        // FIXME: might be worth looking replacing this map with a function
        // that switches based on symbol instead.

        private static map<@string, ptr<dwarf.DWDie>> prototypedies = default;

        private static bool dwarfEnabled(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (FlagW.val)
            { // disable dwarf
                return false;

            }

            if (FlagS && ctxt.HeadType != objabi.Hdarwin.val)
            {
                return false;
            }

            if (ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hjs)
            {
                return false;
            }

            if (ctxt.LinkMode == LinkExternal)
            {

                if (ctxt.IsELF)                 else if (ctxt.HeadType == objabi.Hdarwin)                 else if (ctxt.HeadType == objabi.Hwindows)                 else if (ctxt.HeadType == objabi.Haix) 
                    var (res, err) = dwarf.IsDWARFEnabledOnAIXLd(ctxt.extld());
                    if (err != null)
                    {
                        Exitf("%v", err);
                    }

                    return res;
                else 
                    return false;
                
            }

            return true;

        }

        // mkBuiltinType populates the dwctxt2 sym lookup maps for the
        // newly created builtin type DIE 'typeDie'.
        private static ptr<dwarf.DWDie> mkBuiltinType(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, long abrv, @string tname)
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;
 
            // create type DIE
            var die = d.newdie(_addr_dwtypes, abrv, tname, 0L); 

            // Look up type symbol.
            var gotype = d.lookupOrDiag("type." + tname); 

            // Map from die sym to type sym
            var ds = loader.Sym(die.Sym._<dwSym>());
            d.rtmap[ds] = gotype; 

            // Map from type to def sym
            d.tdmap[gotype] = ds;

            return _addr_die!;

        }

        // dwarfGenerateDebugInfo generated debug info entries for all types,
        // variables and functions in the program.
        // Along with dwarfGenerateDebugSyms they are the two main entry points into
        // dwarf generation: dwarfGenerateDebugInfo does all the work that should be
        // done before symbol names are mangled while dwarfGenerateDebugSyms does
        // all the work that can only be done after addresses have been assigned to
        // text symbols.
        private static void dwarfGenerateDebugInfo(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!dwarfEnabled(_addr_ctxt))
            {
                return ;
            }

            var d = newdwctxt2(_addr_ctxt, true);

            if (ctxt.HeadType == objabi.Haix)
            { 
                // Initial map used to store package size for each DWARF section.
                dwsectCUSize = make_map<@string, ulong>();

            } 

            // For ctxt.Diagnostic messages.
            newattr(_addr_dwtypes, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len("dwtypes")), "dwtypes"); 

            // Unspecified type. There are no references to this in the symbol table.
            d.newdie(_addr_dwtypes, dwarf.DW_ABRV_NULLTYPE, "<unspecified>", 0L); 

            // Some types that must exist to define other ones (uintptr in particular
            // is needed for array size)
            d.mkBuiltinType(ctxt, dwarf.DW_ABRV_BARE_PTRTYPE, "unsafe.Pointer");
            var die = d.mkBuiltinType(ctxt, dwarf.DW_ABRV_BASETYPE, "uintptr");
            newattr(_addr_die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
            newattr(_addr_die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(d.arch.PtrSize), 0L);
            newattr(_addr_die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, objabi.KindUintptr, 0L);
            newattr(_addr_die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_ADDRESS, 0L, dwSym(d.lookupOrDiag("type.uintptr")));

            d.uintptrInfoSym = d.mustFind("uintptr"); 

            // Prototypes needed for type synthesis.
            prototypedies = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<dwarf.DWDie>>{"type.runtime.stringStructDWARF":nil,"type.runtime.slice":nil,"type.runtime.hmap":nil,"type.runtime.bmap":nil,"type.runtime.sudog":nil,"type.runtime.waitq":nil,"type.runtime.hchan":nil,}; 

            // Needed by the prettyprinter code for interface inspection.
            foreach (var (_, typ) in new slice<@string>(new @string[] { "type.runtime._type", "type.runtime.arraytype", "type.runtime.chantype", "type.runtime.functype", "type.runtime.maptype", "type.runtime.ptrtype", "type.runtime.slicetype", "type.runtime.structtype", "type.runtime.interfacetype", "type.runtime.itab", "type.runtime.imethod" }))
            {
                d.defgotype(d.lookupOrDiag(typ));
            } 

            // fake root DIE for compile unit DIEs
            ref dwarf.DWDie dwroot = ref heap(out ptr<dwarf.DWDie> _addr_dwroot);
            var flagVariants = make_map<@string, bool>();

            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in ctxt.Library)
                {
                    lib = __lib;
                    var consts = d.ldr.Lookup(dwarf.ConstInfoPrefix + lib.Pkg, 0L);
                    {
                        var unit__prev2 = unit;

                        foreach (var (_, __unit) in lib.Units)
                        {
                            unit = __unit; 
                            // We drop the constants into the first CU.
                            if (consts != 0L)
                            {
                                unit.Consts2 = sym.LoaderSym(consts);
                                d.importInfoSymbol(ctxt, consts);
                                consts = 0L;
                            }

                            ctxt.compUnits = append(ctxt.compUnits, unit); 

                            // We need at least one runtime unit.
                            if (unit.Lib.Pkg == "runtime")
                            {
                                ctxt.runtimeCU = unit;
                            }

                            unit.DWInfo = d.newdie(_addr_dwroot, dwarf.DW_ABRV_COMPUNIT, unit.Lib.Pkg, 0L);
                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_language, dwarf.DW_CLS_CONSTANT, int64(dwarf.DW_LANG_Go), 0L); 
                            // OS X linker requires compilation dir or absolute path in comp unit name to output debug info.
                            var compDir = getCompilationDir(); 
                            // TODO: Make this be the actual compilation directory, not
                            // the linker directory. If we move CU construction into the
                            // compiler, this should happen naturally.
                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_comp_dir, dwarf.DW_CLS_STRING, int64(len(compDir)), compDir);

                            slice<byte> peData = default;
                            {
                                var producerExtra = d.ldr.Lookup(dwarf.CUInfoPrefix + "producer." + unit.Lib.Pkg, 0L);

                                if (producerExtra != 0L)
                                {
                                    peData = d.ldr.Data(producerExtra);
                                }

                            }

                            @string producer = "Go cmd/compile " + objabi.Version;
                            if (len(peData) > 0L)
                            { 
                                // We put a semicolon before the flags to clearly
                                // separate them from the version, which can be long
                                // and have lots of weird things in it in development
                                // versions. We promise not to put a semicolon in the
                                // version, so it should be safe for readers to scan
                                // forward to the semicolon.
                                producer += "; " + string(peData);
                                flagVariants[string(peData)] = true;

                            }
                            else
                            {
                                flagVariants[""] = true;
                            }

                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_producer, dwarf.DW_CLS_STRING, int64(len(producer)), producer);

                            @string pkgname = default;
                            {
                                var pnSymIdx = d.ldr.Lookup(dwarf.CUInfoPrefix + "packagename." + unit.Lib.Pkg, 0L);

                                if (pnSymIdx != 0L)
                                {
                                    var pnsData = d.ldr.Data(pnSymIdx);
                                    pkgname = string(pnsData);
                                }

                            }

                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_go_package_name, dwarf.DW_CLS_STRING, int64(len(pkgname)), pkgname);

                            if (len(unit.Textp2) == 0L)
                            {
                                unit.DWInfo.Abbrev = dwarf.DW_ABRV_COMPUNIT_TEXTLESS;
                            } 

                            // Scan all functions in this compilation unit, create DIEs for all
                            // referenced types, create the file table for debug_line, find all
                            // referenced abstract functions.
                            // Collect all debug_range symbols in unit.rangeSyms
                            {
                                var s__prev3 = s;

                                foreach (var (_, __s) in unit.Textp2)
                                {
                                    s = __s; // textp2 has been dead-code-eliminated already.
                                    var fnSym = loader.Sym(s);
                                    var (infosym, _, rangesym, _) = d.ldr.GetFuncDwarfAuxSyms(fnSym);
                                    if (infosym == 0L)
                                    {
                                        continue;
                                    }

                                    d.ldr.SetAttrNotInSymbolTable(infosym, true);
                                    d.ldr.SetAttrReachable(infosym, true);

                                    unit.FuncDIEs2 = append(unit.FuncDIEs2, sym.LoaderSym(infosym));
                                    if (rangesym != 0L)
                                    {
                                        var rs = len(d.ldr.Data(rangesym));
                                        d.ldr.SetAttrNotInSymbolTable(rangesym, true);
                                        d.ldr.SetAttrReachable(rangesym, true);
                                        if (ctxt.HeadType == objabi.Haix)
                                        {
                                            addDwsectCUSize(".debug_ranges", unit.Lib.Pkg, uint64(rs));
                                        }

                                        unit.RangeSyms2 = append(unit.RangeSyms2, sym.LoaderSym(rangesym));

                                    }

                                    var drelocs = d.ldr.Relocs(infosym);
                                    for (long ri = 0L; ri < drelocs.Count(); ri++)
                                    {
                                        var r = drelocs.At2(ri);
                                        if (r.Type() == objabi.R_DWARFSECREF)
                                        {
                                            var rsym = r.Sym();
                                            var rsn = d.ldr.SymName(rsym);
                                            if (len(rsn) == 0L)
                                            {
                                                continue;
                                            } 
                                            // NB: there should be a better way to do this that doesn't involve materializing the symbol name and doing string prefix+suffix checks.
                                            if (strings.HasPrefix(rsn, dwarf.InfoPrefix) && strings.HasSuffix(rsn, dwarf.AbstractFuncSuffix) && !d.ldr.AttrOnList(rsym))
                                            { 
                                                // abstract function
                                                d.ldr.SetAttrOnList(rsym, true);
                                                unit.AbsFnDIEs2 = append(unit.AbsFnDIEs2, sym.LoaderSym(rsym));
                                                d.importInfoSymbol(ctxt, rsym);
                                                continue;

                                            }

                                            {
                                                var (_, ok) = d.rtmap[rsym];

                                                if (ok)
                                                { 
                                                    // type already generated
                                                    continue;

                                                }

                                            }

                                            var tn = rsn[len(dwarf.InfoPrefix)..];
                                            var ts = d.ldr.Lookup("type." + tn, 0L);
                                            d.defgotype(ts);

                                        }

                                    }


                                }

                                s = s__prev3;
                            }
                        }

                        unit = unit__prev2;
                    }
                } 

                // Fix for 31034: if the objects feeding into this link were compiled
                // with different sets of flags, then don't issue an error if
                // the -strictdups checks fail.

                lib = lib__prev1;
            }

            if (checkStrictDups > 1L && len(flagVariants) > 1L)
            {
                checkStrictDups = 1L;
            } 

            // Create DIEs for global variables and the types they use.
            // FIXME: ideally this should be done in the compiler, since
            // for globals there isn't any abiguity about which package
            // a global belongs to.
            for (var idx = loader.Sym(1L); idx < loader.Sym(d.ldr.NDef()); idx++)
            {
                if (!d.ldr.AttrReachable(idx) || d.ldr.AttrNotInSymbolTable(idx) || d.ldr.SymVersion(idx) >= sym.SymVerStatic)
                {
                    continue;
                }

                var t = d.ldr.SymType(idx);

                if (t == sym.SRODATA || t == sym.SDATA || t == sym.SNOPTRDATA || t == sym.STYPE || t == sym.SBSS || t == sym.SNOPTRBSS || t == sym.STLSBSS)                 else 
                    continue;
                // Skip things with no type
                if (d.ldr.SymGoType(idx) == 0L)
                {
                    continue;
                }

                var sn = d.ldr.SymName(idx);
                if (ctxt.LinkMode != LinkExternal && isStaticTemp(sn))
                {
                    continue;
                }

                if (sn == "")
                { 
                    // skip aux symbols
                    continue;

                } 

                // Create DIE for global.
                var sv = d.ldr.SymValue(idx);
                var gt = d.ldr.SymGoType(idx);
                d.dwarfDefineGlobal(ctxt, idx, sn, sv, gt);

            } 

            // Create DIEs for variable types indirectly referenced by function
            // autos (which may not appear directly as param/var DIEs).
 

            // Create DIEs for variable types indirectly referenced by function
            // autos (which may not appear directly as param/var DIEs).
            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in ctxt.Library)
                {
                    lib = __lib;
                    {
                        var unit__prev2 = unit;

                        foreach (var (_, __unit) in lib.Units)
                        {
                            unit = __unit;
                            slice<sym.LoaderSym> lists = new slice<slice<sym.LoaderSym>>(new slice<sym.LoaderSym>[] { unit.AbsFnDIEs2, unit.FuncDIEs2 });
                            foreach (var (_, list) in lists)
                            {
                                {
                                    var s__prev4 = s;

                                    foreach (var (_, __s) in list)
                                    {
                                        s = __s;
                                        var symIdx = loader.Sym(s);
                                        var relocs = d.ldr.Relocs(symIdx);
                                        for (long i = 0L; i < relocs.Count(); i++)
                                        {
                                            r = relocs.At2(i);
                                            if (r.Type() == objabi.R_USETYPE)
                                            {
                                                d.defgotype(r.Sym());
                                            }

                                        }


                                    }

                                    s = s__prev4;
                                }
                            }

                        }

                        unit = unit__prev2;
                    }
                }

                lib = lib__prev1;
            }

            d.synthesizestringtypes(ctxt, dwtypes.Child);
            d.synthesizeslicetypes(ctxt, dwtypes.Child);
            d.synthesizemaptypes(ctxt, dwtypes.Child);
            d.synthesizechantypes(ctxt, dwtypes.Child); 

            // NB: at this stage we have all the DIE objects constructed, but
            // they have loader.Sym attributes and not sym.Symbol attributes.
            // At the point when loadlibfull runs we will need to visit
            // every DIE constructed and convert the symbols.
        }

        // dwarfGenerateDebugSyms constructs debug_line, debug_frame, debug_loc,
        // debug_pubnames and debug_pubtypes. It also writes out the debug_info
        // section using symbols generated in dwarfGenerateDebugInfo2.
        private static void dwarfGenerateDebugSyms(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!dwarfEnabled(_addr_ctxt))
            {
                return ;
            }

            ptr<dwctxt2> d = addr(new dwctxt2(linkctxt:ctxt,ldr:ctxt.loader,arch:ctxt.Arch,));
            d.dwarfGenerateDebugSyms();

        }

        private static void dwarfGenerateDebugSyms(this ptr<dwctxt2> _addr_d)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var abbrevSec = d.writeabbrev();
            dwarfp2 = append(dwarfp2, abbrevSec);

            d.calcCompUnitRanges();
            sort.Sort(compilationUnitByStartPC(d.linkctxt.compUnits)); 

            // Create .debug_line and .debug_ranges section symbols
            var debugLine = d.ldr.LookupOrCreateSym(".debug_line", 0L);
            var dlu = d.ldr.MakeSymbolUpdater(debugLine);
            dlu.SetType(sym.SDWARFSECT);
            d.ldr.SetAttrReachable(debugLine, true);
            dwarfp2 = append(dwarfp2, new dwarfSecInfo(syms:[]loader.Sym{debugLine}));

            var debugRanges = d.ldr.LookupOrCreateSym(".debug_ranges", 0L);
            var dru = d.ldr.MakeSymbolUpdater(debugRanges);
            dru.SetType(sym.SDWARFRANGE);
            d.ldr.SetAttrReachable(debugRanges, true); 

            // Write per-package line and range tables and start their CU DIEs.
            foreach (var (_, u) in d.linkctxt.compUnits)
            {
                reversetree(_addr_u.DWInfo.Child);
                if (u.DWInfo.Abbrev == dwarf.DW_ABRV_COMPUNIT_TEXTLESS)
                {
                    continue;
                }

                d.writelines(u, debugLine);
                var @base = loader.Sym(u.Textp2[0L]);
                d.writepcranges(u, base, u.PCs, debugRanges);

            } 

            // newdie adds DIEs to the *beginning* of the parent's DIE list.
            // Now that we're done creating DIEs, reverse the trees so DIEs
            // appear in the order they were created.
            reversetree(_addr_dwtypes.Child);
            movetomodule(_addr_d.linkctxt, _addr_dwtypes);

            var pubNames = newPubWriter2(_addr_d, ".debug_pubnames");
            var pubTypes = newPubWriter2(_addr_d, ".debug_pubtypes");

            var infoSec = d.writeinfo(d.linkctxt.compUnits, abbrevSec.secSym(), pubNames, pubTypes);

            var framesSec = d.writeframes();
            dwarfp2 = append(dwarfp2, framesSec);
            dwarfp2 = append(dwarfp2, new dwarfSecInfo(syms:[]loader.Sym{pubNames.s}));
            dwarfp2 = append(dwarfp2, new dwarfSecInfo(syms:[]loader.Sym{pubTypes.s}));
            var gdbScriptSec = d.writegdbscript();
            if (gdbScriptSec.secSym() != 0L)
            {
                dwarfp2 = append(dwarfp2, gdbScriptSec);
            }

            dwarfp2 = append(dwarfp2, infoSec);
            var locSec = d.collectlocs(d.linkctxt.compUnits);
            if (locSec.secSym() != 0L)
            {
                dwarfp2 = append(dwarfp2, locSec);
            }

            loader.Sym rsyms = new slice<loader.Sym>(new loader.Sym[] { debugRanges });
            foreach (var (_, unit) in d.linkctxt.compUnits)
            {
                foreach (var (_, s) in unit.RangeSyms2)
                {
                    rsyms = append(rsyms, loader.Sym(s));
                }

            }
            dwarfp2 = append(dwarfp2, new dwarfSecInfo(syms:rsyms));

        }

        private static dwarfSecInfo collectlocs(this ptr<dwctxt2> _addr_d, slice<ptr<sym.CompilationUnit>> units)
        {
            ref dwctxt2 d = ref _addr_d.val;

            var empty = true;
            loader.Sym syms = new slice<loader.Sym>(new loader.Sym[] {  });
            {
                var u__prev1 = u;

                foreach (var (_, __u) in units)
                {
                    u = __u;
                    foreach (var (_, fn) in u.FuncDIEs2)
                    {
                        var relocs = d.ldr.Relocs(loader.Sym(fn));
                        for (long i = 0L; i < relocs.Count(); i++)
                        {
                            var reloc = relocs.At2(i);
                            if (reloc.Type() != objabi.R_DWARFSECREF)
                            {
                                continue;
                            }

                            var rsym = reloc.Sym();
                            if (d.ldr.SymType(rsym) == sym.SDWARFLOC)
                            {
                                d.ldr.SetAttrReachable(rsym, true);
                                d.ldr.SetAttrNotInSymbolTable(rsym, true);
                                syms = append(syms, rsym);
                                empty = false; 
                                // One location list entry per function, but many relocations to it. Don't duplicate.
                                break;

                            }

                        }


                    }

                } 

                // Don't emit .debug_loc if it's empty -- it makes the ARM linker mad.

                u = u__prev1;
            }

            if (empty)
            {
                return new dwarfSecInfo();
            }

            var locsym = d.ldr.LookupOrCreateSym(".debug_loc", 0L);
            var u = d.ldr.MakeSymbolUpdater(locsym);
            u.SetType(sym.SDWARFLOC);
            d.ldr.SetAttrReachable(locsym, true);
            return new dwarfSecInfo(syms:append([]loader.Sym{locsym},syms...));

        }

        /*
         *  Elf.
         */
        private static void dwarfaddshstrings(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt, loader.Sym shstrtab) => func((_, panic, __) =>
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            panic("not yet implemented");
        });

        // Add section symbols for DWARF debug info.  This is called before
        // dwarfaddelfheaders.
        private static void dwarfaddelfsectionsyms(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            panic("not yet implemented");
        });

        // dwarfcompress compresses the DWARF sections. Relocations are applied
        // on the fly. After this, dwarfp will contain a different (new) set of
        // symbols, and sections may have been replaced.
        private static void dwarfcompress(this ptr<dwctxt2> _addr_d, ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref dwctxt2 d = ref _addr_d.val;
            ref Link ctxt = ref _addr_ctxt.val;

            panic("not yet implemented");
        });

        // getPkgFromCUSym returns the package name for the compilation unit
        // represented by s.
        // The prefix dwarf.InfoPrefix+".pkg." needs to be removed in order to get
        // the package name.
        private static @string getPkgFromCUSym(this ptr<dwctxt2> _addr_d, loader.Sym s)
        {
            ref dwctxt2 d = ref _addr_d.val;

            return strings.TrimPrefix(d.ldr.SymName(s), dwarf.InfoPrefix + ".pkg.");
        }

        // On AIX, the symbol table needs to know where are the compilation units parts
        // for a specific package in each .dw section.
        // dwsectCUSize map will save the size of a compilation unit for
        // the corresponding .dw section.
        // This size can later be retrieved with the index "sectionName.pkgName".
        private static map<@string, ulong> dwsectCUSize = default;

        // getDwsectCUSize retrieves the corresponding package size inside the current section.
        private static ulong getDwsectCUSize(@string sname, @string pkgname)
        {
            return dwsectCUSize[sname + "." + pkgname];
        }

        private static void saveDwsectCUSize(@string sname, @string pkgname, ulong size)
        {
            dwsectCUSize[sname + "." + pkgname] = size;
        }

        private static void addDwsectCUSize(@string sname, @string pkgname, ulong size)
        {
            dwsectCUSize[sname + "." + pkgname] += size;
        }
    }
}}}}
