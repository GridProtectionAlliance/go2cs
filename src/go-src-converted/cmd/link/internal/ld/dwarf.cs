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

// package ld -- go2cs converted at 2022 March 13 06:34:06 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\dwarf.go
namespace go.cmd.link.@internal;

using dwarf = cmd.@internal.dwarf_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;
using sys = cmd.@internal.sys_package;
using loader = cmd.link.@internal.loader_package;
using sym = cmd.link.@internal.sym_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using log = log_package;
using path = path_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;


// dwctxt is a wrapper intended to satisfy the method set of
// dwarf.Context, so that functions like dwarf.PutAttrs will work with
// DIEs that use loader.Sym as opposed to *sym.Symbol. It is also
// being used as a place to store tables/maps that are useful as part
// of type conversion (this is just a convenience; it would be easy to
// split these things out into another type if need be).

using System;
using System.Threading;
public static partial class ld_package {

private partial struct dwctxt {
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
    public loader.Sym uintptrInfoSym; // Used at various points in that parallel portion of DWARF gen to
// protect against conflicting updates to globals (such as "gdbscript")
    public ptr<sync.Mutex> dwmu;
}

private static dwctxt newdwctxt(ptr<Link> _addr_linkctxt, bool forTypeGen) {
    ref Link linkctxt = ref _addr_linkctxt.val;

    dwctxt d = new dwctxt(linkctxt:linkctxt,ldr:linkctxt.loader,arch:linkctxt.Arch,tmap:make(map[string]loader.Sym),tdmap:make(map[loader.Sym]loader.Sym),rtmap:make(map[loader.Sym]loader.Sym),);
    d.typeRuntimeEface = d.lookupOrDiag("type.runtime.eface");
    d.typeRuntimeIface = d.lookupOrDiag("type.runtime.iface");
    return d;
}

// dwSym wraps a loader.Sym; this type is meant to obey the interface
// rules for dwarf.Sym from the cmd/internal/dwarf package. DwDie and
// DwAttr objects contain references to symbols via this type.
private partial struct dwSym { // : loader.Sym
}

private static long Length(this dwSym s, object dwarfContext) {
    dwctxt l = dwarfContext._<dwctxt>().ldr;
    return int64(len(l.Data(loader.Sym(s))));
}

private static nint PtrSize(this dwctxt c) {
    return c.arch.PtrSize;
}

private static void AddInt(this dwctxt c, dwarf.Sym s, nint size, long i) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    dsu.AddUintXX(c.arch, uint64(i), size);
}

private static void AddBytes(this dwctxt c, dwarf.Sym s, slice<byte> b) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    dsu.AddBytes(b);
}

private static void AddString(this dwctxt c, dwarf.Sym s, @string v) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    dsu.Addstring(v);
}

private static void AddAddress(this dwctxt c, dwarf.Sym s, object data, long value) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    if (value != 0) {
        value -= dsu.Value();
    }
    var tgtds = loader.Sym(data._<dwSym>());
    dsu.AddAddrPlus(c.arch, tgtds, value);
}

private static void AddCURelativeAddress(this dwctxt c, dwarf.Sym s, object data, long value) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    if (value != 0) {
        value -= dsu.Value();
    }
    var tgtds = loader.Sym(data._<dwSym>());
    dsu.AddCURelativeAddrPlus(c.arch, tgtds, value);
}

private static void AddSectionOffset(this dwctxt c, dwarf.Sym s, nint size, object t, long ofs) {
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    var tds = loader.Sym(t._<dwSym>());

    if (size == c.arch.PtrSize || size == 4)     else 
        c.linkctxt.Errorf(ds, "invalid size %d in adddwarfref\n", size);
        dsu.AddSymRef(c.arch, tds, ofs, objabi.R_ADDROFF, size);
}

private static void AddDWARFAddrSectionOffset(this dwctxt c, dwarf.Sym s, object t, long ofs) {
    nint size = 4;
    if (isDwarf64(_addr_c.linkctxt)) {
        size = 8;
    }
    var ds = loader.Sym(s._<dwSym>());
    var dsu = c.ldr.MakeSymbolUpdater(ds);
    var tds = loader.Sym(t._<dwSym>());

    if (size == c.arch.PtrSize || size == 4)     else 
        c.linkctxt.Errorf(ds, "invalid size %d in adddwarfref\n", size);
        dsu.AddSymRef(c.arch, tds, ofs, objabi.R_DWARFSECREF, size);
}

private static void Logf(this dwctxt c, @string format, params object[] args) {
    args = args.Clone();

    c.linkctxt.Logf(format, args);
}

// At the moment these interfaces are only used in the compiler.

private static void AddFileRef(this dwctxt c, dwarf.Sym s, object f) => func((_, panic, _) => {
    panic("should be used only in the compiler");
});

private static long CurrentOffset(this dwctxt c, dwarf.Sym s) => func((_, panic, _) => {
    panic("should be used only in the compiler");
});

private static void RecordDclReference(this dwctxt c, dwarf.Sym s, dwarf.Sym t, nint dclIdx, nint inlIndex) => func((_, panic, _) => {
    panic("should be used only in the compiler");
});

private static void RecordChildDieOffsets(this dwctxt c, dwarf.Sym s, slice<ptr<dwarf.Var>> vars, slice<int> offsets) => func((_, panic, _) => {
    panic("should be used only in the compiler");
});

private static bool isDwarf64(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    return ctxt.HeadType == objabi.Haix;
}

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
private partial struct dwarfSecInfo {
    public slice<loader.Sym> syms;
}

// secSym returns the section symbol for the section.
private static loader.Sym secSym(this ptr<dwarfSecInfo> _addr_dsi) {
    ref dwarfSecInfo dsi = ref _addr_dsi.val;

    if (len(dsi.syms) == 0) {
        return 0;
    }
    return dsi.syms[0];
}

// subSyms returns a list of sub-symbols for the section.
private static slice<loader.Sym> subSyms(this ptr<dwarfSecInfo> _addr_dsi) {
    ref dwarfSecInfo dsi = ref _addr_dsi.val;

    if (len(dsi.syms) == 0) {
        return new slice<loader.Sym>(new loader.Sym[] {  });
    }
    return dsi.syms[(int)1..];
}

// dwarfp stores the collected DWARF symbols created during
// dwarf generation.
private static slice<dwarfSecInfo> dwarfp = default;

private static dwarfSecInfo writeabbrev(this ptr<dwctxt> _addr_d) {
    ref dwctxt d = ref _addr_d.val;

    var abrvs = d.ldr.CreateSymForUpdate(".debug_abbrev", 0);
    abrvs.SetType(sym.SDWARFSECT);
    abrvs.AddBytes(dwarf.GetAbbrev());
    return new dwarfSecInfo(syms:[]loader.Sym{abrvs.Sym()});
}

private static dwarf.DWDie dwtypes = default;

