// Copyright 2010 The Go Authors. All rights reserved.
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

// package ld -- go2cs converted at 2020 October 08 04:40:59 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\dwarf.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private partial struct dwctxt
        {
            public ptr<Link> linkctxt;
        }

        private static long PtrSize(this dwctxt c)
        {
            return c.linkctxt.Arch.PtrSize;
        }
        private static void AddInt(this dwctxt c, dwarf.Sym s, long size, long i)
        {
            ptr<sym.Symbol> ls = s._<ptr<sym.Symbol>>();
            ls.AddUintXX(c.linkctxt.Arch, uint64(i), size);
        }
        private static void AddBytes(this dwctxt c, dwarf.Sym s, slice<byte> b)
        {
            ptr<sym.Symbol> ls = s._<ptr<sym.Symbol>>();
            ls.AddBytes(b);
        }
        private static void AddString(this dwctxt c, dwarf.Sym s, @string v)
        {
            Addstring(s._<ptr<sym.Symbol>>(), v);
        }

        private static void AddAddress(this dwctxt c, dwarf.Sym s, object data, long value)
        {
            if (value != 0L)
            {
                value -= (data._<ptr<sym.Symbol>>()).Value;
            }

            s._<ptr<sym.Symbol>>().AddAddrPlus(c.linkctxt.Arch, data._<ptr<sym.Symbol>>(), value);

        }

        private static void AddCURelativeAddress(this dwctxt c, dwarf.Sym s, object data, long value)
        {
            if (value != 0L)
            {
                value -= (data._<ptr<sym.Symbol>>()).Value;
            }

            s._<ptr<sym.Symbol>>().AddCURelativeAddrPlus(c.linkctxt.Arch, data._<ptr<sym.Symbol>>(), value);

        }

        private static void AddSectionOffset(this dwctxt c, dwarf.Sym s, long size, object t, long ofs)
        {
            ptr<sym.Symbol> ls = s._<ptr<sym.Symbol>>();

            if (size == c.linkctxt.Arch.PtrSize) 
                ls.AddAddr(c.linkctxt.Arch, t._<ptr<sym.Symbol>>());
            else if (size == 4L) 
                ls.AddAddrPlus4(t._<ptr<sym.Symbol>>(), 0L);
            else 
                Errorf(ls, "invalid size %d in adddwarfref\n", size);
                        var r = _addr_ls.R[len(ls.R) - 1L];
            r.Type = objabi.R_ADDROFF;
            r.Add = ofs;

        }

        private static void AddDWARFAddrSectionOffset(this dwctxt c, dwarf.Sym s, object t, long ofs)
        {
            long size = 4L;
            if (isDwarf64(_addr_c.linkctxt))
            {
                size = 8L;
            }

            c.AddSectionOffset(s, size, t, ofs);
            ptr<sym.Symbol> ls = s._<ptr<sym.Symbol>>();
            ls.R[len(ls.R) - 1L].Type = objabi.R_DWARFSECREF;

        }

        private static void Logf(this dwctxt c, @string format, params object[] args)
        {
            args = args.Clone();

            c.linkctxt.Logf(format, args);
        }

        // At the moment these interfaces are only used in the compiler.

        private static void AddFileRef(this dwctxt c, dwarf.Sym s, object f) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static long CurrentOffset(this dwctxt c, dwarf.Sym s) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static void RecordDclReference(this dwctxt c, dwarf.Sym s, dwarf.Sym t, long dclIdx, long inlIndex) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static void RecordChildDieOffsets(this dwctxt c, dwarf.Sym s, slice<ptr<dwarf.Var>> vars, slice<int> offsets) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static bool isDwarf64(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return ctxt.HeadType == objabi.Haix;
        }

        private static @string gdbscript = default;

        private static slice<ptr<sym.Symbol>> dwarfp = default;

        private static ptr<sym.Symbol> writeabbrev(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var s = ctxt.Syms.Lookup(".debug_abbrev", 0L);
            s.Type = sym.SDWARFSECT;
            s.AddBytes(dwarf.GetAbbrev());
            return _addr_s!;
        }

        private static dwarf.DWDie dwtypes = default;

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
        private static ptr<dwarf.DWDie> newdie(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_parent, long abbrev, @string name, long version)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie parent = ref _addr_parent.val;

            ptr<dwarf.DWDie> die = @new<dwarf.DWDie>();
            die.Abbrev = abbrev;
            die.Link = parent.Child;
            parent.Child = die;

            newattr(die, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len(name)), name);

            if (name != "" && (abbrev <= dwarf.DW_ABRV_VARIABLE || abbrev >= dwarf.DW_ABRV_NULLTYPE))
            {
                if (abbrev != dwarf.DW_ABRV_VARIABLE || version == 0L)
                {
                    if (abbrev == dwarf.DW_ABRV_COMPUNIT)
                    { 
                        // Avoid collisions with "real" symbol names.
                        name = fmt.Sprintf(".pkg.%s.%d", name, len(ctxt.compUnits));

                    }

                    var s = ctxt.Syms.Lookup(dwarf.InfoPrefix + name, version);
                    s.Attr |= sym.AttrNotInSymbolTable;
                    s.Type = sym.SDWARFINFO;
                    die.Sym = s;

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

        private static ptr<sym.Symbol> walksymtypedef(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            {
                var t = ctxt.Syms.ROLookup(s.Name + "..def", int(s.Version));

                if (t != null)
                {
                    return _addr_t!;
                }

            }

            return _addr_s!;

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

        private static ptr<sym.Symbol> find(ptr<Link> _addr_ctxt, @string name)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var n = append(prefixBuf, name); 
            // The string allocation below is optimized away because it is only used in a map lookup.
            var s = ctxt.Syms.ROLookup(string(n), 0L);
            prefixBuf = n[..len(dwarf.InfoPrefix)];
            if (s != null && s.Type == sym.SDWARFINFO)
            {
                return _addr_s!;
            }

            return _addr_null!;

        }

        private static ptr<sym.Symbol> mustFind(ptr<Link> _addr_ctxt, @string name)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var r = find(_addr_ctxt, name);
            if (r == null)
            {
                Exitf("dwarf find: cannot find %s", name);
            }

            return _addr_r!;

        }

        private static long adddwarfref(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Symbol> _addr_t, long size)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol t = ref _addr_t.val;

            long result = default;

            if (size == ctxt.Arch.PtrSize) 
                result = s.AddAddr(ctxt.Arch, t);
            else if (size == 4L) 
                result = s.AddAddrPlus4(t, 0L);
            else 
                Errorf(s, "invalid size %d in adddwarfref\n", size);
                        var r = _addr_s.R[len(s.R) - 1L];
            r.Type = objabi.R_DWARFSECREF;
            return result;

        }

        private static ptr<dwarf.DWAttr> newrefattr(ptr<dwarf.DWDie> _addr_die, ushort attr, ptr<sym.Symbol> _addr_@ref)
        {
            ref dwarf.DWDie die = ref _addr_die.val;
            ref sym.Symbol @ref = ref _addr_@ref.val;

            if (ref == null)
            {
                return _addr_null!;
            }

            return _addr_newattr(_addr_die, attr, dwarf.DW_CLS_REFERENCE, 0L, ref)!;

        }

        private static ptr<sym.Symbol> dtolsym(dwarf.Sym s)
        {
            if (s == null)
            {
                return _addr_null!;
            }

            return s._<ptr<sym.Symbol>>();

        }

        private static slice<ptr<sym.Symbol>> putdie(ptr<Link> _addr_linkctxt, dwarf.Context ctxt, slice<ptr<sym.Symbol>> syms, ptr<dwarf.DWDie> _addr_die)
        {
            ref Link linkctxt = ref _addr_linkctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var s = dtolsym(die.Sym);
            if (s == null)
            {
                s = syms[len(syms) - 1L];
            }
            else
            {
                if (s.Attr.OnList())
                {
                    log.Fatalf("symbol %s listed multiple times", s.Name);
                }

                s.Attr |= sym.AttrOnList;
                syms = append(syms, s);

            }

            dwarf.Uleb128put(ctxt, s, int64(die.Abbrev));
            dwarf.PutAttrs(ctxt, s, die.Abbrev, die.Attr);
            if (dwarf.HasChildren(die))
            {
                {
                    var die = die.Child;

                    while (die != null)
                    {
                        syms = putdie(_addr_linkctxt, ctxt, syms, _addr_die);
                        die = die.Link;
                    }

                }
                syms[len(syms) - 1L].AddUint8(0L);

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
        private static void newabslocexprattr(ptr<dwarf.DWDie> _addr_die, long addr, ptr<sym.Symbol> _addr_sym)
        {
            ref dwarf.DWDie die = ref _addr_die.val;
            ref sym.Symbol sym = ref _addr_sym.val;

            newattr(_addr_die, dwarf.DW_AT_location, dwarf.DW_CLS_ADDRESS, addr, sym); 
            // below
        }

        // Lookup predefined types
        private static ptr<sym.Symbol> lookupOrDiag(ptr<Link> _addr_ctxt, @string n)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var s = ctxt.Syms.ROLookup(n, 0L);
            if (s == null || s.Size == 0L)
            {
                Exitf("dwarf: missing type: %s", n);
            }

            return _addr_s!;

        }

        // dwarfFuncSym looks up a DWARF metadata symbol for function symbol s.
        // If the symbol does not exist, it creates it if create is true,
        // or returns nil otherwise.
        private static ptr<sym.Symbol> dwarfFuncSym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, @string meta, bool create)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
 
            // All function ABIs use symbol version 0 for the DWARF data.
            //
            // TODO(austin): It may be useful to have DWARF info for ABI
            // wrappers, in which case we may want these versions to
            // align. Better yet, replace these name lookups with a
            // general way to attach metadata to a symbol.
            long ver = 0L;
            if (s.IsFileLocal())
            {
                ver = int(s.Version);
            }

            if (create)
            {
                return _addr_ctxt.Syms.Lookup(meta + s.Name, ver)!;
            }

            return _addr_ctxt.Syms.ROLookup(meta + s.Name, ver)!;

        }

        private static ptr<dwarf.DWDie> dotypedef(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_parent, @string name, ptr<dwarf.DWDie> _addr_def)
        {
            ref Link ctxt = ref _addr_ctxt.val;
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

            var s = ctxt.Syms.Lookup(dtolsym(def.Sym).Name + "..def", 0L);
            s.Attr |= sym.AttrNotInSymbolTable;
            s.Type = sym.SDWARFINFO;
            def.Sym = s; 

            // The typedef entry must be created after the def,
            // so that future lookups will find the typedef instead
            // of the real definition. This hooks the typedef into any
            // circular definition loops, so that gdb can understand them.
            var die = newdie(_addr_ctxt, _addr_parent, dwarf.DW_ABRV_TYPEDECL, name, 0L);

            newrefattr(_addr_die, dwarf.DW_AT_type, _addr_s);

            return _addr_die!;

        }

        // Define gotype, for composite ones recurse into constituents.
        private static ptr<sym.Symbol> defgotype(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_gotype)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol gotype = ref _addr_gotype.val;

            if (gotype == null)
            {
                return _addr_mustFind(_addr_ctxt, "<unspecified>")!;
            }

            if (!strings.HasPrefix(gotype.Name, "type."))
            {
                Errorf(gotype, "dwarf: type name doesn't start with \"type.\"");
                return _addr_mustFind(_addr_ctxt, "<unspecified>")!;
            }

            var name = gotype.Name[5L..]; // could also decode from Type.string

            var sdie = find(_addr_ctxt, name);

            if (sdie != null)
            {
                return _addr_sdie!;
            }

            return newtype(_addr_ctxt, _addr_gotype).Sym._<ptr<sym.Symbol>>();

        }

        private static ptr<dwarf.DWDie> newtype(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_gotype)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol gotype = ref _addr_gotype.val;

            var name = gotype.Name[5L..]; // could also decode from Type.string
            var kind = decodetypeKind(ctxt.Arch, gotype.P);
            var bytesize = decodetypeSize(ctxt.Arch, gotype.P);

            ptr<dwarf.DWDie> die;            ptr<dwarf.DWDie> typedefdie;


            if (kind == objabi.KindBool) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_boolean, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindInt || kind == objabi.KindInt8 || kind == objabi.KindInt16 || kind == objabi.KindInt32 || kind == objabi.KindInt64) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_signed, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindUint || kind == objabi.KindUint8 || kind == objabi.KindUint16 || kind == objabi.KindUint32 || kind == objabi.KindUint64 || kind == objabi.KindUintptr) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindFloat32 || kind == objabi.KindFloat64) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindComplex64 || kind == objabi.KindComplex128) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_complex_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindArray) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_ARRAYTYPE, name, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                var s = decodetypeArrayElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_type, _addr_defgotype(_addr_ctxt, _addr_s));
                var fld = newdie(_addr_ctxt, die, dwarf.DW_ABRV_ARRAYRANGE, "range", 0L); 

                // use actual length not upper bound; correct for 0-length arrays.
                newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, decodetypeArrayLen(ctxt.Arch, gotype), 0L);

                newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_mustFind(_addr_ctxt, "uintptr"));
            else if (kind == objabi.KindChan) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_CHANTYPE, name, 0L);
                s = decodetypeChanElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_elem, _addr_defgotype(_addr_ctxt, _addr_s)); 
                // Save elem type for synthesizechantypes. We could synthesize here
                // but that would change the order of DIEs we output.
                newrefattr(die, dwarf.DW_AT_type, _addr_s);
            else if (kind == objabi.KindFunc) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_FUNCTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                var nfields = decodetypeFuncInCount(ctxt.Arch, gotype.P);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncInType(ctxt.Arch, gotype, i);
                        fld = newdie(_addr_ctxt, die, dwarf.DW_ABRV_FUNCTYPEPARAM, s.Name[5L..], 0L);
                        newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_defgotype(_addr_ctxt, _addr_s));
                    }


                    i = i__prev1;
                }

                if (decodetypeFuncDotdotdot(ctxt.Arch, gotype.P))
                {
                    newdie(_addr_ctxt, die, dwarf.DW_ABRV_DOTDOTDOT, "...", 0L);
                }

                nfields = decodetypeFuncOutCount(ctxt.Arch, gotype.P);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncOutType(ctxt.Arch, gotype, i);
                        fld = newdie(_addr_ctxt, die, dwarf.DW_ABRV_FUNCTYPEPARAM, s.Name[5L..], 0L);
                        newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_defptrto(_addr_ctxt, _addr_defgotype(_addr_ctxt, _addr_s)));
                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindInterface) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_IFACETYPE, name, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                nfields = int(decodetypeIfaceMethodCount(ctxt.Arch, gotype.P));
                s = ;
                if (nfields == 0L)
                {
                    s = lookupOrDiag(_addr_ctxt, "type.runtime.eface");
                }
                else
                {
                    s = lookupOrDiag(_addr_ctxt, "type.runtime.iface");
                }

                newrefattr(die, dwarf.DW_AT_type, _addr_defgotype(_addr_ctxt, _addr_s));
            else if (kind == objabi.KindMap) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_MAPTYPE, name, 0L);
                s = decodetypeMapKey(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_key, _addr_defgotype(_addr_ctxt, _addr_s));
                s = decodetypeMapValue(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_elem, _addr_defgotype(_addr_ctxt, _addr_s)); 
                // Save gotype for use in synthesizemaptypes. We could synthesize here,
                // but that would change the order of the DIEs.
                newrefattr(die, dwarf.DW_AT_type, _addr_gotype);
            else if (kind == objabi.KindPtr) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, name, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                s = decodetypePtrElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_type, _addr_defgotype(_addr_ctxt, _addr_s));
            else if (kind == objabi.KindSlice) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_SLICETYPE, name, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                s = decodetypeArrayElem(ctxt.Arch, gotype);
                var elem = defgotype(_addr_ctxt, _addr_s);
                newrefattr(die, dwarf.DW_AT_go_elem, _addr_elem);
            else if (kind == objabi.KindString) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_STRINGTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindStruct) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_STRUCTTYPE, name, 0L);
                typedefdie = dotypedef(_addr_ctxt, _addr_dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                nfields = decodetypeStructFieldCount(ctxt.Arch, gotype);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < nfields; i++)
                    {
                        var f = decodetypeStructFieldName(ctxt.Arch, gotype, i);
                        s = decodetypeStructFieldType(ctxt.Arch, gotype, i);
                        if (f == "")
                        {
                            f = s.Name[5L..]; // skip "type."
                        }

                        fld = newdie(_addr_ctxt, die, dwarf.DW_ABRV_STRUCTFIELD, f, 0L);
                        newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_defgotype(_addr_ctxt, _addr_s));
                        var offsetAnon = decodetypeStructFieldOffsAnon(ctxt.Arch, gotype, i);
                        newmemberoffsetattr(_addr_fld, int32(offsetAnon >> (int)(1L)));
                        if (offsetAnon & 1L != 0L)
                        { // is embedded field
                            newattr(_addr_fld, dwarf.DW_AT_go_embedded_field, dwarf.DW_CLS_FLAG, 1L, 0L);

                        }

                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindUnsafePointer) 
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, name, 0L);
            else 
                Errorf(gotype, "dwarf: definition of unknown kind %d", kind);
                die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_TYPEDECL, name, 0L);
                newrefattr(die, dwarf.DW_AT_type, _addr_mustFind(_addr_ctxt, "<unspecified>"));
                        newattr(die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, int64(kind), 0L);
            if (gotype.Attr.Reachable())
            {
                newattr(die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0L, gotype);
            }

            {
                var (_, ok) = prototypedies[gotype.Name];

                if (ok)
                {
                    prototypedies[gotype.Name] = die;
                }

            }


            if (typedefdie != null)
            {
                return _addr_typedefdie!;
            }

            return _addr_die!;

        }

        private static @string nameFromDIESym(ptr<sym.Symbol> _addr_dwtype)
        {
            ref sym.Symbol dwtype = ref _addr_dwtype.val;

            return strings.TrimSuffix(dwtype.Name[len(dwarf.InfoPrefix)..], "..def");
        }

        // Find or construct *T given T.
        private static ptr<sym.Symbol> defptrto(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_dwtype)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol dwtype = ref _addr_dwtype.val;

            @string ptrname = "*" + nameFromDIESym(_addr_dwtype);
            {
                var die = find(_addr_ctxt, ptrname);

                if (die != null)
                {
                    return _addr_die!;
                }

            }


            var pdie = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, ptrname, 0L);
            newrefattr(_addr_pdie, dwarf.DW_AT_type, _addr_dwtype); 

            // The DWARF info synthesizes pointer types that don't exist at the
            // language level, like *hash<...> and *bucket<...>, and the data
            // pointers of slices. Link to the ones we can find.
            var gotype = ctxt.Syms.ROLookup("type." + ptrname, 0L);
            if (gotype != null && gotype.Attr.Reachable())
            {
                newattr(_addr_pdie, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0L, gotype);
            }

            return _addr_dtolsym(pdie.Sym)!;

        }

        // Copies src's children into dst. Copies attributes by value.
        // DWAttr.data is copied as pointer only. If except is one of
        // the top-level children, it will not be copied.
        private static void copychildrenexcept(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src, ptr<dwarf.DWDie> _addr_except)
        {
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

                var c = newdie(_addr_ctxt, _addr_dst, src.Abbrev, getattr(_addr_src, dwarf.DW_AT_name).Data._<@string>(), 0L);
                {
                    var a = src.Attr;

                    while (a != null)
                    {
                        newattr(_addr_c, a.Atr, int(a.Cls), a.Value, a.Data);
                        a = a.Link;
                    }

                }
                copychildrenexcept(_addr_ctxt, _addr_c, _addr_src, _addr_null);

            }


            reverselist(_addr_dst.Child);

        }

        private static void copychildren(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie dst = ref _addr_dst.val;
            ref dwarf.DWDie src = ref _addr_src.val;

            copychildrenexcept(_addr_ctxt, _addr_dst, _addr_src, _addr_null);
        }

        // Search children (assumed to have TAG_member) for the one named
        // field and set its AT_type to dwtype
        private static void substitutetype(ptr<dwarf.DWDie> _addr_structdie, @string field, ptr<sym.Symbol> _addr_dwtype)
        {
            ref dwarf.DWDie structdie = ref _addr_structdie.val;
            ref sym.Symbol dwtype = ref _addr_dwtype.val;

            var child = findchild(_addr_structdie, field);
            if (child == null)
            {
                Exitf("dwarf substitutetype: %s does not have member %s", getattr(_addr_structdie, dwarf.DW_AT_name).Data, field);
                return ;
            }

            var a = getattr(_addr_child, dwarf.DW_AT_type);
            if (a != null)
            {
                a.Data = dwtype;
            }
            else
            {
                newrefattr(_addr_child, dwarf.DW_AT_type, _addr_dwtype);
            }

        }

        private static ptr<dwarf.DWDie> findprotodie(ptr<Link> _addr_ctxt, @string name)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var (die, ok) = prototypedies[name];
            if (ok && die == null)
            {
                defgotype(_addr_ctxt, _addr_lookupOrDiag(_addr_ctxt, name));
                die = prototypedies[name];
            }

            return _addr_die!;

        }

        private static void synthesizestringtypes(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var prototype = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.stringStructDWARF"));
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

                copychildren(_addr_ctxt, _addr_die, _addr_prototype);

            }


        }

        private static void synthesizeslicetypes(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var prototype = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.slice"));
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

                copychildren(_addr_ctxt, _addr_die, _addr_prototype);
                ptr<sym.Symbol> elem = getattr(_addr_die, dwarf.DW_AT_go_elem).Data._<ptr<sym.Symbol>>();
                substitutetype(_addr_die, "array", _addr_defptrto(_addr_ctxt, elem));

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


        private static ptr<sym.Symbol> mkinternaltype(ptr<Link> _addr_ctxt, long abbrev, @string typename, @string keyname, @string valname, Action<ptr<dwarf.DWDie>> f)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var name = mkinternaltypename(typename, keyname, valname);
            var symname = dwarf.InfoPrefix + name;
            var s = ctxt.Syms.ROLookup(symname, 0L);
            if (s != null && s.Type == sym.SDWARFINFO)
            {
                return _addr_s!;
            }

            var die = newdie(_addr_ctxt, _addr_dwtypes, abbrev, name, 0L);
            f(die);
            return _addr_dtolsym(die.Sym)!;

        }

        private static void synthesizemaptypes(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var hash = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.hmap"));
            var bucket = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.bmap"));

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

                ptr<sym.Symbol> gotype = getattr(_addr_die, dwarf.DW_AT_type).Data._<ptr<sym.Symbol>>();
                var keytype = decodetypeMapKey(ctxt.Arch, gotype);
                var valtype = decodetypeMapValue(ctxt.Arch, gotype);
                var keysize = decodetypeSize(ctxt.Arch, keytype.P);
                var valsize = decodetypeSize(ctxt.Arch, valtype.P);
                keytype = walksymtypedef(_addr_ctxt, _addr_defgotype(_addr_ctxt, _addr_keytype));
                valtype = walksymtypedef(_addr_ctxt, _addr_defgotype(_addr_ctxt, _addr_valtype)); 

                // compute size info like hashmap.c does.
                var indirectKey = false;
                var indirectVal = false;
                if (keysize > MaxKeySize)
                {
                    keysize = int64(ctxt.Arch.PtrSize);
                    indirectKey = true;
                }

                if (valsize > MaxValSize)
                {
                    valsize = int64(ctxt.Arch.PtrSize);
                    indirectVal = true;
                } 

                // Construct type to represent an array of BucketSize keys
                var keyname = nameFromDIESym(_addr_keytype);
                var dwhks = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]key", keyname, "", dwhk =>
                {
                    newattr(_addr_dwhk, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * keysize, 0L);
                    var t = keytype;
                    if (indirectKey)
                    {
                        t = defptrto(_addr_ctxt, _addr_keytype);
                    }

                    newrefattr(_addr_dwhk, dwarf.DW_AT_type, _addr_t);
                    var fld = newdie(_addr_ctxt, _addr_dwhk, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_mustFind(_addr_ctxt, "uintptr"));

                }); 

                // Construct type to represent an array of BucketSize values
                var valname = nameFromDIESym(_addr_valtype);
                var dwhvs = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]val", valname, "", dwhv =>
                {
                    newattr(_addr_dwhv, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * valsize, 0L);
                    t = valtype;
                    if (indirectVal)
                    {
                        t = defptrto(_addr_ctxt, _addr_valtype);
                    }

                    newrefattr(_addr_dwhv, dwarf.DW_AT_type, _addr_t);
                    fld = newdie(_addr_ctxt, _addr_dwhv, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_mustFind(_addr_ctxt, "uintptr"));

                }); 

                // Construct bucket<K,V>
                var dwhbs = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_STRUCTTYPE, "bucket", keyname, valname, dwhb =>
                { 
                    // Copy over all fields except the field "data" from the generic
                    // bucket. "data" will be replaced with keys/values below.
                    copychildrenexcept(_addr_ctxt, _addr_dwhb, _addr_bucket, _addr_findchild(_addr_bucket, "data"));

                    fld = newdie(_addr_ctxt, _addr_dwhb, dwarf.DW_ABRV_STRUCTFIELD, "keys", 0L);
                    newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_dwhks);
                    newmemberoffsetattr(_addr_fld, BucketSize);
                    fld = newdie(_addr_ctxt, _addr_dwhb, dwarf.DW_ABRV_STRUCTFIELD, "values", 0L);
                    newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_dwhvs);
                    newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * int32(keysize));
                    fld = newdie(_addr_ctxt, _addr_dwhb, dwarf.DW_ABRV_STRUCTFIELD, "overflow", 0L);
                    newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_defptrto(_addr_ctxt, _addr_dtolsym(dwhb.Sym)));
                    newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)));
                    if (ctxt.Arch.RegSize > ctxt.Arch.PtrSize)
                    {
                        fld = newdie(_addr_ctxt, _addr_dwhb, dwarf.DW_ABRV_STRUCTFIELD, "pad", 0L);
                        newrefattr(_addr_fld, dwarf.DW_AT_type, _addr_mustFind(_addr_ctxt, "uintptr"));
                        newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)) + int32(ctxt.Arch.PtrSize));
                    }

                    newattr(_addr_dwhb, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize + BucketSize * keysize + BucketSize * valsize + int64(ctxt.Arch.RegSize), 0L);

                }); 

                // Construct hash<K,V>
                var dwhs = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hash", keyname, valname, dwh =>
                {
                    copychildren(_addr_ctxt, _addr_dwh, _addr_hash);
                    substitutetype(_addr_dwh, "buckets", _addr_defptrto(_addr_ctxt, _addr_dwhbs));
                    substitutetype(_addr_dwh, "oldbuckets", _addr_defptrto(_addr_ctxt, _addr_dwhbs));
                    newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hash, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // make map type a pointer to hash<K,V>
                newrefattr(_addr_die, dwarf.DW_AT_type, _addr_defptrto(_addr_ctxt, _addr_dwhs));

            }


        }

        private static void synthesizechantypes(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var sudog = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.sudog"));
            var waitq = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.waitq"));
            var hchan = walktypedef(_addr_findprotodie(_addr_ctxt, "type.runtime.hchan"));
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

                ptr<sym.Symbol> elemgotype = getattr(_addr_die, dwarf.DW_AT_type).Data._<ptr<sym.Symbol>>();
                var elemname = elemgotype.Name[5L..];
                var elemtype = walksymtypedef(_addr_ctxt, _addr_defgotype(_addr_ctxt, elemgotype)); 

                // sudog<T>
                var dwss = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_STRUCTTYPE, "sudog", elemname, "", dws =>
                {
                    copychildren(_addr_ctxt, _addr_dws, _addr_sudog);
                    substitutetype(_addr_dws, "elem", _addr_defptrto(_addr_ctxt, _addr_elemtype));
                    newattr(_addr_dws, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(sudogsize), null);
                }); 

                // waitq<T>
                var dwws = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_STRUCTTYPE, "waitq", elemname, "", dww =>
                {
                    copychildren(_addr_ctxt, _addr_dww, _addr_waitq);
                    substitutetype(_addr_dww, "first", _addr_defptrto(_addr_ctxt, _addr_dwss));
                    substitutetype(_addr_dww, "last", _addr_defptrto(_addr_ctxt, _addr_dwss));
                    newattr(_addr_dww, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_waitq, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // hchan<T>
                var dwhs = mkinternaltype(_addr_ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hchan", elemname, "", dwh =>
                {
                    copychildren(_addr_ctxt, _addr_dwh, _addr_hchan);
                    substitutetype(_addr_dwh, "recvq", _addr_dwws);
                    substitutetype(_addr_dwh, "sendq", _addr_dwws);
                    newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hchan, dwarf.DW_AT_byte_size).Value, null);
                });

                newrefattr(_addr_die, dwarf.DW_AT_type, _addr_defptrto(_addr_ctxt, _addr_dwhs));

            }


        }

        private static void dwarfDefineGlobal(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, @string str, long v, ptr<sym.Symbol> _addr_gotype)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol gotype = ref _addr_gotype.val;
 
            // Find a suitable CU DIE to include the global.
            // One would think it's as simple as just looking at the unit, but that might
            // not have any reachable code. So, we go to the runtime's CU if our unit
            // isn't otherwise reachable.
            ptr<sym.CompilationUnit> unit;
            if (s.Unit != null)
            {
                unit = s.Unit;
            }
            else
            {
                unit = ctxt.runtimeCU;
            }

            var dv = newdie(_addr_ctxt, _addr_unit.DWInfo, dwarf.DW_ABRV_VARIABLE, str, int(s.Version));
            newabslocexprattr(_addr_dv, v, _addr_s);
            if (!s.IsFileLocal())
            {
                newattr(_addr_dv, dwarf.DW_AT_external, dwarf.DW_CLS_FLAG, 1L, 0L);
            }

            var dt = defgotype(_addr_ctxt, _addr_gotype);
            newrefattr(_addr_dv, dwarf.DW_AT_type, _addr_dt);

        }

        // For use with pass.c::genasmsym
        private static void defdwsymb(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, @string str, SymbolType t, long v, ptr<sym.Symbol> _addr_gotype)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol gotype = ref _addr_gotype.val;

            if (strings.HasPrefix(str, "go.string."))
            {
                return ;
            }

            if (strings.HasPrefix(str, "runtime.gcbits."))
            {
                return ;
            }


            if (t == DataSym || t == BSSSym) 

                if (s.Type == sym.SDATA || s.Type == sym.SNOPTRDATA || s.Type == sym.STYPE || s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS || s.Type == sym.STLSBSS)                 else if (s.Type == sym.SRODATA) 
                    if (gotype != null)
                    {
                        defgotype(_addr_ctxt, _addr_gotype);
                    }

                    return ;
                else 
                    return ;
                                if (ctxt.LinkMode != LinkExternal && isStaticTemp(s.Name))
                {
                    return ;
                }

                dwarfDefineGlobal(_addr_ctxt, _addr_s, str, v, _addr_gotype);
            else if (t == AutoSym || t == ParamSym || t == DeletedAutoSym) 
                defgotype(_addr_ctxt, _addr_gotype);
            
        }

        // createUnitLength creates the initial length field with value v and update
        // offset of unit_length if needed.
        private static void createUnitLength(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ulong v)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (isDwarf64(_addr_ctxt))
            {
                s.AddUint32(ctxt.Arch, 0xFFFFFFFFUL);
            }

            addDwarfAddrField(_addr_ctxt, _addr_s, v);

        }

        // addDwarfAddrField adds a DWARF field in DWARF 64bits or 32bits.
        private static void addDwarfAddrField(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ulong v)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (isDwarf64(_addr_ctxt))
            {
                s.AddUint(ctxt.Arch, v);
            }
            else
            {
                s.AddUint32(ctxt.Arch, uint32(v));
            }

        }

        // addDwarfAddrRef adds a DWARF pointer in DWARF 64bits or 32bits.
        private static void addDwarfAddrRef(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s, ptr<sym.Symbol> _addr_t)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;
            ref sym.Symbol t = ref _addr_t.val;

            if (isDwarf64(_addr_ctxt))
            {
                adddwarfref(_addr_ctxt, _addr_s, _addr_t, 8L);
            }
            else
            {
                adddwarfref(_addr_ctxt, _addr_s, _addr_t, 4L);
            }

        }

        // calcCompUnitRanges calculates the PC ranges of the compilation units.
        private static void calcCompUnitRanges(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ptr<sym.CompilationUnit> prevUnit;
            foreach (var (_, s) in ctxt.Textp)
            {
                if (s.FuncInfo == null)
                {
                    continue;
                } 
                // Skip linker-created functions (ex: runtime.addmoduledata), since they
                // don't have DWARF to begin with.
                if (s.Unit == null)
                {
                    continue;
                }

                var unit = s.Unit; 
                // Update PC ranges.
                //
                // We don't simply compare the end of the previous
                // symbol with the start of the next because there's
                // often a little padding between them. Instead, we
                // only create boundaries between symbols from
                // different units.
                if (prevUnit != unit)
                {
                    unit.PCs = append(unit.PCs, new dwarf.Range(Start:s.Value-unit.Textp[0].Value));
                    prevUnit = unit;
                }

                unit.PCs[len(unit.PCs) - 1L].End = s.Value - unit.Textp[0L].Value + s.Size;

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

        // If the pcln table contains runtime/proc.go, use that to set gdbscript path.
        private static void finddebugruntimepath(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (gdbscript != "")
            {
                return ;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s.FuncInfo.File)
                {
                    i = __i;
                    var f = s.FuncInfo.File[i]; 
                    // We can't use something that may be dead-code
                    // eliminated from a binary here. proc.go contains
                    // main and the scheduler, so it's not going anywhere.
                    {
                        var i__prev1 = i;

                        var i = strings.Index(f.Name, "runtime/proc.go");

                        if (i >= 0L)
                        {
                            gdbscript = f.Name[..i] + "runtime/runtime-gdb.py";
                            break;
                        }

                        i = i__prev1;

                    }

                }

                i = i__prev1;
            }
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

        private static void importInfoSymbol(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_dsym)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol dsym = ref _addr_dsym.val;

            dsym.Attr |= sym.AttrNotInSymbolTable | sym.AttrReachable;
            dsym.Type = sym.SDWARFINFO;
            foreach (var (i) in dsym.R)
            {
                var r = _addr_dsym.R[i]; // Copying sym.Reloc has measurable impact on performance
                if (r.Type == objabi.R_DWARFSECREF && r.Sym.Size == 0L)
                {
                    var n = nameFromDIESym(_addr_r.Sym);
                    defgotype(_addr_ctxt, _addr_ctxt.Syms.Lookup("type." + n, 0L));
                }

            }

        }

        private static void writelines(ptr<Link> _addr_ctxt, ptr<sym.CompilationUnit> _addr_unit, ptr<sym.Symbol> _addr_ls)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;
            ref sym.Symbol ls = ref _addr_ls.val;

            dwarf.Context dwarfctxt = new dwctxt(ctxt);
            var is_stmt = uint8(1L); // initially = recommended default_is_stmt = 1, tracks is_stmt toggles.

            var unitstart = int64(-1L);
            var headerstart = int64(-1L);
            var headerend = int64(-1L);

            newattr(_addr_unit.DWInfo, dwarf.DW_AT_stmt_list, dwarf.DW_CLS_PTR, ls.Size, ls); 

            // Write .debug_line Line Number Program Header (sec 6.2.4)
            // Fields marked with (*) must be changed for 64-bit dwarf
            var unitLengthOffset = ls.Size;
            createUnitLength(_addr_ctxt, _addr_ls, 0L); // unit_length (*), filled in at end
            unitstart = ls.Size;
            ls.AddUint16(ctxt.Arch, 2L); // dwarf version (appendix F) -- version 3 is incompatible w/ XCode 9.0's dsymutil, latest supported on OSX 10.12 as of 2018-05
            var headerLengthOffset = ls.Size;
            addDwarfAddrField(_addr_ctxt, _addr_ls, 0L); // header_length (*), filled in at end
            headerstart = ls.Size; 

            // cpos == unitstart + 4 + 2 + 4
            ls.AddUint8(1L); // minimum_instruction_length
            ls.AddUint8(is_stmt); // default_is_stmt
            ls.AddUint8(LINE_BASE & 0xFFUL); // line_base
            ls.AddUint8(LINE_RANGE); // line_range
            ls.AddUint8(OPCODE_BASE); // opcode_base
            ls.AddUint8(0L); // standard_opcode_lengths[1]
            ls.AddUint8(1L); // standard_opcode_lengths[2]
            ls.AddUint8(1L); // standard_opcode_lengths[3]
            ls.AddUint8(1L); // standard_opcode_lengths[4]
            ls.AddUint8(1L); // standard_opcode_lengths[5]
            ls.AddUint8(0L); // standard_opcode_lengths[6]
            ls.AddUint8(0L); // standard_opcode_lengths[7]
            ls.AddUint8(0L); // standard_opcode_lengths[8]
            ls.AddUint8(1L); // standard_opcode_lengths[9]
            ls.AddUint8(0L); // standard_opcode_lengths[10]
            ls.AddUint8(0L); // include_directories  (empty)

            // Copy over the file table.
            var fileNums = make_map<@string, long>();
            {
                var name__prev1 = name;

                foreach (var (__i, __name) in unit.DWARFFileTable)
                {
                    i = __i;
                    name = __name;
                    if (len(name) != 0L)
                    {
                        if (strings.HasPrefix(name, src.FileSymPrefix))
                        {
                            name = name[len(src.FileSymPrefix)..];
                        }

                        name = expandGoroot(name);

                    }
                    else
                    { 
                        // Can't have empty filenames, and having a unique filename is quite useful
                        // for debugging.
                        name = fmt.Sprintf("<missing>_%d", i);

                    }

                    fileNums[name] = i + 1L;
                    dwarfctxt.AddString(ls, name);
                    ls.AddUint8(0L);
                    ls.AddUint8(0L);
                    ls.AddUint8(0L);

                } 
                // Grab files for inlined functions.
                // TODO: With difficulty, this could be moved into the compiler.

                name = name__prev1;
            }

            {
                var s__prev1 = s;

                foreach (var (_, __s) in unit.Textp)
                {
                    s = __s;
                    var dsym = dwarfFuncSym(_addr_ctxt, _addr_s, dwarf.InfoPrefix, true);
                    {
                        long ri__prev2 = ri;

                        for (long ri = 0L; ri < len(dsym.R); ri++)
                        {
                            var r = _addr_dsym.R[ri];
                            if (r.Type != objabi.R_DWARFFILEREF)
                            {
                                continue;
                            }

                            var name = r.Sym.Name;
                            {
                                var (_, ok) = fileNums[name];

                                if (ok)
                                {
                                    continue;
                                }

                            }

                            fileNums[name] = len(fileNums) + 1L;
                            dwarfctxt.AddString(ls, name);
                            ls.AddUint8(0L);
                            ls.AddUint8(0L);
                            ls.AddUint8(0L);

                        }


                        ri = ri__prev2;
                    }

                } 

                // 4 zeros: the string termination + 3 fields.

                s = s__prev1;
            }

            ls.AddUint8(0L); 
            // terminate file_names.
            headerend = ls.Size; 

            // Output the state machine for each function remaining.
            long lastAddr = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in unit.Textp)
                {
                    s = __s;
                    finddebugruntimepath(_addr_s); 

                    // Set the PC.
                    ls.AddUint8(0L);
                    dwarf.Uleb128put(dwarfctxt, ls, 1L + int64(ctxt.Arch.PtrSize));
                    ls.AddUint8(dwarf.DW_LNE_set_address);
                    var addr = ls.AddAddr(ctxt.Arch, s); 
                    // Make sure the units are sorted.
                    if (addr < lastAddr)
                    {
                        Errorf(s, "address wasn't increasing %x < %x", addr, lastAddr);
                    }

                    lastAddr = addr; 

                    // Output the line table.
                    // TODO: Now that we have all the debug information in separate
                    // symbols, it would make sense to use a rope, and concatenate them all
                    // together rather then the append() below. This would allow us to have
                    // the compiler emit the DW_LNE_set_address and a rope data structure
                    // to concat them all together in the output.
                    var lines = dwarfFuncSym(_addr_ctxt, _addr_s, dwarf.DebugLinesPrefix, false);
                    if (lines != null)
                    {
                        ls.P = append(ls.P, lines.P);
                    }

                }

                s = s__prev1;
            }

            ls.AddUint8(0L); // start extended opcode
            dwarf.Uleb128put(dwarfctxt, ls, 1L);
            ls.AddUint8(dwarf.DW_LNE_end_sequence);

            if (ctxt.HeadType == objabi.Haix)
            {
                saveDwsectCUSize(".debug_line", unit.Lib.Pkg, uint64(ls.Size - unitLengthOffset));
            }

            if (isDwarf64(_addr_ctxt))
            {
                ls.SetUint(ctxt.Arch, unitLengthOffset + 4L, uint64(ls.Size - unitstart)); // +4 because of 0xFFFFFFFF
                ls.SetUint(ctxt.Arch, headerLengthOffset, uint64(headerend - headerstart));

            }
            else
            {
                ls.SetUint32(ctxt.Arch, unitLengthOffset, uint32(ls.Size - unitstart));
                ls.SetUint32(ctxt.Arch, headerLengthOffset, uint32(headerend - headerstart));
            } 

            // Process any R_DWARFFILEREF relocations, since we now know the
            // line table file indices for this compilation unit. Note that
            // this loop visits only subprogram DIEs: if the compiler is
            // changed to generate DW_AT_decl_file attributes for other
            // DIE flavors (ex: variables) then those DIEs would need to
            // be included below.
            var missing = make();
            var s = unit.Textp[0L];
            foreach (var (_, f) in unit.FuncDIEs)
            {
                {
                    long ri__prev2 = ri;

                    foreach (var (__ri) in f.R)
                    {
                        ri = __ri;
                        r = _addr_f.R[ri];
                        if (r.Type != objabi.R_DWARFFILEREF)
                        {
                            continue;
                        }

                        var (idx, ok) = fileNums[r.Sym.Name];
                        if (ok)
                        {
                            if (int(int32(idx)) != idx)
                            {
                                Errorf(f, "bad R_DWARFFILEREF relocation: file index overflow");
                            }

                            if (r.Siz != 4L)
                            {
                                Errorf(f, "bad R_DWARFFILEREF relocation: has size %d, expected 4", r.Siz);
                            }

                            if (r.Off < 0L || r.Off + 4L > int32(len(f.P)))
                            {
                                Errorf(f, "bad R_DWARFFILEREF relocation offset %d + 4 would write past length %d", r.Off, len(s.P));
                                continue;
                            }

                            if (r.Add != 0L)
                            {
                                Errorf(f, "bad R_DWARFFILEREF relocation: addend not zero");
                            }

                            r.Sym.Attr |= sym.AttrReachable | sym.AttrNotInSymbolTable;
                            r.Add = int64(idx); // record the index in r.Add, we'll apply it in the reloc phase.
                        }
                        else
                        {
                            var (_, found) = missing[int(r.Sym.Value)];
                            if (!found)
                            {
                                Errorf(f, "R_DWARFFILEREF relocation file missing: %v idx %d", r.Sym, r.Sym.Value);
                                missing[int(r.Sym.Value)] = null;
                            }

                        }

                    }

                    ri = ri__prev2;
                }
            }

        }

        // writepcranges generates the DW_AT_ranges table for compilation unit cu.
        private static void writepcranges(ptr<Link> _addr_ctxt, ptr<sym.CompilationUnit> _addr_unit, ptr<sym.Symbol> _addr_@base, slice<dwarf.Range> pcs, ptr<sym.Symbol> _addr_ranges)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.CompilationUnit unit = ref _addr_unit.val;
            ref sym.Symbol @base = ref _addr_@base.val;
            ref sym.Symbol ranges = ref _addr_ranges.val;

            dwarf.Context dwarfctxt = new dwctxt(ctxt);

            var unitLengthOffset = ranges.Size; 

            // Create PC ranges for this CU.
            newattr(_addr_unit.DWInfo, dwarf.DW_AT_ranges, dwarf.DW_CLS_PTR, ranges.Size, ranges);
            newattr(_addr_unit.DWInfo, dwarf.DW_AT_low_pc, dwarf.DW_CLS_ADDRESS, @base.Value, base);
            dwarf.PutBasedRanges(dwarfctxt, ranges, pcs);

            if (ctxt.HeadType == objabi.Haix)
            {
                addDwsectCUSize(".debug_ranges", unit.Lib.Pkg, uint64(ranges.Size - unitLengthOffset));
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

        private static slice<ptr<sym.Symbol>> writeframes(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            dwarf.Context dwarfctxt = new dwctxt(ctxt);
            var fs = ctxt.Syms.Lookup(".debug_frame", 0L);
            fs.Type = sym.SDWARFSECT;
            syms = append(syms, fs); 

            // Length field is 4 bytes on Dwarf32 and 12 bytes on Dwarf64
            var lengthFieldSize = int64(4L);
            if (isDwarf64(_addr_ctxt))
            {
                lengthFieldSize += 8L;
            } 

            // Emit the CIE, Section 6.4.1
            var cieReserve = uint32(16L);
            if (haslinkregister(ctxt))
            {
                cieReserve = 32L;
            }

            if (isDwarf64(_addr_ctxt))
            {
                cieReserve += 4L; // 4 bytes added for cid
            }

            createUnitLength(_addr_ctxt, _addr_fs, uint64(cieReserve)); // initial length, must be multiple of thearch.ptrsize
            addDwarfAddrField(_addr_ctxt, _addr_fs, ~uint64(0L)); // cid
            fs.AddUint8(3L); // dwarf version (appendix F)
            fs.AddUint8(0L); // augmentation ""
            dwarf.Uleb128put(dwarfctxt, fs, 1L); // code_alignment_factor
            dwarf.Sleb128put(dwarfctxt, fs, dataAlignmentFactor); // all CFI offset calculations include multiplication with this factor
            dwarf.Uleb128put(dwarfctxt, fs, int64(thearch.Dwarfreglr)); // return_address_register

            fs.AddUint8(dwarf.DW_CFA_def_cfa); // Set the current frame address..
            dwarf.Uleb128put(dwarfctxt, fs, int64(thearch.Dwarfregsp)); // ...to use the value in the platform's SP register (defined in l.go)...
            if (haslinkregister(ctxt))
            {
                dwarf.Uleb128put(dwarfctxt, fs, int64(0L)); // ...plus a 0 offset.

                fs.AddUint8(dwarf.DW_CFA_same_value); // The platform's link register is unchanged during the prologue.
                dwarf.Uleb128put(dwarfctxt, fs, int64(thearch.Dwarfreglr));

                fs.AddUint8(dwarf.DW_CFA_val_offset); // The previous value...
                dwarf.Uleb128put(dwarfctxt, fs, int64(thearch.Dwarfregsp)); // ...of the platform's SP register...
                dwarf.Uleb128put(dwarfctxt, fs, int64(0L)); // ...is CFA+0.
            }
            else
            {
                dwarf.Uleb128put(dwarfctxt, fs, int64(ctxt.Arch.PtrSize)); // ...plus the word size (because the call instruction implicitly adds one word to the frame).

                fs.AddUint8(dwarf.DW_CFA_offset_extended); // The previous value...
                dwarf.Uleb128put(dwarfctxt, fs, int64(thearch.Dwarfreglr)); // ...of the return address...
                dwarf.Uleb128put(dwarfctxt, fs, int64(-ctxt.Arch.PtrSize) / dataAlignmentFactor); // ...is saved at [CFA - (PtrSize/4)].
            }

            var pad = int64(cieReserve) + lengthFieldSize - fs.Size;

            if (pad < 0L)
            {
                Exitf("dwarf: cieReserve too small by %d bytes.", -pad);
            }

            fs.AddBytes(zeros[..pad]);

            slice<byte> deltaBuf = default;
            var pcsp = obj.NewPCIter(uint32(ctxt.Arch.MinLC));
            foreach (var (_, s) in ctxt.Textp)
            {
                if (s.FuncInfo == null)
                {
                    continue;
                } 

                // Emit a FDE, Section 6.4.1.
                // First build the section contents into a byte buffer.
                deltaBuf = deltaBuf[..0L];
                if (haslinkregister(ctxt) && s.Attr.TopFrame())
                { 
                    // Mark the link register as having an undefined value.
                    // This stops call stack unwinders progressing any further.
                    // TODO: similar mark on non-LR architectures.
                    deltaBuf = append(deltaBuf, dwarf.DW_CFA_undefined);
                    deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(thearch.Dwarfreglr));

                }

                pcsp.Init(s.FuncInfo.Pcsp.P);

                while (!pcsp.Done)
                {
                    var nextpc = pcsp.NextPC; 

                    // pciterinit goes up to the end of the function,
                    // but DWARF expects us to stop just before the end.
                    if (int64(nextpc) == s.Size)
                    {
                        nextpc--;
                        if (nextpc < pcsp.PC)
                        {
                            continue;
                    pcsp.Next();
                        }

                    }

                    var spdelta = int64(pcsp.Value);
                    if (!haslinkregister(ctxt))
                    { 
                        // Return address has been pushed onto stack.
                        spdelta += int64(ctxt.Arch.PtrSize);

                    }

                    if (haslinkregister(ctxt) && !s.Attr.TopFrame())
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

                    deltaBuf = appendPCDeltaCFA(_addr_ctxt.Arch, deltaBuf, int64(nextpc) - int64(pcsp.PC), spdelta);

                }

                pad = int(Rnd(int64(len(deltaBuf)), int64(ctxt.Arch.PtrSize))) - len(deltaBuf);
                deltaBuf = append(deltaBuf, zeros[..pad]); 

                // Emit the FDE header, Section 6.4.1.
                //    4 bytes: length, must be multiple of thearch.ptrsize
                //    4/8 bytes: Pointer to the CIE above, at offset 0
                //    ptrsize: initial location
                //    ptrsize: address range

                var fdeLength = uint64(4L + 2L * ctxt.Arch.PtrSize + len(deltaBuf));
                if (isDwarf64(_addr_ctxt))
                {
                    fdeLength += 4L; // 4 bytes added for CIE pointer
                }

                createUnitLength(_addr_ctxt, _addr_fs, fdeLength);

                if (ctxt.LinkMode == LinkExternal)
                {
                    addDwarfAddrRef(_addr_ctxt, _addr_fs, _addr_fs);
                }
                else
                {
                    addDwarfAddrField(_addr_ctxt, _addr_fs, 0L); // CIE offset
                }

                fs.AddAddr(ctxt.Arch, s);
                fs.AddUintXX(ctxt.Arch, uint64(s.Size), ctxt.Arch.PtrSize); // address range
                fs.AddBytes(deltaBuf);

                if (ctxt.HeadType == objabi.Haix)
                {
                    addDwsectCUSize(".debug_frame", s.File, fdeLength + uint64(lengthFieldSize));
                }

            }
            return syms;

        }

        /*
         *  Walk DWarfDebugInfoEntries, and emit .debug_info
         */
        public static readonly long COMPUNITHEADERSIZE = (long)4L + 2L + 4L + 1L;


        private static slice<ptr<sym.Symbol>> writeinfo(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> syms, slice<ptr<sym.CompilationUnit>> units, ptr<sym.Symbol> _addr_abbrevsym, ptr<pubWriter> _addr_pubNames, ptr<pubWriter> _addr_pubTypes)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol abbrevsym = ref _addr_abbrevsym.val;
            ref pubWriter pubNames = ref _addr_pubNames.val;
            ref pubWriter pubTypes = ref _addr_pubTypes.val;

            var infosec = ctxt.Syms.Lookup(".debug_info", 0L);
            infosec.Type = sym.SDWARFINFO;
            infosec.Attr |= sym.AttrReachable;
            syms = append(syms, infosec);

            dwarf.Context dwarfctxt = new dwctxt(ctxt);

            foreach (var (_, u) in units)
            {
                var compunit = u.DWInfo;
                var s = dtolsym(compunit.Sym);

                if (len(u.Textp) == 0L && u.DWInfo.Child == null)
                {
                    continue;
                }

                pubNames.beginCompUnit(compunit);
                pubTypes.beginCompUnit(compunit); 

                // Write .debug_info Compilation Unit Header (sec 7.5.1)
                // Fields marked with (*) must be changed for 64-bit dwarf
                // This must match COMPUNITHEADERSIZE above.
                createUnitLength(_addr_ctxt, _addr_s, 0L); // unit_length (*), will be filled in later.
                s.AddUint16(ctxt.Arch, 4L); // dwarf version (appendix F)

                // debug_abbrev_offset (*)
                addDwarfAddrRef(_addr_ctxt, _addr_s, _addr_abbrevsym);

                s.AddUint8(uint8(ctxt.Arch.PtrSize)); // address_size

                dwarf.Uleb128put(dwarfctxt, s, int64(compunit.Abbrev));
                dwarf.PutAttrs(dwarfctxt, s, compunit.Abbrev, compunit.Attr);

                ptr<sym.Symbol> cu = new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] { s });
                cu = append(cu, u.AbsFnDIEs);
                cu = append(cu, u.FuncDIEs);
                if (u.Consts != null)
                {
                    cu = append(cu, u.Consts);
                }

                long cusize = default;
                {
                    var child__prev2 = child;

                    foreach (var (_, __child) in cu)
                    {
                        child = __child;
                        cusize += child.Size;
                    }

                    child = child__prev2;
                }

                {
                    var die = compunit.Child;

                    while (die != null)
                    {
                        var l = len(cu);
                        var lastSymSz = cu[l - 1L].Size;
                        cu = putdie(_addr_ctxt, dwarfctxt, cu, _addr_die);
                        if (ispubname(_addr_die))
                        {
                            pubNames.add(die, cusize);
                        die = die.Link;
                        }

                        if (ispubtype(_addr_die))
                        {
                            pubTypes.add(die, cusize);
                        }

                        if (lastSymSz != cu[l - 1L].Size)
                        { 
                            // putdie will sometimes append directly to the last symbol of the list
                            cusize = cusize - lastSymSz + cu[l - 1L].Size;

                        }

                        {
                            var child__prev3 = child;

                            foreach (var (_, __child) in cu[l..])
                            {
                                child = __child;
                                cusize += child.Size;
                            }

                            child = child__prev3;
                        }
                    }

                }
                cu[len(cu) - 1L].AddUint8(0L); // closes compilation unit DIE
                cusize++; 

                // Save size for AIX symbol table.
                if (ctxt.HeadType == objabi.Haix)
                {
                    saveDwsectCUSize(".debug_info", getPkgFromCUSym(_addr_s), uint64(cusize));
                }

                if (isDwarf64(_addr_ctxt))
                {
                    cusize -= 12L; // exclude the length field.
                    s.SetUint(ctxt.Arch, 4L, uint64(cusize)); // 4 because of 0XFFFFFFFF
                }
                else
                {
                    cusize -= 4L; // exclude the length field.
                    s.SetUint32(ctxt.Arch, 0L, uint32(cusize));

                }

                pubNames.endCompUnit(compunit, uint32(cusize) + 4L);
                pubTypes.endCompUnit(compunit, uint32(cusize) + 4L);
                syms = append(syms, cu);

            }
            return syms;

        }

        /*
         *  Emit .debug_pubnames/_types.  _info must have been written before,
         *  because we need die->offs and infoo/infosize;
         */
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

        private partial struct pubWriter
        {
            public ptr<Link> ctxt;
            public ptr<sym.Symbol> s;
            public @string sname;
            public long sectionstart;
            public long culengthOff;
        }

        private static ptr<pubWriter> newPubWriter(ptr<Link> _addr_ctxt, @string sname)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var s = ctxt.Syms.Lookup(sname, 0L);
            s.Type = sym.SDWARFSECT;
            return addr(new pubWriter(ctxt:ctxt,s:s,sname:sname));
        }

        private static void beginCompUnit(this ptr<pubWriter> _addr_pw, ptr<dwarf.DWDie> _addr_compunit)
        {
            ref pubWriter pw = ref _addr_pw.val;
            ref dwarf.DWDie compunit = ref _addr_compunit.val;

            pw.sectionstart = pw.s.Size; 

            // Write .debug_pubnames/types    Header (sec 6.1.1)
            createUnitLength(_addr_pw.ctxt, _addr_pw.s, 0L); // unit_length (*), will be filled in later.
            pw.s.AddUint16(pw.ctxt.Arch, 2L); // dwarf version (appendix F)
            addDwarfAddrRef(_addr_pw.ctxt, _addr_pw.s, _addr_dtolsym(compunit.Sym)); // debug_info_offset (of the Comp unit Header)
            pw.culengthOff = pw.s.Size;
            addDwarfAddrField(_addr_pw.ctxt, _addr_pw.s, uint64(0L)); // debug_info_length, will be filled in later.
        }

        private static void add(this ptr<pubWriter> _addr_pw, ptr<dwarf.DWDie> _addr_die, long offset)
        {
            ref pubWriter pw = ref _addr_pw.val;
            ref dwarf.DWDie die = ref _addr_die.val;

            var dwa = getattr(_addr_die, dwarf.DW_AT_name);
            @string name = dwa.Data._<@string>();
            if (die.Sym == null)
            {
                fmt.Println("Missing sym for ", name);
            }

            addDwarfAddrField(_addr_pw.ctxt, _addr_pw.s, uint64(offset));
            Addstring(pw.s, name);

        }

        private static void endCompUnit(this ptr<pubWriter> _addr_pw, ptr<dwarf.DWDie> _addr_compunit, uint culength)
        {
            ref pubWriter pw = ref _addr_pw.val;
            ref dwarf.DWDie compunit = ref _addr_compunit.val;

            addDwarfAddrField(_addr_pw.ctxt, _addr_pw.s, 0L); // Null offset

            // On AIX, save the current size of this compilation unit.
            if (pw.ctxt.HeadType == objabi.Haix)
            {
                saveDwsectCUSize(pw.sname, getPkgFromCUSym(_addr_dtolsym(compunit.Sym)), uint64(pw.s.Size - pw.sectionstart));
            }

            if (isDwarf64(_addr_pw.ctxt))
            {
                pw.s.SetUint(pw.ctxt.Arch, pw.sectionstart + 4L, uint64(pw.s.Size - pw.sectionstart) - 12L); // exclude the length field.
                pw.s.SetUint(pw.ctxt.Arch, pw.culengthOff, uint64(culength));

            }
            else
            {
                pw.s.SetUint32(pw.ctxt.Arch, pw.sectionstart, uint32(pw.s.Size - pw.sectionstart) - 4L); // exclude the length field.
                pw.s.SetUint32(pw.ctxt.Arch, pw.culengthOff, culength);

            }

        }

        private static slice<ptr<sym.Symbol>> writegdbscript(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> syms)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // TODO (aix): make it available
            if (ctxt.HeadType == objabi.Haix)
            {
                return syms;
            }

            if (ctxt.LinkMode == LinkExternal && ctxt.HeadType == objabi.Hwindows && ctxt.BuildMode == BuildModeCArchive)
            { 
                // gcc on Windows places .debug_gdb_scripts in the wrong location, which
                // causes the program not to run. See https://golang.org/issue/20183
                // Non c-archives can avoid this issue via a linker script
                // (see fix near writeGDBLinkerScript).
                // c-archive users would need to specify the linker script manually.
                // For UX it's better not to deal with this.
                return syms;

            }

            if (gdbscript != "")
            {
                var s = ctxt.Syms.Lookup(".debug_gdb_scripts", 0L);
                s.Type = sym.SDWARFSECT;
                syms = append(syms, s);
                s.AddUint8(1L); // magic 1 byte?
                Addstring(s, gdbscript);

            }

            return syms;

        }

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

        // dwarfGenerateDebugInfo generated debug info entries for all types,
        // variables and functions in the program.
        // Along with dwarfGenerateDebugSyms they are the two main entry points into
        // dwarf generation: dwarfGenerateDebugInfo does all the work that should be
        // done before symbol names are mangled while dwarfgeneratedebugsyms does
        // all the work that can only be done after addresses have been assigned to
        // text symbols.
        private static void dwarfGenerateDebugInfo(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!dwarfEnabled(_addr_ctxt))
            {
                return ;
            }

            if (ctxt.HeadType == objabi.Haix)
            { 
                // Initial map used to store package size for each DWARF section.
                dwsectCUSize = make_map<@string, ulong>();

            } 

            // Forctxt.Diagnostic messages.
            newattr(_addr_dwtypes, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len("dwtypes")), "dwtypes"); 

            // Some types that must exist to define other ones.
            newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_NULLTYPE, "<unspecified>", 0L);

            newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_NULLTYPE, "void", 0L);
            newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, "unsafe.Pointer", 0L);

            var die = newdie(_addr_ctxt, _addr_dwtypes, dwarf.DW_ABRV_BASETYPE, "uintptr", 0L); // needed for array size
            newattr(_addr_die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
            newattr(_addr_die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(ctxt.Arch.PtrSize), 0L);
            newattr(_addr_die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, objabi.KindUintptr, 0L);
            newattr(_addr_die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_ADDRESS, 0L, lookupOrDiag(_addr_ctxt, "type.uintptr")); 

            // Prototypes needed for type synthesis.
            prototypedies = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<dwarf.DWDie>>{"type.runtime.stringStructDWARF":nil,"type.runtime.slice":nil,"type.runtime.hmap":nil,"type.runtime.bmap":nil,"type.runtime.sudog":nil,"type.runtime.waitq":nil,"type.runtime.hchan":nil,}; 

            // Needed by the prettyprinter code for interface inspection.
            foreach (var (_, typ) in new slice<@string>(new @string[] { "type.runtime._type", "type.runtime.arraytype", "type.runtime.chantype", "type.runtime.functype", "type.runtime.maptype", "type.runtime.ptrtype", "type.runtime.slicetype", "type.runtime.structtype", "type.runtime.interfacetype", "type.runtime.itab", "type.runtime.imethod" }))
            {
                defgotype(_addr_ctxt, _addr_lookupOrDiag(_addr_ctxt, typ));
            } 

            // fake root DIE for compile unit DIEs
            ref dwarf.DWDie dwroot = ref heap(out ptr<dwarf.DWDie> _addr_dwroot);
            var flagVariants = make_map<@string, bool>();

            {
                var lib__prev1 = lib;

                foreach (var (_, __lib) in ctxt.Library)
                {
                    lib = __lib;
                    var consts = ctxt.Syms.ROLookup(dwarf.ConstInfoPrefix + lib.Pkg, 0L);
                    {
                        var unit__prev2 = unit;

                        foreach (var (_, __unit) in lib.Units)
                        {
                            unit = __unit; 
                            // We drop the constants into the first CU.
                            if (consts != null)
                            {
                                importInfoSymbol(_addr_ctxt, _addr_consts);
                                unit.Consts = consts;
                                consts = null;
                            }

                            ctxt.compUnits = append(ctxt.compUnits, unit); 

                            // We need at least one runtime unit.
                            if (unit.Lib.Pkg == "runtime")
                            {
                                ctxt.runtimeCU = unit;
                            }

                            unit.DWInfo = newdie(_addr_ctxt, _addr_dwroot, dwarf.DW_ABRV_COMPUNIT, unit.Lib.Pkg, 0L);
                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_language, dwarf.DW_CLS_CONSTANT, int64(dwarf.DW_LANG_Go), 0L); 
                            // OS X linker requires compilation dir or absolute path in comp unit name to output debug info.
                            var compDir = getCompilationDir(); 
                            // TODO: Make this be the actual compilation directory, not
                            // the linker directory. If we move CU construction into the
                            // compiler, this should happen naturally.
                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_comp_dir, dwarf.DW_CLS_STRING, int64(len(compDir)), compDir);
                            var producerExtra = ctxt.Syms.Lookup(dwarf.CUInfoPrefix + "producer." + unit.Lib.Pkg, 0L);
                            @string producer = "Go cmd/compile " + objabi.Version;
                            if (len(producerExtra.P) > 0L)
                            { 
                                // We put a semicolon before the flags to clearly
                                // separate them from the version, which can be long
                                // and have lots of weird things in it in development
                                // versions. We promise not to put a semicolon in the
                                // version, so it should be safe for readers to scan
                                // forward to the semicolon.
                                producer += "; " + string(producerExtra.P);
                                flagVariants[string(producerExtra.P)] = true;

                            }
                            else
                            {
                                flagVariants[""] = true;
                            }

                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_producer, dwarf.DW_CLS_STRING, int64(len(producer)), producer);

                            @string pkgname = default;
                            {
                                var s__prev1 = s;

                                var s = ctxt.Syms.ROLookup(dwarf.CUInfoPrefix + "packagename." + unit.Lib.Pkg, 0L);

                                if (s != null)
                                {
                                    pkgname = string(s.P);
                                }

                                s = s__prev1;

                            }

                            newattr(_addr_unit.DWInfo, dwarf.DW_AT_go_package_name, dwarf.DW_CLS_STRING, int64(len(pkgname)), pkgname);

                            if (len(unit.Textp) == 0L)
                            {
                                unit.DWInfo.Abbrev = dwarf.DW_ABRV_COMPUNIT_TEXTLESS;
                            } 

                            // Scan all functions in this compilation unit, create DIEs for all
                            // referenced types, create the file table for debug_line, find all
                            // referenced abstract functions.
                            // Collect all debug_range symbols in unit.rangeSyms
                            {
                                var s__prev3 = s;

                                foreach (var (_, __s) in unit.Textp)
                                {
                                    s = __s; // textp has been dead-code-eliminated already.
                                    var dsym = dwarfFuncSym(_addr_ctxt, _addr_s, dwarf.InfoPrefix, false);
                                    dsym.Attr |= sym.AttrNotInSymbolTable | sym.AttrReachable;
                                    dsym.Type = sym.SDWARFINFO;
                                    unit.FuncDIEs = append(unit.FuncDIEs, dsym);

                                    var rangeSym = dwarfFuncSym(_addr_ctxt, _addr_s, dwarf.RangePrefix, false);
                                    if (rangeSym != null && rangeSym.Size > 0L)
                                    {
                                        rangeSym.Attr |= sym.AttrReachable | sym.AttrNotInSymbolTable;
                                        rangeSym.Type = sym.SDWARFRANGE;
                                        if (ctxt.HeadType == objabi.Haix)
                                        {
                                            addDwsectCUSize(".debug_ranges", unit.Lib.Pkg, uint64(rangeSym.Size));
                                        }

                                        unit.RangeSyms = append(unit.RangeSyms, rangeSym);

                                    }

                                    for (long ri = 0L; ri < len(dsym.R); ri++)
                                    {
                                        var r = _addr_dsym.R[ri];
                                        if (r.Type == objabi.R_DWARFSECREF)
                                        {
                                            var rsym = r.Sym;
                                            if (strings.HasPrefix(rsym.Name, dwarf.InfoPrefix) && strings.HasSuffix(rsym.Name, dwarf.AbstractFuncSuffix) && !rsym.Attr.OnList())
                                            { 
                                                // abstract function
                                                rsym.Attr |= sym.AttrOnList;
                                                unit.AbsFnDIEs = append(unit.AbsFnDIEs, rsym);
                                                importInfoSymbol(_addr_ctxt, _addr_rsym);

                                            }
                                            else if (rsym.Size == 0L)
                                            { 
                                                // a type we do not have a DIE for
                                                var n = nameFromDIESym(_addr_rsym);
                                                defgotype(_addr_ctxt, _addr_ctxt.Syms.Lookup("type." + n, 0L));

                                            }

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
            genasmsym(ctxt, defdwsymb); 

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
                            slice<ptr<sym.Symbol>> lists = new slice<slice<ptr<sym.Symbol>>>(new slice<ptr<sym.Symbol>>[] { unit.AbsFnDIEs, unit.FuncDIEs });
                            foreach (var (_, list) in lists)
                            {
                                {
                                    var s__prev4 = s;

                                    foreach (var (_, __s) in list)
                                    {
                                        s = __s;
                                        for (long i = 0L; i < len(s.R); i++)
                                        {
                                            r = _addr_s.R[i];
                                            if (r.Type == objabi.R_USETYPE)
                                            {
                                                defgotype(_addr_ctxt, _addr_r.Sym);
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

            synthesizestringtypes(_addr_ctxt, _addr_dwtypes.Child);
            synthesizeslicetypes(_addr_ctxt, _addr_dwtypes.Child);
            synthesizemaptypes(_addr_ctxt, _addr_dwtypes.Child);
            synthesizechantypes(_addr_ctxt, _addr_dwtypes.Child);

        }

        // dwarfGenerateDebugSyms constructs debug_line, debug_frame, debug_loc,
        // debug_pubnames and debug_pubtypes. It also writes out the debug_info
        // section using symbols generated in dwarfGenerateDebugInfo.
        private static void dwarfGenerateDebugSyms(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (!dwarfEnabled(_addr_ctxt))
            {
                return ;
            }

            var abbrev = writeabbrev(_addr_ctxt);
            ptr<sym.Symbol> syms = new slice<ptr<sym.Symbol>>(new ptr<sym.Symbol>[] { abbrev });

            calcCompUnitRanges(_addr_ctxt);
            sort.Sort(compilationUnitByStartPC(ctxt.compUnits)); 

            // Write per-package line and range tables and start their CU DIEs.
            var debugLine = ctxt.Syms.Lookup(".debug_line", 0L);
            debugLine.Type = sym.SDWARFSECT;
            var debugRanges = ctxt.Syms.Lookup(".debug_ranges", 0L);
            debugRanges.Type = sym.SDWARFRANGE;
            debugRanges.Attr |= sym.AttrReachable;
            syms = append(syms, debugLine);
            foreach (var (_, u) in ctxt.compUnits)
            {
                reversetree(_addr_u.DWInfo.Child);
                if (u.DWInfo.Abbrev == dwarf.DW_ABRV_COMPUNIT_TEXTLESS)
                {
                    continue;
                }

                writelines(_addr_ctxt, _addr_u, _addr_debugLine);
                writepcranges(_addr_ctxt, _addr_u, _addr_u.Textp[0L], u.PCs, _addr_debugRanges);

            } 

            // newdie adds DIEs to the *beginning* of the parent's DIE list.
            // Now that we're done creating DIEs, reverse the trees so DIEs
            // appear in the order they were created.
            reversetree(_addr_dwtypes.Child);
            movetomodule(_addr_ctxt, _addr_dwtypes);

            var pubNames = newPubWriter(_addr_ctxt, ".debug_pubnames");
            var pubTypes = newPubWriter(_addr_ctxt, ".debug_pubtypes"); 

            // Need to reorder symbols so sym.SDWARFINFO is after all sym.SDWARFSECT
            var infosyms = writeinfo(_addr_ctxt, null, ctxt.compUnits, _addr_abbrev, _addr_pubNames, _addr_pubTypes);

            syms = writeframes(_addr_ctxt, syms);
            syms = append(syms, pubNames.s, pubTypes.s);
            syms = writegdbscript(_addr_ctxt, syms); 
            // Now we're done writing SDWARFSECT symbols, so we can write
            // other SDWARF* symbols.
            syms = append(syms, infosyms);
            syms = collectlocs(_addr_ctxt, syms, ctxt.compUnits);
            syms = append(syms, debugRanges);
            foreach (var (_, unit) in ctxt.compUnits)
            {
                syms = append(syms, unit.RangeSyms);
            }
            dwarfp = syms;

        }

        private static slice<ptr<sym.Symbol>> collectlocs(ptr<Link> _addr_ctxt, slice<ptr<sym.Symbol>> syms, slice<ptr<sym.CompilationUnit>> units)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var empty = true;
            foreach (var (_, u) in units)
            {
                foreach (var (_, fn) in u.FuncDIEs)
                {
                    foreach (var (i) in fn.R)
                    {
                        var reloc = _addr_fn.R[i]; // Copying sym.Reloc has measurable impact on performance
                        if (reloc.Type == objabi.R_DWARFSECREF && strings.HasPrefix(reloc.Sym.Name, dwarf.LocPrefix))
                        {
                            reloc.Sym.Attr |= sym.AttrReachable | sym.AttrNotInSymbolTable;
                            syms = append(syms, reloc.Sym);
                            empty = false; 
                            // One location list entry per function, but many relocations to it. Don't duplicate.
                            break;

                        }

                    }

                }

            } 
            // Don't emit .debug_loc if it's empty -- it makes the ARM linker mad.
            if (!empty)
            {
                var locsym = ctxt.Syms.Lookup(".debug_loc", 0L);
                locsym.Type = sym.SDWARFLOC;
                locsym.Attr |= sym.AttrReachable;
                syms = append(syms, locsym);
            }

            return syms;

        }

        // Read a pointer-sized uint from the beginning of buf.
        private static ulong readPtr(ptr<Link> _addr_ctxt, slice<byte> buf) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;

            switch (ctxt.Arch.PtrSize)
            {
                case 4L: 
                    return uint64(ctxt.Arch.ByteOrder.Uint32(buf));
                    break;
                case 8L: 
                    return ctxt.Arch.ByteOrder.Uint64(buf);
                    break;
                default: 
                    panic("unexpected pointer size");
                    break;
            }

        });

        /*
         *  Elf.
         */
        private static void dwarfaddshstrings(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_shstrtab)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol shstrtab = ref _addr_shstrtab.val;

            if (FlagW.val)
            { // disable dwarf
                return ;

            }

            @string secs = new slice<@string>(new @string[] { "abbrev", "frame", "info", "loc", "line", "pubnames", "pubtypes", "gdb_scripts", "ranges" });
            foreach (var (_, sec) in secs)
            {
                Addstring(shstrtab, ".debug_" + sec);
                if (ctxt.LinkMode == LinkExternal)
                {
                    Addstring(shstrtab, elfRelType + ".debug_" + sec);
                }
                else
                {
                    Addstring(shstrtab, ".zdebug_" + sec);
                }

            }

        }

        // Add section symbols for DWARF debug info.  This is called before
        // dwarfaddelfheaders.
        private static void dwarfaddelfsectionsyms(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (FlagW.val)
            { // disable dwarf
                return ;

            }

            if (ctxt.LinkMode != LinkExternal)
            {
                return ;
            }

            var s = ctxt.Syms.Lookup(".debug_info", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_abbrev", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_line", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_frame", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            s = ctxt.Syms.Lookup(".debug_loc", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            }

            s = ctxt.Syms.Lookup(".debug_ranges", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ptr<ElfShdr>>().shnum);
            }

        }

        // dwarfcompress compresses the DWARF sections. Relocations are applied
        // on the fly. After this, dwarfp will contain a different (new) set of
        // symbols, and sections may have been replaced.
        private static void dwarfcompress(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var supported = ctxt.IsELF || ctxt.HeadType == objabi.Hwindows || ctxt.HeadType == objabi.Hdarwin;
            if (!ctxt.compressDWARF || !supported || ctxt.LinkMode != LinkInternal)
            {
                return ;
            }

            long start = default;
            slice<ptr<sym.Symbol>> newDwarfp = default;
            Segdwarf.Sections = Segdwarf.Sections[..0L];
            {
                var s__prev1 = s;

                foreach (var (__i, __s) in dwarfp)
                {
                    i = __i;
                    s = __s; 
                    // Find the boundaries between sections and compress
                    // the whole section once we've found the last of its
                    // symbols.
                    if (i + 1L >= len(dwarfp) || s.Sect != dwarfp[i + 1L].Sect)
                    {
                        var s1 = compressSyms(ctxt, dwarfp[start..i + 1L]);
                        if (s1 == null)
                        { 
                            // Compression didn't help.
                            newDwarfp = append(newDwarfp, dwarfp[start..i + 1L]);
                            Segdwarf.Sections = append(Segdwarf.Sections, s.Sect);

                        }
                        else
                        {
                            @string compressedSegName = ".zdebug_" + s.Sect.Name[len(".debug_")..];
                            var sect = addsection(ctxt.Arch, _addr_Segdwarf, compressedSegName, 04L);
                            sect.Length = uint64(len(s1));
                            var newSym = ctxt.Syms.Lookup(compressedSegName, 0L);
                            newSym.P = s1;
                            newSym.Size = int64(len(s1));
                            newSym.Sect = sect;
                            newDwarfp = append(newDwarfp, newSym);
                        }

                        start = i + 1L;

                    }

                }

                s = s__prev1;
            }

            dwarfp = newDwarfp;
            ctxt.relocbuf = null; // no longer needed, don't hold it live

            // Re-compute the locations of the compressed DWARF symbols
            // and sections, since the layout of these within the file is
            // based on Section.Vaddr and Symbol.Value.
            var pos = Segdwarf.Vaddr;
            ptr<sym.Section> prevSect;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in dwarfp)
                {
                    s = __s;
                    s.Value = int64(pos);
                    if (s.Sect != prevSect)
                    {
                        s.Sect.Vaddr = uint64(s.Value);
                        prevSect = s.Sect;
                    }

                    if (s.Sub != null)
                    {
                        log.Fatalf("%s: unexpected sub-symbols", s);
                    }

                    pos += uint64(s.Size);
                    if (ctxt.HeadType == objabi.Hwindows)
                    {
                        pos = uint64(Rnd(int64(pos), PEFILEALIGN));
                    }

                }

                s = s__prev1;
            }

            Segdwarf.Length = pos - Segdwarf.Vaddr;

        }

        private partial struct compilationUnitByStartPC // : slice<ptr<sym.CompilationUnit>>
        {
        }

        private static long Len(this compilationUnitByStartPC v)
        {
            return len(v);
        }
        private static void Swap(this compilationUnitByStartPC v, long i, long j)
        {
            v[i] = v[j];
            v[j] = v[i];
        }

        private static bool Less(this compilationUnitByStartPC v, long i, long j)
        {

            if (len(v[i].Textp) == 0L && len(v[j].Textp) == 0L) 
                return v[i].Lib.Pkg < v[j].Lib.Pkg;
            else if (len(v[i].Textp) != 0L && len(v[j].Textp) == 0L) 
                return true;
            else if (len(v[i].Textp) == 0L && len(v[j].Textp) != 0L) 
                return false;
            else 
                return v[i].Textp[0L].Value < v[j].Textp[0L].Value;
            
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

        // getPkgFromCUSym returns the package name for the compilation unit
        // represented by s.
        // The prefix dwarf.InfoPrefix+".pkg." needs to be removed in order to get
        // the package name.
        private static @string getPkgFromCUSym(ptr<sym.Symbol> _addr_s)
        {
            ref sym.Symbol s = ref _addr_s.val;

            return strings.TrimPrefix(s.Name, dwarf.InfoPrefix + ".pkg.");
        }
    }
}}}}
