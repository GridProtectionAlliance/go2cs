// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO/NICETOHAVE:
//   - eliminate DW_CLS_ if not used
//   - package info in compilation units
//   - assign global variables and types to their packages
//   - gdb uses c syntax, meaning clumsy quoting is needed for go identifiers. eg
//     ptype struct '[]uint8' and qualifiers need to be quoted away
//   - file:line info for variables
//   - make strings a typedef so prettyprinters can see the underlying string type

// package ld -- go2cs converted at 2020 August 29 10:03:28 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\dwarf.go
using dwarf = go.cmd.@internal.dwarf_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using log = go.log_package;
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
            ref sym.Symbol ls = s._<ref sym.Symbol>();
            ls.AddUintXX(c.linkctxt.Arch, uint64(i), size);
        }
        private static void AddBytes(this dwctxt c, dwarf.Sym s, slice<byte> b)
        {
            ref sym.Symbol ls = s._<ref sym.Symbol>();
            ls.AddBytes(b);
        }
        private static void AddString(this dwctxt c, dwarf.Sym s, @string v)
        {
            Addstring(s._<ref sym.Symbol>(), v);
        }

        private static void AddAddress(this dwctxt c, dwarf.Sym s, object data, long value)
        {
            if (value != 0L)
            {
                value -= (data._<ref sym.Symbol>()).Value;
            }
            s._<ref sym.Symbol>().AddAddrPlus(c.linkctxt.Arch, data._<ref sym.Symbol>(), value);
        }

        private static void AddCURelativeAddress(this dwctxt c, dwarf.Sym s, object data, long value)
        {
            if (value != 0L)
            {
                value -= (data._<ref sym.Symbol>()).Value;
            }
            s._<ref sym.Symbol>().AddCURelativeAddrPlus(c.linkctxt.Arch, data._<ref sym.Symbol>(), value);
        }

        private static void AddSectionOffset(this dwctxt c, dwarf.Sym s, long size, object t, long ofs)
        {
            ref sym.Symbol ls = s._<ref sym.Symbol>();

            if (size == c.linkctxt.Arch.PtrSize) 
                ls.AddAddr(c.linkctxt.Arch, t._<ref sym.Symbol>());
            else if (size == 4L) 
                ls.AddAddrPlus4(t._<ref sym.Symbol>(), 0L);
            else 
                Errorf(ls, "invalid size %d in adddwarfref\n", size);
                        var r = ref ls.R[len(ls.R) - 1L];
            r.Type = objabi.R_DWARFSECREF;
            r.Add = ofs;
        }

        private static void Logf(this dwctxt c, @string format, params object[] args)
        {
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

        private static void RecordChildDieOffsets(this dwctxt c, dwarf.Sym s, slice<ref dwarf.Var> vars, slice<int> offsets) => func((_, panic, __) =>
        {
            panic("should be used only in the compiler");
        });

        private static @string gdbscript = default;

        private static slice<ref sym.Symbol> dwarfp = default;

        private static ref sym.Symbol writeabbrev(ref Link ctxt)
        {
            var s = ctxt.Syms.Lookup(".debug_abbrev", 0L);
            s.Type = sym.SDWARFSECT;
            s.AddBytes(dwarf.GetAbbrev());
            return s;
        }

        /*
         * Root DIEs for compilation units, types and global variables.
         */
        private static dwarf.DWDie dwroot = default;

        private static dwarf.DWDie dwtypes = default;

        private static dwarf.DWDie dwglobals = default;

        private static ref dwarf.DWAttr newattr(ref dwarf.DWDie die, ushort attr, long cls, long value, object data)
        {
            ptr<dwarf.DWAttr> a = @new<dwarf.DWAttr>();
            a.Link = die.Attr;
            die.Attr = a;
            a.Atr = attr;
            a.Cls = uint8(cls);
            a.Value = value;
            a.Data = data;
            return a;
        }

        // Each DIE (except the root ones) has at least 1 attribute: its
        // name. getattr moves the desired one to the front so
        // frequently searched ones are found faster.
        private static ref dwarf.DWAttr getattr(ref dwarf.DWDie die, ushort attr)
        {
            if (die.Attr.Atr == attr)
            {
                return die.Attr;
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
                    return b;
                }
                a = b;
                b = b.Link;
            }


            return null;
        }

        // Every DIE manufactured by the linker has at least an AT_name
        // attribute (but it will only be written out if it is listed in the abbrev).
        // The compiler does create nameless DWARF DIEs (ex: concrete subprogram
        // instance).
        private static ref dwarf.DWDie newdie(ref Link ctxt, ref dwarf.DWDie parent, long abbrev, @string name, long version)
        {
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
                        name = ".pkg." + name;
                    }
                    var s = ctxt.Syms.Lookup(dwarf.InfoPrefix + name, version);
                    s.Attr |= sym.AttrNotInSymbolTable;
                    s.Type = sym.SDWARFINFO;
                    die.Sym = s;
                }
            }
            return die;
        }

        private static ref dwarf.DWDie walktypedef(ref dwarf.DWDie die)
        {
            if (die == null)
            {
                return null;
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
                            return attr.Data._<ref dwarf.DWDie>();
                        attr = attr.Link;
                        }
                    }

                }
            }
            return die;
        }

        private static ref sym.Symbol walksymtypedef(ref Link ctxt, ref sym.Symbol s)
        {
            {
                var t = ctxt.Syms.ROLookup(s.Name + "..def", int(s.Version));

                if (t != null)
                {
                    return t;
                }

            }
            return s;
        }

        // Find child by AT_name using hashtable if available or linear scan
        // if not.
        private static ref dwarf.DWDie findchild(ref dwarf.DWDie die, @string name)
        {
            ref dwarf.DWDie prev = default;
            while (die != prev)
            {
                {
                    var a = die.Child;

                    while (a != null)
                    {
                        if (name == getattr(a, dwarf.DW_AT_name).Data)
                        {
                            return a;
                        a = a.Link;
                        }
                prev = die;
            die = walktypedef(die);
                    }

                }
                continue;
            }

            return null;
        }

        // Used to avoid string allocation when looking up dwarf symbols
        private static slice<byte> prefixBuf = (slice<byte>)dwarf.InfoPrefix;

        private static ref sym.Symbol find(ref Link ctxt, @string name)
        {
            var n = append(prefixBuf, name); 
            // The string allocation below is optimized away because it is only used in a map lookup.
            var s = ctxt.Syms.ROLookup(string(n), 0L);
            prefixBuf = n[..len(dwarf.InfoPrefix)];
            if (s != null && s.Type == sym.SDWARFINFO)
            {
                return s;
            }
            return null;
        }

        private static ref sym.Symbol mustFind(ref Link ctxt, @string name)
        {
            var r = find(ctxt, name);
            if (r == null)
            {
                Exitf("dwarf find: cannot find %s", name);
            }
            return r;
        }

        private static long adddwarfref(ref Link ctxt, ref sym.Symbol s, ref sym.Symbol t, long size)
        {
            long result = default;

            if (size == ctxt.Arch.PtrSize) 
                result = s.AddAddr(ctxt.Arch, t);
            else if (size == 4L) 
                result = s.AddAddrPlus4(t, 0L);
            else 
                Errorf(s, "invalid size %d in adddwarfref\n", size);
                        var r = ref s.R[len(s.R) - 1L];
            r.Type = objabi.R_DWARFSECREF;
            return result;
        }

        private static ref dwarf.DWAttr newrefattr(ref dwarf.DWDie die, ushort attr, ref sym.Symbol @ref)
        {
            if (ref == null)
            {
                return null;
            }
            return newattr(die, attr, dwarf.DW_CLS_REFERENCE, 0L, ref);
        }

        private static slice<ref sym.Symbol> putdies(ref Link linkctxt, dwarf.Context ctxt, slice<ref sym.Symbol> syms, ref dwarf.DWDie die)
        {
            while (die != null)
            {
                syms = putdie(linkctxt, ctxt, syms, die);
                die = die.Link;
            }

            syms[len(syms) - 1L].AddUint8(0L);

            return syms;
        }

        private static ref sym.Symbol dtolsym(dwarf.Sym s)
        {
            if (s == null)
            {
                return null;
            }
            return s._<ref sym.Symbol>();
        }

        private static slice<ref sym.Symbol> putdie(ref Link linkctxt, dwarf.Context ctxt, slice<ref sym.Symbol> syms, ref dwarf.DWDie die)
        {
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
                return putdies(linkctxt, ctxt, syms, die.Child);
            }
            return syms;
        }

        private static void reverselist(ptr<ptr<dwarf.DWDie>> list)
        {
            var curr = list.Value;
            ref dwarf.DWDie prev = default;
            while (curr != null)
            {
                var next = curr.Link;
                curr.Link = prev;
                prev = curr;
                curr = next;
            }


            list.Value = prev;
        }

        private static void reversetree(ptr<ptr<dwarf.DWDie>> list)
        {
            reverselist(list);
            {
                var die = list.Value;

                while (die != null)
                {
                    if (dwarf.HasChildren(die))
                    {
                        reversetree(ref die.Child);
                    die = die.Link;
                    }
                }

            }
        }

        private static void newmemberoffsetattr(ref dwarf.DWDie die, int offs)
        {
            newattr(die, dwarf.DW_AT_data_member_location, dwarf.DW_CLS_CONSTANT, int64(offs), null);
        }

        // GDB doesn't like FORM_addr for AT_location, so emit a
        // location expression that evals to a const.
        private static void newabslocexprattr(ref dwarf.DWDie die, long addr, ref sym.Symbol sym)
        {
            newattr(die, dwarf.DW_AT_location, dwarf.DW_CLS_ADDRESS, addr, sym); 
            // below
        }

        // Lookup predefined types
        private static ref sym.Symbol lookupOrDiag(ref Link ctxt, @string n)
        {
            var s = ctxt.Syms.ROLookup(n, 0L);
            if (s == null || s.Size == 0L)
            {
                Exitf("dwarf: missing type: %s", n);
            }
            return s;
        }

        private static void dotypedef(ref Link ctxt, ref dwarf.DWDie parent, @string name, ref dwarf.DWDie def)
        { 
            // Only emit typedefs for real names.
            if (strings.HasPrefix(name, "map["))
            {
                return;
            }
            if (strings.HasPrefix(name, "struct {"))
            {
                return;
            }
            if (strings.HasPrefix(name, "chan "))
            {
                return;
            }
            if (name[0L] == '[' || name[0L] == '*')
            {
                return;
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
            var die = newdie(ctxt, parent, dwarf.DW_ABRV_TYPEDECL, name, 0L);

            newrefattr(die, dwarf.DW_AT_type, s);
        }

        // Define gotype, for composite ones recurse into constituents.
        private static ref sym.Symbol defgotype(ref Link ctxt, ref sym.Symbol gotype)
        {
            if (gotype == null)
            {
                return mustFind(ctxt, "<unspecified>");
            }
            if (!strings.HasPrefix(gotype.Name, "type."))
            {
                Errorf(gotype, "dwarf: type name doesn't start with \"type.\"");
                return mustFind(ctxt, "<unspecified>");
            }
            var name = gotype.Name[5L..]; // could also decode from Type.string

            var sdie = find(ctxt, name);

            if (sdie != null)
            {
                return sdie;
            }
            return newtype(ctxt, gotype).Sym._<ref sym.Symbol>();
        }

        private static ref dwarf.DWDie newtype(ref Link ctxt, ref sym.Symbol gotype)
        {
            var name = gotype.Name[5L..]; // could also decode from Type.string
            var kind = decodetypeKind(ctxt.Arch, gotype);
            var bytesize = decodetypeSize(ctxt.Arch, gotype);

            ref dwarf.DWDie die = default;

            if (kind == objabi.KindBool) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_boolean, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindInt || kind == objabi.KindInt8 || kind == objabi.KindInt16 || kind == objabi.KindInt32 || kind == objabi.KindInt64) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_signed, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindUint || kind == objabi.KindUint8 || kind == objabi.KindUint16 || kind == objabi.KindUint32 || kind == objabi.KindUint64 || kind == objabi.KindUintptr) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindFloat32 || kind == objabi.KindFloat64) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindComplex64 || kind == objabi.KindComplex128) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0L);
                newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_complex_float, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindArray) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_ARRAYTYPE, name, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                var s = decodetypeArrayElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_type, defgotype(ctxt, s));
                var fld = newdie(ctxt, die, dwarf.DW_ABRV_ARRAYRANGE, "range", 0L); 

                // use actual length not upper bound; correct for 0-length arrays.
                newattr(fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, decodetypeArrayLen(ctxt.Arch, gotype), 0L);

                newrefattr(fld, dwarf.DW_AT_type, mustFind(ctxt, "uintptr"));
            else if (kind == objabi.KindChan) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_CHANTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                s = decodetypeChanElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_elem, defgotype(ctxt, s)); 
                // Save elem type for synthesizechantypes. We could synthesize here
                // but that would change the order of DIEs we output.
                newrefattr(die, dwarf.DW_AT_type, s);
            else if (kind == objabi.KindFunc) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_FUNCTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
                newrefattr(die, dwarf.DW_AT_type, mustFind(ctxt, "void"));
                var nfields = decodetypeFuncInCount(ctxt.Arch, gotype);
                fld = default;
                s = default;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncInType(ctxt.Arch, gotype, i);
                        fld = newdie(ctxt, die, dwarf.DW_ABRV_FUNCTYPEPARAM, s.Name[5L..], 0L);
                        newrefattr(fld, dwarf.DW_AT_type, defgotype(ctxt, s));
                    }


                    i = i__prev1;
                }

                if (decodetypeFuncDotdotdot(ctxt.Arch, gotype))
                {
                    newdie(ctxt, die, dwarf.DW_ABRV_DOTDOTDOT, "...", 0L);
                }
                nfields = decodetypeFuncOutCount(ctxt.Arch, gotype);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < nfields; i++)
                    {
                        s = decodetypeFuncOutType(ctxt.Arch, gotype, i);
                        fld = newdie(ctxt, die, dwarf.DW_ABRV_FUNCTYPEPARAM, s.Name[5L..], 0L);
                        newrefattr(fld, dwarf.DW_AT_type, defptrto(ctxt, defgotype(ctxt, s)));
                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindInterface) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_IFACETYPE, name, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                nfields = int(decodetypeIfaceMethodCount(ctxt.Arch, gotype));
                s = default;
                if (nfields == 0L)
                {
                    s = lookupOrDiag(ctxt, "type.runtime.eface");
                }
                else
                {
                    s = lookupOrDiag(ctxt, "type.runtime.iface");
                }
                newrefattr(die, dwarf.DW_AT_type, defgotype(ctxt, s));
            else if (kind == objabi.KindMap) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_MAPTYPE, name, 0L);
                s = decodetypeMapKey(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_key, defgotype(ctxt, s));
                s = decodetypeMapValue(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_go_elem, defgotype(ctxt, s)); 
                // Save gotype for use in synthesizemaptypes. We could synthesize here,
                // but that would change the order of the DIEs.
                newrefattr(die, dwarf.DW_AT_type, gotype);
            else if (kind == objabi.KindPtr) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_PTRTYPE, name, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
                s = decodetypePtrElem(ctxt.Arch, gotype);
                newrefattr(die, dwarf.DW_AT_type, defgotype(ctxt, s));
            else if (kind == objabi.KindSlice) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_SLICETYPE, name, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
                s = decodetypeArrayElem(ctxt.Arch, gotype);
                var elem = defgotype(ctxt, s);
                newrefattr(die, dwarf.DW_AT_go_elem, elem);
            else if (kind == objabi.KindString) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_STRINGTYPE, name, 0L);
                newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0L);
            else if (kind == objabi.KindStruct) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_STRUCTTYPE, name, 0L);
                dotypedef(ctxt, ref dwtypes, name, die);
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
                        fld = newdie(ctxt, die, dwarf.DW_ABRV_STRUCTFIELD, f, 0L);
                        newrefattr(fld, dwarf.DW_AT_type, defgotype(ctxt, s));
                        var offsetAnon = decodetypeStructFieldOffsAnon(ctxt.Arch, gotype, i);
                        newmemberoffsetattr(fld, int32(offsetAnon >> (int)(1L)));
                        if (offsetAnon & 1L != 0L)
                        { // is embedded field
                            newattr(fld, dwarf.DW_AT_go_embedded_field, dwarf.DW_CLS_FLAG, 1L, 0L);
                        }
                    }


                    i = i__prev1;
                }
            else if (kind == objabi.KindUnsafePointer) 
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, name, 0L);
            else 
                Errorf(gotype, "dwarf: definition of unknown kind %d", kind);
                die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_TYPEDECL, name, 0L);
                newrefattr(die, dwarf.DW_AT_type, mustFind(ctxt, "<unspecified>"));
                        newattr(die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, int64(kind), 0L);

            {
                var (_, ok) = prototypedies[gotype.Name];

                if (ok)
                {
                    prototypedies[gotype.Name] = die;
                }

            }

            return die;
        }

        private static @string nameFromDIESym(ref sym.Symbol dwtype)
        {
            return strings.TrimSuffix(dwtype.Name[len(dwarf.InfoPrefix)..], "..def");
        }

        // Find or construct *T given T.
        private static ref sym.Symbol defptrto(ref Link ctxt, ref sym.Symbol dwtype)
        {
            @string ptrname = "*" + nameFromDIESym(dwtype);
            var die = find(ctxt, ptrname);
            if (die == null)
            {
                var pdie = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_PTRTYPE, ptrname, 0L);
                newrefattr(pdie, dwarf.DW_AT_type, dwtype);
                return dtolsym(pdie.Sym);
            }
            return die;
        }

        // Copies src's children into dst. Copies attributes by value.
        // DWAttr.data is copied as pointer only. If except is one of
        // the top-level children, it will not be copied.
        private static void copychildrenexcept(ref Link ctxt, ref dwarf.DWDie dst, ref dwarf.DWDie src, ref dwarf.DWDie except)
        {
            src = src.Child;

            while (src != null)
            {
                if (src == except)
                {
                    continue;
                src = src.Link;
                }
                var c = newdie(ctxt, dst, src.Abbrev, getattr(src, dwarf.DW_AT_name).Data._<@string>(), 0L);
                {
                    var a = src.Attr;

                    while (a != null)
                    {
                        newattr(c, a.Atr, int(a.Cls), a.Value, a.Data);
                        a = a.Link;
                    }

                }
                copychildrenexcept(ctxt, c, src, null);
            }


            reverselist(ref dst.Child);
        }

        private static void copychildren(ref Link ctxt, ref dwarf.DWDie dst, ref dwarf.DWDie src)
        {
            copychildrenexcept(ctxt, dst, src, null);
        }

        // Search children (assumed to have TAG_member) for the one named
        // field and set its AT_type to dwtype
        private static void substitutetype(ref dwarf.DWDie structdie, @string field, ref sym.Symbol dwtype)
        {
            var child = findchild(structdie, field);
            if (child == null)
            {
                Exitf("dwarf substitutetype: %s does not have member %s", getattr(structdie, dwarf.DW_AT_name).Data, field);
                return;
            }
            var a = getattr(child, dwarf.DW_AT_type);
            if (a != null)
            {
                a.Data = dwtype;
            }
            else
            {
                newrefattr(child, dwarf.DW_AT_type, dwtype);
            }
        }

        private static ref dwarf.DWDie findprotodie(ref Link ctxt, @string name)
        {
            var (die, ok) = prototypedies[name];
            if (ok && die == null)
            {
                defgotype(ctxt, lookupOrDiag(ctxt, name));
                die = prototypedies[name];
            }
            return die;
        }

        private static void synthesizestringtypes(ref Link ctxt, ref dwarf.DWDie die)
        {
            var prototype = walktypedef(findprotodie(ctxt, "type.runtime.stringStructDWARF"));
            if (prototype == null)
            {
                return;
            }
            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_STRINGTYPE)
                {
                    continue;
                die = die.Link;
                }
                copychildren(ctxt, die, prototype);
            }

        }

        private static void synthesizeslicetypes(ref Link ctxt, ref dwarf.DWDie die)
        {
            var prototype = walktypedef(findprotodie(ctxt, "type.runtime.slice"));
            if (prototype == null)
            {
                return;
            }
            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_SLICETYPE)
                {
                    continue;
                die = die.Link;
                }
                copychildren(ctxt, die, prototype);
                ref sym.Symbol elem = getattr(die, dwarf.DW_AT_go_elem).Data._<ref sym.Symbol>();
                substitutetype(die, "array", defptrto(ctxt, elem));
            }

        }

        private static @string mkinternaltypename(@string @base, @string arg1, @string arg2)
        {
            @string buf = default;

            if (arg2 == "")
            {
                buf = fmt.Sprintf("%s<%s>", base, arg1);
            }
            else
            {
                buf = fmt.Sprintf("%s<%s,%s>", base, arg1, arg2);
            }
            var n = buf;
            return n;
        }

        // synthesizemaptypes is way too closely married to runtime/hashmap.c
        public static readonly long MaxKeySize = 128L;
        public static readonly long MaxValSize = 128L;
        public static readonly long BucketSize = 8L;

        private static ref sym.Symbol mkinternaltype(ref Link ctxt, long abbrev, @string typename, @string keyname, @string valname, Action<ref dwarf.DWDie> f)
        {
            var name = mkinternaltypename(typename, keyname, valname);
            var symname = dwarf.InfoPrefix + name;
            var s = ctxt.Syms.ROLookup(symname, 0L);
            if (s != null && s.Type == sym.SDWARFINFO)
            {
                return s;
            }
            var die = newdie(ctxt, ref dwtypes, abbrev, name, 0L);
            f(die);
            return dtolsym(die.Sym);
        }

        private static void synthesizemaptypes(ref Link ctxt, ref dwarf.DWDie die)
        {
            var hash = walktypedef(findprotodie(ctxt, "type.runtime.hmap"));
            var bucket = walktypedef(findprotodie(ctxt, "type.runtime.bmap"));

            if (hash == null)
            {
                return;
            }
            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_MAPTYPE)
                {
                    continue;
                die = die.Link;
                }
                ref sym.Symbol gotype = getattr(die, dwarf.DW_AT_type).Data._<ref sym.Symbol>();
                var keytype = decodetypeMapKey(ctxt.Arch, gotype);
                var valtype = decodetypeMapValue(ctxt.Arch, gotype);
                var keysize = decodetypeSize(ctxt.Arch, keytype);
                var valsize = decodetypeSize(ctxt.Arch, valtype);
                keytype = walksymtypedef(ctxt, defgotype(ctxt, keytype));
                valtype = walksymtypedef(ctxt, defgotype(ctxt, valtype)); 

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
                var keyname = nameFromDIESym(keytype);
                var dwhks = mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]key", keyname, "", dwhk =>
                {
                    newattr(dwhk, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * keysize, 0L);
                    var t = keytype;
                    if (indirectKey)
                    {
                        t = defptrto(ctxt, keytype);
                    }
                    newrefattr(dwhk, dwarf.DW_AT_type, t);
                    var fld = newdie(ctxt, dwhk, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    newrefattr(fld, dwarf.DW_AT_type, mustFind(ctxt, "uintptr"));
                }); 

                // Construct type to represent an array of BucketSize values
                var valname = nameFromDIESym(valtype);
                var dwhvs = mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]val", valname, "", dwhv =>
                {
                    newattr(dwhv, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * valsize, 0L);
                    t = valtype;
                    if (indirectVal)
                    {
                        t = defptrto(ctxt, valtype);
                    }
                    newrefattr(dwhv, dwarf.DW_AT_type, t);
                    fld = newdie(ctxt, dwhv, dwarf.DW_ABRV_ARRAYRANGE, "size", 0L);
                    newattr(fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0L);
                    newrefattr(fld, dwarf.DW_AT_type, mustFind(ctxt, "uintptr"));
                }); 

                // Construct bucket<K,V>
                var dwhbs = mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "bucket", keyname, valname, dwhb =>
                { 
                    // Copy over all fields except the field "data" from the generic
                    // bucket. "data" will be replaced with keys/values below.
                    copychildrenexcept(ctxt, dwhb, bucket, findchild(bucket, "data"));

                    fld = newdie(ctxt, dwhb, dwarf.DW_ABRV_STRUCTFIELD, "keys", 0L);
                    newrefattr(fld, dwarf.DW_AT_type, dwhks);
                    newmemberoffsetattr(fld, BucketSize);
                    fld = newdie(ctxt, dwhb, dwarf.DW_ABRV_STRUCTFIELD, "values", 0L);
                    newrefattr(fld, dwarf.DW_AT_type, dwhvs);
                    newmemberoffsetattr(fld, BucketSize + BucketSize * int32(keysize));
                    fld = newdie(ctxt, dwhb, dwarf.DW_ABRV_STRUCTFIELD, "overflow", 0L);
                    newrefattr(fld, dwarf.DW_AT_type, defptrto(ctxt, dtolsym(dwhb.Sym)));
                    newmemberoffsetattr(fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)));
                    if (ctxt.Arch.RegSize > ctxt.Arch.PtrSize)
                    {
                        fld = newdie(ctxt, dwhb, dwarf.DW_ABRV_STRUCTFIELD, "pad", 0L);
                        newrefattr(fld, dwarf.DW_AT_type, mustFind(ctxt, "uintptr"));
                        newmemberoffsetattr(fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)) + int32(ctxt.Arch.PtrSize));
                    }
                    newattr(dwhb, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize + BucketSize * keysize + BucketSize * valsize + int64(ctxt.Arch.RegSize), 0L);
                }); 

                // Construct hash<K,V>
                var dwhs = mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hash", keyname, valname, dwh =>
                {
                    copychildren(ctxt, dwh, hash);
                    substitutetype(dwh, "buckets", defptrto(ctxt, dwhbs));
                    substitutetype(dwh, "oldbuckets", defptrto(ctxt, dwhbs));
                    newattr(dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(hash, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // make map type a pointer to hash<K,V>
                newrefattr(die, dwarf.DW_AT_type, defptrto(ctxt, dwhs));
            }

        }

        private static void synthesizechantypes(ref Link ctxt, ref dwarf.DWDie die)
        {
            var sudog = walktypedef(findprotodie(ctxt, "type.runtime.sudog"));
            var waitq = walktypedef(findprotodie(ctxt, "type.runtime.waitq"));
            var hchan = walktypedef(findprotodie(ctxt, "type.runtime.hchan"));
            if (sudog == null || waitq == null || hchan == null)
            {
                return;
            }
            var sudogsize = int(getattr(sudog, dwarf.DW_AT_byte_size).Value);

            while (die != null)
            {
                if (die.Abbrev != dwarf.DW_ABRV_CHANTYPE)
                {
                    continue;
                die = die.Link;
                }
                ref sym.Symbol elemgotype = getattr(die, dwarf.DW_AT_type).Data._<ref sym.Symbol>();
                var elemname = elemgotype.Name[5L..];
                var elemtype = walksymtypedef(ctxt, defgotype(ctxt, elemgotype)); 

                // sudog<T>
                var dwss = mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "sudog", elemname, "", dws =>
                {
                    copychildren(ctxt, dws, sudog);
                    substitutetype(dws, "elem", defptrto(ctxt, elemtype));
                    newattr(dws, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(sudogsize), null);
                }); 

                // waitq<T>
                var dwws = mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "waitq", elemname, "", dww =>
                {
                    copychildren(ctxt, dww, waitq);
                    substitutetype(dww, "first", defptrto(ctxt, dwss));
                    substitutetype(dww, "last", defptrto(ctxt, dwss));
                    newattr(dww, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(waitq, dwarf.DW_AT_byte_size).Value, null);
                }); 

                // hchan<T>
                var dwhs = mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hchan", elemname, "", dwh =>
                {
                    copychildren(ctxt, dwh, hchan);
                    substitutetype(dwh, "recvq", dwws);
                    substitutetype(dwh, "sendq", dwws);
                    newattr(dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(hchan, dwarf.DW_AT_byte_size).Value, null);
                });

                newrefattr(die, dwarf.DW_AT_type, defptrto(ctxt, dwhs));
            }

        }

        // For use with pass.c::genasmsym
        private static void defdwsymb(ref Link ctxt, ref sym.Symbol s, @string str, SymbolType t, long v, ref sym.Symbol gotype)
        {
            if (strings.HasPrefix(str, "go.string."))
            {
                return;
            }
            if (strings.HasPrefix(str, "runtime.gcbits."))
            {
                return;
            }
            if (strings.HasPrefix(str, "type.") && str != "type.*" && !strings.HasPrefix(str, "type.."))
            {
                defgotype(ctxt, s);
                return;
            }
            ref dwarf.DWDie dv = default;

            ref sym.Symbol dt = default;

            if (t == DataSym || t == BSSSym)
            {
                dv = newdie(ctxt, ref dwglobals, dwarf.DW_ABRV_VARIABLE, str, int(s.Version));
                newabslocexprattr(dv, v, s);
                if (s.Version == 0L)
                {
                    newattr(dv, dwarf.DW_AT_external, dwarf.DW_CLS_FLAG, 1L, 0L);
                }
                fallthrough = true;

            }
            if (fallthrough || t == AutoSym || t == ParamSym || t == DeletedAutoSym)
            {
                dt = defgotype(ctxt, gotype);
                goto __switch_break0;
            }
            // default: 
                return;

            __switch_break0:;

            if (dv != null)
            {
                newrefattr(dv, dwarf.DW_AT_type, dt);
            }
        }

        // compilationUnit is per-compilation unit (equivalently, per-package)
        // debug-related data.
        private partial struct compilationUnit
        {
            public ptr<sym.Library> lib;
            public ptr<sym.Symbol> consts; // Package constants DIEs
            public slice<dwarf.Range> pcs; // PC ranges, relative to textp[0]
            public ptr<dwarf.DWDie> dwinfo; // CU root DIE
            public slice<ref sym.Symbol> funcDIEs; // Function DIE subtrees
            public slice<ref sym.Symbol> absFnDIEs; // Abstract function DIE subtrees
        }

        // getCompilationUnits divides the symbols in ctxt.Textp by package.
        private static slice<ref compilationUnit> getCompilationUnits(ref Link ctxt)
        {
            ref compilationUnit units = new slice<ref compilationUnit>(new ref compilationUnit[] {  });
            var index = make_map<ref sym.Library, ref compilationUnit>();
            ref compilationUnit prevUnit = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Textp)
                {
                    s = __s;
                    if (s.FuncInfo == null)
                    {
                        continue;
                    }
                    var unit = index[s.Lib];
                    if (unit == null)
                    {
                        unit = ref new compilationUnit(lib:s.Lib);
                        {
                            var s__prev2 = s;

                            var s = ctxt.Syms.ROLookup(dwarf.ConstInfoPrefix + s.Lib.Pkg, 0L);

                            if (s != null)
                            {
                                importInfoSymbol(ctxt, s);
                                unit.consts = s;
                            }

                            s = s__prev2;

                        }
                        units = append(units, unit);
                        index[s.Lib] = unit;
                    } 

                    // Update PC ranges.
                    //
                    // We don't simply compare the end of the previous
                    // symbol with the start of the next because there's
                    // often a little padding between them. Instead, we
                    // only create boundaries between symbols from
                    // different units.
                    if (prevUnit != unit)
                    {
                        unit.pcs = append(unit.pcs, new dwarf.Range(Start:s.Value-unit.lib.Textp[0].Value));
                        prevUnit = unit;
                    }
                    unit.pcs[len(unit.pcs) - 1L].End = s.Value - unit.lib.Textp[0L].Value + s.Size;
                }

                s = s__prev1;
            }

            return units;
        }

        private static void movetomodule(ref dwarf.DWDie parent)
        {
            var die = dwroot.Child.Child;
            if (die == null)
            {
                dwroot.Child.Child = parent.Child;
                return;
            }
            while (die.Link != null)
            {
                die = die.Link;
            }

            die.Link = parent.Child;
        }

        // If the pcln table contains runtime/proc.go, use that to set gdbscript path.
        private static void finddebugruntimepath(ref sym.Symbol s)
        {
            if (gdbscript != "")
            {
                return;
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
        public static readonly long LINE_BASE = -4L;
        public static readonly long LINE_RANGE = 10L;
        public static readonly long PC_RANGE = (255L - OPCODE_BASE) / LINE_RANGE;
        public static readonly long OPCODE_BASE = 10L;

        private static void putpclcdelta(ref Link _linkctxt, dwarf.Context ctxt, ref sym.Symbol _s, ulong deltaPC, long deltaLC) => func(_linkctxt, _s, (ref Link linkctxt, ref sym.Symbol s, Defer _, Panic panic, Recover __) =>
        { 
            // Choose a special opcode that minimizes the number of bytes needed to
            // encode the remaining PC delta and LC delta.
            long opcode = default;
            if (deltaLC < LINE_BASE)
            {
                if (deltaPC >= PC_RANGE)
                {
                    opcode = OPCODE_BASE + (LINE_RANGE * PC_RANGE);
                }
                else
                {
                    opcode = OPCODE_BASE + (LINE_RANGE * int64(deltaPC));
                }
            }
            else if (deltaLC < LINE_BASE + LINE_RANGE)
            {
                if (deltaPC >= PC_RANGE)
                {
                    opcode = OPCODE_BASE + (deltaLC - LINE_BASE) + (LINE_RANGE * PC_RANGE);
                    if (opcode > 255L)
                    {
                        opcode -= LINE_RANGE;
                    }
                }
                else
                {
                    opcode = OPCODE_BASE + (deltaLC - LINE_BASE) + (LINE_RANGE * int64(deltaPC));
                }
            }
            else
            {
                if (deltaPC <= PC_RANGE)
                {
                    opcode = OPCODE_BASE + (LINE_RANGE - 1L) + (LINE_RANGE * int64(deltaPC));
                    if (opcode > 255L)
                    {
                        opcode = 255L;
                    }
                }
                else
                { 
                    // Use opcode 249 (pc+=23, lc+=5) or 255 (pc+=24, lc+=1).
                    //
                    // Let x=deltaPC-PC_RANGE.  If we use opcode 255, x will be the remaining
                    // deltaPC that we need to encode separately before emitting 255.  If we
                    // use opcode 249, we will need to encode x+1.  If x+1 takes one more
                    // byte to encode than x, then we use opcode 255.
                    //
                    // In all other cases x and x+1 take the same number of bytes to encode,
                    // so we use opcode 249, which may save us a byte in encoding deltaLC,
                    // for similar reasons.

                    // PC_RANGE is the largest deltaPC we can encode in one byte, using
                    // DW_LNS_const_add_pc.
                    //
                    // (1<<16)-1 is the largest deltaPC we can encode in three bytes, using
                    // DW_LNS_fixed_advance_pc.
                    //
                    // (1<<(7n))-1 is the largest deltaPC we can encode in n+1 bytes for
                    // n=1,3,4,5,..., using DW_LNS_advance_pc.
                    if (deltaPC - PC_RANGE == PC_RANGE || deltaPC - PC_RANGE == (1L << (int)(7L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(16L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(21L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(28L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(35L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(42L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(49L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(56L)) - 1L || deltaPC - PC_RANGE == (1L << (int)(63L)) - 1L) 
                        opcode = 255L;
                    else 
                        opcode = OPCODE_BASE + LINE_RANGE * PC_RANGE - 1L; // 249
                                    }
            }
            if (opcode < OPCODE_BASE || opcode > 255L)
            {
                panic(fmt.Sprintf("produced invalid special opcode %d", opcode));
            } 

            // Subtract from deltaPC and deltaLC the amounts that the opcode will add.
            deltaPC -= uint64((opcode - OPCODE_BASE) / LINE_RANGE);
            deltaLC -= int64((opcode - OPCODE_BASE) % LINE_RANGE + LINE_BASE); 

            // Encode deltaPC.
            if (deltaPC != 0L)
            {
                if (deltaPC <= PC_RANGE)
                { 
                    // Adjust the opcode so that we can use the 1-byte DW_LNS_const_add_pc
                    // instruction.
                    opcode -= LINE_RANGE * int64(PC_RANGE - deltaPC);
                    if (opcode < OPCODE_BASE)
                    {
                        panic(fmt.Sprintf("produced invalid special opcode %d", opcode));
                    }
                    s.AddUint8(dwarf.DW_LNS_const_add_pc);
                }
                else if ((1L << (int)(14L)) <= deltaPC && deltaPC < (1L << (int)(16L)))
                {
                    s.AddUint8(dwarf.DW_LNS_fixed_advance_pc);
                    s.AddUint16(linkctxt.Arch, uint16(deltaPC));
                }
                else
                {
                    s.AddUint8(dwarf.DW_LNS_advance_pc);
                    dwarf.Uleb128put(ctxt, s, int64(deltaPC));
                }
            } 

            // Encode deltaLC.
            if (deltaLC != 0L)
            {
                s.AddUint8(dwarf.DW_LNS_advance_line);
                dwarf.Sleb128put(ctxt, s, deltaLC);
            } 

            // Output the special opcode.
            s.AddUint8(uint8(opcode));
        });

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

        private static void importInfoSymbol(ref Link ctxt, ref sym.Symbol dsym)
        {
            dsym.Attr |= sym.AttrNotInSymbolTable | sym.AttrReachable;
            dsym.Type = sym.SDWARFINFO;
            foreach (var (_, r) in dsym.R)
            {
                if (r.Type == objabi.R_DWARFSECREF && r.Sym.Size == 0L)
                {
                    if (ctxt.BuildMode == BuildModeShared)
                    { 
                        // These type symbols may not be present in BuildModeShared. Skip.
                        continue;
                    }
                    var n = nameFromDIESym(r.Sym);
                    defgotype(ctxt, ctxt.Syms.Lookup("type." + n, 0L));
                }
            }
        }

        // For the specified function, collect symbols corresponding to any
        // "abstract" subprogram DIEs referenced. The first case of interest
        // is a concrete subprogram DIE, which will refer to its corresponding
        // abstract subprogram DIE, and then there can be references from a
        // non-abstract subprogram DIE to the abstract subprogram DIEs for any
        // functions inlined into this one.
        //
        // A given abstract subprogram DIE can be referenced in numerous
        // places (even within the same DIE), so it is important to make sure
        // it gets imported and added to the absfuncs lists only once.

        private static slice<ref sym.Symbol> collectAbstractFunctions(ref Link ctxt, ref sym.Symbol fn, ref sym.Symbol dsym, slice<ref sym.Symbol> absfuncs)
        {
            slice<ref sym.Symbol> newabsfns = default; 

            // Walk the relocations on the primary subprogram DIE and look for
            // references to abstract funcs.
            foreach (var (_, reloc) in dsym.R)
            {
                var candsym = reloc.Sym;
                if (reloc.Type != objabi.R_DWARFSECREF)
                {
                    continue;
                }
                if (!strings.HasPrefix(candsym.Name, dwarf.InfoPrefix))
                {
                    continue;
                }
                if (!strings.HasSuffix(candsym.Name, dwarf.AbstractFuncSuffix))
                {
                    continue;
                }
                if (candsym.Attr.OnList())
                {
                    continue;
                }
                candsym.Attr |= sym.AttrOnList;
                newabsfns = append(newabsfns, candsym);
            } 

            // Import any new symbols that have turned up.
            foreach (var (_, absdsym) in newabsfns)
            {
                importInfoSymbol(ctxt, absdsym);
                absfuncs = append(absfuncs, absdsym);
            }
            return absfuncs;
        }

        private static (ref dwarf.DWDie, slice<ref sym.Symbol>, slice<ref sym.Symbol>) writelines(ref Link ctxt, ref sym.Library lib, slice<ref sym.Symbol> textp, ref sym.Symbol ls)
        {
            dwarf.Context dwarfctxt = new dwctxt(ctxt);

            var unitstart = int64(-1L);
            var headerstart = int64(-1L);
            var headerend = int64(-1L);

            var lang = dwarf.DW_LANG_Go;

            dwinfo = newdie(ctxt, ref dwroot, dwarf.DW_ABRV_COMPUNIT, lib.Pkg, 0L);
            newattr(dwinfo, dwarf.DW_AT_language, dwarf.DW_CLS_CONSTANT, int64(lang), 0L);
            newattr(dwinfo, dwarf.DW_AT_stmt_list, dwarf.DW_CLS_PTR, ls.Size, ls); 
            // OS X linker requires compilation dir or absolute path in comp unit name to output debug info.
            var compDir = getCompilationDir(); 
            // TODO: Make this be the actual compilation directory, not
            // the linker directory. If we move CU construction into the
            // compiler, this should happen naturally.
            newattr(dwinfo, dwarf.DW_AT_comp_dir, dwarf.DW_CLS_STRING, int64(len(compDir)), compDir);
            var producerExtra = ctxt.Syms.Lookup(dwarf.CUInfoPrefix + "producer." + lib.Pkg, 0L);
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
            }
            newattr(dwinfo, dwarf.DW_AT_producer, dwarf.DW_CLS_STRING, int64(len(producer)), producer); 

            // Write .debug_line Line Number Program Header (sec 6.2.4)
            // Fields marked with (*) must be changed for 64-bit dwarf
            var unitLengthOffset = ls.Size;
            ls.AddUint32(ctxt.Arch, 0L); // unit_length (*), filled in at end.
            unitstart = ls.Size;
            ls.AddUint16(ctxt.Arch, 2L); // dwarf version (appendix F)
            var headerLengthOffset = ls.Size;
            ls.AddUint32(ctxt.Arch, 0L); // header_length (*), filled in at end.
            headerstart = ls.Size; 

            // cpos == unitstart + 4 + 2 + 4
            ls.AddUint8(1L); // minimum_instruction_length
            ls.AddUint8(1L); // default_is_stmt
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
            ls.AddUint8(0L); // include_directories  (empty)

            // Create the file table. fileNums maps from global file
            // indexes (created by numberfile) to CU-local indexes.
            var fileNums = make_map<long, long>();
            {
                var s__prev1 = s;

                foreach (var (_, __s) in textp)
                {
                    s = __s;
                    {
                        var f__prev2 = f;

                        foreach (var (_, __f) in s.FuncInfo.File)
                        {
                            f = __f;
                            {
                                var (_, ok) = fileNums[int(f.Value)];

                                if (ok)
                                {
                                    continue;
                                } 
                                // File indexes are 1-based.

                            } 
                            // File indexes are 1-based.
                            fileNums[int(f.Value)] = len(fileNums) + 1L;
                            Addstring(ls, f.Name);
                            ls.AddUint8(0L);
                            ls.AddUint8(0L);
                            ls.AddUint8(0L);
                        } 

                        // Look up the .debug_info sym for the function. We do this
                        // now so that we can walk the sym's relocations to discover
                        // files that aren't mentioned in S.FuncInfo.File (for
                        // example, files mentioned only in an inlined subroutine).

                        f = f__prev2;
                    }

                    var dsym = ctxt.Syms.Lookup(dwarf.InfoPrefix + s.Name, int(s.Version));
                    importInfoSymbol(ctxt, dsym);
                    {
                        long ri__prev2 = ri;

                        for (long ri = 0L; ri < len(dsym.R); ri++)
                        {
                            var r = ref dsym.R[ri];
                            if (r.Type != objabi.R_DWARFFILEREF)
                            {
                                continue;
                            }
                            (_, ok) = fileNums[int(r.Sym.Value)];
                            if (!ok)
                            {
                                fileNums[int(r.Sym.Value)] = len(fileNums) + 1L;
                                Addstring(ls, r.Sym.Name);
                                ls.AddUint8(0L);
                                ls.AddUint8(0L);
                                ls.AddUint8(0L);
                            }
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

            ls.AddUint8(0L); // start extended opcode
            dwarf.Uleb128put(dwarfctxt, ls, 1L + int64(ctxt.Arch.PtrSize));
            ls.AddUint8(dwarf.DW_LNE_set_address);

            var s = textp[0L];
            var pc = s.Value;
            long line = 1L;
            long file = 1L;
            ls.AddAddr(ctxt.Arch, s);

            Pciter pcfile = default;
            Pciter pcline = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in textp)
                {
                    s = __s;
                    dsym = ctxt.Syms.Lookup(dwarf.InfoPrefix + s.Name, int(s.Version));
                    funcs = append(funcs, dsym);
                    absfuncs = collectAbstractFunctions(ctxt, s, dsym, absfuncs);

                    finddebugruntimepath(s);

                    pciterinit(ctxt, ref pcfile, ref s.FuncInfo.Pcfile);
                    pciterinit(ctxt, ref pcline, ref s.FuncInfo.Pcline);
                    var epc = pc;
                    while (pcfile.done == 0L && pcline.done == 0L)
                    {
                        if (epc - s.Value >= int64(pcfile.nextpc))
                        {
                            pciternext(ref pcfile);
                            continue;
                        }
                        if (epc - s.Value >= int64(pcline.nextpc))
                        {
                            pciternext(ref pcline);
                            continue;
                        }
                        if (int32(file) != pcfile.value)
                        {
                            ls.AddUint8(dwarf.DW_LNS_set_file);
                            var (idx, ok) = fileNums[int(pcfile.value)];
                            if (!ok)
                            {
                                Exitf("pcln table file missing from DWARF line table");
                            }
                            dwarf.Uleb128put(dwarfctxt, ls, int64(idx));
                            file = int(pcfile.value);
                        }
                        putpclcdelta(ctxt, dwarfctxt, ls, uint64(s.Value + int64(pcline.pc) - pc), int64(pcline.value) - int64(line));

                        pc = s.Value + int64(pcline.pc);
                        line = int(pcline.value);
                        if (pcfile.nextpc < pcline.nextpc)
                        {
                            epc = int64(pcfile.nextpc);
                        }
                        else
                        {
                            epc = int64(pcline.nextpc);
                        }
                        epc += s.Value;
                    }

                }

                s = s__prev1;
            }

            ls.AddUint8(0L); // start extended opcode
            dwarf.Uleb128put(dwarfctxt, ls, 1L);
            ls.AddUint8(dwarf.DW_LNE_end_sequence);

            ls.SetUint32(ctxt.Arch, unitLengthOffset, uint32(ls.Size - unitstart));
            ls.SetUint32(ctxt.Arch, headerLengthOffset, uint32(headerend - headerstart)); 

            // Apply any R_DWARFFILEREF relocations, since we now know the
            // line table file indices for this compilation unit. Note that
            // this loop visits only subprogram DIEs: if the compiler is
            // changed to generate DW_AT_decl_file attributes for other
            // DIE flavors (ex: variables) then those DIEs would need to
            // be included below.
            var missing = make();
            for (long fidx = 0L; fidx < len(funcs); fidx++)
            {
                var f = funcs[fidx];
                {
                    long ri__prev2 = ri;

                    for (ri = 0L; ri < len(f.R); ri++)
                    {
                        r = ref f.R[ri];
                        if (r.Type != objabi.R_DWARFFILEREF)
                        {
                            continue;
                        } 
                        // Mark relocation as applied (signal to relocsym)
                        r.Done = true;
                        (idx, ok) = fileNums[int(r.Sym.Value)];
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
                            ctxt.Arch.ByteOrder.PutUint32(f.P[r.Off..r.Off + 4L], uint32(idx));
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


            return (dwinfo, funcs, absfuncs);
        }

        // writepcranges generates the DW_AT_ranges table for compilation unit cu.
        private static void writepcranges(ref Link ctxt, ref dwarf.DWDie cu, ref sym.Symbol @base, slice<dwarf.Range> pcs, ref sym.Symbol ranges)
        {
            dwarf.Context dwarfctxt = new dwctxt(ctxt); 

            // Create PC ranges for this CU.
            newattr(cu, dwarf.DW_AT_ranges, dwarf.DW_CLS_PTR, ranges.Size, ranges);
            newattr(cu, dwarf.DW_AT_low_pc, dwarf.DW_CLS_ADDRESS, @base.Value, base);
            dwarf.PutRanges(dwarfctxt, ranges, null, pcs);
        }

        /*
         *  Emit .debug_frame
         */
        private static readonly long dataAlignmentFactor = -4L;

        // appendPCDeltaCFA appends per-PC CFA deltas to b and returns the final slice.
        private static slice<byte> appendPCDeltaCFA(ref sys.Arch arch, slice<byte> b, long deltapc, long cfa)
        {
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

        private static slice<ref sym.Symbol> writeframes(ref Link ctxt, slice<ref sym.Symbol> syms)
        {
            dwarf.Context dwarfctxt = new dwctxt(ctxt);
            var fs = ctxt.Syms.Lookup(".debug_frame", 0L);
            fs.Type = sym.SDWARFSECT;
            syms = append(syms, fs); 

            // Emit the CIE, Section 6.4.1
            var cieReserve = uint32(16L);
            if (haslinkregister(ctxt))
            {
                cieReserve = 32L;
            }
            fs.AddUint32(ctxt.Arch, cieReserve); // initial length, must be multiple of thearch.ptrsize
            fs.AddUint32(ctxt.Arch, 0xffffffffUL); // cid.
            fs.AddUint8(3L); // dwarf version (appendix F)
            fs.AddUint8(0L); // augmentation ""
            dwarf.Uleb128put(dwarfctxt, fs, 1L); // code_alignment_factor
            dwarf.Sleb128put(dwarfctxt, fs, dataAlignmentFactor); // all CFI offset calculations include multiplication with this factor
            dwarf.Uleb128put(dwarfctxt, fs, int64(Thearch.Dwarfreglr)); // return_address_register

            fs.AddUint8(dwarf.DW_CFA_def_cfa); // Set the current frame address..
            dwarf.Uleb128put(dwarfctxt, fs, int64(Thearch.Dwarfregsp)); // ...to use the value in the platform's SP register (defined in l.go)...
            if (haslinkregister(ctxt))
            {
                dwarf.Uleb128put(dwarfctxt, fs, int64(0L)); // ...plus a 0 offset.

                fs.AddUint8(dwarf.DW_CFA_same_value); // The platform's link register is unchanged during the prologue.
                dwarf.Uleb128put(dwarfctxt, fs, int64(Thearch.Dwarfreglr));

                fs.AddUint8(dwarf.DW_CFA_val_offset); // The previous value...
                dwarf.Uleb128put(dwarfctxt, fs, int64(Thearch.Dwarfregsp)); // ...of the platform's SP register...
                dwarf.Uleb128put(dwarfctxt, fs, int64(0L)); // ...is CFA+0.
            }
            else
            {
                dwarf.Uleb128put(dwarfctxt, fs, int64(ctxt.Arch.PtrSize)); // ...plus the word size (because the call instruction implicitly adds one word to the frame).

                fs.AddUint8(dwarf.DW_CFA_offset_extended); // The previous value...
                dwarf.Uleb128put(dwarfctxt, fs, int64(Thearch.Dwarfreglr)); // ...of the return address...
                dwarf.Uleb128put(dwarfctxt, fs, int64(-ctxt.Arch.PtrSize) / dataAlignmentFactor); // ...is saved at [CFA - (PtrSize/4)].
            } 

            // 4 is to exclude the length field.
            var pad = int64(cieReserve) + 4L - fs.Size;

            if (pad < 0L)
            {
                Exitf("dwarf: cieReserve too small by %d bytes.", -pad);
            }
            fs.AddBytes(zeros[..pad]);

            slice<byte> deltaBuf = default;
            Pciter pcsp = default;
            foreach (var (_, s) in ctxt.Textp)
            {
                if (s.FuncInfo == null)
                {
                    continue;
                } 

                // Emit a FDE, Section 6.4.1.
                // First build the section contents into a byte buffer.
                deltaBuf = deltaBuf[..0L];
                pciterinit(ctxt, ref pcsp, ref s.FuncInfo.Pcsp);

                while (pcsp.done == 0L)
                {
                    var nextpc = pcsp.nextpc; 

                    // pciterinit goes up to the end of the function,
                    // but DWARF expects us to stop just before the end.
                    if (int64(nextpc) == s.Size)
                    {
                        nextpc--;
                        if (nextpc < pcsp.pc)
                        {
                            continue;
                    pciternext(ref pcsp);
                        }
                    }
                    if (haslinkregister(ctxt))
                    { 
                        // TODO(bryanpkc): This is imprecise. In general, the instruction
                        // that stores the return address to the stack frame is not the
                        // same one that allocates the frame.
                        if (pcsp.value > 0L)
                        { 
                            // The return address is preserved at (CFA-frame_size)
                            // after a stack frame has been allocated.
                            deltaBuf = append(deltaBuf, dwarf.DW_CFA_offset_extended_sf);
                            deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(Thearch.Dwarfreglr));
                            deltaBuf = dwarf.AppendSleb128(deltaBuf, -int64(pcsp.value) / dataAlignmentFactor);
                        }
                        else
                        { 
                            // The return address is restored into the link register
                            // when a stack frame has been de-allocated.
                            deltaBuf = append(deltaBuf, dwarf.DW_CFA_same_value);
                            deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(Thearch.Dwarfreglr));
                        }
                        deltaBuf = appendPCDeltaCFA(ctxt.Arch, deltaBuf, int64(nextpc) - int64(pcsp.pc), int64(pcsp.value));
                    }
                    else
                    {
                        deltaBuf = appendPCDeltaCFA(ctxt.Arch, deltaBuf, int64(nextpc) - int64(pcsp.pc), int64(ctxt.Arch.PtrSize) + int64(pcsp.value));
                    }
                }

                pad = int(Rnd(int64(len(deltaBuf)), int64(ctxt.Arch.PtrSize))) - len(deltaBuf);
                deltaBuf = append(deltaBuf, zeros[..pad]); 

                // Emit the FDE header, Section 6.4.1.
                //    4 bytes: length, must be multiple of thearch.ptrsize
                //    4 bytes: Pointer to the CIE above, at offset 0
                //    ptrsize: initial location
                //    ptrsize: address range
                fs.AddUint32(ctxt.Arch, uint32(4L + 2L * ctxt.Arch.PtrSize + len(deltaBuf))); // length (excludes itself)
                if (ctxt.LinkMode == LinkExternal)
                {
                    adddwarfref(ctxt, fs, fs, 4L);
                }
                else
                {
                    fs.AddUint32(ctxt.Arch, 0L); // CIE offset
                }
                fs.AddAddr(ctxt.Arch, s);
                fs.AddUintXX(ctxt.Arch, uint64(s.Size), ctxt.Arch.PtrSize); // address range
                fs.AddBytes(deltaBuf);
            }
            return syms;
        }

        private static slice<ref sym.Symbol> writeranges(ref Link ctxt, slice<ref sym.Symbol> syms)
        {
            foreach (var (_, s) in ctxt.Textp)
            {
                var rangeSym = ctxt.Syms.ROLookup(dwarf.RangePrefix + s.Name, int(s.Version));
                if (rangeSym == null || rangeSym.Size == 0L)
                {
                    continue;
                }
                rangeSym.Attr |= sym.AttrReachable | sym.AttrNotInSymbolTable;
                rangeSym.Type = sym.SDWARFRANGE;
                syms = append(syms, rangeSym);
            }
            return syms;
        }

        /*
         *  Walk DWarfDebugInfoEntries, and emit .debug_info
         */
        public static readonly long COMPUNITHEADERSIZE = 4L + 2L + 4L + 1L;

        private static slice<ref sym.Symbol> writeinfo(ref Link ctxt, slice<ref sym.Symbol> syms, slice<ref compilationUnit> units, ref sym.Symbol abbrevsym)
        {
            var infosec = ctxt.Syms.Lookup(".debug_info", 0L);
            infosec.Type = sym.SDWARFINFO;
            infosec.Attr |= sym.AttrReachable;
            syms = append(syms, infosec);

            dwarf.Context dwarfctxt = new dwctxt(ctxt); 

            // Re-index per-package information by its CU die.
            var unitByDIE = make_map<ref dwarf.DWDie, ref compilationUnit>();
            {
                var u__prev1 = u;

                foreach (var (_, __u) in units)
                {
                    u = __u;
                    unitByDIE[u.dwinfo] = u;
                }

                u = u__prev1;
            }

            {
                var compunit = dwroot.Child;

                while (compunit != null)
                {
                    var s = dtolsym(compunit.Sym);
                    var u = unitByDIE[compunit]; 

                    // Write .debug_info Compilation Unit Header (sec 7.5.1)
                    // Fields marked with (*) must be changed for 64-bit dwarf
                    // This must match COMPUNITHEADERSIZE above.
                    s.AddUint32(ctxt.Arch, 0L); // unit_length (*), will be filled in later.
                    s.AddUint16(ctxt.Arch, 4L); // dwarf version (appendix F)

                    // debug_abbrev_offset (*)
                    adddwarfref(ctxt, s, abbrevsym, 4L);

                    s.AddUint8(uint8(ctxt.Arch.PtrSize)); // address_size

                    dwarf.Uleb128put(dwarfctxt, s, int64(compunit.Abbrev));
                    dwarf.PutAttrs(dwarfctxt, s, compunit.Abbrev, compunit.Attr);

                    ref sym.Symbol cu = new slice<ref sym.Symbol>(new ref sym.Symbol[] { s });
                    cu = append(cu, u.absFnDIEs);
                    cu = append(cu, u.funcDIEs);
                    if (u.consts != null)
                    {
                        cu = append(cu, u.consts);
                    compunit = compunit.Link;
                    }
                    cu = putdies(ctxt, dwarfctxt, cu, compunit.Child);
                    long cusize = default;
                    foreach (var (_, child) in cu)
                    {
                        cusize += child.Size;
                    }
                    cusize -= 4L; // exclude the length field.
                    s.SetUint32(ctxt.Arch, 0L, uint32(cusize)); 
                    // Leave a breadcrumb for writepub. This does not
                    // appear in the DWARF output.
                    newattr(compunit, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, cusize, 0L);
                    syms = append(syms, cu);
                }

            }
            return syms;
        }

        /*
         *  Emit .debug_pubnames/_types.  _info must have been written before,
         *  because we need die->offs and infoo/infosize;
         */
        private static bool ispubname(ref dwarf.DWDie die)
        {

            if (die.Abbrev == dwarf.DW_ABRV_FUNCTION || die.Abbrev == dwarf.DW_ABRV_VARIABLE) 
                var a = getattr(die, dwarf.DW_AT_external);
                return a != null && a.Value != 0L;
                        return false;
        }

        private static bool ispubtype(ref dwarf.DWDie die)
        {
            return die.Abbrev >= dwarf.DW_ABRV_NULLTYPE;
        }

        private static slice<ref sym.Symbol> writepub(ref Link ctxt, @string sname, Func<ref dwarf.DWDie, bool> ispub, slice<ref sym.Symbol> syms)
        {
            var s = ctxt.Syms.Lookup(sname, 0L);
            s.Type = sym.SDWARFSECT;
            syms = append(syms, s);

            {
                var compunit = dwroot.Child;

                while (compunit != null)
                {
                    var sectionstart = s.Size;
                    var culength = uint32(getattr(compunit, dwarf.DW_AT_byte_size).Value) + 4L; 

                    // Write .debug_pubnames/types    Header (sec 6.1.1)
                    s.AddUint32(ctxt.Arch, 0L); // unit_length (*), will be filled in later.
                    s.AddUint16(ctxt.Arch, 2L); // dwarf version (appendix F)
                    adddwarfref(ctxt, s, dtolsym(compunit.Sym), 4L); // debug_info_offset (of the Comp unit Header)
                    s.AddUint32(ctxt.Arch, culength); // debug_info_length

                    {
                        var die = compunit.Child;

                        while (die != null)
                        {
                            if (!ispub(die))
                            {
                                continue;
                            die = die.Link;
                            }
                            var dwa = getattr(die, dwarf.DW_AT_name);
                            @string name = dwa.Data._<@string>();
                            if (die.Sym == null)
                            {
                                fmt.Println("Missing sym for ", name);
                    compunit = compunit.Link;
                            }
                            adddwarfref(ctxt, s, dtolsym(die.Sym), 4L);
                            Addstring(s, name);
                        }

                    }

                    s.AddUint32(ctxt.Arch, 0L);

                    s.SetUint32(ctxt.Arch, sectionstart, uint32(s.Size - sectionstart) - 4L); // exclude the length field.
                }

            }

            return syms;
        }

        private static slice<ref sym.Symbol> writegdbscript(ref Link ctxt, slice<ref sym.Symbol> syms)
        {
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

        private static map<@string, ref dwarf.DWDie> prototypedies = default;

        /*
         * This is the main entry point for generating dwarf.  After emitting
         * the mandatory debug_abbrev section, it calls writelines() to set up
         * the per-compilation unit part of the DIE tree, while simultaneously
         * emitting the debug_line section.  When the final tree contains
         * forward references, it will write the debug_info section in 2
         * passes.
         *
         */
        private static void dwarfgeneratedebugsyms(ref Link ctxt)
        {
            if (FlagW.Value)
            { // disable dwarf
                return;
            }
            if (FlagS && ctxt.HeadType != objabi.Hdarwin.Value)
            {
                return;
            }
            if (ctxt.HeadType == objabi.Hplan9)
            {
                return;
            }
            if (ctxt.LinkMode == LinkExternal)
            {

                if (ctxt.IsELF)                 else if (ctxt.HeadType == objabi.Hdarwin)                 else if (ctxt.HeadType == objabi.Hwindows)                 else 
                    return;
                            }
            if (ctxt.Debugvlog != 0L)
            {
                ctxt.Logf("%5.2f dwarf\n", Cputime());
            } 

            // Forctxt.Diagnostic messages.
            newattr(ref dwtypes, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len("dwtypes")), "dwtypes"); 

            // Some types that must exist to define other ones.
            newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_NULLTYPE, "<unspecified>", 0L);

            newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_NULLTYPE, "void", 0L);
            newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, "unsafe.Pointer", 0L);

            var die = newdie(ctxt, ref dwtypes, dwarf.DW_ABRV_BASETYPE, "uintptr", 0L); // needed for array size
            newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0L);
            newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(ctxt.Arch.PtrSize), 0L);
            newattr(die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, objabi.KindUintptr, 0L); 

            // Prototypes needed for type synthesis.
            prototypedies = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref dwarf.DWDie>{"type.runtime.stringStructDWARF":nil,"type.runtime.slice":nil,"type.runtime.hmap":nil,"type.runtime.bmap":nil,"type.runtime.sudog":nil,"type.runtime.waitq":nil,"type.runtime.hchan":nil,}; 

            // Needed by the prettyprinter code for interface inspection.
            foreach (var (_, typ) in new slice<@string>(new @string[] { "type.runtime._type", "type.runtime.arraytype", "type.runtime.chantype", "type.runtime.functype", "type.runtime.maptype", "type.runtime.ptrtype", "type.runtime.slicetype", "type.runtime.structtype", "type.runtime.interfacetype", "type.runtime.itab", "type.runtime.imethod" }))
            {
                defgotype(ctxt, lookupOrDiag(ctxt, typ));
            }
            genasmsym(ctxt, defdwsymb);

            var abbrev = writeabbrev(ctxt);
            ref sym.Symbol syms = new slice<ref sym.Symbol>(new ref sym.Symbol[] { abbrev });

            var units = getCompilationUnits(ctxt); 

            // Write per-package line and range tables and start their CU DIEs.
            var debugLine = ctxt.Syms.Lookup(".debug_line", 0L);
            debugLine.Type = sym.SDWARFSECT;
            var debugRanges = ctxt.Syms.Lookup(".debug_ranges", 0L);
            debugRanges.Type = sym.SDWARFRANGE;
            debugRanges.Attr |= sym.AttrReachable;
            syms = append(syms, debugLine);
            foreach (var (_, u) in units)
            {
                u.dwinfo, u.funcDIEs, u.absFnDIEs = writelines(ctxt, u.lib, u.lib.Textp, debugLine);
                writepcranges(ctxt, u.dwinfo, u.lib.Textp[0L], u.pcs, debugRanges);
            }
            synthesizestringtypes(ctxt, dwtypes.Child);
            synthesizeslicetypes(ctxt, dwtypes.Child);
            synthesizemaptypes(ctxt, dwtypes.Child);
            synthesizechantypes(ctxt, dwtypes.Child); 

            // newdie adds DIEs to the *beginning* of the parent's DIE list.
            // Now that we're done creating DIEs, reverse the trees so DIEs
            // appear in the order they were created.
            reversetree(ref dwroot.Child);
            reversetree(ref dwtypes.Child);
            reversetree(ref dwglobals.Child);

            movetomodule(ref dwtypes);
            movetomodule(ref dwglobals); 

            // Need to reorder symbols so sym.SDWARFINFO is after all sym.SDWARFSECT
            // (but we need to generate dies before writepub)
            var infosyms = writeinfo(ctxt, null, units, abbrev);

            syms = writeframes(ctxt, syms);
            syms = writepub(ctxt, ".debug_pubnames", ispubname, syms);
            syms = writepub(ctxt, ".debug_pubtypes", ispubtype, syms);
            syms = writegdbscript(ctxt, syms); 
            // Now we're done writing SDWARFSECT symbols, so we can write
            // other SDWARF* symbols.
            syms = append(syms, infosyms);
            syms = collectlocs(ctxt, syms, units);
            syms = append(syms, debugRanges);
            syms = writeranges(ctxt, syms);
            dwarfp = syms;
        }

        private static slice<ref sym.Symbol> collectlocs(ref Link ctxt, slice<ref sym.Symbol> syms, slice<ref compilationUnit> units)
        {
            var empty = true;
            foreach (var (_, u) in units)
            {
                foreach (var (_, fn) in u.funcDIEs)
                {
                    foreach (var (_, reloc) in fn.R)
                    {
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

        /*
         *  Elf.
         */
        private static void dwarfaddshstrings(ref Link ctxt, ref sym.Symbol shstrtab)
        {
            if (FlagW.Value)
            { // disable dwarf
                return;
            }
            Addstring(shstrtab, ".debug_abbrev");
            Addstring(shstrtab, ".debug_frame");
            Addstring(shstrtab, ".debug_info");
            Addstring(shstrtab, ".debug_loc");
            Addstring(shstrtab, ".debug_line");
            Addstring(shstrtab, ".debug_pubnames");
            Addstring(shstrtab, ".debug_pubtypes");
            Addstring(shstrtab, ".debug_gdb_scripts");
            Addstring(shstrtab, ".debug_ranges");
            if (ctxt.LinkMode == LinkExternal)
            {
                Addstring(shstrtab, elfRelType + ".debug_info");
                Addstring(shstrtab, elfRelType + ".debug_loc");
                Addstring(shstrtab, elfRelType + ".debug_line");
                Addstring(shstrtab, elfRelType + ".debug_frame");
                Addstring(shstrtab, elfRelType + ".debug_pubnames");
                Addstring(shstrtab, elfRelType + ".debug_pubtypes");
                Addstring(shstrtab, elfRelType + ".debug_ranges");
            }
        }

        // Add section symbols for DWARF debug info.  This is called before
        // dwarfaddelfheaders.
        private static void dwarfaddelfsectionsyms(ref Link ctxt)
        {
            if (FlagW.Value)
            { // disable dwarf
                return;
            }
            if (ctxt.LinkMode != LinkExternal)
            {
                return;
            }
            var s = ctxt.Syms.Lookup(".debug_info", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            s = ctxt.Syms.Lookup(".debug_abbrev", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            s = ctxt.Syms.Lookup(".debug_line", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            s = ctxt.Syms.Lookup(".debug_frame", 0L);
            putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            s = ctxt.Syms.Lookup(".debug_loc", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            }
            s = ctxt.Syms.Lookup(".debug_ranges", 0L);
            if (s.Sect != null)
            {
                putelfsectionsym(ctxt.Out, s, s.Sect.Elfsect._<ref ElfShdr>().shnum);
            }
        }
    }
}}}}