// newattr attaches a new attribute to the specified DIE.
//
// FIXME: at the moment attributes are stored in a linked list in a
// fairly space-inefficient way -- it might be better to instead look
// up all attrs in a single large table, then store indices into the
// table in the DIE. This would allow us to common up storage for
// attributes that are shared by many DIEs (ex: byte size of N).
private static ptr<dwarf.DWAttr> newattr(ptr<dwarf.DWDie> _addr_die, ushort attr, nint cls, long value, object data) {
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
private static ptr<dwarf.DWAttr> getattr(ptr<dwarf.DWDie> _addr_die, ushort attr) {
    ref dwarf.DWDie die = ref _addr_die.val;

    if (die.Attr.Atr == attr) {
        return _addr_die.Attr!;
    }
    var a = die.Attr;
    var b = a.Link;
    while (b != null) {
        if (b.Atr == attr) {
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
private static ptr<dwarf.DWDie> newdie(this ptr<dwctxt> _addr_d, ptr<dwarf.DWDie> _addr_parent, nint abbrev, @string name, nint version) => func((_, panic, _) => {
    ref dwctxt d = ref _addr_d.val;
    ref dwarf.DWDie parent = ref _addr_parent.val;

    ptr<dwarf.DWDie> die = @new<dwarf.DWDie>();
    die.Abbrev = abbrev;
    die.Link = parent.Child;
    parent.Child = die;

    newattr(die, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len(name)), name); 

    // Sanity check: all DIEs created in the linker should have a non-empty
    // name and be version zero.
    if (name == "" || version != 0) {
        panic("nameless or version non-zero DWARF DIE");
    }
    sym.SymKind st = default;

    if (abbrev == dwarf.DW_ABRV_FUNCTYPEPARAM || abbrev == dwarf.DW_ABRV_DOTDOTDOT || abbrev == dwarf.DW_ABRV_STRUCTFIELD || abbrev == dwarf.DW_ABRV_ARRAYRANGE) 
        // There are no relocations against these dies, and their names
        // are not unique, so don't create a symbol.
        return _addr_die!;
    else if (abbrev == dwarf.DW_ABRV_COMPUNIT || abbrev == dwarf.DW_ABRV_COMPUNIT_TEXTLESS) 
        // Avoid collisions with "real" symbol names.
        name = fmt.Sprintf(".pkg.%s.%d", name, len(d.linkctxt.compUnits));
        st = sym.SDWARFCUINFO;
    else if (abbrev == dwarf.DW_ABRV_VARIABLE) 
        st = sym.SDWARFVAR;
    else 
        // Everything else is assigned a type of SDWARFTYPE. that
        // this also includes loose ends such as STRUCT_FIELD.
        st = sym.SDWARFTYPE;
        var ds = d.ldr.LookupOrCreateSym(dwarf.InfoPrefix + name, version);
    var dsu = d.ldr.MakeSymbolUpdater(ds);
    dsu.SetType(st);
    d.ldr.SetAttrNotInSymbolTable(ds, true);
    d.ldr.SetAttrReachable(ds, true);
    die.Sym = dwSym(ds);
    if (abbrev >= dwarf.DW_ABRV_NULLTYPE && abbrev <= dwarf.DW_ABRV_TYPEDECL) {
        d.tmap[name] = ds;
    }
    return _addr_die!;
});

private static ptr<dwarf.DWDie> walktypedef(ptr<dwarf.DWDie> _addr_die) {
    ref dwarf.DWDie die = ref _addr_die.val;

    if (die == null) {
        return _addr_null!;
    }
    if (die.Abbrev == dwarf.DW_ABRV_TYPEDECL) {
        {
            var attr = die.Attr;

            while (attr != null) {
                if (attr.Atr == dwarf.DW_AT_type && attr.Cls == dwarf.DW_CLS_REFERENCE && attr.Data != null) {
                    return attr.Data._<ptr<dwarf.DWDie>>();
                attr = attr.Link;
                }
            }

        }
    }
    return _addr_die!;
}

private static loader.Sym walksymtypedef(this ptr<dwctxt> _addr_d, loader.Sym symIdx) {
    ref dwctxt d = ref _addr_d.val;

    // We're being given the loader symbol for the type DIE, e.g.
    // "go.info.type.uintptr". Map that first to the type symbol (e.g.
    // "type.uintptr") and then to the typedef DIE for the type.
    // FIXME: this seems clunky, maybe there is a better way to do this.

    {
        var (ts, ok) = d.rtmap[symIdx];

        if (ok) {
            {
                var (def, ok) = d.tdmap[ts];

                if (ok) {
                    return def;
                }

            }
            d.linkctxt.Errorf(ts, "internal error: no entry for sym %d in tdmap\n", ts);
            return 0;
        }
    }
    d.linkctxt.Errorf(symIdx, "internal error: no entry for sym %d in rtmap\n", symIdx);
    return 0;
}

// Find child by AT_name using hashtable if available or linear scan
// if not.
private static ptr<dwarf.DWDie> findchild(ptr<dwarf.DWDie> _addr_die, @string name) {
    ref dwarf.DWDie die = ref _addr_die.val;

    ptr<dwarf.DWDie> prev;
    while (die != prev) {
        {
            var a = die.Child;

            while (a != null) {
                if (name == getattr(_addr_a, dwarf.DW_AT_name).Data) {
                    return _addr_a!;
                a = a.Link;
                }
        (prev, die) = (die, walktypedef(_addr_die));
            }

        }
        continue;
    }
    return _addr_null!;
}

// find looks up the loader symbol for the DWARF DIE generated for the
// type with the specified name.
private static loader.Sym find(this ptr<dwctxt> _addr_d, @string name) {
    ref dwctxt d = ref _addr_d.val;

    return d.tmap[name];
}

private static loader.Sym mustFind(this ptr<dwctxt> _addr_d, @string name) {
    ref dwctxt d = ref _addr_d.val;

    var r = d.find(name);
    if (r == 0) {
        Exitf("dwarf find: cannot find %s", name);
    }
    return r;
}

private static long adddwarfref(this ptr<dwctxt> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, loader.Sym t, nint size) {
    ref dwctxt d = ref _addr_d.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    long result = default;

    if (size == d.arch.PtrSize || size == 4)     else 
        d.linkctxt.Errorf(sb.Sym(), "invalid size %d in adddwarfref\n", size);
        result = sb.AddSymRef(d.arch, t, 0, objabi.R_DWARFSECREF, size);
    return result;
}

private static ptr<dwarf.DWAttr> newrefattr(this ptr<dwctxt> _addr_d, ptr<dwarf.DWDie> _addr_die, ushort attr, loader.Sym @ref) {
    ref dwctxt d = ref _addr_d.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    if (ref == 0) {
        return _addr_null!;
    }
    return _addr_newattr(_addr_die, attr, dwarf.DW_CLS_REFERENCE, 0, dwSym(ref))!;
}

private static loader.Sym dtolsym(this ptr<dwctxt> _addr_d, dwarf.Sym s) {
    ref dwctxt d = ref _addr_d.val;

    if (s == null) {
        return 0;
    }
    var dws = loader.Sym(s._<dwSym>());
    return dws;
}

private static slice<loader.Sym> putdie(this ptr<dwctxt> _addr_d, slice<loader.Sym> syms, ptr<dwarf.DWDie> _addr_die) {
    ref dwctxt d = ref _addr_d.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    var s = d.dtolsym(die.Sym);
    if (s == 0) {
        s = syms[len(syms) - 1];
    }
    else
 {
        syms = append(syms, s);
    }
    var sDwsym = dwSym(s);
    dwarf.Uleb128put(d, sDwsym, int64(die.Abbrev));
    dwarf.PutAttrs(d, sDwsym, die.Abbrev, die.Attr);
    if (dwarf.HasChildren(die)) {
        {
            var die = die.Child;

            while (die != null) {
                syms = d.putdie(syms, die);
                die = die.Link;
            }

        }
        var dsu = d.ldr.MakeSymbolUpdater(syms[len(syms) - 1]);
        dsu.AddUint8(0);
    }
    return syms;
}

private static void reverselist(ptr<ptr<dwarf.DWDie>> _addr_list) {
    ref ptr<dwarf.DWDie> list = ref _addr_list.val;

    ptr<ptr<dwarf.DWDie>> curr = list.val;
    ptr<dwarf.DWDie> prev;
    while (curr != null) {
        var next = curr.Link;
        curr.Link = prev;
        prev = addr(curr);
        curr = next;
    }

    list.val = addr(prev);
}

private static void reversetree(ptr<ptr<dwarf.DWDie>> _addr_list) {
    ref ptr<dwarf.DWDie> list = ref _addr_list.val;

    reverselist(_addr_list);
    {
        ptr<ptr<dwarf.DWDie>> die = list.val;

        while (die != null) {
            if (dwarf.HasChildren(die)) {
                reversetree(_addr_die.Child);
            die = die.Link;
            }
        }
    }
}

private static void newmemberoffsetattr(ptr<dwarf.DWDie> _addr_die, int offs) {
    ref dwarf.DWDie die = ref _addr_die.val;

    newattr(_addr_die, dwarf.DW_AT_data_member_location, dwarf.DW_CLS_CONSTANT, int64(offs), null);
}

private static loader.Sym lookupOrDiag(this ptr<dwctxt> _addr_d, @string n) {
    ref dwctxt d = ref _addr_d.val;

    var symIdx = d.ldr.Lookup(n, 0);
    if (symIdx == 0) {
        Exitf("dwarf: missing type: %s", n);
    }
    if (len(d.ldr.Data(symIdx)) == 0) {
        Exitf("dwarf: missing type (no data): %s", n);
    }
    return symIdx;
}

private static ptr<dwarf.DWDie> dotypedef(this ptr<dwctxt> _addr_d, ptr<dwarf.DWDie> _addr_parent, loader.Sym gotype, @string name, ptr<dwarf.DWDie> _addr_def) {
    ref dwctxt d = ref _addr_d.val;
    ref dwarf.DWDie parent = ref _addr_parent.val;
    ref dwarf.DWDie def = ref _addr_def.val;
 
    // Only emit typedefs for real names.
    if (strings.HasPrefix(name, "map[")) {
        return _addr_null!;
    }
    if (strings.HasPrefix(name, "struct {")) {
        return _addr_null!;
    }
    if (strings.HasPrefix(name, "chan ")) {
        return _addr_null!;
    }
    if (name[0] == '[' || name[0] == '*') {
        return _addr_null!;
    }
    if (def == null) {
        Errorf(null, "dwarf: bad def in dotypedef");
    }
    var tds = d.ldr.CreateExtSym("", 0);
    var tdsu = d.ldr.MakeSymbolUpdater(tds);
    tdsu.SetType(sym.SDWARFTYPE);
    def.Sym = dwSym(tds);
    d.ldr.SetAttrNotInSymbolTable(tds, true);
    d.ldr.SetAttrReachable(tds, true); 

    // The typedef entry must be created after the def,
    // so that future lookups will find the typedef instead
    // of the real definition. This hooks the typedef into any
    // circular definition loops, so that gdb can understand them.
    var die = d.newdie(parent, dwarf.DW_ABRV_TYPEDECL, name, 0);

    d.newrefattr(die, dwarf.DW_AT_type, tds);

    return _addr_die!;
}

// Define gotype, for composite ones recurse into constituents.
private static loader.Sym defgotype(this ptr<dwctxt> _addr_d, loader.Sym gotype) {
    ref dwctxt d = ref _addr_d.val;

    if (gotype == 0) {
        return d.mustFind("<unspecified>");
    }
    {
        var (ds, ok) = d.tdmap[gotype];

        if (ok) {
            return ds;
        }
    }

    var sn = d.ldr.SymName(gotype);
    if (!strings.HasPrefix(sn, "type.")) {
        d.linkctxt.Errorf(gotype, "dwarf: type name doesn't start with \"type.\"");
        return d.mustFind("<unspecified>");
    }
    var name = sn[(int)5..]; // could also decode from Type.string

    var sdie = d.find(name);
    if (sdie != 0) {
        return sdie;
    }
    var gtdwSym = d.newtype(gotype);
    d.tdmap[gotype] = loader.Sym(gtdwSym.Sym._<dwSym>());
    return loader.Sym(gtdwSym.Sym._<dwSym>());
}

private static ptr<dwarf.DWDie> newtype(this ptr<dwctxt> _addr_d, loader.Sym gotype) {
    ref dwctxt d = ref _addr_d.val;

    var sn = d.ldr.SymName(gotype);
    var name = sn[(int)5..]; // could also decode from Type.string
    var tdata = d.ldr.Data(gotype);
    var kind = decodetypeKind(d.arch, tdata);
    var bytesize = decodetypeSize(d.arch, tdata);

    ptr<dwarf.DWDie> die;    ptr<dwarf.DWDie> typedefdie;


    if (kind == objabi.KindBool) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0);
        newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_boolean, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindInt || kind == objabi.KindInt8 || kind == objabi.KindInt16 || kind == objabi.KindInt32 || kind == objabi.KindInt64) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0);
        newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_signed, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindUint || kind == objabi.KindUint8 || kind == objabi.KindUint16 || kind == objabi.KindUint32 || kind == objabi.KindUint64 || kind == objabi.KindUintptr) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0);
        newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindFloat32 || kind == objabi.KindFloat64) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0);
        newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_float, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindComplex64 || kind == objabi.KindComplex128) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BASETYPE, name, 0);
        newattr(die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_complex_float, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindArray) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_ARRAYTYPE, name, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
        var s = decodetypeArrayElem(d.ldr, d.arch, gotype);
        d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
        var fld = d.newdie(die, dwarf.DW_ABRV_ARRAYRANGE, "range", 0); 

        // use actual length not upper bound; correct for 0-length arrays.
        newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, decodetypeArrayLen(d.ldr, d.arch, gotype), 0);

        d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
    else if (kind == objabi.KindChan) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_CHANTYPE, name, 0);
        s = decodetypeChanElem(d.ldr, d.arch, gotype);
        d.newrefattr(die, dwarf.DW_AT_go_elem, d.defgotype(s)); 
        // Save elem type for synthesizechantypes. We could synthesize here
        // but that would change the order of DIEs we output.
        d.newrefattr(die, dwarf.DW_AT_type, s);
    else if (kind == objabi.KindFunc) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_FUNCTYPE, name, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        var data = d.ldr.Data(gotype); 
        // FIXME: add caching or reuse reloc slice.
        ref var relocs = ref heap(d.ldr.Relocs(gotype), out ptr<var> _addr_relocs);
        var nfields = decodetypeFuncInCount(d.arch, data);
        {
            nint i__prev1 = i;

            for (nint i = 0; i < nfields; i++) {
                s = decodetypeFuncInType(d.ldr, d.arch, gotype, _addr_relocs, i);
                sn = d.ldr.SymName(s);
                fld = d.newdie(die, dwarf.DW_ABRV_FUNCTYPEPARAM, sn[(int)5..], 0);
                d.newrefattr(fld, dwarf.DW_AT_type, d.defgotype(s));
            }


            i = i__prev1;
        }

        if (decodetypeFuncDotdotdot(d.arch, data)) {
            d.newdie(die, dwarf.DW_ABRV_DOTDOTDOT, "...", 0);
        }
        nfields = decodetypeFuncOutCount(d.arch, data);
        {
            nint i__prev1 = i;

            for (i = 0; i < nfields; i++) {
                s = decodetypeFuncOutType(d.ldr, d.arch, gotype, _addr_relocs, i);
                sn = d.ldr.SymName(s);
                fld = d.newdie(die, dwarf.DW_ABRV_FUNCTYPEPARAM, sn[(int)5..], 0);
                d.newrefattr(fld, dwarf.DW_AT_type, d.defptrto(d.defgotype(s)));
            }


            i = i__prev1;
        }
    else if (kind == objabi.KindInterface) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_IFACETYPE, name, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        data = d.ldr.Data(gotype);
        nfields = int(decodetypeIfaceMethodCount(d.arch, data));
        s = default;
        if (nfields == 0) {
            s = d.typeRuntimeEface;
        }
        else
 {
            s = d.typeRuntimeIface;
        }
        d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
    else if (kind == objabi.KindMap) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_MAPTYPE, name, 0);
        s = decodetypeMapKey(d.ldr, d.arch, gotype);
        d.newrefattr(die, dwarf.DW_AT_go_key, d.defgotype(s));
        s = decodetypeMapValue(d.ldr, d.arch, gotype);
        d.newrefattr(die, dwarf.DW_AT_go_elem, d.defgotype(s)); 
        // Save gotype for use in synthesizemaptypes. We could synthesize here,
        // but that would change the order of the DIEs.
        d.newrefattr(die, dwarf.DW_AT_type, gotype);
    else if (kind == objabi.KindPtr) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, name, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        s = decodetypePtrElem(d.ldr, d.arch, gotype);
        d.newrefattr(die, dwarf.DW_AT_type, d.defgotype(s));
    else if (kind == objabi.KindSlice) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_SLICETYPE, name, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
        s = decodetypeArrayElem(d.ldr, d.arch, gotype);
        var elem = d.defgotype(s);
        d.newrefattr(die, dwarf.DW_AT_go_elem, elem);
    else if (kind == objabi.KindString) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_STRINGTYPE, name, 0);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
    else if (kind == objabi.KindStruct) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_STRUCTTYPE, name, 0);
        typedefdie = d.dotypedef(_addr_dwtypes, gotype, name, die);
        newattr(die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, bytesize, 0);
        nfields = decodetypeStructFieldCount(d.ldr, d.arch, gotype);
        {
            nint i__prev1 = i;

            for (i = 0; i < nfields; i++) {
                var f = decodetypeStructFieldName(d.ldr, d.arch, gotype, i);
                s = decodetypeStructFieldType(d.ldr, d.arch, gotype, i);
                if (f == "") {
                    sn = d.ldr.SymName(s);
                    f = sn[(int)5..]; // skip "type."
                }
                fld = d.newdie(die, dwarf.DW_ABRV_STRUCTFIELD, f, 0);
                d.newrefattr(fld, dwarf.DW_AT_type, d.defgotype(s));
                var offsetAnon = decodetypeStructFieldOffsAnon(d.ldr, d.arch, gotype, i);
                newmemberoffsetattr(_addr_fld, int32(offsetAnon >> 1));
                if (offsetAnon & 1 != 0) { // is embedded field
                    newattr(_addr_fld, dwarf.DW_AT_go_embedded_field, dwarf.DW_CLS_FLAG, 1, 0);
                }
            }


            i = i__prev1;
        }
    else if (kind == objabi.KindUnsafePointer) 
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_BARE_PTRTYPE, name, 0);
    else 
        d.linkctxt.Errorf(gotype, "dwarf: definition of unknown kind %d", kind);
        die = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_TYPEDECL, name, 0);
        d.newrefattr(die, dwarf.DW_AT_type, d.mustFind("<unspecified>"));
        newattr(die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, int64(kind), 0);

    if (d.ldr.AttrReachable(gotype)) {
        newattr(die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0, dwSym(gotype));
    }
    {
        var (_, ok) = d.rtmap[gotype];

        if (ok) {
            log.Fatalf("internal error: rtmap entry already installed\n");
        }
    }

    var ds = loader.Sym(die.Sym._<dwSym>());
    if (typedefdie != null) {
        ds = loader.Sym(typedefdie.Sym._<dwSym>());
    }
    d.rtmap[ds] = gotype;

    {
        (_, ok) = prototypedies[sn];

        if (ok) {
            prototypedies[sn] = die;
        }
    }

    if (typedefdie != null) {
        return _addr_typedefdie!;
    }
    return _addr_die!;
}

private static @string nameFromDIESym(this ptr<dwctxt> _addr_d, loader.Sym dwtypeDIESym) {
    ref dwctxt d = ref _addr_d.val;

    var sn = d.ldr.SymName(dwtypeDIESym);
    return sn[(int)len(dwarf.InfoPrefix)..];
}

private static loader.Sym defptrto(this ptr<dwctxt> _addr_d, loader.Sym dwtype) {
    ref dwctxt d = ref _addr_d.val;

    // FIXME: it would be nice if the compiler attached an aux symbol
    // ref from the element type to the pointer type -- it would be
    // more efficient to do it this way as opposed to via name lookups.

    @string ptrname = "*" + d.nameFromDIESym(dwtype);
    {
        var die = d.find(ptrname);

        if (die != 0) {
            return die;
        }
    }

    var pdie = d.newdie(_addr_dwtypes, dwarf.DW_ABRV_PTRTYPE, ptrname, 0);
    d.newrefattr(pdie, dwarf.DW_AT_type, dwtype); 

    // The DWARF info synthesizes pointer types that don't exist at the
    // language level, like *hash<...> and *bucket<...>, and the data
    // pointers of slices. Link to the ones we can find.
    var gts = d.ldr.Lookup("type." + ptrname, 0);
    if (gts != 0 && d.ldr.AttrReachable(gts)) {
        newattr(_addr_pdie, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_GO_TYPEREF, 0, dwSym(gts));
    }
    if (gts != 0) {
        var ds = loader.Sym(pdie.Sym._<dwSym>());
        d.rtmap[ds] = gts;
        d.tdmap[gts] = ds;
    }
    return d.dtolsym(pdie.Sym);
}

// Copies src's children into dst. Copies attributes by value.
// DWAttr.data is copied as pointer only. If except is one of
// the top-level children, it will not be copied.
private static void copychildrenexcept(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src, ptr<dwarf.DWDie> _addr_except) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie dst = ref _addr_dst.val;
    ref dwarf.DWDie src = ref _addr_src.val;
    ref dwarf.DWDie except = ref _addr_except.val;

    src = src.Child;

    while (src != null) {
        if (src == except) {
            continue;
        src = src.Link;
        }
        var c = d.newdie(dst, src.Abbrev, getattr(_addr_src, dwarf.DW_AT_name).Data._<@string>(), 0);
        {
            var a = src.Attr;

            while (a != null) {
                newattr(_addr_c, a.Atr, int(a.Cls), a.Value, a.Data);
                a = a.Link;
            }

        }
        d.copychildrenexcept(ctxt, c, src, null);
    }

    reverselist(_addr_dst.Child);
}

private static void copychildren(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_dst, ptr<dwarf.DWDie> _addr_src) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie dst = ref _addr_dst.val;
    ref dwarf.DWDie src = ref _addr_src.val;

    d.copychildrenexcept(ctxt, dst, src, null);
}

// Search children (assumed to have TAG_member) for the one named
// field and set its AT_type to dwtype
private static void substitutetype(this ptr<dwctxt> _addr_d, ptr<dwarf.DWDie> _addr_structdie, @string field, loader.Sym dwtype) {
    ref dwctxt d = ref _addr_d.val;
    ref dwarf.DWDie structdie = ref _addr_structdie.val;

    var child = findchild(_addr_structdie, field);
    if (child == null) {
        Exitf("dwarf substitutetype: %s does not have member %s", getattr(_addr_structdie, dwarf.DW_AT_name).Data, field);
        return ;
    }
    var a = getattr(_addr_child, dwarf.DW_AT_type);
    if (a != null) {
        a.Data = dwSym(dwtype);
    }
    else
 {
        d.newrefattr(child, dwarf.DW_AT_type, dwtype);
    }
}

private static ptr<dwarf.DWDie> findprotodie(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, @string name) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var (die, ok) = prototypedies[name];
    if (ok && die == null) {
        d.defgotype(d.lookupOrDiag(name));
        die = prototypedies[name];
    }
    if (die == null) {
        log.Fatalf("internal error: DIE generation failed for %s\n", name);
    }
    return _addr_die!;
}

private static void synthesizestringtypes(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    var prototype = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.stringStructDWARF"));
    if (prototype == null) {
        return ;
    }
    while (die != null) {
        if (die.Abbrev != dwarf.DW_ABRV_STRINGTYPE) {
            continue;
        die = die.Link;
        }
        d.copychildren(ctxt, die, prototype);
    }
}

private static void synthesizeslicetypes(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    var prototype = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.slice"));
    if (prototype == null) {
        return ;
    }
    while (die != null) {
        if (die.Abbrev != dwarf.DW_ABRV_SLICETYPE) {
            continue;
        die = die.Link;
        }
        d.copychildren(ctxt, die, prototype);
        var elem = loader.Sym(getattr(_addr_die, dwarf.DW_AT_go_elem).Data._<dwSym>());
        d.substitutetype(die, "array", d.defptrto(elem));
    }
}

private static @string mkinternaltypename(@string @base, @string arg1, @string arg2) {
    if (arg2 == "") {
        return fmt.Sprintf("%s<%s>", base, arg1);
    }
    return fmt.Sprintf("%s<%s,%s>", base, arg1, arg2);
}

// synthesizemaptypes is way too closely married to runtime/hashmap.c
public static readonly nint MaxKeySize = 128;
public static readonly nint MaxValSize = 128;
public static readonly nint BucketSize = 8;

private static loader.Sym mkinternaltype(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, nint abbrev, @string typename, @string keyname, @string valname, Action<ptr<dwarf.DWDie>> f) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;

    var name = mkinternaltypename(typename, keyname, valname);
    var symname = dwarf.InfoPrefix + name;
    var s = d.ldr.Lookup(symname, 0);
    if (s != 0 && d.ldr.SymType(s) == sym.SDWARFTYPE) {
        return s;
    }
    var die = d.newdie(_addr_dwtypes, abbrev, name, 0);
    f(die);
    return d.dtolsym(die.Sym);
}

private static void synthesizemaptypes(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    var hash = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.hmap"));
    var bucket = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.bmap"));

    if (hash == null) {
        return ;
    }
    while (die != null) {
        if (die.Abbrev != dwarf.DW_ABRV_MAPTYPE) {
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
        (keytype, valtype) = (d.walksymtypedef(d.defgotype(keytype)), d.walksymtypedef(d.defgotype(valtype)));        var indirectKey = false;
        var indirectVal = false;
        if (keysize > MaxKeySize) {
            keysize = int64(d.arch.PtrSize);
            indirectKey = true;
        }
        if (valsize > MaxValSize) {
            valsize = int64(d.arch.PtrSize);
            indirectVal = true;
        }
        var keyname = d.nameFromDIESym(keytype);
        var dwhks = d.mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]key", keyname, "", dwhk => {
            newattr(_addr_dwhk, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * keysize, 0);
            var t = keytype;
            if (indirectKey) {
                t = d.defptrto(keytype);
            }
            d.newrefattr(dwhk, dwarf.DW_AT_type, t);
            var fld = d.newdie(dwhk, dwarf.DW_ABRV_ARRAYRANGE, "size", 0);
            newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0);
            d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
        }); 

        // Construct type to represent an array of BucketSize values
        var valname = d.nameFromDIESym(valtype);
        var dwhvs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_ARRAYTYPE, "[]val", valname, "", dwhv => {
            newattr(_addr_dwhv, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize * valsize, 0);
            t = valtype;
            if (indirectVal) {
                t = d.defptrto(valtype);
            }
            d.newrefattr(dwhv, dwarf.DW_AT_type, t);
            fld = d.newdie(dwhv, dwarf.DW_ABRV_ARRAYRANGE, "size", 0);
            newattr(_addr_fld, dwarf.DW_AT_count, dwarf.DW_CLS_CONSTANT, BucketSize, 0);
            d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
        }); 

        // Construct bucket<K,V>
        var dwhbs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "bucket", keyname, valname, dwhb => { 
            // Copy over all fields except the field "data" from the generic
            // bucket. "data" will be replaced with keys/values below.
            d.copychildrenexcept(ctxt, dwhb, bucket, findchild(_addr_bucket, "data"));

            fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "keys", 0);
            d.newrefattr(fld, dwarf.DW_AT_type, dwhks);
            newmemberoffsetattr(_addr_fld, BucketSize);
            fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "values", 0);
            d.newrefattr(fld, dwarf.DW_AT_type, dwhvs);
            newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * int32(keysize));
            fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "overflow", 0);
            d.newrefattr(fld, dwarf.DW_AT_type, d.defptrto(d.dtolsym(dwhb.Sym)));
            newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)));
            if (d.arch.RegSize > d.arch.PtrSize) {
                fld = d.newdie(dwhb, dwarf.DW_ABRV_STRUCTFIELD, "pad", 0);
                d.newrefattr(fld, dwarf.DW_AT_type, d.uintptrInfoSym);
                newmemberoffsetattr(_addr_fld, BucketSize + BucketSize * (int32(keysize) + int32(valsize)) + int32(d.arch.PtrSize));
            }
            newattr(_addr_dwhb, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, BucketSize + BucketSize * keysize + BucketSize * valsize + int64(d.arch.RegSize), 0);
        }); 

        // Construct hash<K,V>
        var dwhs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hash", keyname, valname, dwh => {
            d.copychildren(ctxt, dwh, hash);
            d.substitutetype(dwh, "buckets", d.defptrto(dwhbs));
            d.substitutetype(dwh, "oldbuckets", d.defptrto(dwhbs));
            newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hash, dwarf.DW_AT_byte_size).Value, null);
        }); 

        // make map type a pointer to hash<K,V>
        d.newrefattr(die, dwarf.DW_AT_type, d.defptrto(dwhs));
    }
}

private static void synthesizechantypes(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_die) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie die = ref _addr_die.val;

    var sudog = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.sudog"));
    var waitq = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.waitq"));
    var hchan = walktypedef(_addr_d.findprotodie(ctxt, "type.runtime.hchan"));
    if (sudog == null || waitq == null || hchan == null) {
        return ;
    }
    var sudogsize = int(getattr(_addr_sudog, dwarf.DW_AT_byte_size).Value);

    while (die != null) {
        if (die.Abbrev != dwarf.DW_ABRV_CHANTYPE) {
            continue;
        die = die.Link;
        }
        var elemgotype = loader.Sym(getattr(_addr_die, dwarf.DW_AT_type).Data._<dwSym>());
        var tname = d.ldr.SymName(elemgotype);
        var elemname = tname[(int)5..];
        var elemtype = d.walksymtypedef(d.defgotype(d.lookupOrDiag(tname))); 

        // sudog<T>
        var dwss = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "sudog", elemname, "", dws => {
            d.copychildren(ctxt, dws, sudog);
            d.substitutetype(dws, "elem", d.defptrto(elemtype));
            newattr(_addr_dws, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(sudogsize), null);
        }); 

        // waitq<T>
        var dwws = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "waitq", elemname, "", dww => {
            d.copychildren(ctxt, dww, waitq);
            d.substitutetype(dww, "first", d.defptrto(dwss));
            d.substitutetype(dww, "last", d.defptrto(dwss));
            newattr(_addr_dww, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_waitq, dwarf.DW_AT_byte_size).Value, null);
        }); 

        // hchan<T>
        var dwhs = d.mkinternaltype(ctxt, dwarf.DW_ABRV_STRUCTTYPE, "hchan", elemname, "", dwh => {
            d.copychildren(ctxt, dwh, hchan);
            d.substitutetype(dwh, "recvq", dwws);
            d.substitutetype(dwh, "sendq", dwws);
            newattr(_addr_dwh, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, getattr(_addr_hchan, dwarf.DW_AT_byte_size).Value, null);
        });

        d.newrefattr(die, dwarf.DW_AT_type, d.defptrto(dwhs));
    }
}

// createUnitLength creates the initial length field with value v and update
// offset of unit_length if needed.
private static void createUnitLength(this ptr<dwctxt> _addr_d, ptr<loader.SymbolBuilder> _addr_su, ulong v) {
    ref dwctxt d = ref _addr_d.val;
    ref loader.SymbolBuilder su = ref _addr_su.val;

    if (isDwarf64(_addr_d.linkctxt)) {
        su.AddUint32(d.arch, 0xFFFFFFFF);
    }
    d.addDwarfAddrField(su, v);
}

// addDwarfAddrField adds a DWARF field in DWARF 64bits or 32bits.
private static void addDwarfAddrField(this ptr<dwctxt> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, ulong v) {
    ref dwctxt d = ref _addr_d.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    if (isDwarf64(_addr_d.linkctxt)) {
        sb.AddUint(d.arch, v);
    }
    else
 {
        sb.AddUint32(d.arch, uint32(v));
    }
}

// addDwarfAddrRef adds a DWARF pointer in DWARF 64bits or 32bits.
private static void addDwarfAddrRef(this ptr<dwctxt> _addr_d, ptr<loader.SymbolBuilder> _addr_sb, loader.Sym t) {
    ref dwctxt d = ref _addr_d.val;
    ref loader.SymbolBuilder sb = ref _addr_sb.val;

    if (isDwarf64(_addr_d.linkctxt)) {
        d.adddwarfref(sb, t, 8);
    }
    else
 {
        d.adddwarfref(sb, t, 4);
    }
}

// calcCompUnitRanges calculates the PC ranges of the compilation units.
private static void calcCompUnitRanges(this ptr<dwctxt> _addr_d) {
    ref dwctxt d = ref _addr_d.val;

    ptr<sym.CompilationUnit> prevUnit;
    foreach (var (_, s) in d.linkctxt.Textp) {
        var sym = loader.Sym(s);

        var fi = d.ldr.FuncInfo(sym);
        if (!fi.Valid()) {
            continue;
        }
        var unit = d.ldr.SymUnit(sym);
        if (unit == null) {
            continue;
        }
        var sval = d.ldr.SymValue(sym);
        var u0val = d.ldr.SymValue(loader.Sym(unit.Textp[0]));
        if (prevUnit != unit) {
            unit.PCs = append(unit.PCs, new dwarf.Range(Start:sval-u0val));
            prevUnit = unit;
        }
        unit.PCs[len(unit.PCs) - 1].End = sval - u0val + int64(len(d.ldr.Data(sym)));
    }
}

private static void movetomodule(ptr<Link> _addr_ctxt, ptr<dwarf.DWDie> _addr_parent) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref dwarf.DWDie parent = ref _addr_parent.val;

    var die = ctxt.runtimeCU.DWInfo.Child;
    if (die == null) {
        ctxt.runtimeCU.DWInfo.Child = parent.Child;
        return ;
    }
    while (die.Link != null) {
        die = die.Link;
    }
    die.Link = parent.Child;
}

/*
 * Generate a sequence of opcodes that is as short as possible.
 * See section 6.2.5
 */
public static readonly nint LINE_BASE = -4;
public static readonly nint LINE_RANGE = 10;
public static readonly nint PC_RANGE = (255 - OPCODE_BASE) / LINE_RANGE;
public static readonly nint OPCODE_BASE = 11;

/*
 * Walk prog table, emit line program and build DIE tree.
 */

private static @string getCompilationDir() { 
    // OSX requires this be set to something, but it's not easy to choose
    // a value. Linking takes place in a temporary directory, so there's
    // no point including it here. Paths in the file table are usually
    // absolute, in which case debuggers will ignore this value. -trimpath
    // produces relative paths, but we don't know where they start, so
    // all we can do here is try not to make things worse.
    return ".";
}

private static void importInfoSymbol(this ptr<dwctxt> _addr_d, loader.Sym dsym) {
    ref dwctxt d = ref _addr_d.val;

    d.ldr.SetAttrReachable(dsym, true);
    d.ldr.SetAttrNotInSymbolTable(dsym, true);
    var dst = d.ldr.SymType(dsym);
    if (dst != sym.SDWARFCONST && dst != sym.SDWARFABSFCN) {
        log.Fatalf("error: DWARF info sym %d/%s with incorrect type %s", dsym, d.ldr.SymName(dsym), d.ldr.SymType(dsym).String());
    }
    var relocs = d.ldr.Relocs(dsym);
    for (nint i = 0; i < relocs.Count(); i++) {
        var r = relocs.At(i);
        if (r.Type() != objabi.R_DWARFSECREF) {
            continue;
        }
        var rsym = r.Sym(); 
        // If there is an entry for the symbol in our rtmap, then it
        // means we've processed the type already, and can skip this one.
        {
            var (_, ok) = d.rtmap[rsym];

            if (ok) { 
                // type already generated
                continue;
            } 
            // FIXME: is there a way we could avoid materializing the
            // symbol name here?

        } 
        // FIXME: is there a way we could avoid materializing the
        // symbol name here?
        var sn = d.ldr.SymName(rsym);
        var tn = sn[(int)len(dwarf.InfoPrefix)..];
        var ts = d.ldr.Lookup("type." + tn, 0);
        d.defgotype(ts);
    }
}

private static @string expandFile(@string fname) {
    if (strings.HasPrefix(fname, src.FileSymPrefix)) {
        fname = fname[(int)len(src.FileSymPrefix)..];
    }
    return expandGoroot(fname);
}

// writeDirFileTables emits the portion of the DWARF line table
// prologue containing the include directories and file names,
// described in section 6.2.4 of the DWARF 4 standard. It walks the
// filepaths for the unit to discover any common directories, which
// are emitted to the directory table first, then the file table is
// emitted after that.
private static void writeDirFileTables(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_unit, ptr<loader.SymbolBuilder> _addr_lsu) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit unit = ref _addr_unit.val;
    ref loader.SymbolBuilder lsu = ref _addr_lsu.val;

    private partial struct fileDir {
        public @string @base;
        public nint dir;
    }
    var dirNums = make_map<@string, nint>();
    @string dirs = new slice<@string>(new @string[] { "" });
    fileDir files = new slice<fileDir>(new fileDir[] {  }); 

    // Preprocess files to collect directories. This assumes that the
    // file table is already de-duped.
    {
        var i__prev1 = i;
        var name__prev1 = name;

        foreach (var (__i, __name) in unit.FileTable) {
            i = __i;
            name = __name;
            var name = expandFile(name);
            if (len(name) == 0) { 
                // Can't have empty filenames, and having a unique
                // filename is quite useful for debugging.
                name = fmt.Sprintf("<missing>_%d", i);
            } 
            // Note the use of "path" here and not "filepath". The compiler
            // hard-codes to use "/" in DWARF paths (even for Windows), so we
            // want to maintain that here.
            var file = path.Base(name);
            var dir = path.Dir(name);
            var (dirIdx, ok) = dirNums[dir];
            if (!ok && dir != ".") {
                dirIdx = len(dirNums) + 1;
                dirNums[dir] = dirIdx;
                dirs = append(dirs, dir);
            }
            files = append(files, new fileDir(base:file,dir:dirIdx)); 

            // We can't use something that may be dead-code
            // eliminated from a binary here. proc.go contains
            // main and the scheduler, so it's not going anywhere.
            {
                var i__prev1 = i;

                var i = strings.Index(name, "runtime/proc.go");

                if (i >= 0) {
                    d.dwmu.Lock();
                    if (gdbscript == "") {
                        var k = strings.Index(name, "runtime/proc.go");
                        gdbscript = name[..(int)k] + "runtime/runtime-gdb.py";
                    }
                    d.dwmu.Unlock();
                }

                i = i__prev1;

            }
        }
        i = i__prev1;
        name = name__prev1;
    }

    var lsDwsym = dwSym(lsu.Sym());
    {
        var k__prev1 = k;

        for (k = 1; k < len(dirs); k++) {
            d.AddString(lsDwsym, dirs[k]);
        }

        k = k__prev1;
    }
    lsu.AddUint8(0); // terminator

    // Emit file section.
    {
        var k__prev1 = k;

        for (k = 0; k < len(files); k++) {
            d.AddString(lsDwsym, files[k].@base);
            dwarf.Uleb128put(d, lsDwsym, int64(files[k].dir));
            lsu.AddUint8(0); // mtime
            lsu.AddUint8(0); // length
        }

        k = k__prev1;
    }
    lsu.AddUint8(0); // terminator
}

// writelines collects up and chains together the symbols needed to
// form the DWARF line table for the specified compilation unit,
// returning a list of symbols. The returned list will include an
// initial symbol containing the line table header and prologue (with
// file table), then a series of compiler-emitted line table symbols
// (one per live function), and finally an epilog symbol containing an
// end-of-sequence operator. The prologue and epilog symbols are passed
// in (having been created earlier); here we add content to them.
private static slice<loader.Sym> writelines(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_unit, loader.Sym lineProlog) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit unit = ref _addr_unit.val;

    var is_stmt = uint8(1); // initially = recommended default_is_stmt = 1, tracks is_stmt toggles.

    var unitstart = int64(-1);
    var headerstart = int64(-1);
    var headerend = int64(-1);

    var syms = make_slice<loader.Sym>(0, len(unit.Textp) + 2);
    syms = append(syms, lineProlog);
    var lsu = d.ldr.MakeSymbolUpdater(lineProlog);
    var lsDwsym = dwSym(lineProlog);
    newattr(_addr_unit.DWInfo, dwarf.DW_AT_stmt_list, dwarf.DW_CLS_PTR, 0, lsDwsym); 

    // Write .debug_line Line Number Program Header (sec 6.2.4)
    // Fields marked with (*) must be changed for 64-bit dwarf
    var unitLengthOffset = lsu.Size();
    d.createUnitLength(lsu, 0); // unit_length (*), filled in at end
    unitstart = lsu.Size();
    lsu.AddUint16(d.arch, 2); // dwarf version (appendix F) -- version 3 is incompatible w/ XCode 9.0's dsymutil, latest supported on OSX 10.12 as of 2018-05
    var headerLengthOffset = lsu.Size();
    d.addDwarfAddrField(lsu, 0); // header_length (*), filled in at end
    headerstart = lsu.Size(); 

    // cpos == unitstart + 4 + 2 + 4
    lsu.AddUint8(1); // minimum_instruction_length
    lsu.AddUint8(is_stmt); // default_is_stmt
    lsu.AddUint8(LINE_BASE & 0xFF); // line_base
    lsu.AddUint8(LINE_RANGE); // line_range
    lsu.AddUint8(OPCODE_BASE); // opcode_base
    lsu.AddUint8(0); // standard_opcode_lengths[1]
    lsu.AddUint8(1); // standard_opcode_lengths[2]
    lsu.AddUint8(1); // standard_opcode_lengths[3]
    lsu.AddUint8(1); // standard_opcode_lengths[4]
    lsu.AddUint8(1); // standard_opcode_lengths[5]
    lsu.AddUint8(0); // standard_opcode_lengths[6]
    lsu.AddUint8(0); // standard_opcode_lengths[7]
    lsu.AddUint8(0); // standard_opcode_lengths[8]
    lsu.AddUint8(1); // standard_opcode_lengths[9]
    lsu.AddUint8(0); // standard_opcode_lengths[10]

    // Call helper to emit dir and file sections.
    d.writeDirFileTables(unit, lsu); 

    // capture length at end of file names.
    headerend = lsu.Size();
    var unitlen = lsu.Size() - unitstart; 

    // Output the state machine for each function remaining.
    foreach (var (_, s) in unit.Textp) {
        var fnSym = loader.Sym(s);
        var (_, _, _, lines) = d.ldr.GetFuncDwarfAuxSyms(fnSym); 

        // Chain the line symbol onto the list.
        if (lines != 0) {
            syms = append(syms, lines);
            unitlen += int64(len(d.ldr.Data(lines)));
        }
    }    if (d.linkctxt.HeadType == objabi.Haix) {
        addDwsectCUSize(".debug_line", unit.Lib.Pkg, uint64(unitlen));
    }
    if (isDwarf64(_addr_d.linkctxt)) {
        lsu.SetUint(d.arch, unitLengthOffset + 4, uint64(unitlen)); // +4 because of 0xFFFFFFFF
        lsu.SetUint(d.arch, headerLengthOffset, uint64(headerend - headerstart));
    }
    else
 {
        lsu.SetUint32(d.arch, unitLengthOffset, uint32(unitlen));
        lsu.SetUint32(d.arch, headerLengthOffset, uint32(headerend - headerstart));
    }
    return syms;
}

// writepcranges generates the DW_AT_ranges table for compilation unit
// "unit", and returns a collection of ranges symbols (one for the
// compilation unit DIE itself and the remainder from functions in the unit).
private static slice<loader.Sym> writepcranges(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_unit, loader.Sym @base, slice<dwarf.Range> pcs, loader.Sym rangeProlog) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit unit = ref _addr_unit.val;

    var syms = make_slice<loader.Sym>(0, len(unit.RangeSyms) + 1);
    syms = append(syms, rangeProlog);
    var rsu = d.ldr.MakeSymbolUpdater(rangeProlog);
    var rDwSym = dwSym(rangeProlog); 

    // Create PC ranges for the compilation unit DIE.
    newattr(_addr_unit.DWInfo, dwarf.DW_AT_ranges, dwarf.DW_CLS_PTR, rsu.Size(), rDwSym);
    newattr(_addr_unit.DWInfo, dwarf.DW_AT_low_pc, dwarf.DW_CLS_ADDRESS, 0, dwSym(base));
    dwarf.PutBasedRanges(d, rDwSym, pcs); 

    // Collect up the ranges for functions in the unit.
    var rsize = uint64(rsu.Size());
    foreach (var (_, ls) in unit.RangeSyms) {
        var s = loader.Sym(ls);
        syms = append(syms, s);
        rsize += uint64(d.ldr.SymSize(s));
    }    if (d.linkctxt.HeadType == objabi.Haix) {
        addDwsectCUSize(".debug_ranges", unit.Lib.Pkg, rsize);
    }
    return syms;
}

/*
 *  Emit .debug_frame
 */
private static readonly nint dataAlignmentFactor = -4;

// appendPCDeltaCFA appends per-PC CFA deltas to b and returns the final slice.
private static slice<byte> appendPCDeltaCFA(ptr<sys.Arch> _addr_arch, slice<byte> b, long deltapc, long cfa) {
    ref sys.Arch arch = ref _addr_arch.val;

    b = append(b, dwarf.DW_CFA_def_cfa_offset_sf);
    b = dwarf.AppendSleb128(b, cfa / dataAlignmentFactor);


    if (deltapc < 0x40) 
        b = append(b, uint8(dwarf.DW_CFA_advance_loc + deltapc));
    else if (deltapc < 0x100) 
        b = append(b, dwarf.DW_CFA_advance_loc1);
        b = append(b, uint8(deltapc));
    else if (deltapc < 0x10000) 
        b = append(b, dwarf.DW_CFA_advance_loc2, 0, 0);
        arch.ByteOrder.PutUint16(b[(int)len(b) - 2..], uint16(deltapc));
    else 
        b = append(b, dwarf.DW_CFA_advance_loc4, 0, 0, 0, 0);
        arch.ByteOrder.PutUint32(b[(int)len(b) - 4..], uint32(deltapc));
        return b;
}

private static dwarfSecInfo writeframes(this ptr<dwctxt> _addr_d, loader.Sym fs) {
    ref dwctxt d = ref _addr_d.val;

    var fsd = dwSym(fs);
    var fsu = d.ldr.MakeSymbolUpdater(fs);
    fsu.SetType(sym.SDWARFSECT);
    var isdw64 = isDwarf64(_addr_d.linkctxt);
    var haslr = haslinkregister(d.linkctxt); 

    // Length field is 4 bytes on Dwarf32 and 12 bytes on Dwarf64
    var lengthFieldSize = int64(4);
    if (isdw64) {
        lengthFieldSize += 8;
    }
    var cieReserve = uint32(16);
    if (haslr) {
        cieReserve = 32;
    }
    if (isdw64) {
        cieReserve += 4; // 4 bytes added for cid
    }
    d.createUnitLength(fsu, uint64(cieReserve)); // initial length, must be multiple of thearch.ptrsize
    d.addDwarfAddrField(fsu, ~uint64(0)); // cid
    fsu.AddUint8(3); // dwarf version (appendix F)
    fsu.AddUint8(0); // augmentation ""
    dwarf.Uleb128put(d, fsd, 1); // code_alignment_factor
    dwarf.Sleb128put(d, fsd, dataAlignmentFactor); // all CFI offset calculations include multiplication with this factor
    dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr)); // return_address_register

    fsu.AddUint8(dwarf.DW_CFA_def_cfa); // Set the current frame address..
    dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfregsp)); // ...to use the value in the platform's SP register (defined in l.go)...
    if (haslr) {
        dwarf.Uleb128put(d, fsd, int64(0)); // ...plus a 0 offset.

        fsu.AddUint8(dwarf.DW_CFA_same_value); // The platform's link register is unchanged during the prologue.
        dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr));

        fsu.AddUint8(dwarf.DW_CFA_val_offset); // The previous value...
        dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfregsp)); // ...of the platform's SP register...
        dwarf.Uleb128put(d, fsd, int64(0)); // ...is CFA+0.
    }
    else
 {
        dwarf.Uleb128put(d, fsd, int64(d.arch.PtrSize)); // ...plus the word size (because the call instruction implicitly adds one word to the frame).

        fsu.AddUint8(dwarf.DW_CFA_offset_extended); // The previous value...
        dwarf.Uleb128put(d, fsd, int64(thearch.Dwarfreglr)); // ...of the return address...
        dwarf.Uleb128put(d, fsd, int64(-d.arch.PtrSize) / dataAlignmentFactor); // ...is saved at [CFA - (PtrSize/4)].
    }
    var pad = int64(cieReserve) + lengthFieldSize - int64(len(d.ldr.Data(fs)));

    if (pad < 0) {
        Exitf("dwarf: cieReserve too small by %d bytes.", -pad);
    }
    var internalExec = d.linkctxt.BuildMode == BuildModeExe && d.linkctxt.IsInternal();
    var addAddrPlus = loader.GenAddAddrPlusFunc(internalExec);

    fsu.AddBytes(zeros[..(int)pad]);

    slice<byte> deltaBuf = default;
    var pcsp = obj.NewPCIter(uint32(d.arch.MinLC));
    foreach (var (_, s) in d.linkctxt.Textp) {
        var fn = loader.Sym(s);
        var fi = d.ldr.FuncInfo(fn);
        if (!fi.Valid()) {
            continue;
        }
        var fpcsp = fi.Pcsp(); 

        // Emit a FDE, Section 6.4.1.
        // First build the section contents into a byte buffer.
        deltaBuf = deltaBuf[..(int)0];
        if (haslr && fi.TopFrame()) { 
            // Mark the link register as having an undefined value.
            // This stops call stack unwinders progressing any further.
            // TODO: similar mark on non-LR architectures.
            deltaBuf = append(deltaBuf, dwarf.DW_CFA_undefined);
            deltaBuf = dwarf.AppendUleb128(deltaBuf, uint64(thearch.Dwarfreglr));
        }
        pcsp.Init(d.linkctxt.loader.Data(fpcsp));

        while (!pcsp.Done) {
            var nextpc = pcsp.NextPC; 

            // pciterinit goes up to the end of the function,
            // but DWARF expects us to stop just before the end.
            if (int64(nextpc) == int64(len(d.ldr.Data(fn)))) {
                nextpc--;
                if (nextpc < pcsp.PC) {
                    continue;
            pcsp.Next();
                }
            }
            var spdelta = int64(pcsp.Value);
            if (!haslr) { 
                // Return address has been pushed onto stack.
                spdelta += int64(d.arch.PtrSize);
            }
            if (haslr && !fi.TopFrame()) { 
                // TODO(bryanpkc): This is imprecise. In general, the instruction
                // that stores the return address to the stack frame is not the
                // same one that allocates the frame.
                if (pcsp.Value > 0) { 
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
        deltaBuf = append(deltaBuf, zeros[..(int)pad]); 

        // Emit the FDE header, Section 6.4.1.
        //    4 bytes: length, must be multiple of thearch.ptrsize
        //    4/8 bytes: Pointer to the CIE above, at offset 0
        //    ptrsize: initial location
        //    ptrsize: address range

        var fdeLength = uint64(4 + 2 * d.arch.PtrSize + len(deltaBuf));
        if (isdw64) {
            fdeLength += 4; // 4 bytes added for CIE pointer
        }
        d.createUnitLength(fsu, fdeLength);

        if (d.linkctxt.LinkMode == LinkExternal) {
            d.addDwarfAddrRef(fsu, fs);
        }
        else
 {
            d.addDwarfAddrField(fsu, 0); // CIE offset
        }
        addAddrPlus(fsu, d.arch, s, 0);
        fsu.AddUintXX(d.arch, uint64(len(d.ldr.Data(fn))), d.arch.PtrSize); // address range
        fsu.AddBytes(deltaBuf);

        if (d.linkctxt.HeadType == objabi.Haix) {
            addDwsectCUSize(".debug_frame", d.ldr.SymPkg(fn), fdeLength + uint64(lengthFieldSize));
        }
    }    return new dwarfSecInfo(syms:[]loader.Sym{fs});
}

/*
 *  Walk DWarfDebugInfoEntries, and emit .debug_info
 */

public static readonly nint COMPUNITHEADERSIZE = 4 + 2 + 4 + 1;

// appendSyms appends the syms from 'src' into 'syms' and returns the
// result. This can go away once we do away with sym.LoaderSym
// entirely.
private static slice<loader.Sym> appendSyms(slice<loader.Sym> syms, slice<sym.LoaderSym> src) {
    foreach (var (_, s) in src) {
        syms = append(syms, loader.Sym(s));
    }    return syms;
}

private static slice<loader.Sym> writeUnitInfo(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_u, loader.Sym abbrevsym, loader.Sym infoEpilog) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit u = ref _addr_u.val;

    loader.Sym syms = new slice<loader.Sym>(new loader.Sym[] {  });
    if (len(u.Textp) == 0 && u.DWInfo.Child == null && len(u.VarDIEs) == 0) {
        return syms;
    }
    var compunit = u.DWInfo;
    var s = d.dtolsym(compunit.Sym);
    var su = d.ldr.MakeSymbolUpdater(s); 

    // Write .debug_info Compilation Unit Header (sec 7.5.1)
    // Fields marked with (*) must be changed for 64-bit dwarf
    // This must match COMPUNITHEADERSIZE above.
    d.createUnitLength(su, 0); // unit_length (*), will be filled in later.
    su.AddUint16(d.arch, 4); // dwarf version (appendix F)

    // debug_abbrev_offset (*)
    d.addDwarfAddrRef(su, abbrevsym);

    su.AddUint8(uint8(d.arch.PtrSize)); // address_size

    var ds = dwSym(s);
    dwarf.Uleb128put(d, ds, int64(compunit.Abbrev));
    dwarf.PutAttrs(d, ds, compunit.Abbrev, compunit.Attr); 

    // This is an under-estimate; more will be needed for type DIEs.
    var cu = make_slice<loader.Sym>(0, len(u.AbsFnDIEs) + len(u.FuncDIEs));
    cu = append(cu, s);
    cu = appendSyms(cu, u.AbsFnDIEs);
    cu = appendSyms(cu, u.FuncDIEs);
    if (u.Consts != 0) {
        cu = append(cu, loader.Sym(u.Consts));
    }
    cu = appendSyms(cu, u.VarDIEs);
    long cusize = default;
    {
        var child__prev1 = child;

        foreach (var (_, __child) in cu) {
            child = __child;
            cusize += int64(len(d.ldr.Data(child)));
        }
        child = child__prev1;
    }

    {
        var die = compunit.Child;

        while (die != null) {
            var l = len(cu);
            var lastSymSz = int64(len(d.ldr.Data(cu[l - 1])));
            cu = d.putdie(cu, die);
            if (lastSymSz != int64(len(d.ldr.Data(cu[l - 1])))) { 
                // putdie will sometimes append directly to the last symbol of the list
                cusize = cusize - lastSymSz + int64(len(d.ldr.Data(cu[l - 1])));
            die = die.Link;
            }
            {
                var child__prev2 = child;

                foreach (var (_, __child) in cu[(int)l..]) {
                    child = __child;
                    cusize += int64(len(d.ldr.Data(child)));
                }

                child = child__prev2;
            }
        }
    }

    var culu = d.ldr.MakeSymbolUpdater(infoEpilog);
    culu.AddUint8(0); // closes compilation unit DIE
    cu = append(cu, infoEpilog);
    cusize++; 

    // Save size for AIX symbol table.
    if (d.linkctxt.HeadType == objabi.Haix) {
        addDwsectCUSize(".debug_info", d.getPkgFromCUSym(s), uint64(cusize));
    }
    if (isDwarf64(_addr_d.linkctxt)) {
        cusize -= 12; // exclude the length field.
        su.SetUint(d.arch, 4, uint64(cusize)); // 4 because of 0XFFFFFFFF
    }
    else
 {
        cusize -= 4; // exclude the length field.
        su.SetUint32(d.arch, 0, uint32(cusize));
    }
    return append(syms, cu);
}

private static dwarfSecInfo writegdbscript(this ptr<dwctxt> _addr_d) {
    ref dwctxt d = ref _addr_d.val;
 
    // TODO (aix): make it available
    if (d.linkctxt.HeadType == objabi.Haix) {
        return new dwarfSecInfo();
    }
    if (d.linkctxt.LinkMode == LinkExternal && d.linkctxt.HeadType == objabi.Hwindows && d.linkctxt.BuildMode == BuildModeCArchive) { 
        // gcc on Windows places .debug_gdb_scripts in the wrong location, which
        // causes the program not to run. See https://golang.org/issue/20183
        // Non c-archives can avoid this issue via a linker script
        // (see fix near writeGDBLinkerScript).
        // c-archive users would need to specify the linker script manually.
        // For UX it's better not to deal with this.
        return new dwarfSecInfo();
    }
    if (gdbscript == "") {
        return new dwarfSecInfo();
    }
    var gs = d.ldr.CreateSymForUpdate(".debug_gdb_scripts", 0);
    gs.SetType(sym.SDWARFSECT);

    gs.AddUint8(1); // magic 1 byte?
    gs.Addstring(gdbscript);
    return new dwarfSecInfo(syms:[]loader.Sym{gs.Sym()});
}

// FIXME: might be worth looking replacing this map with a function
// that switches based on symbol instead.

private static map<@string, ptr<dwarf.DWDie>> prototypedies = default;

private static bool dwarfEnabled(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (FlagW.val) { // disable dwarf
        return false;
    }
    if (FlagS && ctxt.HeadType != objabi.Hdarwin.val) {
        return false;
    }
    if (ctxt.HeadType == objabi.Hplan9 || ctxt.HeadType == objabi.Hjs) {
        return false;
    }
    if (ctxt.LinkMode == LinkExternal) {

        if (ctxt.IsELF)         else if (ctxt.HeadType == objabi.Hdarwin)         else if (ctxt.HeadType == objabi.Hwindows)         else if (ctxt.HeadType == objabi.Haix) 
            var (res, err) = dwarf.IsDWARFEnabledOnAIXLd(ctxt.extld());
            if (err != null) {
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
private static ptr<dwarf.DWDie> mkBuiltinType(this ptr<dwctxt> _addr_d, ptr<Link> _addr_ctxt, nint abrv, @string tname) {
    ref dwctxt d = ref _addr_d.val;
    ref Link ctxt = ref _addr_ctxt.val;
 
    // create type DIE
    var die = d.newdie(_addr_dwtypes, abrv, tname, 0); 

    // Look up type symbol.
    var gotype = d.lookupOrDiag("type." + tname); 

    // Map from die sym to type sym
    var ds = loader.Sym(die.Sym._<dwSym>());
    d.rtmap[ds] = gotype; 

    // Map from type to def sym
    d.tdmap[gotype] = ds;

    return _addr_die!;
}

// dwarfVisitFunction takes a function (text) symbol and processes the
// subprogram DIE for the function and picks up any other DIEs
// (absfns, types) that it references.
private static void dwarfVisitFunction(this ptr<dwctxt> _addr_d, loader.Sym fnSym, ptr<sym.CompilationUnit> _addr_unit) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit unit = ref _addr_unit.val;
 
    // The DWARF subprogram DIE symbol is listed as an aux sym
    // of the text (fcn) symbol, so ask the loader to retrieve it,
    // as well as the associated range symbol.
    var (infosym, _, rangesym, _) = d.ldr.GetFuncDwarfAuxSyms(fnSym);
    if (infosym == 0) {
        return ;
    }
    d.ldr.SetAttrNotInSymbolTable(infosym, true);
    d.ldr.SetAttrReachable(infosym, true);
    unit.FuncDIEs = append(unit.FuncDIEs, sym.LoaderSym(infosym));
    if (rangesym != 0) {
        d.ldr.SetAttrNotInSymbolTable(rangesym, true);
        d.ldr.SetAttrReachable(rangesym, true);
        unit.RangeSyms = append(unit.RangeSyms, sym.LoaderSym(rangesym));
    }
    var drelocs = d.ldr.Relocs(infosym);
    for (nint ri = 0; ri < drelocs.Count(); ri++) {
        var r = drelocs.At(ri); 
        // Look for "use type" relocs.
        if (r.Type() == objabi.R_USETYPE) {
            d.defgotype(r.Sym());
            continue;
        }
        if (r.Type() != objabi.R_DWARFSECREF) {
            continue;
        }
        var rsym = r.Sym();
        var rst = d.ldr.SymType(rsym); 

        // Look for abstract function references.
        if (rst == sym.SDWARFABSFCN) {
            if (!d.ldr.AttrOnList(rsym)) { 
                // abstract function
                d.ldr.SetAttrOnList(rsym, true);
                unit.AbsFnDIEs = append(unit.AbsFnDIEs, sym.LoaderSym(rsym));
                d.importInfoSymbol(rsym);
            }
            continue;
        }
        if (rst != sym.SDWARFTYPE && rst != sym.Sxxx) {
            continue;
        }
        {
            var (_, ok) = d.rtmap[rsym];

            if (ok) { 
                // type already generated
                continue;
            }

        }

        var rsn = d.ldr.SymName(rsym);
        var tn = rsn[(int)len(dwarf.InfoPrefix)..];
        var ts = d.ldr.Lookup("type." + tn, 0);
        d.defgotype(ts);
    }
}

// dwarfGenerateDebugInfo generated debug info entries for all types,
// variables and functions in the program.
// Along with dwarfGenerateDebugSyms they are the two main entry points into
// dwarf generation: dwarfGenerateDebugInfo does all the work that should be
// done before symbol names are mangled while dwarfGenerateDebugSyms does
// all the work that can only be done after addresses have been assigned to
// text symbols.
private static void dwarfGenerateDebugInfo(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!dwarfEnabled(_addr_ctxt)) {
        return ;
    }
    var d = newdwctxt(_addr_ctxt, true);

    if (ctxt.HeadType == objabi.Haix) { 
        // Initial map used to store package size for each DWARF section.
        dwsectCUSize = make_map<@string, ulong>();
    }
    newattr(_addr_dwtypes, dwarf.DW_AT_name, dwarf.DW_CLS_STRING, int64(len("dwtypes")), "dwtypes"); 

    // Unspecified type. There are no references to this in the symbol table.
    d.newdie(_addr_dwtypes, dwarf.DW_ABRV_NULLTYPE, "<unspecified>", 0); 

    // Some types that must exist to define other ones (uintptr in particular
    // is needed for array size)
    d.mkBuiltinType(ctxt, dwarf.DW_ABRV_BARE_PTRTYPE, "unsafe.Pointer");
    var die = d.mkBuiltinType(ctxt, dwarf.DW_ABRV_BASETYPE, "uintptr");
    newattr(_addr_die, dwarf.DW_AT_encoding, dwarf.DW_CLS_CONSTANT, dwarf.DW_ATE_unsigned, 0);
    newattr(_addr_die, dwarf.DW_AT_byte_size, dwarf.DW_CLS_CONSTANT, int64(d.arch.PtrSize), 0);
    newattr(_addr_die, dwarf.DW_AT_go_kind, dwarf.DW_CLS_CONSTANT, objabi.KindUintptr, 0);
    newattr(_addr_die, dwarf.DW_AT_go_runtime_type, dwarf.DW_CLS_ADDRESS, 0, dwSym(d.lookupOrDiag("type.uintptr")));

    d.uintptrInfoSym = d.mustFind("uintptr"); 

    // Prototypes needed for type synthesis.
    prototypedies = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<dwarf.DWDie>>{"type.runtime.stringStructDWARF":nil,"type.runtime.slice":nil,"type.runtime.hmap":nil,"type.runtime.bmap":nil,"type.runtime.sudog":nil,"type.runtime.waitq":nil,"type.runtime.hchan":nil,}; 

    // Needed by the prettyprinter code for interface inspection.
    foreach (var (_, typ) in new slice<@string>(new @string[] { "type.runtime._type", "type.runtime.arraytype", "type.runtime.chantype", "type.runtime.functype", "type.runtime.maptype", "type.runtime.ptrtype", "type.runtime.slicetype", "type.runtime.structtype", "type.runtime.interfacetype", "type.runtime.itab", "type.runtime.imethod" })) {
        d.defgotype(d.lookupOrDiag(typ));
    }    ref dwarf.DWDie dwroot = ref heap(out ptr<dwarf.DWDie> _addr_dwroot);
    var flagVariants = make_map<@string, bool>();

    foreach (var (_, lib) in ctxt.Library) {
        var consts = d.ldr.Lookup(dwarf.ConstInfoPrefix + lib.Pkg, 0);
        {
            var unit__prev2 = unit;

            foreach (var (_, __unit) in lib.Units) {
                unit = __unit; 
                // We drop the constants into the first CU.
                if (consts != 0) {
                    unit.Consts = sym.LoaderSym(consts);
                    d.importInfoSymbol(consts);
                    consts = 0;
                }
                ctxt.compUnits = append(ctxt.compUnits, unit); 

                // We need at least one runtime unit.
                if (unit.Lib.Pkg == "runtime") {
                    ctxt.runtimeCU = unit;
                }
                var cuabrv = dwarf.DW_ABRV_COMPUNIT;
                if (len(unit.Textp) == 0) {
                    cuabrv = dwarf.DW_ABRV_COMPUNIT_TEXTLESS;
                }
                unit.DWInfo = d.newdie(_addr_dwroot, cuabrv, unit.Lib.Pkg, 0);
                newattr(_addr_unit.DWInfo, dwarf.DW_AT_language, dwarf.DW_CLS_CONSTANT, int64(dwarf.DW_LANG_Go), 0); 
                // OS X linker requires compilation dir or absolute path in comp unit name to output debug info.
                var compDir = getCompilationDir(); 
                // TODO: Make this be the actual compilation directory, not
                // the linker directory. If we move CU construction into the
                // compiler, this should happen naturally.
                newattr(_addr_unit.DWInfo, dwarf.DW_AT_comp_dir, dwarf.DW_CLS_STRING, int64(len(compDir)), compDir);

                slice<byte> peData = default;
                {
                    var producerExtra = d.ldr.Lookup(dwarf.CUInfoPrefix + "producer." + unit.Lib.Pkg, 0);

                    if (producerExtra != 0) {
                        peData = d.ldr.Data(producerExtra);
                    }

                }
                @string producer = "Go cmd/compile " + buildcfg.Version;
                if (len(peData) > 0) { 
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
                    var pnSymIdx = d.ldr.Lookup(dwarf.CUInfoPrefix + "packagename." + unit.Lib.Pkg, 0);

                    if (pnSymIdx != 0) {
                        var pnsData = d.ldr.Data(pnSymIdx);
                        pkgname = string(pnsData);
                    }

                }
                newattr(_addr_unit.DWInfo, dwarf.DW_AT_go_package_name, dwarf.DW_CLS_STRING, int64(len(pkgname)), pkgname); 

                // Scan all functions in this compilation unit, create
                // DIEs for all referenced types, find all referenced
                // abstract functions, visit range symbols. Note that
                // Textp has been dead-code-eliminated already.
                foreach (var (_, s) in unit.Textp) {
                    d.dwarfVisitFunction(loader.Sym(s), unit);
                }
            }

            unit = unit__prev2;
        }
    }    if (checkStrictDups > 1 && len(flagVariants) > 1) {
        checkStrictDups = 1;
    }
    for (var idx = loader.Sym(1); idx < loader.Sym(d.ldr.NDef()); idx++) {
        if (!d.ldr.AttrReachable(idx) || d.ldr.AttrNotInSymbolTable(idx) || d.ldr.SymVersion(idx) >= sym.SymVerStatic) {
            continue;
        }
        var t = d.ldr.SymType(idx);

        if (t == sym.SRODATA || t == sym.SDATA || t == sym.SNOPTRDATA || t == sym.STYPE || t == sym.SBSS || t == sym.SNOPTRBSS || t == sym.STLSBSS)         else 
            continue;
        // Skip things with no type
        var gt = d.ldr.SymGoType(idx);
        if (gt == 0) {
            continue;
        }
        if (d.ldr.IsFileLocal(idx)) {
            continue;
        }
        var sn = d.ldr.SymName(idx);
        if (sn == "") { 
            // skip aux symbols
            continue;
        }
        var varDIE = d.ldr.Lookup(dwarf.InfoPrefix + sn, 0);
        if (varDIE != 0) {
            var unit = d.ldr.SymUnit(idx);
            d.defgotype(gt);
            unit.VarDIEs = append(unit.VarDIEs, sym.LoaderSym(varDIE));
        }
    }

    d.synthesizestringtypes(ctxt, dwtypes.Child);
    d.synthesizeslicetypes(ctxt, dwtypes.Child);
    d.synthesizemaptypes(ctxt, dwtypes.Child);
    d.synthesizechantypes(ctxt, dwtypes.Child);
}

// dwarfGenerateDebugSyms constructs debug_line, debug_frame, and
// debug_loc. It also writes out the debug_info section using symbols
// generated in dwarfGenerateDebugInfo2.
private static void dwarfGenerateDebugSyms(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (!dwarfEnabled(_addr_ctxt)) {
        return ;
    }
    ptr<dwctxt> d = addr(new dwctxt(linkctxt:ctxt,ldr:ctxt.loader,arch:ctxt.Arch,dwmu:new(sync.Mutex),));
    d.dwarfGenerateDebugSyms();
}

// dwUnitSyms stores input and output symbols for DWARF generation
// for a given compilation unit.
private partial struct dwUnitSyms {
    public loader.Sym lineProlog;
    public loader.Sym rangeProlog;
    public loader.Sym infoEpilog; // Outputs for a given unit.
    public slice<loader.Sym> linesyms;
    public slice<loader.Sym> infosyms;
    public slice<loader.Sym> locsyms;
    public slice<loader.Sym> rangessyms;
}

// dwUnitPortion assembles the DWARF content for a given compilation
// unit: debug_info, debug_lines, debug_ranges, debug_loc (debug_frame
// is handled elsewere). Order is important; the calls to writelines
// and writepcranges below make updates to the compilation unit DIE,
// hence they have to happen before the call to writeUnitInfo.
private static void dwUnitPortion(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_u, loader.Sym abbrevsym, ptr<dwUnitSyms> _addr_us) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit u = ref _addr_u.val;
    ref dwUnitSyms us = ref _addr_us.val;

    if (u.DWInfo.Abbrev != dwarf.DW_ABRV_COMPUNIT_TEXTLESS) {
        us.linesyms = d.writelines(u, us.lineProlog);
        var @base = loader.Sym(u.Textp[0]);
        us.rangessyms = d.writepcranges(u, base, u.PCs, us.rangeProlog);
        us.locsyms = d.collectUnitLocs(u);
    }
    us.infosyms = d.writeUnitInfo(u, abbrevsym, us.infoEpilog);
}

private static void dwarfGenerateDebugSyms(this ptr<dwctxt> _addr_d) => func((defer, _, _) => {
    ref dwctxt d = ref _addr_d.val;

    var abbrevSec = d.writeabbrev();
    dwarfp = append(dwarfp, abbrevSec);
    d.calcCompUnitRanges();
    sort.Sort(compilationUnitByStartPC(d.linkctxt.compUnits)); 

    // newdie adds DIEs to the *beginning* of the parent's DIE list.
    // Now that we're done creating DIEs, reverse the trees so DIEs
    // appear in the order they were created.
    foreach (var (_, u) in d.linkctxt.compUnits) {
        reversetree(_addr_u.DWInfo.Child);
    }    reversetree(_addr_dwtypes.Child);
    movetomodule(_addr_d.linkctxt, _addr_dwtypes);

    Func<@string, loader.Sym> mkSecSym = name => {
        var s = d.ldr.CreateSymForUpdate(name, 0);
        s.SetType(sym.SDWARFSECT);
        s.SetReachable(true);
        return s.Sym();
    };
    Func<sym.SymKind, loader.Sym> mkAnonSym = kind => {
        s = d.ldr.MakeSymbolUpdater(d.ldr.CreateExtSym("", 0));
        s.SetType(kind);
        s.SetReachable(true);
        return s.Sym();
    }; 

    // Create the section symbols.
    var frameSym = mkSecSym(".debug_frame");
    var locSym = mkSecSym(".debug_loc");
    var lineSym = mkSecSym(".debug_line");
    var rangesSym = mkSecSym(".debug_ranges");
    var infoSym = mkSecSym(".debug_info"); 

    // Create the section objects
    dwarfSecInfo lineSec = new dwarfSecInfo(syms:[]loader.Sym{lineSym});
    dwarfSecInfo locSec = new dwarfSecInfo(syms:[]loader.Sym{locSym});
    dwarfSecInfo rangesSec = new dwarfSecInfo(syms:[]loader.Sym{rangesSym});
    dwarfSecInfo frameSec = new dwarfSecInfo(syms:[]loader.Sym{frameSym});
    dwarfSecInfo infoSec = new dwarfSecInfo(syms:[]loader.Sym{infoSym}); 

    // Create any new symbols that will be needed during the
    // parallel portion below.
    var ncu = len(d.linkctxt.compUnits);
    var unitSyms = make_slice<dwUnitSyms>(ncu);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < ncu; i++) {
            var us = _addr_unitSyms[i];
            us.lineProlog = mkAnonSym(sym.SDWARFLINES);
            us.rangeProlog = mkAnonSym(sym.SDWARFRANGE);
            us.infoEpilog = mkAnonSym(sym.SDWARFFCN);
        }

        i = i__prev1;
    }

    sync.WaitGroup wg = default;
    var sema = make_channel<object>(runtime.GOMAXPROCS(0)); 

    // Kick off generation of .debug_frame, since it doesn't have
    // any entanglements and can be started right away.
    wg.Add(1);
    go_(() => () => {
        sema.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
        defer(() => {
            sema.Receive();
            wg.Done();
        }());
        frameSec = d.writeframes(frameSym);
    }()); 

    // Create a goroutine per comp unit to handle the generation that
    // unit's portion of .debug_line, .debug_loc, .debug_ranges, and
    // .debug_info.
    wg.Add(len(d.linkctxt.compUnits));
    {
        nint i__prev1 = i;

        for (i = 0; i < ncu; i++) {
            go_(() => (u, us) => {
                sema.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                defer(() => {
                    sema.Receive();
                    wg.Done();
                }());
                d.dwUnitPortion(u, abbrevSec.secSym(), us);
            }(d.linkctxt.compUnits[i], _addr_unitSyms[i]));
        }

        i = i__prev1;
    }
    wg.Wait();

    Func<slice<loader.Sym>, slice<loader.Sym>> markReachable = syms => {
        {
            var s__prev1 = s;

            foreach (var (_, __s) in syms) {
                s = __s;
                d.ldr.SetAttrNotInSymbolTable(s, true);
                d.ldr.SetAttrReachable(s, true);
            }

            s = s__prev1;
        }

        return syms;
    }; 

    // Stitch together the results.
    {
        nint i__prev1 = i;

        for (i = 0; i < ncu; i++) {
            var r = _addr_unitSyms[i];
            lineSec.syms = append(lineSec.syms, markReachable(r.linesyms));
            infoSec.syms = append(infoSec.syms, markReachable(r.infosyms));
            locSec.syms = append(locSec.syms, markReachable(r.locsyms));
            rangesSec.syms = append(rangesSec.syms, markReachable(r.rangessyms));
        }

        i = i__prev1;
    }
    dwarfp = append(dwarfp, lineSec);
    dwarfp = append(dwarfp, frameSec);
    var gdbScriptSec = d.writegdbscript();
    if (gdbScriptSec.secSym() != 0) {
        dwarfp = append(dwarfp, gdbScriptSec);
    }
    dwarfp = append(dwarfp, infoSec);
    if (len(locSec.syms) > 1) {
        dwarfp = append(dwarfp, locSec);
    }
    dwarfp = append(dwarfp, rangesSec); 

    // Check to make sure we haven't listed any symbols more than once
    // in the info section. This used to be done by setting and
    // checking the OnList attribute in "putdie", but that strategy
    // was not friendly for concurrency.
    var seen = loader.MakeBitmap(d.ldr.NSym());
    {
        var s__prev1 = s;

        foreach (var (_, __s) in infoSec.syms) {
            s = __s;
            if (seen.Has(s)) {
                log.Fatalf("symbol %s listed multiple times", d.ldr.SymName(s));
            }
            seen.Set(s);
        }
        s = s__prev1;
    }
});

private static slice<loader.Sym> collectUnitLocs(this ptr<dwctxt> _addr_d, ptr<sym.CompilationUnit> _addr_u) {
    ref dwctxt d = ref _addr_d.val;
    ref sym.CompilationUnit u = ref _addr_u.val;

    loader.Sym syms = new slice<loader.Sym>(new loader.Sym[] {  });
    foreach (var (_, fn) in u.FuncDIEs) {
        var relocs = d.ldr.Relocs(loader.Sym(fn));
        for (nint i = 0; i < relocs.Count(); i++) {
            var reloc = relocs.At(i);
            if (reloc.Type() != objabi.R_DWARFSECREF) {
                continue;
            }
            var rsym = reloc.Sym();
            if (d.ldr.SymType(rsym) == sym.SDWARFLOC) {
                syms = append(syms, rsym); 
                // One location list entry per function, but many relocations to it. Don't duplicate.
                break;
            }
        }
    }    return syms;
}

/*
 *  Elf.
 */
private static void dwarfaddshstrings(ptr<Link> _addr_ctxt, ptr<loader.SymbolBuilder> _addr_shstrtab) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref loader.SymbolBuilder shstrtab = ref _addr_shstrtab.val;

    if (FlagW.val) { // disable dwarf
        return ;
    }
    @string secs = new slice<@string>(new @string[] { "abbrev", "frame", "info", "loc", "line", "gdb_scripts", "ranges" });
    foreach (var (_, sec) in secs) {
        shstrtab.Addstring(".debug_" + sec);
        if (ctxt.IsExternal()) {
            shstrtab.Addstring(elfRelType + ".debug_" + sec);
        }
        else
 {
            shstrtab.Addstring(".zdebug_" + sec);
        }
    }
}

private static void dwarfaddelfsectionsyms(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (FlagW.val) { // disable dwarf
        return ;
    }
    if (ctxt.LinkMode != LinkExternal) {
        return ;
    }
    var ldr = ctxt.loader;
    foreach (var (_, si) in dwarfp) {
        var s = si.secSym();
        var sect = ldr.SymSect(si.secSym());
        putelfsectionsym(ctxt, ctxt.Out, s, sect.Elfsect._<ptr<ElfShdr>>().shnum);
    }
}

// dwarfcompress compresses the DWARF sections. Relocations are applied
// on the fly. After this, dwarfp will contain a different (new) set of
// symbols, and sections may have been replaced.
private static void dwarfcompress(ptr<Link> _addr_ctxt) {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // compressedSect is a helper type for parallelizing compression.
    private partial struct compressedSect {
        public nint index;
        public slice<byte> compressed;
        public slice<loader.Sym> syms;
    }

    var supported = ctxt.IsELF || ctxt.IsWindows() || ctxt.IsDarwin();
    if (!ctxt.compressDWARF || !supported || ctxt.IsExternal()) {
        return ;
    }
    nint compressedCount = default;
    var resChannel = make_channel<compressedSect>();
    foreach (var (i) in dwarfp) {
        go_(() => (resIndex, syms) => {
            resChannel.Send(new compressedSect(resIndex,compressSyms(ctxt,syms),syms));
        }(compressedCount, dwarfp[i].syms));
        compressedCount++;
    }    var res = make_slice<compressedSect>(compressedCount);
    while (compressedCount > 0) {
        var r = resChannel.Receive();
        res[r.index] = r;
        compressedCount--;
    }

    var ldr = ctxt.loader;
    slice<dwarfSecInfo> newDwarfp = default;
    Segdwarf.Sections = Segdwarf.Sections[..(int)0];
    foreach (var (_, z) in res) {
        var s = z.syms[0];
        if (z.compressed == null) { 
            // Compression didn't help.
            dwarfSecInfo ds = new dwarfSecInfo(syms:z.syms);
            newDwarfp = append(newDwarfp, ds);
            Segdwarf.Sections = append(Segdwarf.Sections, ldr.SymSect(s));
        }
        else
 {
            @string compressedSegName = ".zdebug_" + ldr.SymSect(s).Name[(int)len(".debug_")..];
            var sect = addsection(ctxt.loader, ctxt.Arch, _addr_Segdwarf, compressedSegName, 04);
            sect.Align = 1;
            sect.Length = uint64(len(z.compressed));
            var newSym = ldr.CreateSymForUpdate(compressedSegName, 0);
            newSym.SetData(z.compressed);
            newSym.SetSize(int64(len(z.compressed)));
            ldr.SetSymSect(newSym.Sym(), sect);
            ds = new dwarfSecInfo(syms:[]loader.Sym{newSym.Sym()});
            newDwarfp = append(newDwarfp, ds); 

            // compressed symbols are no longer needed.
            {
                var s__prev2 = s;

                foreach (var (_, __s) in z.syms) {
                    s = __s;
                    ldr.SetAttrReachable(s, false);
                    ldr.FreeSym(s);
                }

                s = s__prev2;
            }
        }
    }    dwarfp = newDwarfp; 

    // Re-compute the locations of the compressed DWARF symbols
    // and sections, since the layout of these within the file is
    // based on Section.Vaddr and Symbol.Value.
    var pos = Segdwarf.Vaddr;
    ptr<sym.Section> prevSect;
    foreach (var (_, si) in dwarfp) {
        {
            var s__prev2 = s;

            foreach (var (_, __s) in si.syms) {
                s = __s;
                ldr.SetSymValue(s, int64(pos));
                sect = ldr.SymSect(s);
                if (sect != prevSect) {
                    sect.Vaddr = uint64(pos);
                    prevSect = sect;
                }
                if (ldr.SubSym(s) != 0) {
                    log.Fatalf("%s: unexpected sub-symbols", ldr.SymName(s));
                }
                pos += uint64(ldr.SymSize(s));
                if (ctxt.IsWindows()) {
                    pos = uint64(Rnd(int64(pos), PEFILEALIGN));
                }
            }

            s = s__prev2;
        }
    }    Segdwarf.Length = pos - Segdwarf.Vaddr;
}

private partial struct compilationUnitByStartPC { // : slice<ptr<sym.CompilationUnit>>
}

private static nint Len(this compilationUnitByStartPC v) {
    return len(v);
}
private static void Swap(this compilationUnitByStartPC v, nint i, nint j) {
    (v[i], v[j]) = (v[j], v[i]);
}

private static bool Less(this compilationUnitByStartPC v, nint i, nint j) {

    if (len(v[i].Textp) == 0 && len(v[j].Textp) == 0) 
        return v[i].Lib.Pkg < v[j].Lib.Pkg;
    else if (len(v[i].Textp) != 0 && len(v[j].Textp) == 0) 
        return true;
    else if (len(v[i].Textp) == 0 && len(v[j].Textp) != 0) 
        return false;
    else 
        return v[i].PCs[0].Start < v[j].PCs[0].Start;
    }

// getPkgFromCUSym returns the package name for the compilation unit
// represented by s.
// The prefix dwarf.InfoPrefix+".pkg." needs to be removed in order to get
// the package name.
private static @string getPkgFromCUSym(this ptr<dwctxt> _addr_d, loader.Sym s) {
    ref dwctxt d = ref _addr_d.val;

    return strings.TrimPrefix(d.ldr.SymName(s), dwarf.InfoPrefix + ".pkg.");
}

// On AIX, the symbol table needs to know where are the compilation units parts
// for a specific package in each .dw section.
// dwsectCUSize map will save the size of a compilation unit for
// the corresponding .dw section.
// This size can later be retrieved with the index "sectionName.pkgName".
private static sync.Mutex dwsectCUSizeMu = default;
private static map<@string, ulong> dwsectCUSize = default;

// getDwsectCUSize retrieves the corresponding package size inside the current section.
private static ulong getDwsectCUSize(@string sname, @string pkgname) {
    return dwsectCUSize[sname + "." + pkgname];
}

private static void addDwsectCUSize(@string sname, @string pkgname, ulong size) => func((defer, _, _) => {
    dwsectCUSizeMu.Lock();
    defer(dwsectCUSizeMu.Unlock());
    dwsectCUSize[sname + "." + pkgname] += size;
});

} // end ld_package
