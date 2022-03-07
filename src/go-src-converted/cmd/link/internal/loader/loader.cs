// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package loader -- go2cs converted at 2022 March 06 23:20:34 UTC
// import "cmd/link/internal/loader" ==> using loader = go.cmd.link.@internal.loader_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\loader\loader.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using goobj = go.cmd.@internal.goobj_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using bits = go.math.bits_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using System;


namespace go.cmd.link.@internal;

public static partial class loader_package {

private static var _ = fmt.Print;

// Sym encapsulates a global symbol index, used to identify a specific
// Go symbol. The 0-valued Sym is corresponds to an invalid symbol.
public partial struct Sym { // : nint
}

// Relocs encapsulates the set of relocations on a given symbol; an
// instance of this type is returned by the Loader Relocs() method.
public partial struct Relocs {
    public slice<goobj.Reloc> rs;
    public uint li; // local index of symbol whose relocs we're examining
    public ptr<oReader> r; // object reader for containing package
    public ptr<Loader> l; // loader
}

// ExtReloc contains the payload for an external relocation.
public partial struct ExtReloc {
    public Sym Xsym;
    public long Xadd;
    public objabi.RelocType Type;
    public byte Size;
}

// Reloc holds a "handle" to access a relocation record from an
// object file.
public partial struct Reloc {
    public ref ptr<goobj.Reloc> Reloc> => ref Reloc>_ptr;
    public ptr<oReader> r;
    public ptr<Loader> l;
}

public static objabi.RelocType Type(this Reloc rel) {
    return objabi.RelocType(rel.Reloc.Type()) & ~objabi.R_WEAK;
}
public static bool Weak(this Reloc rel) {
    return objabi.RelocType(rel.Reloc.Type()) & objabi.R_WEAK != 0;
}
public static void SetType(this Reloc rel, objabi.RelocType t) {
    rel.Reloc.SetType(uint16(t));
}
public static Sym Sym(this Reloc rel) {
    return rel.l.resolve(rel.r, rel.Reloc.Sym());
}
public static void SetSym(this Reloc rel, Sym s) {
    rel.Reloc.SetSym(new goobj.SymRef(PkgIdx:0,SymIdx:uint32(s)));
}
public static bool IsMarker(this Reloc rel) {
    return rel.Siz() == 0;
}

// Aux holds a "handle" to access an aux symbol record from an
// object file.
public partial struct Aux {
    public ref ptr<goobj.Aux> Aux> => ref Aux>_ptr;
    public ptr<oReader> r;
    public ptr<Loader> l;
}

public static Sym Sym(this Aux a) {
    return a.l.resolve(a.r, a.Aux.Sym());
}

// oReader is a wrapper type of obj.Reader, along with some
// extra information.
private partial struct oReader {
    public ref ptr<goobj.Reader> Reader> => ref Reader>_ptr;
    public ptr<sym.CompilationUnit> unit;
    public nint version; // version of static symbol
    public uint flags; // read from object file
    public @string pkgprefix;
    public slice<Sym> syms; // Sym's global index, indexed by local index
    public slice<uint> pkg; // indices of referenced package by PkgIdx (index into loader.objs array)
    public nint ndef; // cache goobj.Reader.NSym()
    public nint nhashed64def; // cache goobj.Reader.NHashed64Def()
    public nint nhasheddef; // cache goobj.Reader.NHashedDef()
    public uint objidx; // index of this reader in the objs slice
}

// Total number of defined symbols (package symbols, hashed symbols, and
// non-package symbols).
private static nint NAlldef(this ptr<oReader> _addr_r) {
    ref oReader r = ref _addr_r.val;

    return r.ndef + r.nhashed64def + r.nhasheddef + r.NNonpkgdef();
}

private partial struct objIdx {
    public ptr<oReader> r;
    public Sym i; // start index
}

// objSym represents a symbol in an object file. It is a tuple of
// the object and the symbol's local index.
// For external symbols, objidx is the index of l.extReader (extObj),
// s is its index into the payload array.
// {0, 0} represents the nil symbol.
private partial struct objSym {
    public uint objidx; // index of the object (in l.objs array)
    public uint s; // local index
}

private partial struct nameVer {
    public @string name;
    public nint v;
}

public partial struct Bitmap { // : slice<uint>
}

// set the i-th bit.
public static void Set(this Bitmap bm, Sym i) {
    var n = uint(i) / 32;
    var r = uint(i) % 32;
    bm[n] |= 1 << (int)(r);

}

// unset the i-th bit.
public static void Unset(this Bitmap bm, Sym i) {
    var n = uint(i) / 32;
    var r = uint(i) % 32;
    bm[n] &= (1 << (int)(r));

}

// whether the i-th bit is set.
public static bool Has(this Bitmap bm, Sym i) {
    var n = uint(i) / 32;
    var r = uint(i) % 32;
    return bm[n] & (1 << (int)(r)) != 0;

}

// return current length of bitmap in bits.
public static nint Len(this Bitmap bm) {
    return len(bm) * 32;
}

// return the number of bits set.
public static nint Count(this Bitmap bm) {
    nint s = 0;
    foreach (var (_, x) in bm) {
        s += bits.OnesCount32(x);
    }    return s;
}

public static Bitmap MakeBitmap(nint n) {
    return make(Bitmap, (n + 31) / 32);
}

// growBitmap insures that the specified bitmap has enough capacity,
// reallocating (doubling the size) if needed.
private static Bitmap growBitmap(nint reqLen, Bitmap b) {
    var curLen = b.Len();
    if (reqLen > curLen) {
        b = append(b, MakeBitmap(reqLen + 1 - curLen));
    }
    return b;

}

private partial struct symAndSize {
    public Sym sym;
    public uint size;
}

// A Loader loads new object files and resolves indexed symbol references.
//
// Notes on the layout of global symbol index space:
//
// - Go object files are read before host object files; each Go object
//   read adds its defined package symbols to the global index space.
//   Nonpackage symbols are not yet added.
//
// - In loader.LoadNonpkgSyms, add non-package defined symbols and
//   references in all object files to the global index space.
//
// - Host object file loading happens; the host object loader does a
//   name/version lookup for each symbol it finds; this can wind up
//   extending the external symbol index space range. The host object
//   loader stores symbol payloads in loader.payloads using SymbolBuilder.
//
// - Each symbol gets a unique global index. For duplicated and
//   overwriting/overwritten symbols, the second (or later) appearance
//   of the symbol gets the same global index as the first appearance.
public partial struct Loader {
    public map<ptr<oReader>, Sym> start; // map from object file to its start index
    public slice<objIdx> objs; // sorted by start index (i.e. objIdx.i)
    public Sym extStart; // from this index on, the symbols are externally defined
    public slice<Sym> builtinSyms; // global index of builtin symbols

    public slice<objSym> objSyms; // global index mapping to local index

    public array<map<@string, Sym>> symsByName; // map symbol name to index, two maps are for ABI0 and ABIInternal
    public map<nameVer, Sym> extStaticSyms; // externally defined static symbols, keyed by name

    public ptr<oReader> extReader; // a dummy oReader, for external symbols
    public slice<extSymPayload> payloadBatch;
    public slice<ptr<extSymPayload>> payloads; // contents of linker-materialized external syms
    public slice<long> values; // symbol values, indexed by global sym index

    public slice<ptr<sym.Section>> sects; // sections
    public slice<ushort> symSects; // symbol's section, index to sects array

    public slice<byte> align; // symbol 2^N alignment, indexed by global index

    public map<Sym, bool> deferReturnTramp; // whether the symbol is a trampoline of a deferreturn call

    public map<@string, uint> objByPkg; // map package path to the index of its Go object reader

    public nint anonVersion; // most recently assigned ext static sym pseudo-version

// Bitmaps and other side structures used to store data used to store
// symbol flags/attributes; these are to be accessed via the
// corresponding loader "AttrXXX" and "SetAttrXXX" methods. Please
// visit the comments on these methods for more details on the
// semantics / interpretation of the specific flags or attribute.
    public Bitmap attrReachable; // reachable symbols, indexed by global index
    public Bitmap attrOnList; // "on list" symbols, indexed by global index
    public Bitmap attrLocal; // "local" symbols, indexed by global index
    public Bitmap attrNotInSymbolTable; // "not in symtab" symbols, indexed by global idx
    public Bitmap attrUsedInIface; // "used in interface" symbols, indexed by global idx
    public Bitmap attrVisibilityHidden; // hidden symbols, indexed by ext sym index
    public Bitmap attrDuplicateOK; // dupOK symbols, indexed by ext sym index
    public Bitmap attrShared; // shared symbols, indexed by ext sym index
    public Bitmap attrExternal; // external symbols, indexed by ext sym index

    public map<Sym, bool> attrReadOnly; // readonly data for this sym
    public map<Sym, Sym> outer;
    public map<Sym, Sym> sub;
    public map<Sym, @string> dynimplib; // stores Dynimplib symbol attribute
    public map<Sym, @string> dynimpvers; // stores Dynimpvers symbol attribute
    public map<Sym, byte> localentry; // stores Localentry symbol attribute
    public map<Sym, @string> extname; // stores Extname symbol attribute
    public map<Sym, elf.SymType> elfType; // stores elf type symbol property
    public map<Sym, int> elfSym; // stores elf sym symbol property
    public map<Sym, int> localElfSym; // stores "local" elf sym symbol property
    public map<Sym, @string> symPkg; // stores package for symbol, or library for shlib-derived syms
    public map<Sym, int> plt; // stores dynimport for pe objects
    public map<Sym, int> got; // stores got for pe objects
    public map<Sym, int> dynid; // stores Dynid for symbol

    public map<relocId, sym.RelocVariant> relocVariant; // stores variant relocs

// Used to implement field tracking; created during deadcode if
// field tracking is enabled. Reachparent[K] contains the index of
// the symbol that triggered the marking of symbol K as live.
    public slice<Sym> Reachparent; // CgoExports records cgo-exported symbols by SymName.
    public map<@string, Sym> CgoExports;
    public uint flags;
    public bool hasUnknownPkgPath; // if any Go object has unknown package path

    public nint strictDupMsgs; // number of strict-dup warning/errors, when FlagStrictDups is enabled

    public elfsetstringFunc elfsetstring;
    public ptr<ErrorReporter> errorReporter;
    public nint npkgsyms; // number of package symbols, for accounting
    public nint nhashedsyms; // number of hashed symbols, for accounting
}

private static readonly var pkgDef = iota;
private static readonly var hashed64Def = 0;
private static readonly var hashedDef = 1;
private static readonly var nonPkgDef = 2;
private static readonly var nonPkgRef = 3;


// objidx
private static readonly var nilObj = iota;
private static readonly var extObj = 0;
private static readonly var goObjStart = 1;


public delegate void elfsetstringFunc(@string, nint);

// extSymPayload holds the payload (data + relocations) for linker-synthesized
// external symbols (note that symbol value is stored in a separate slice).
private partial struct extSymPayload {
    public @string name; // TODO: would this be better as offset into str table?
    public long size;
    public nint ver;
    public sym.SymKind kind;
    public uint objidx; // index of original object if sym made by cloneToExternal
    public slice<goobj.Reloc> relocs;
    public slice<byte> data;
    public slice<goobj.Aux> auxs;
}

 
// Loader.flags
public static readonly nint FlagStrictDups = 1 << (int)(iota);
public static readonly var FlagUseABIAlias = 0;


public static ptr<Loader> NewLoader(uint flags, elfsetstringFunc elfsetstring, ptr<ErrorReporter> _addr_reporter) {
    ref ErrorReporter reporter = ref _addr_reporter.val;

    var nbuiltin = goobj.NBuiltin();
    ptr<oReader> extReader = addr(new oReader(objidx:extObj));
    ptr<Loader> ldr = addr(new Loader(start:make(map[*oReader]Sym),objs:[]objIdx{{},{extReader,0}},objSyms:make([]objSym,1,1),extReader:extReader,symsByName:[2]map[string]Sym{make(map[string]Sym,80000),make(map[string]Sym,50000)},objByPkg:make(map[string]uint32),outer:make(map[Sym]Sym),sub:make(map[Sym]Sym),dynimplib:make(map[Sym]string),dynimpvers:make(map[Sym]string),localentry:make(map[Sym]uint8),extname:make(map[Sym]string),attrReadOnly:make(map[Sym]bool),elfType:make(map[Sym]elf.SymType),elfSym:make(map[Sym]int32),localElfSym:make(map[Sym]int32),symPkg:make(map[Sym]string),plt:make(map[Sym]int32),got:make(map[Sym]int32),dynid:make(map[Sym]int32),attrSpecial:make(map[Sym]struct{}),attrCgoExportDynamic:make(map[Sym]struct{}),attrCgoExportStatic:make(map[Sym]struct{}),generatedSyms:make(map[Sym]struct{}),deferReturnTramp:make(map[Sym]bool),extStaticSyms:make(map[nameVer]Sym),builtinSyms:make([]Sym,nbuiltin),flags:flags,elfsetstring:elfsetstring,errorReporter:reporter,sects:[]*sym.Section{nil},));
    reporter.ldr = ldr;
    return _addr_ldr!;
}

// Add object file r, return the start index.
private static Sym addObj(this ptr<Loader> _addr_l, @string pkg, ptr<oReader> _addr_r) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    {
        var (_, ok) = l.start[r];

        if (ok) {
            panic("already added");
        }
    }

    pkg = objabi.PathToPrefix(pkg); // the object file contains escaped package path
    {
        (_, ok) = l.objByPkg[pkg];

        if (!ok) {
            l.objByPkg[pkg] = r.objidx;
        }
    }

    var i = Sym(len(l.objSyms));
    l.start[r] = i;
    l.objs = append(l.objs, new objIdx(r,i));
    if (r.NeedNameExpansion() && !r.FromAssembly()) {
        l.hasUnknownPkgPath = true;
    }
    return i;

});

// Add a symbol from an object file, return the global index.
// If the symbol already exist, it returns the index of that symbol.
private static Sym addSym(this ptr<loadState> _addr_st, @string name, nint ver, ptr<oReader> _addr_r, uint li, nint kind, ptr<goobj.Sym> _addr_osym) => func((_, panic, _) => {
    ref loadState st = ref _addr_st.val;
    ref oReader r = ref _addr_r.val;
    ref goobj.Sym osym = ref _addr_osym.val;

    var l = st.l;
    if (l.extStart != 0) {
        panic("addSym called after external symbol is created");
    }
    var i = Sym(len(l.objSyms));
    Action addToGlobal = () => {
        l.objSyms = append(l.objSyms, new objSym(r.objidx,li));
    };
    if (name == "" && kind != hashed64Def && kind != hashedDef) {
        addToGlobal();
        return i; // unnamed aux symbol
    }
    if (ver == r.version) { 
        // Static symbol. Add its global index but don't
        // add to name lookup table, as it cannot be
        // referenced by name.
        addToGlobal();
        return i;

    }

    if (kind == pkgDef) 
        // Defined package symbols cannot be dup to each other.
        // We load all the package symbols first, so we don't need
        // to check dup here.
        // We still add it to the lookup table, as it may still be
        // referenced by name (e.g. through linkname).
        l.symsByName[ver][name] = i;
        addToGlobal();
        return i;
    else if (kind == hashed64Def || kind == hashedDef) 
        // Hashed (content-addressable) symbol. Check the hash
        // but don't add to name lookup table, as they are not
        // referenced by name. Also no need to do overwriting
        // check, as same hash indicates same content.
        Func<(symAndSize, bool)> checkHash = default;
        Action<symAndSize> addToHashMap = default;
        ulong h64 = default; // only used for hashed64Def
        ptr<goobj.HashType> h; // only used for hashedDef
        if (kind == hashed64Def) {
            checkHash = () => {
                h64 = r.Hash64(li - uint32(r.ndef));
                var (s, existed) = st.hashed64Syms[h64];
                return (s, existed);
            }
        else
;
            addToHashMap = ss => {
                st.hashed64Syms[h64] = ss;
            }
;

        } {
            checkHash = () => {
                h = r.Hash(li - uint32(r.ndef + r.nhashed64def));
                (s, existed) = st.hashedSyms[h.val];
                return (s, existed);
            }
;
            addToHashMap = ss => {
                st.hashedSyms[h.val] = ss;
            }
;

        }
        var siz = osym.Siz();
        {
            var s__prev1 = s;

            (s, existed) = checkHash();

            if (existed) { 
                // The content hash is built from symbol data and relocations. In the
                // object file, the symbol data may not always contain trailing zeros,
                // e.g. for [5]int{1,2,3} and [100]int{1,2,3}, the data is same
                // (although the size is different).
                // Also, for short symbols, the content hash is the identity function of
                // the 8 bytes, and trailing zeros doesn't change the hash value, e.g.
                // hash("A") == hash("A\0\0\0").
                // So when two symbols have the same hash, we need to use the one with
                // larger size.
                if (siz > s.size) { 
                    // New symbol has larger size, use the new one. Rewrite the index mapping.
                    l.objSyms[s.sym] = new objSym(r.objidx,li);
                    addToHashMap(new symAndSize(s.sym,siz));

                }

                return s.sym;

            }

            s = s__prev1;

        }

        addToHashMap(new symAndSize(i,siz));
        addToGlobal();
        return i;
    // Non-package (named) symbol. Check if it already exists.
    var (oldi, existed) = l.symsByName[ver][name];
    if (!existed) {
        l.symsByName[ver][name] = i;
        addToGlobal();
        return i;
    }
    if (osym.Dupok()) {
        if (l.flags & FlagStrictDups != 0) {
            l.checkdup(name, r, li, oldi);
        }
        var szdup = l.SymSize(oldi);
        var sz = int64(r.Sym(li).Siz());
        if (szdup < sz) { 
            // new symbol overwrites old symbol.
            l.objSyms[oldi] = new objSym(r.objidx,li);

        }
        return oldi;

    }
    var (oldr, oldli) = l.toLocal(oldi);
    var oldsym = oldr.Sym(oldli);
    if (oldsym.Dupok()) {
        return oldi;
    }
    var overwrite = r.DataSize(li) != 0;
    if (overwrite) { 
        // new symbol overwrites old symbol.
        var oldtyp = sym.AbiSymKindToSymKind[objabi.SymKind(oldsym.Type())];
        if (!(oldtyp.IsData() && oldr.DataSize(oldli) == 0)) {
            log.Fatalf("duplicated definition of symbol %s, from %s and %s", name, r.unit.Lib.Pkg, oldr.unit.Lib.Pkg);
        }
        l.objSyms[oldi] = new objSym(r.objidx,li);

    }
    else
 { 
        // old symbol overwrites new symbol.
        var typ = sym.AbiSymKindToSymKind[objabi.SymKind(oldsym.Type())];
        if (!typ.IsData()) { // only allow overwriting data symbol
            log.Fatalf("duplicated definition of symbol %s, from %s and %s", name, r.unit.Lib.Pkg, oldr.unit.Lib.Pkg);

        }
    }
    return oldi;

});

// newExtSym creates a new external sym with the specified
// name/version.
private static Sym newExtSym(this ptr<Loader> _addr_l, @string name, nint ver) {
    ref Loader l = ref _addr_l.val;

    var i = Sym(len(l.objSyms));
    if (l.extStart == 0) {
        l.extStart = i;
    }
    l.growValues(int(i) + 1);
    l.growAttrBitmaps(int(i) + 1);
    var pi = l.newPayload(name, ver);
    l.objSyms = append(l.objSyms, new objSym(l.extReader.objidx,uint32(pi)));
    l.extReader.syms = append(l.extReader.syms, i);
    return i;

}

// LookupOrCreateSym looks up the symbol with the specified name/version,
// returning its Sym index if found. If the lookup fails, a new external
// Sym will be created, entered into the lookup tables, and returned.
private static Sym LookupOrCreateSym(this ptr<Loader> _addr_l, @string name, nint ver) {
    ref Loader l = ref _addr_l.val;

    var i = l.Lookup(name, ver);
    if (i != 0) {
        return i;
    }
    i = l.newExtSym(name, ver);
    var @static = ver >= sym.SymVerStatic || ver < 0;
    if (static) {
        l.extStaticSyms[new nameVer(name,ver)] = i;
    }
    else
 {
        l.symsByName[ver][name] = i;
    }
    return i;

}

// AddCgoExport records a cgo-exported symbol in l.CgoExports.
// This table is used to identify the correct Go symbol ABI to use
// to resolve references from host objects (which don't have ABIs).
private static void AddCgoExport(this ptr<Loader> _addr_l, Sym s) {
    ref Loader l = ref _addr_l.val;

    if (l.CgoExports == null) {
        l.CgoExports = make_map<@string, Sym>();
    }
    l.CgoExports[l.SymName(s)] = s;

}

// LookupOrCreateCgoExport is like LookupOrCreateSym, but if ver
// indicates a global symbol, it uses the CgoExport table to determine
// the appropriate symbol version (ABI) to use. ver must be either 0
// or a static symbol version.
private static Sym LookupOrCreateCgoExport(this ptr<Loader> _addr_l, @string name, nint ver) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (ver >= sym.SymVerStatic) {
        return l.LookupOrCreateSym(name, ver);
    }
    if (ver != 0) {
        panic("ver must be 0 or a static version");
    }
    {
        var (s, ok) = l.CgoExports[name];

        if (ok) {
            return s;
        }
    } 
    // Otherwise, this must just be a symbol in the host object.
    // Create a version 0 symbol for it.
    return l.LookupOrCreateSym(name, 0);

});

private static bool IsExternal(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (r, _) = l.toLocal(i);
    return l.isExtReader(r);
}

private static bool isExtReader(this ptr<Loader> _addr_l, ptr<oReader> _addr_r) {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    return r == l.extReader;
}

// For external symbol, return its index in the payloads array.
// XXX result is actually not a global index. We (ab)use the Sym type
// so we don't need conversion for accessing bitmaps.
private static Sym extIndex(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (_, li) = l.toLocal(i);
    return Sym(li);
}

// Get a new payload for external symbol, return its index in
// the payloads array.
private static nint newPayload(this ptr<Loader> _addr_l, @string name, nint ver) {
    ref Loader l = ref _addr_l.val;

    var pi = len(l.payloads);
    var pp = l.allocPayload();
    pp.name = name;
    pp.ver = ver;
    l.payloads = append(l.payloads, pp);
    l.growExtAttrBitmaps();
    return pi;
}

// getPayload returns a pointer to the extSymPayload struct for an
// external symbol if the symbol has a payload. Will panic if the
// symbol in question is bogus (zero or not an external sym).
private static ptr<extSymPayload> getPayload(this ptr<Loader> _addr_l, Sym i) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic(fmt.Sprintf("bogus symbol index %d in getPayload", i));
    }
    var pi = l.extIndex(i);
    return _addr_l.payloads[pi]!;

});

// allocPayload allocates a new payload.
private static ptr<extSymPayload> allocPayload(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    var batch = l.payloadBatch;
    if (len(batch) == 0) {
        batch = make_slice<extSymPayload>(1000);
    }
    var p = _addr_batch[0];
    l.payloadBatch = batch[(int)1..];
    return _addr_p!;

}

private static void Grow(this ptr<extSymPayload> _addr_ms, long siz) {
    ref extSymPayload ms = ref _addr_ms.val;

    if (int64(int(siz)) != siz) {
        log.Fatalf("symgrow size %d too long", siz);
    }
    if (int64(len(ms.data)) >= siz) {
        return ;
    }
    if (cap(ms.data) < int(siz)) {
        var cl = len(ms.data);
        ms.data = append(ms.data, make_slice<byte>(int(siz) + 1 - cl));
        ms.data = ms.data[(int)0..(int)cl];
    }
    ms.data = ms.data[..(int)siz];

}

// Convert a local index to a global index.
private static Sym toGlobal(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, uint i) {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    return r.syms[i];
}

// Convert a global index to a local index.
private static (ptr<oReader>, uint) toLocal(this ptr<Loader> _addr_l, Sym i) {
    ptr<oReader> _p0 = default!;
    uint _p0 = default;
    ref Loader l = ref _addr_l.val;

    return (_addr_l.objs[l.objSyms[i].objidx].r!, l.objSyms[i].s);
}

// Resolve a local symbol reference. Return global index.
private static Sym resolve(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, goobj.SymRef s) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    ptr<oReader> rr;
    {
        var p = s.PkgIdx;


        if (p == goobj.PkgIdxInvalid) 
            // {0, X} with non-zero X is never a valid sym reference from a Go object.
            // We steal this space for symbol references from external objects.
            // In this case, X is just the global index.
            if (l.isExtReader(r)) {
                return Sym(s.SymIdx);
            }
            if (s.SymIdx != 0) {
                panic("bad sym ref");
            }
            return 0;
        else if (p == goobj.PkgIdxHashed64) 
            var i = int(s.SymIdx) + r.ndef;
            return r.syms[i];
        else if (p == goobj.PkgIdxHashed) 
            i = int(s.SymIdx) + r.ndef + r.nhashed64def;
            return r.syms[i];
        else if (p == goobj.PkgIdxNone) 
            i = int(s.SymIdx) + r.ndef + r.nhashed64def + r.nhasheddef;
            return r.syms[i];
        else if (p == goobj.PkgIdxBuiltin) 
            {
                var bi = l.builtinSyms[s.SymIdx];

                if (bi != 0) {
                    return bi;
                }

            }

            l.reportMissingBuiltin(int(s.SymIdx), r.unit.Lib.Pkg);
            return 0;
        else if (p == goobj.PkgIdxSelf) 
            rr = r;
        else 
            rr = l.objs[r.pkg[p]].r;

    }
    return l.toGlobal(rr, s.SymIdx);

});

// reportMissingBuiltin issues an error in the case where we have a
// relocation against a runtime builtin whose definition is not found
// when the runtime package is built. The canonical example is
// "runtime.racefuncenter" -- currently if you do something like
//
//    go build -gcflags=-race myprogram.go
//
// the compiler will insert calls to the builtin runtime.racefuncenter,
// but the version of the runtime used for linkage won't actually contain
// definitions of that symbol. See issue #42396 for details.
//
// As currently implemented, this is a fatal error. This has drawbacks
// in that if there are multiple missing builtins, the error will only
// cite the first one. On the plus side, terminating the link here has
// advantages in that we won't run the risk of panics or crashes later
// on in the linker due to R_CALL relocations with 0-valued target
// symbols.
private static void reportMissingBuiltin(this ptr<Loader> _addr_l, nint bsym, @string reflib) {
    ref Loader l = ref _addr_l.val;

    var (bname, _) = goobj.BuiltinName(bsym);
    log.Fatalf("reference to undefined builtin %q from package %q", bname, reflib);
}

// Look up a symbol by name, return global index, or 0 if not found.
// This is more like Syms.ROLookup than Lookup -- it doesn't create
// new symbol.
private static Sym Lookup(this ptr<Loader> _addr_l, @string name, nint ver) {
    ref Loader l = ref _addr_l.val;

    if (ver >= sym.SymVerStatic || ver < 0) {
        return l.extStaticSyms[new nameVer(name,ver)];
    }
    return l.symsByName[ver][name];

}

// Check that duplicate symbols have same contents.
private static void checkdup(this ptr<Loader> _addr_l, @string name, ptr<oReader> _addr_r, uint li, Sym dup) {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    var p = r.Data(li);
    var (rdup, ldup) = l.toLocal(dup);
    var pdup = rdup.Data(ldup);
    @string reason = "same length but different contents";
    if (len(p) != len(pdup)) {
        reason = fmt.Sprintf("new length %d != old length %d", len(p), len(pdup));
    }
    else if (bytes.Equal(p, pdup)) { 
        // For BSS symbols, we need to check size as well, see issue 46653.
        var szdup = l.SymSize(dup);
        var sz = int64(r.Sym(li).Siz());
        if (szdup == sz) {
            return ;
        }
        reason = fmt.Sprintf("different sizes: new size %d != old size %d", sz, szdup);

    }
    fmt.Fprintf(os.Stderr, "cmd/link: while reading object for '%v': duplicate symbol '%s', previous def at '%v', with mismatched payload: %s\n", r.unit.Lib, name, rdup.unit.Lib, reason); 

    // For the moment, allow DWARF subprogram DIEs for
    // auto-generated wrapper functions. What seems to happen
    // here is that we get different line numbers on formal
    // params; I am guessing that the pos is being inherited
    // from the spot where the wrapper is needed.
    var allowed = strings.HasPrefix(name, "go.info.go.interface") || strings.HasPrefix(name, "go.info.go.builtin") || strings.HasPrefix(name, "go.debuglines");
    if (!allowed) {
        l.strictDupMsgs++;
    }
}

private static nint NStrictDupMsgs(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    return l.strictDupMsgs;
}

// Number of total symbols.
private static nint NSym(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    return len(l.objSyms);
}

// Number of defined Go symbols.
private static nint NDef(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    return int(l.extStart);
}

// Number of reachable symbols.
private static nint NReachableSym(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    return l.attrReachable.Count();
}

// SymNameLen returns the length of the symbol name, trying hard not to load
// the name.
private static nint SymNameLen(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;
 
    // Not much we can do about external symbols.
    if (l.IsExternal(i)) {
        return len(l.SymName(i));
    }
    var (r, li) = l.toLocal(i);
    var le = r.Sym(li).NameLen(r.Reader);
    if (!r.NeedNameExpansion()) {
        return le;
    }
    return len(l.SymName(i));

}

// Returns the raw (unpatched) name of the i-th symbol.
private static @string RawSymName(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        return pp.name;
    }
    var (r, li) = l.toLocal(i);
    return r.Sym(li).Name(r.Reader);

}

// Returns the (patched) name of the i-th symbol.
private static @string SymName(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        return pp.name;
    }
    var (r, li) = l.toLocal(i);
    if (r == null) {
        return "?";
    }
    var name = r.Sym(li).Name(r.Reader);
    if (!r.NeedNameExpansion()) {
        return name;
    }
    return strings.Replace(name, "\"\".", r.pkgprefix, -1);

}

// Returns the version of the i-th symbol.
private static nint SymVersion(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        return pp.ver;
    }
    var (r, li) = l.toLocal(i);
    return int(abiToVer(r.Sym(li).ABI(), r.version));

}

private static bool IsFileLocal(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.SymVersion(i) >= sym.SymVerStatic;
}

// IsFromAssembly returns true if this symbol is derived from an
// object file generated by the Go assembler.
private static bool IsFromAssembly(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        return false;
    }
    var (r, _) = l.toLocal(i);
    return r.FromAssembly();

}

// Returns the type of the i-th symbol.
private static sym.SymKind SymType(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp != null) {
            return pp.kind;
        }
        return 0;

    }
    var (r, li) = l.toLocal(i);
    return sym.AbiSymKindToSymKind[objabi.SymKind(r.Sym(li).Type())];

}

// Returns the attributes of the i-th symbol.
private static byte SymAttr(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) { 
        // TODO: do something? External symbols have different representation of attributes.
        // For now, ReflectMethod, NoSplit, GoType, and Typelink are used and they cannot be
        // set by external symbol.
        return 0;

    }
    var (r, li) = l.toLocal(i);
    return r.Sym(li).Flag();

}

// Returns the size of the i-th symbol.
private static long SymSize(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        return pp.size;
    }
    var (r, li) = l.toLocal(i);
    return int64(r.Sym(li).Siz());

}

// AttrReachable returns true for symbols that are transitively
// referenced from the entry points. Unreachable symbols are not
// written to the output.
private static bool AttrReachable(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.attrReachable.Has(i);
}

// SetAttrReachable sets the reachability property for a symbol (see
// AttrReachable).
private static void SetAttrReachable(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrReachable.Set(i);
    }
    else
 {
        l.attrReachable.Unset(i);
    }
}

// AttrOnList returns true for symbols that are on some list (such as
// the list of all text symbols, or one of the lists of data symbols)
// and is consulted to avoid bugs where a symbol is put on a list
// twice.
private static bool AttrOnList(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.attrOnList.Has(i);
}

// SetAttrOnList sets the "on list" property for a symbol (see
// AttrOnList).
private static void SetAttrOnList(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrOnList.Set(i);
    }
    else
 {
        l.attrOnList.Unset(i);
    }
}

// AttrLocal returns true for symbols that are only visible within the
// module (executable or shared library) being linked. This attribute
// is applied to thunks and certain other linker-generated symbols.
private static bool AttrLocal(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.attrLocal.Has(i);
}

// SetAttrLocal the "local" property for a symbol (see AttrLocal above).
private static void SetAttrLocal(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrLocal.Set(i);
    }
    else
 {
        l.attrLocal.Unset(i);
    }
}

// AttrUsedInIface returns true for a type symbol that is used in
// an interface.
private static bool AttrUsedInIface(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.attrUsedInIface.Has(i);
}

private static void SetAttrUsedInIface(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrUsedInIface.Set(i);
    }
    else
 {
        l.attrUsedInIface.Unset(i);
    }
}

// SymAddr checks that a symbol is reachable, and returns its value.
private static long SymAddr(this ptr<Loader> _addr_l, Sym i) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.AttrReachable(i)) {
        panic("unreachable symbol in symaddr");
    }
    return l.values[i];

});

// AttrNotInSymbolTable returns true for symbols that should not be
// added to the symbol table of the final generated load module.
private static bool AttrNotInSymbolTable(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.attrNotInSymbolTable.Has(i);
}

// SetAttrNotInSymbolTable the "not in symtab" property for a symbol
// (see AttrNotInSymbolTable above).
private static void SetAttrNotInSymbolTable(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrNotInSymbolTable.Set(i);
    }
    else
 {
        l.attrNotInSymbolTable.Unset(i);
    }
}

// AttrVisibilityHidden symbols returns true for ELF symbols with
// visibility set to STV_HIDDEN. They become local symbols in
// the final executable. Only relevant when internally linking
// on an ELF platform.
private static bool AttrVisibilityHidden(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        return false;
    }
    return l.attrVisibilityHidden.Has(l.extIndex(i));

}

// SetAttrVisibilityHidden sets the "hidden visibility" property for a
// symbol (see AttrVisibilityHidden).
private static void SetAttrVisibilityHidden(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic("tried to set visibility attr on non-external symbol");
    }
    if (v) {
        l.attrVisibilityHidden.Set(l.extIndex(i));
    }
    else
 {
        l.attrVisibilityHidden.Unset(l.extIndex(i));
    }
});

// AttrDuplicateOK returns true for a symbol that can be present in
// multiple object files.
private static bool AttrDuplicateOK(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) { 
        // TODO: if this path winds up being taken frequently, it
        // might make more sense to copy the flag value out of the object
        // into a larger bitmap during preload.
        var (r, li) = l.toLocal(i);
        return r.Sym(li).Dupok();

    }
    return l.attrDuplicateOK.Has(l.extIndex(i));

}

// SetAttrDuplicateOK sets the "duplicate OK" property for an external
// symbol (see AttrDuplicateOK).
private static void SetAttrDuplicateOK(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic("tried to set dupok attr on non-external symbol");
    }
    if (v) {
        l.attrDuplicateOK.Set(l.extIndex(i));
    }
    else
 {
        l.attrDuplicateOK.Unset(l.extIndex(i));
    }
});

// AttrShared returns true for symbols compiled with the -shared option.
private static bool AttrShared(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) { 
        // TODO: if this path winds up being taken frequently, it
        // might make more sense to copy the flag value out of the
        // object into a larger bitmap during preload.
        var (r, _) = l.toLocal(i);
        return r.Shared();

    }
    return l.attrShared.Has(l.extIndex(i));

}

// SetAttrShared sets the "shared" property for an external
// symbol (see AttrShared).
private static void SetAttrShared(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic(fmt.Sprintf("tried to set shared attr on non-external symbol %d %s", i, l.SymName(i)));
    }
    if (v) {
        l.attrShared.Set(l.extIndex(i));
    }
    else
 {
        l.attrShared.Unset(l.extIndex(i));
    }
});

// AttrExternal returns true for function symbols loaded from host
// object files.
private static bool AttrExternal(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        return false;
    }
    return l.attrExternal.Has(l.extIndex(i));

}

// SetAttrExternal sets the "external" property for an host object
// symbol (see AttrExternal).
private static void SetAttrExternal(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic(fmt.Sprintf("tried to set external attr on non-external symbol %q", l.RawSymName(i)));
    }
    if (v) {
        l.attrExternal.Set(l.extIndex(i));
    }
    else
 {
        l.attrExternal.Unset(l.extIndex(i));
    }
});

// AttrSpecial returns true for a symbols that do not have their
// address (i.e. Value) computed by the usual mechanism of
// data.go:dodata() & data.go:address().
private static bool AttrSpecial(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (_, ok) = l.attrSpecial[i];
    return ok;
}

// SetAttrSpecial sets the "special" property for a symbol (see
// AttrSpecial).
private static void SetAttrSpecial(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrSpecial[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
    }
    else
 {
        delete(l.attrSpecial, i);
    }
}

// AttrCgoExportDynamic returns true for a symbol that has been
// specially marked via the "cgo_export_dynamic" compiler directive
// written by cgo (in response to //export directives in the source).
private static bool AttrCgoExportDynamic(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (_, ok) = l.attrCgoExportDynamic[i];
    return ok;
}

// SetAttrCgoExportDynamic sets the "cgo_export_dynamic" for a symbol
// (see AttrCgoExportDynamic).
private static void SetAttrCgoExportDynamic(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrCgoExportDynamic[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
    }
    else
 {
        delete(l.attrCgoExportDynamic, i);
    }
}

// AttrCgoExportStatic returns true for a symbol that has been
// specially marked via the "cgo_export_static" directive
// written by cgo.
private static bool AttrCgoExportStatic(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (_, ok) = l.attrCgoExportStatic[i];
    return ok;
}

// SetAttrCgoExportStatic sets the "cgo_export_static" for a symbol
// (see AttrCgoExportStatic).
private static void SetAttrCgoExportStatic(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    if (v) {
        l.attrCgoExportStatic[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
    }
    else
 {
        delete(l.attrCgoExportStatic, i);
    }
}

// IsGeneratedSym returns true if a symbol's been previously marked as a
// generator symbol through the SetIsGeneratedSym. The functions for generator
// symbols are kept in the Link context.
private static bool IsGeneratedSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    var (_, ok) = l.generatedSyms[i];
    return ok;
}

// SetIsGeneratedSym marks symbols as generated symbols. Data shouldn't be
// stored in generated symbols, and a function is registered and called for
// each of these symbols.
private static void SetIsGeneratedSym(this ptr<Loader> _addr_l, Sym i, bool v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(i)) {
        panic("only external symbols can be generated");
    }
    if (v) {
        l.generatedSyms[i] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
    }
    else
 {
        delete(l.generatedSyms, i);
    }
});

private static bool AttrCgoExport(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.AttrCgoExportDynamic(i) || l.AttrCgoExportStatic(i);
}

// AttrReadOnly returns true for a symbol whose underlying data
// is stored via a read-only mmap.
private static bool AttrReadOnly(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    {
        var (v, ok) = l.attrReadOnly[i];

        if (ok) {
            return v;
        }
    }

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp.objidx != 0) {
            return l.objs[pp.objidx].r.ReadOnly();
        }
        return false;

    }
    var (r, _) = l.toLocal(i);
    return r.ReadOnly();

}

// SetAttrReadOnly sets the "data is read only" property for a symbol
// (see AttrReadOnly).
private static void SetAttrReadOnly(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    l.attrReadOnly[i] = v;
}

// AttrSubSymbol returns true for symbols that are listed as a
// sub-symbol of some other outer symbol. The sub/outer mechanism is
// used when loading host objects (sections from the host object
// become regular linker symbols and symbols go on the Sub list of
// their section) and for constructing the global offset table when
// internally linking a dynamic executable.
//
// Note that in later stages of the linker, we set Outer(S) to some
// container symbol C, but don't set Sub(C). Thus we have two
// distinct scenarios:
//
// - Outer symbol covers the address ranges of its sub-symbols.
//   Outer.Sub is set in this case.
// - Outer symbol doesn't conver the address ranges. It is zero-sized
//   and doesn't have sub-symbols. In the case, the inner symbol is
//   not actually a "SubSymbol". (Tricky!)
//
// This method returns TRUE only for sub-symbols in the first scenario.
//
// FIXME: would be better to do away with this and have a better way
// to represent container symbols.

private static bool AttrSubSymbol(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;
 
    // we don't explicitly store this attribute any more -- return
    // a value based on the sub-symbol setting.
    var o = l.OuterSym(i);
    if (o == 0) {
        return false;
    }
    return l.SubSym(o) != 0;

}

// Note that we don't have a 'SetAttrSubSymbol' method in the loader;
// clients should instead use the AddInteriorSym method to establish
// containment relationships for host object symbols.

// Returns whether the i-th symbol has ReflectMethod attribute set.
private static bool IsReflectMethod(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.SymAttr(i) & goobj.SymFlagReflectMethod != 0;
}

// Returns whether the i-th symbol is nosplit.
private static bool IsNoSplit(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.SymAttr(i) & goobj.SymFlagNoSplit != 0;
}

// Returns whether this is a Go type symbol.
private static bool IsGoType(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.SymAttr(i) & goobj.SymFlagGoType != 0;
}

// Returns whether this symbol should be included in typelink.
private static bool IsTypelink(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.SymAttr(i) & goobj.SymFlagTypelink != 0;
}

// Returns whether this symbol is an itab symbol.
private static bool IsItab(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        return false;
    }
    var (r, li) = l.toLocal(i);
    return r.Sym(li).IsItab();

}

// Return whether this is a trampoline of a deferreturn call.
private static bool IsDeferReturnTramp(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.deferReturnTramp[i];
}

// Set that i is a trampoline of a deferreturn call.
private static void SetIsDeferReturnTramp(this ptr<Loader> _addr_l, Sym i, bool v) {
    ref Loader l = ref _addr_l.val;

    l.deferReturnTramp[i] = v;
}

// growValues grows the slice used to store symbol values.
private static void growValues(this ptr<Loader> _addr_l, nint reqLen) {
    ref Loader l = ref _addr_l.val;

    var curLen = len(l.values);
    if (reqLen > curLen) {
        l.values = append(l.values, make_slice<long>(reqLen + 1 - curLen));
    }
}

// SymValue returns the value of the i-th symbol. i is global index.
private static long SymValue(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.values[i];
}

// SetSymValue sets the value of the i-th symbol. i is global index.
private static void SetSymValue(this ptr<Loader> _addr_l, Sym i, long val) {
    ref Loader l = ref _addr_l.val;

    l.values[i] = val;
}

// AddToSymValue adds to the value of the i-th symbol. i is the global index.
private static void AddToSymValue(this ptr<Loader> _addr_l, Sym i, long val) {
    ref Loader l = ref _addr_l.val;

    l.values[i] += val;
}

// Returns the symbol content of the i-th symbol. i is global index.
private static slice<byte> Data(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp != null) {
            return pp.data;
        }
        return null;

    }
    var (r, li) = l.toLocal(i);
    return r.Data(li);

}

// FreeData clears the symbol data of an external symbol, allowing the memory
// to be freed earlier. No-op for non-external symbols.
// i is global index.
private static void FreeData(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp != null) {
            pp.data = null;
        }
    }
}

// SymAlign returns the alignment for a symbol.
private static int SymAlign(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (int(i) >= len(l.align)) { 
        // align is extended lazily -- it the sym in question is
        // outside the range of the existing slice, then we assume its
        // alignment has not yet been set.
        return 0;

    }
    var abits = l.align[i];
    if (abits == 0) {
        return 0;
    }
    return int32(1 << (int)((abits - 1)));

}

// SetSymAlign sets the alignment for a symbol.
private static void SetSymAlign(this ptr<Loader> _addr_l, Sym i, int align) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // Reject nonsense alignments.
    if (align < 0 || align & (align - 1) != 0) {
        panic("bad alignment value");
    }
    if (int(i) >= len(l.align)) {
        l.align = append(l.align, make_slice<byte>(l.NSym() - len(l.align)));
    }
    if (align == 0) {
        l.align[i] = 0;
    }
    l.align[i] = uint8(bits.Len32(uint32(align)));

});

// SymValue returns the section of the i-th symbol. i is global index.
private static ptr<sym.Section> SymSect(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (int(i) >= len(l.symSects)) { 
        // symSects is extended lazily -- it the sym in question is
        // outside the range of the existing slice, then we assume its
        // section has not yet been set.
        return _addr_null!;

    }
    return _addr_l.sects[l.symSects[i]]!;

}

// SetSymSect sets the section of the i-th symbol. i is global index.
private static void SetSymSect(this ptr<Loader> _addr_l, Sym i, ptr<sym.Section> _addr_sect) {
    ref Loader l = ref _addr_l.val;
    ref sym.Section sect = ref _addr_sect.val;

    if (int(i) >= len(l.symSects)) {
        l.symSects = append(l.symSects, make_slice<ushort>(l.NSym() - len(l.symSects)));
    }
    l.symSects[i] = sect.Index;

}

// growSects grows the slice used to store symbol sections.
private static void growSects(this ptr<Loader> _addr_l, nint reqLen) {
    ref Loader l = ref _addr_l.val;

    var curLen = len(l.symSects);
    if (reqLen > curLen) {
        l.symSects = append(l.symSects, make_slice<ushort>(reqLen + 1 - curLen));
    }
}

// NewSection creates a new (output) section.
private static ptr<sym.Section> NewSection(this ptr<Loader> _addr_l) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    ptr<sym.Section> sect = @new<sym.Section>();
    var idx = len(l.sects);
    if (idx != int(uint16(idx))) {
        panic("too many sections created");
    }
    sect.Index = uint16(idx);
    l.sects = append(l.sects, sect);
    return _addr_sect!;

});

// SymDynImplib returns the "dynimplib" attribute for the specified
// symbol, making up a portion of the info for a symbol specified
// on a "cgo_import_dynamic" compiler directive.
private static @string SymDynimplib(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.dynimplib[i];
}

// SetSymDynimplib sets the "dynimplib" attribute for a symbol.
private static void SetSymDynimplib(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetDynimplib");
    }
    if (value == "") {
        delete(l.dynimplib, i);
    }
    else
 {
        l.dynimplib[i] = value;
    }
});

// SymDynimpvers returns the "dynimpvers" attribute for the specified
// symbol, making up a portion of the info for a symbol specified
// on a "cgo_import_dynamic" compiler directive.
private static @string SymDynimpvers(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.dynimpvers[i];
}

// SetSymDynimpvers sets the "dynimpvers" attribute for a symbol.
private static void SetSymDynimpvers(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetDynimpvers");
    }
    if (value == "") {
        delete(l.dynimpvers, i);
    }
    else
 {
        l.dynimpvers[i] = value;
    }
});

// SymExtname returns the "extname" value for the specified
// symbol.
private static @string SymExtname(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    {
        var (s, ok) = l.extname[i];

        if (ok) {
            return s;
        }
    }

    return l.SymName(i);

}

// SetSymExtname sets the  "extname" attribute for a symbol.
private static void SetSymExtname(this ptr<Loader> _addr_l, Sym i, @string value) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetExtname");
    }
    if (value == "") {
        delete(l.extname, i);
    }
    else
 {
        l.extname[i] = value;
    }
});

// SymElfType returns the previously recorded ELF type for a symbol
// (used only for symbols read from shared libraries by ldshlibsyms).
// It is not set for symbols defined by the packages being linked or
// by symbols read by ldelf (and so is left as elf.STT_NOTYPE).
private static elf.SymType SymElfType(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    {
        var (et, ok) = l.elfType[i];

        if (ok) {
            return et;
        }
    }

    return elf.STT_NOTYPE;

}

// SetSymElfType sets the elf type attribute for a symbol.
private static void SetSymElfType(this ptr<Loader> _addr_l, Sym i, elf.SymType et) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetSymElfType");
    }
    if (et == elf.STT_NOTYPE) {
        delete(l.elfType, i);
    }
    else
 {
        l.elfType[i] = et;
    }
});

// SymElfSym returns the ELF symbol index for a given loader
// symbol, assigned during ELF symtab generation.
private static int SymElfSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.elfSym[i];
}

// SetSymElfSym sets the elf symbol index for a symbol.
private static void SetSymElfSym(this ptr<Loader> _addr_l, Sym i, int es) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (i == 0) {
        panic("bad sym index");
    }
    if (es == 0) {
        delete(l.elfSym, i);
    }
    else
 {
        l.elfSym[i] = es;
    }
});

// SymLocalElfSym returns the "local" ELF symbol index for a given loader
// symbol, assigned during ELF symtab generation.
private static int SymLocalElfSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.localElfSym[i];
}

// SetSymLocalElfSym sets the "local" elf symbol index for a symbol.
private static void SetSymLocalElfSym(this ptr<Loader> _addr_l, Sym i, int es) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (i == 0) {
        panic("bad sym index");
    }
    if (es == 0) {
        delete(l.localElfSym, i);
    }
    else
 {
        l.localElfSym[i] = es;
    }
});

// SymPlt returns the PLT offset of symbol s.
private static int SymPlt(this ptr<Loader> _addr_l, Sym s) {
    ref Loader l = ref _addr_l.val;

    {
        var (v, ok) = l.plt[s];

        if (ok) {
            return v;
        }
    }

    return -1;

}

// SetPlt sets the PLT offset of symbol i.
private static void SetPlt(this ptr<Loader> _addr_l, Sym i, int v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol for SetPlt");
    }
    if (v == -1) {
        delete(l.plt, i);
    }
    else
 {
        l.plt[i] = v;
    }
});

// SymGot returns the GOT offset of symbol s.
private static int SymGot(this ptr<Loader> _addr_l, Sym s) {
    ref Loader l = ref _addr_l.val;

    {
        var (v, ok) = l.got[s];

        if (ok) {
            return v;
        }
    }

    return -1;

}

// SetGot sets the GOT offset of symbol i.
private static void SetGot(this ptr<Loader> _addr_l, Sym i, int v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol for SetGot");
    }
    if (v == -1) {
        delete(l.got, i);
    }
    else
 {
        l.got[i] = v;
    }
});

// SymDynid returns the "dynid" property for the specified symbol.
private static int SymDynid(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    {
        var (s, ok) = l.dynid[i];

        if (ok) {
            return s;
        }
    }

    return -1;

}

// SetSymDynid sets the "dynid" property for a symbol.
private static void SetSymDynid(this ptr<Loader> _addr_l, Sym i, int val) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetSymDynid");
    }
    if (val == -1) {
        delete(l.dynid, i);
    }
    else
 {
        l.dynid[i] = val;
    }
});

// DynIdSyms returns the set of symbols for which dynID is set to an
// interesting (non-default) value. This is expected to be a fairly
// small set.
private static slice<Sym> DynidSyms(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    var sl = make_slice<Sym>(0, len(l.dynid));
    foreach (var (s) in l.dynid) {
        sl = append(sl, s);
    }    sort.Slice(sl, (i, j) => sl[i] < sl[j]);
    return sl;
}

// SymGoType returns the 'Gotype' property for a given symbol (set by
// the Go compiler for variable symbols). This version relies on
// reading aux symbols for the target sym -- it could be that a faster
// approach would be to check for gotype during preload and copy the
// results in to a map (might want to try this at some point and see
// if it helps speed things up).
private static Sym SymGoType(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    ptr<oReader> r;
    slice<goobj.Aux> auxs = default;
    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        r = l.objs[pp.objidx].r;
        auxs = pp.auxs;
    }
    else
 {
        uint li = default;
        r, li = l.toLocal(i);
        auxs = r.Auxs(li);
    }
    foreach (var (j) in auxs) {
        var a = _addr_auxs[j];

        if (a.Type() == goobj.AuxGotype) 
            return l.resolve(r, a.Sym());
        
    }    return 0;

}

// SymUnit returns the compilation unit for a given symbol (which will
// typically be nil for external or linker-manufactured symbols).
private static ptr<sym.CompilationUnit> SymUnit(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp.objidx != 0) {
            var r = l.objs[pp.objidx].r;
            return _addr_r.unit!;
        }
        return _addr_null!;

    }
    var (r, _) = l.toLocal(i);
    return _addr_r.unit!;

}

// SymPkg returns the package where the symbol came from (for
// regular compiler-generated Go symbols), but in the case of
// building with "-linkshared" (when a symbol is read from a
// shared library), will hold the library name.
// NOTE: this corresponds to sym.Symbol.File field.
private static @string SymPkg(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    {
        var (f, ok) = l.symPkg[i];

        if (ok) {
            return f;
        }
    }

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp.objidx != 0) {
            var r = l.objs[pp.objidx].r;
            return r.unit.Lib.Pkg;
        }
        return "";

    }
    var (r, _) = l.toLocal(i);
    return r.unit.Lib.Pkg;

}

// SetSymPkg sets the package/library for a symbol. This is
// needed mainly for external symbols, specifically those imported
// from shared libraries.
private static void SetSymPkg(this ptr<Loader> _addr_l, Sym i, @string pkg) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetSymPkg");
    }
    l.symPkg[i] = pkg;

});

// SymLocalentry returns the "local entry" value for the specified
// symbol.
private static byte SymLocalentry(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    return l.localentry[i];
}

// SetSymLocalentry sets the "local entry" attribute for a symbol.
private static void SetSymLocalentry(this ptr<Loader> _addr_l, Sym i, byte value) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // reject bad symbols
    if (i >= Sym(len(l.objSyms)) || i == 0) {
        panic("bad symbol index in SetSymLocalentry");
    }
    if (value == 0) {
        delete(l.localentry, i);
    }
    else
 {
        l.localentry[i] = value;
    }
});

// Returns the number of aux symbols given a global index.
private static nint NAux(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        return 0;
    }
    var (r, li) = l.toLocal(i);
    return r.NAux(li);

}

// Returns the "handle" to the j-th aux symbol of the i-th symbol.
private static Aux Aux(this ptr<Loader> _addr_l, Sym i, nint j) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        return new Aux();
    }
    var (r, li) = l.toLocal(i);
    if (j >= r.NAux(li)) {
        return new Aux();
    }
    return new Aux(r.Aux(li,j),r,l);

}

// GetFuncDwarfAuxSyms collects and returns the auxiliary DWARF
// symbols associated with a given function symbol.  Prior to the
// introduction of the loader, this was done purely using name
// lookups, e.f. for function with name XYZ we would then look up
// go.info.XYZ, etc.
private static (Sym, Sym, Sym, Sym) GetFuncDwarfAuxSyms(this ptr<Loader> _addr_l, Sym fnSymIdx) => func((_, panic, _) => {
    Sym auxDwarfInfo = default;
    Sym auxDwarfLoc = default;
    Sym auxDwarfRanges = default;
    Sym auxDwarfLines = default;
    ref Loader l = ref _addr_l.val;

    if (l.SymType(fnSymIdx) != sym.STEXT) {
        log.Fatalf("error: non-function sym %d/%s t=%s passed to GetFuncDwarfAuxSyms", fnSymIdx, l.SymName(fnSymIdx), l.SymType(fnSymIdx).String());
    }
    if (l.IsExternal(fnSymIdx)) { 
        // Current expectation is that any external function will
        // not have auxsyms.
        return ;

    }
    var (r, li) = l.toLocal(fnSymIdx);
    var auxs = r.Auxs(li);
    foreach (var (i) in auxs) {
        var a = _addr_auxs[i];

        if (a.Type() == goobj.AuxDwarfInfo) 
            auxDwarfInfo = l.resolve(r, a.Sym());
            if (l.SymType(auxDwarfInfo) != sym.SDWARFFCN) {
                panic("aux dwarf info sym with wrong type");
            }
        else if (a.Type() == goobj.AuxDwarfLoc) 
            auxDwarfLoc = l.resolve(r, a.Sym());
            if (l.SymType(auxDwarfLoc) != sym.SDWARFLOC) {
                panic("aux dwarf loc sym with wrong type");
            }
        else if (a.Type() == goobj.AuxDwarfRanges) 
            auxDwarfRanges = l.resolve(r, a.Sym());
            if (l.SymType(auxDwarfRanges) != sym.SDWARFRANGE) {
                panic("aux dwarf ranges sym with wrong type");
            }
        else if (a.Type() == goobj.AuxDwarfLines) 
            auxDwarfLines = l.resolve(r, a.Sym());
            if (l.SymType(auxDwarfLines) != sym.SDWARFLINES) {
                panic("aux dwarf lines sym with wrong type");
            }
        
    }    return ;

});

// AddInteriorSym sets up 'interior' as an interior symbol of
// container/payload symbol 'container'. An interior symbol does not
// itself have data, but gives a name to a subrange of the data in its
// container symbol. The container itself may or may not have a name.
// This method is intended primarily for use in the host object
// loaders, to capture the semantics of symbols and sections in an
// object file. When reading a host object file, we'll typically
// encounter a static section symbol (ex: ".text") containing content
// for a collection of functions, then a series of ELF (or macho, etc)
// symbol table entries each of which points into a sub-section
// (offset and length) of its corresponding container symbol. Within
// the go linker we create a loader.Sym for the container (which is
// expected to have the actual content/payload) and then a set of
// interior loader.Sym's that point into a portion of the container.
private static void AddInteriorSym(this ptr<Loader> _addr_l, Sym container, Sym interior) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // Container symbols are expected to have content/data.
    // NB: this restriction may turn out to be too strict (it's possible
    // to imagine a zero-sized container with an interior symbol pointing
    // into it); it's ok to relax or remove it if we counter an
    // oddball host object that triggers this.
    if (l.SymSize(container) == 0 && len(l.Data(container)) == 0) {
        panic("unexpected empty container symbol");
    }
    if (len(l.Data(interior)) != 0) {
        panic("unexpected non-empty interior symbol");
    }
    if (l.AttrNotInSymbolTable(interior)) {
        panic("interior symbol must be in symtab");
    }
    if (l.OuterSym(container) != 0) {
        panic("outer has outer itself");
    }
    if (l.SubSym(interior) != 0) {
        panic("sub set for subsym");
    }
    if (l.OuterSym(interior) != 0) {
        panic("outer already set for subsym");
    }
    l.sub[interior] = l.sub[container];
    l.sub[container] = interior;
    l.outer[interior] = container;

});

// OuterSym gets the outer symbol for host object loaded symbols.
private static Sym OuterSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;
 
    // FIXME: add check for isExternal?
    return l.outer[i];

}

// SubSym gets the subsymbol for host object loaded symbols.
private static Sym SubSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;
 
    // NB: note -- no check for l.isExternal(), since I am pretty sure
    // that later phases in the linker set subsym for "type." syms
    return l.sub[i];

}

// SetCarrierSym declares that 'c' is the carrier or container symbol
// for 's'. Carrier symbols are used in the linker to as a container
// for a collection of sub-symbols where the content of the
// sub-symbols is effectively concatenated to form the content of the
// carrier. The carrier is given a name in the output symbol table
// while the sub-symbol names are not. For example, the Go compiler
// emits named string symbols (type SGOSTRING) when compiling a
// package; after being deduplicated, these symbols are collected into
// a single unit by assigning them a new carrier symbol named
// "go.string.*" (which appears in the final symbol table for the
// output load module).
private static void SetCarrierSym(this ptr<Loader> _addr_l, Sym s, Sym c) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (c == 0) {
        panic("invalid carrier in SetCarrierSym");
    }
    if (s == 0) {
        panic("invalid sub-symbol in SetCarrierSym");
    }
    if (len(l.Data(c)) != 0) {
        panic("unexpected non-empty carrier symbol");
    }
    l.outer[s] = c; 
    // relocsym's foldSubSymbolOffset requires that we only
    // have a single level of containment-- enforce here.
    if (l.outer[c] != 0) {
        panic("invalid nested carrier sym");
    }
});

// Initialize Reachable bitmap and its siblings for running deadcode pass.
private static void InitReachable(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    l.growAttrBitmaps(l.NSym() + 1);
}

private partial struct symWithVal {
    public Sym s;
    public long v;
}
private partial struct bySymValue { // : slice<symWithVal>
}

private static nint Len(this bySymValue s) {
    return len(s);
}
private static void Swap(this bySymValue s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}
private static bool Less(this bySymValue s, nint i, nint j) {
    return s[i].v < s[j].v;
}

// SortSub walks through the sub-symbols for 's' and sorts them
// in place by increasing value. Return value is the new
// sub symbol for the specified outer symbol.
private static Sym SortSub(this ptr<Loader> _addr_l, Sym s) {
    ref Loader l = ref _addr_l.val;

    if (s == 0 || l.sub[s] == 0) {
        return s;
    }
    symWithVal sl = new slice<symWithVal>(new symWithVal[] {  });
    {
        var ss = l.sub[s];

        while (ss != 0) {
            sl = append(sl, new symWithVal(s:ss,v:l.SymValue(ss)));
            ss = l.sub[ss];
        }
    }
    sort.Stable(bySymValue(sl)); 

    // Then apply any changes needed to the sub map.
    var ns = Sym(0);
    for (var i = len(sl) - 1; i >= 0; i--) {
        var s = sl[i].s;
        l.sub[s] = ns;
        ns = s;
    } 

    // Update sub for outer symbol, then return
    l.sub[s] = sl[0].s;
    return sl[0].s;

}

// SortSyms sorts a list of symbols by their value.
private static void SortSyms(this ptr<Loader> _addr_l, slice<Sym> ss) {
    ref Loader l = ref _addr_l.val;

    sort.SliceStable(ss, (i, j) => l.SymValue(ss[i]) < l.SymValue(ss[j]));
}

// Insure that reachable bitmap and its siblings have enough size.
private static void growAttrBitmaps(this ptr<Loader> _addr_l, nint reqLen) {
    ref Loader l = ref _addr_l.val;

    if (reqLen > l.attrReachable.Len()) { 
        // These are indexed by global symbol
        l.attrReachable = growBitmap(reqLen, l.attrReachable);
        l.attrOnList = growBitmap(reqLen, l.attrOnList);
        l.attrLocal = growBitmap(reqLen, l.attrLocal);
        l.attrNotInSymbolTable = growBitmap(reqLen, l.attrNotInSymbolTable);
        l.attrUsedInIface = growBitmap(reqLen, l.attrUsedInIface);

    }
    l.growExtAttrBitmaps();

}

private static void growExtAttrBitmaps(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;
 
    // These are indexed by external symbol index (e.g. l.extIndex(i))
    var extReqLen = len(l.payloads);
    if (extReqLen > l.attrVisibilityHidden.Len()) {
        l.attrVisibilityHidden = growBitmap(extReqLen, l.attrVisibilityHidden);
        l.attrDuplicateOK = growBitmap(extReqLen, l.attrDuplicateOK);
        l.attrShared = growBitmap(extReqLen, l.attrShared);
        l.attrExternal = growBitmap(extReqLen, l.attrExternal);
    }
}

private static nint Count(this ptr<Relocs> _addr_relocs) {
    ref Relocs relocs = ref _addr_relocs.val;

    return len(relocs.rs);
}

// At returns the j-th reloc for a global symbol.
private static Reloc At(this ptr<Relocs> _addr_relocs, nint j) {
    ref Relocs relocs = ref _addr_relocs.val;

    if (relocs.l.isExtReader(relocs.r)) {
        return new Reloc(&relocs.rs[j],relocs.r,relocs.l);
    }
    return new Reloc(&relocs.rs[j],relocs.r,relocs.l);

}

// Relocs returns a Relocs object for the given global sym.
private static Relocs Relocs(this ptr<Loader> _addr_l, Sym i) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    var (r, li) = l.toLocal(i);
    if (r == null) {
        panic(fmt.Sprintf("trying to get oreader for invalid sym %d\n\n", i));
    }
    return l.relocs(r, li);

});

// Relocs returns a Relocs object given a local sym index and reader.
private static Relocs relocs(this ptr<Loader> _addr_l, ptr<oReader> _addr_r, uint li) {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;

    slice<goobj.Reloc> rs = default;
    if (l.isExtReader(r)) {
        var pp = l.payloads[li];
        rs = pp.relocs;
    }
    else
 {
        rs = r.Relocs(li);
    }
    return new Relocs(rs:rs,li:li,r:r,l:l,);

}

// FuncInfo provides hooks to access goobj.FuncInfo in the objects.
public partial struct FuncInfo {
    public ptr<Loader> l;
    public ptr<oReader> r;
    public slice<byte> data;
    public slice<goobj.Aux> auxs;
    public goobj.FuncInfoLengths lengths;
}

private static bool Valid(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return fi.r != null;
}

private static nint Args(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return int((goobj.FuncInfo.val)(null).ReadArgs(fi.data));
}

private static nint Locals(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return int((goobj.FuncInfo.val)(null).ReadLocals(fi.data));
}

private static objabi.FuncID FuncID(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return (goobj.FuncInfo.val)(null).ReadFuncID(fi.data);
}

private static objabi.FuncFlag FuncFlag(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return (goobj.FuncInfo.val)(null).ReadFuncFlag(fi.data);
}

private static Sym Pcsp(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    var sym = (goobj.FuncInfo.val)(null).ReadPcsp(fi.data);
    return fi.l.resolve(fi.r, sym);
}

private static Sym Pcfile(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    var sym = (goobj.FuncInfo.val)(null).ReadPcfile(fi.data);
    return fi.l.resolve(fi.r, sym);
}

private static Sym Pcline(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    var sym = (goobj.FuncInfo.val)(null).ReadPcline(fi.data);
    return fi.l.resolve(fi.r, sym);
}

private static Sym Pcinline(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    var sym = (goobj.FuncInfo.val)(null).ReadPcinline(fi.data);
    return fi.l.resolve(fi.r, sym);
}

// Preload has to be called prior to invoking the various methods
// below related to pcdata, funcdataoff, files, and inltree nodes.
private static void Preload(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    fi.lengths = (goobj.FuncInfo.val)(null).ReadFuncInfoLengths(fi.data);
}

private static slice<Sym> Pcdata(this ptr<FuncInfo> _addr_fi) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    var syms = (goobj.FuncInfo.val)(null).ReadPcdata(fi.data);
    var ret = make_slice<Sym>(len(syms));
    foreach (var (i) in ret) {
        ret[i] = fi.l.resolve(fi.r, syms[i]);
    }    return ret;

});

private static uint NumFuncdataoff(this ptr<FuncInfo> _addr_fi) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    return fi.lengths.NumFuncdataoff;

});

private static long Funcdataoff(this ptr<FuncInfo> _addr_fi, nint k) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    return (goobj.FuncInfo.val)(null).ReadFuncdataoff(fi.data, fi.lengths.FuncdataoffOff, uint32(k));

});

private static slice<Sym> Funcdata(this ptr<FuncInfo> _addr_fi, slice<Sym> syms) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    if (int(fi.lengths.NumFuncdataoff) > cap(syms)) {
        syms = make_slice<Sym>(0, fi.lengths.NumFuncdataoff);
    }
    else
 {
        syms = syms[..(int)0];
    }
    foreach (var (j) in fi.auxs) {
        var a = _addr_fi.auxs[j];
        if (a.Type() == goobj.AuxFuncdata) {
            syms = append(syms, fi.l.resolve(fi.r, a.Sym()));
        }
    }    return syms;

});

private static uint NumFile(this ptr<FuncInfo> _addr_fi) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    return fi.lengths.NumFile;

});

private static goobj.CUFileIndex File(this ptr<FuncInfo> _addr_fi, nint k) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    return (goobj.FuncInfo.val)(null).ReadFile(fi.data, fi.lengths.FileOff, uint32(k));

});

// TopFrame returns true if the function associated with this FuncInfo
// is an entry point, meaning that unwinders should stop when they hit
// this function.
private static bool TopFrame(this ptr<FuncInfo> _addr_fi) {
    ref FuncInfo fi = ref _addr_fi.val;

    return (fi.FuncFlag() & objabi.FuncFlag_TOPFRAME) != 0;
}

public partial struct InlTreeNode {
    public int Parent;
    public goobj.CUFileIndex File;
    public int Line;
    public Sym Func;
    public int ParentPC;
}

private static uint NumInlTree(this ptr<FuncInfo> _addr_fi) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    return fi.lengths.NumInlTree;

});

private static InlTreeNode InlTree(this ptr<FuncInfo> _addr_fi, nint k) => func((_, panic, _) => {
    ref FuncInfo fi = ref _addr_fi.val;

    if (!fi.lengths.Initialized) {
        panic("need to call Preload first");
    }
    var node = (goobj.FuncInfo.val)(null).ReadInlTree(fi.data, fi.lengths.InlTreeOff, uint32(k));
    return new InlTreeNode(Parent:node.Parent,File:node.File,Line:node.Line,Func:fi.l.resolve(fi.r,node.Func),ParentPC:node.ParentPC,);

});

private static FuncInfo FuncInfo(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    ptr<oReader> r;
    slice<goobj.Aux> auxs = default;
    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        if (pp.objidx == 0) {
            return new FuncInfo();
        }
        r = l.objs[pp.objidx].r;
        auxs = pp.auxs;

    }
    else
 {
        uint li = default;
        r, li = l.toLocal(i);
        auxs = r.Auxs(li);
    }
    foreach (var (j) in auxs) {
        var a = _addr_auxs[j];
        if (a.Type() == goobj.AuxFuncInfo) {
            var b = r.Data(a.Sym().SymIdx);
            return new FuncInfo(l,r,b,auxs,goobj.FuncInfoLengths{});
        }
    }    return new FuncInfo();

}

// Preload a package: adds autolib.
// Does not add defined package or non-packaged symbols to the symbol table.
// These are done in LoadSyms.
// Does not read symbol data.
// Returns the fingerprint of the object.
private static goobj.FingerprintType Preload(this ptr<Loader> _addr_l, nint localSymVersion, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, ptr<sym.CompilationUnit> _addr_unit, long length) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
    ref bio.Reader f = ref _addr_f.val;
    ref sym.Library lib = ref _addr_lib.val;
    ref sym.CompilationUnit unit = ref _addr_unit.val;

    var (roObject, readonly, err) = f.Slice(uint64(length)); // TODO: no need to map blocks that are for tools only (e.g. RefName)
    if (err != null) {
        log.Fatal("cannot read object file:", err);
    }
    var r = goobj.NewReaderFromBytes(roObject, readonly);
    if (r == null) {
        if (len(roObject) >= 8 && bytes.Equal(roObject[..(int)8], (slice<byte>)"\x00go114ld")) {
            log.Fatalf("found object file %s in old format", f.File().Name());
        }
        panic("cannot read object file");

    }
    var pkgprefix = objabi.PathToPrefix(lib.Pkg) + ".";
    var ndef = r.NSym();
    var nhashed64def = r.NHashed64def();
    var nhasheddef = r.NHasheddef();
    ptr<oReader> or = addr(new oReader(Reader:r,unit:unit,version:localSymVersion,flags:r.Flags(),pkgprefix:pkgprefix,syms:make([]Sym,ndef+nhashed64def+nhasheddef+r.NNonpkgdef()+r.NNonpkgref()),ndef:ndef,nhasheddef:nhasheddef,nhashed64def:nhashed64def,objidx:uint32(len(l.objs)),)); 

    // Autolib
    lib.Autolib = append(lib.Autolib, r.Autolib()); 

    // DWARF file table
    var nfile = r.NFile();
    unit.FileTable = make_slice<@string>(nfile);
    foreach (var (i) in unit.FileTable) {
        unit.FileTable[i] = r.File(i);
    }    l.addObj(lib.Pkg, or); 

    // The caller expects us consuming all the data
    f.MustSeek(length, os.SEEK_CUR);

    return r.Fingerprint();

});

// Holds the loader along with temporary states for loading symbols.
private partial struct loadState {
    public ptr<Loader> l;
    public map<ulong, symAndSize> hashed64Syms; // short hashed (content-addressable) symbols, keyed by content hash
    public map<goobj.HashType, symAndSize> hashedSyms; // hashed (content-addressable) symbols, keyed by content hash
}

// Preload symbols of given kind from an object.
private static void preloadSyms(this ptr<loadState> _addr_st, ptr<oReader> _addr_r, nint kind) => func((_, panic, _) => {
    ref loadState st = ref _addr_st.val;
    ref oReader r = ref _addr_r.val;

    var l = st.l;
    uint start = default;    uint end = default;


    if (kind == pkgDef) 
        start = 0;
        end = uint32(r.ndef);
    else if (kind == hashed64Def) 
        start = uint32(r.ndef);
        end = uint32(r.ndef + r.nhashed64def);
    else if (kind == hashedDef) 
        start = uint32(r.ndef + r.nhashed64def);
        end = uint32(r.ndef + r.nhashed64def + r.nhasheddef);
        if (l.hasUnknownPkgPath) { 
            // The content hash depends on symbol name expansion. If any package is
            // built without fully expanded names, the content hash is unreliable.
            // Treat them as named symbols.
            // This is rare.
            // (We don't need to do this for hashed64Def case, as there the hash
            // function is simply the identity function, which doesn't depend on
            // name expansion.)
            kind = nonPkgDef;

        }
    else if (kind == nonPkgDef) 
        start = uint32(r.ndef + r.nhashed64def + r.nhasheddef);
        end = uint32(r.ndef + r.nhashed64def + r.nhasheddef + r.NNonpkgdef());
    else 
        panic("preloadSyms: bad kind");
        l.growAttrBitmaps(len(l.objSyms) + int(end - start));
    var needNameExpansion = r.NeedNameExpansion();
    var loadingRuntimePkg = r.unit.Lib.Pkg == "runtime";
    for (var i = start; i < end; i++) {
        var osym = r.Sym(i);
        @string name = default;
        nint v = default;
        if (kind != hashed64Def && kind != hashedDef) { // we don't need the name, etc. for hashed symbols
            name = osym.Name(r.Reader);
            if (needNameExpansion) {
                name = strings.Replace(name, "\"\".", r.pkgprefix, -1);
            }

            v = abiToVer(osym.ABI(), r.version);

        }
        var gi = st.addSym(name, v, r, i, kind, osym);
        r.syms[i] = gi;
        if (osym.Local()) {
            l.SetAttrLocal(gi, true);
        }
        if (osym.UsedInIface()) {
            l.SetAttrUsedInIface(gi, true);
        }
        if (strings.HasPrefix(name, "runtime.") || (loadingRuntimePkg && strings.HasPrefix(name, "type."))) {
            {
                var bi = goobj.BuiltinIdx(name, v);

                if (bi != -1) { 
                    // This is a definition of a builtin symbol. Record where it is.
                    l.builtinSyms[bi] = gi;

                }

            }

        }
        {
            var a = int32(osym.Align());

            if (a != 0 && a > l.SymAlign(gi)) {
                l.SetSymAlign(gi, a);
            }

        }

    }

});

// Add syms, hashed (content-addressable) symbols, non-package symbols, and
// references to external symbols (which are always named).
private static void LoadSyms(this ptr<Loader> _addr_l, ptr<sys.Arch> _addr_arch) {
    ref Loader l = ref _addr_l.val;
    ref sys.Arch arch = ref _addr_arch.val;
 
    // Allocate space for symbols, making a guess as to how much space we need.
    // This function was determined empirically by looking at the cmd/compile on
    // Darwin, and picking factors for hashed and hashed64 syms.
    nint symSize = default;    nint hashedSize = default;    nint hashed64Size = default;

    {
        var o__prev1 = o;

        foreach (var (_, __o) in l.objs[(int)goObjStart..]) {
            o = __o;
            symSize += o.r.ndef + o.r.nhasheddef / 2 + o.r.nhashed64def / 2 + o.r.NNonpkgdef();
            hashedSize += o.r.nhasheddef / 2;
            hashed64Size += o.r.nhashed64def / 2;
        }
        o = o__prev1;
    }

    l.objSyms = make_slice<objSym>(1, symSize);

    l.npkgsyms = l.NSym();
    loadState st = new loadState(l:l,hashed64Syms:make(map[uint64]symAndSize,hashed64Size),hashedSyms:make(map[goobj.HashType]symAndSize,hashedSize),);

    {
        var o__prev1 = o;

        foreach (var (_, __o) in l.objs[(int)goObjStart..]) {
            o = __o;
            st.preloadSyms(o.r, pkgDef);
        }
        o = o__prev1;
    }

    {
        var o__prev1 = o;

        foreach (var (_, __o) in l.objs[(int)goObjStart..]) {
            o = __o;
            st.preloadSyms(o.r, hashed64Def);
            st.preloadSyms(o.r, hashedDef);
            st.preloadSyms(o.r, nonPkgDef);
        }
        o = o__prev1;
    }

    l.nhashedsyms = len(st.hashed64Syms) + len(st.hashedSyms);
    {
        var o__prev1 = o;

        foreach (var (_, __o) in l.objs[(int)goObjStart..]) {
            o = __o;
            loadObjRefs(_addr_l, _addr_o.r, _addr_arch);
        }
        o = o__prev1;
    }

    l.values = make_slice<long>(l.NSym(), l.NSym() + 1000); // +1000 make some room for external symbols
}

private static void loadObjRefs(ptr<Loader> _addr_l, ptr<oReader> _addr_r, ptr<sys.Arch> _addr_arch) {
    ref Loader l = ref _addr_l.val;
    ref oReader r = ref _addr_r.val;
    ref sys.Arch arch = ref _addr_arch.val;
 
    // load non-package refs
    var ndef = uint32(r.NAlldef());
    var needNameExpansion = r.NeedNameExpansion();
    {
        var i__prev1 = i;
        var n__prev1 = n;

        for (var i = uint32(0);
        var n = uint32(r.NNonpkgref()); i < n; i++) {
            var osym = r.Sym(ndef + i);
            var name = osym.Name(r.Reader);
            if (needNameExpansion) {
                name = strings.Replace(name, "\"\".", r.pkgprefix, -1);
            }
            var v = abiToVer(osym.ABI(), r.version);
            r.syms[ndef + i] = l.LookupOrCreateSym(name, v);
            var gi = r.syms[ndef + i];
            if (osym.Local()) {
                l.SetAttrLocal(gi, true);
            }
            if (osym.UsedInIface()) {
                l.SetAttrUsedInIface(gi, true);
            }
        }

        i = i__prev1;
        n = n__prev1;
    } 

    // referenced packages
    var npkg = r.NPkg();
    r.pkg = make_slice<uint>(npkg);
    {
        var i__prev1 = i;

        for (i = 1; i < npkg; i++) { // PkgIdx 0 is a dummy invalid package
            var pkg = r.Pkg(i);
            var (objidx, ok) = l.objByPkg[pkg];
            if (!ok) {
                log.Fatalf("%v: reference to nonexistent package %s", r.unit.Lib, pkg);
            }

            r.pkg[i] = objidx;

        }

        i = i__prev1;
    } 

    // load flags of package refs
    {
        var i__prev1 = i;
        var n__prev1 = n;

        for (i = 0;
        n = r.NRefFlags(); i < n; i++) {
            var rf = r.RefFlags(i);
            gi = l.resolve(r, rf.Sym());
            if (rf.Flag2() & goobj.SymFlagUsedInIface != 0) {
                l.SetAttrUsedInIface(gi, true);
            }
        }

        i = i__prev1;
        n = n__prev1;
    }

}

private static nint abiToVer(ushort abi, nint localSymVersion) {
    nint v = default;
    if (abi == goobj.SymABIstatic) { 
        // Static
        v = localSymVersion;

    }    {
        var abiver = sym.ABIToVersion(obj.ABI(abi));


        else if (abiver != -1) { 
            // Note that data symbols are "ABI0", which maps to version 0.
            v = abiver;

        }
        else
 {
            log.Fatalf("invalid symbol ABI: %d", abi);
        }
    }

    return v;

}

// ResolveABIAlias given a symbol returns the ABI alias target of that
// symbol. If the sym in question is not an alias, the sym itself is
// returned.
private static Sym ResolveABIAlias(this ptr<Loader> _addr_l, Sym s) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (l.flags & FlagUseABIAlias == 0) {
        return s;
    }
    if (s == 0) {
        return 0;
    }
    if (l.SymType(s) != sym.SABIALIAS) {
        return s;
    }
    var relocs = l.Relocs(s);
    var target = relocs.At(0).Sym();
    if (l.SymType(target) == sym.SABIALIAS) {
        panic(fmt.Sprintf("ABI alias %s references another ABI alias %s", l.SymName(s), l.SymName(target)));
    }
    return target;

});

// TopLevelSym tests a symbol (by name and kind) to determine whether
// the symbol first class sym (participating in the link) or is an
// anonymous aux or sub-symbol containing some sub-part or payload of
// another symbol.
private static bool TopLevelSym(this ptr<Loader> _addr_l, Sym s) {
    ref Loader l = ref _addr_l.val;

    return topLevelSym(l.RawSymName(s), l.SymType(s));
}

// topLevelSym tests a symbol name and kind to determine whether
// the symbol first class sym (participating in the link) or is an
// anonymous aux or sub-symbol containing some sub-part or payload of
// another symbol.
private static bool topLevelSym(@string sname, sym.SymKind skind) {
    if (sname != "") {
        return true;
    }

    if (skind == sym.SDWARFFCN || skind == sym.SDWARFABSFCN || skind == sym.SDWARFTYPE || skind == sym.SDWARFCONST || skind == sym.SDWARFCUINFO || skind == sym.SDWARFRANGE || skind == sym.SDWARFLOC || skind == sym.SDWARFLINES || skind == sym.SGOFUNC) 
        return true;
    else 
        return false;
    
}

// cloneToExternal takes the existing object file symbol (symIdx)
// and creates a new external symbol payload that is a clone with
// respect to name, version, type, relocations, etc. The idea here
// is that if the linker decides it wants to update the contents of
// a symbol originally discovered as part of an object file, it's
// easier to do this if we make the updates to an external symbol
// payload.
private static void cloneToExternal(this ptr<Loader> _addr_l, Sym symIdx) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(symIdx)) {
        panic("sym is already external, no need for clone");
    }
    var (r, li) = l.toLocal(symIdx);
    var osym = r.Sym(li);
    var sname = osym.Name(r.Reader);
    if (r.NeedNameExpansion()) {
        sname = strings.Replace(sname, "\"\".", r.pkgprefix, -1);
    }
    var sver = abiToVer(osym.ABI(), r.version);
    var skind = sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())]; 

    // Create new symbol, update version and kind.
    var pi = l.newPayload(sname, sver);
    var pp = l.payloads[pi];
    pp.kind = skind;
    pp.ver = sver;
    pp.size = int64(osym.Siz());
    pp.objidx = r.objidx; 

    // If this is a def, then copy the guts. We expect this case
    // to be very rare (one case it may come up is with -X).
    if (li < uint32(r.NAlldef())) {
        // Copy relocations
        var relocs = l.Relocs(symIdx);
        pp.relocs = make_slice<goobj.Reloc>(relocs.Count());
        foreach (var (i) in pp.relocs) { 
            // Copy the relocs slice.
            // Convert local reference to global reference.
            var rel = relocs.At(i);
            pp.relocs[i].Set(rel.Off(), rel.Siz(), uint16(rel.Type()), rel.Add(), new goobj.SymRef(PkgIdx:0,SymIdx:uint32(rel.Sym())));

        }        pp.data = r.Data(li);

    }
    var auxs = r.Auxs(li);
    pp.auxs = auxs; 

    // Install new payload to global index space.
    // (This needs to happen at the end, as the accessors above
    // need to access the old symbol content.)
    l.objSyms[symIdx] = new objSym(l.extReader.objidx,uint32(pi));
    l.extReader.syms = append(l.extReader.syms, symIdx);

});

// Copy the payload of symbol src to dst. Both src and dst must be external
// symbols.
// The intended use case is that when building/linking against a shared library,
// where we do symbol name mangling, the Go object file may have reference to
// the original symbol name whereas the shared library provides a symbol with
// the mangled name. When we do mangling, we copy payload of mangled to original.
private static void CopySym(this ptr<Loader> _addr_l, Sym src, Sym dst) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    if (!l.IsExternal(dst)) {
        panic("dst is not external"); //l.newExtSym(l.SymName(dst), l.SymVersion(dst))
    }
    if (!l.IsExternal(src)) {
        panic("src is not external"); //l.cloneToExternal(src)
    }
    l.payloads[l.extIndex(dst)] = l.payloads[l.extIndex(src)];
    l.SetSymPkg(dst, l.SymPkg(src)); 
    // TODO: other attributes?
});

// CopyAttributes copies over all of the attributes of symbol 'src' to
// symbol 'dst'.
private static void CopyAttributes(this ptr<Loader> _addr_l, Sym src, Sym dst) {
    ref Loader l = ref _addr_l.val;

    l.SetAttrReachable(dst, l.AttrReachable(src));
    l.SetAttrOnList(dst, l.AttrOnList(src));
    l.SetAttrLocal(dst, l.AttrLocal(src));
    l.SetAttrNotInSymbolTable(dst, l.AttrNotInSymbolTable(src));
    if (l.IsExternal(dst)) {
        l.SetAttrVisibilityHidden(dst, l.AttrVisibilityHidden(src));
        l.SetAttrDuplicateOK(dst, l.AttrDuplicateOK(src));
        l.SetAttrShared(dst, l.AttrShared(src));
        l.SetAttrExternal(dst, l.AttrExternal(src));
    }
    else
 { 
        // Some attributes are modifiable only for external symbols.
        // In such cases, don't try to transfer over the attribute
        // from the source even if there is a clash. This comes up
        // when copying attributes from a dupOK ABI wrapper symbol to
        // the real target symbol (which may not be marked dupOK).
    }
    l.SetAttrSpecial(dst, l.AttrSpecial(src));
    l.SetAttrCgoExportDynamic(dst, l.AttrCgoExportDynamic(src));
    l.SetAttrCgoExportStatic(dst, l.AttrCgoExportStatic(src));
    l.SetAttrReadOnly(dst, l.AttrReadOnly(src));

}

// CreateExtSym creates a new external symbol with the specified name
// without adding it to any lookup tables, returning a Sym index for it.
private static Sym CreateExtSym(this ptr<Loader> _addr_l, @string name, nint ver) {
    ref Loader l = ref _addr_l.val;

    return l.newExtSym(name, ver);
}

// CreateStaticSym creates a new static symbol with the specified name
// without adding it to any lookup tables, returning a Sym index for it.
private static Sym CreateStaticSym(this ptr<Loader> _addr_l, @string name) {
    ref Loader l = ref _addr_l.val;
 
    // Assign a new unique negative version -- this is to mark the
    // symbol so that it is not included in the name lookup table.
    l.anonVersion--;
    return l.newExtSym(name, l.anonVersion);

}

private static void FreeSym(this ptr<Loader> _addr_l, Sym i) {
    ref Loader l = ref _addr_l.val;

    if (l.IsExternal(i)) {
        var pp = l.getPayload(i);
        pp.val = new extSymPayload();
    }
}

// relocId is essentially a <S,R> tuple identifying the Rth
// relocation of symbol S.
private partial struct relocId {
    public Sym sym;
    public nint ridx;
}

// SetRelocVariant sets the 'variant' property of a relocation on
// some specific symbol.
private static void SetRelocVariant(this ptr<Loader> _addr_l, Sym s, nint ri, sym.RelocVariant v) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;
 
    // sanity check
    {
        var relocs = l.Relocs(s);

        if (ri >= relocs.Count()) {
            panic("invalid relocation ID");
        }
    }

    if (l.relocVariant == null) {
        l.relocVariant = make_map<relocId, sym.RelocVariant>();
    }
    if (v != 0) {
        l.relocVariant[new relocId(s,ri)] = v;
    }
    else
 {
        delete(l.relocVariant, new relocId(s,ri));
    }
});

// RelocVariant returns the 'variant' property of a relocation on
// some specific symbol.
private static sym.RelocVariant RelocVariant(this ptr<Loader> _addr_l, Sym s, nint ri) {
    ref Loader l = ref _addr_l.val;

    return l.relocVariant[new relocId(s,ri)];
}

// UndefinedRelocTargets iterates through the global symbol index
// space, looking for symbols with relocations targeting undefined
// references. The linker's loadlib method uses this to determine if
// there are unresolved references to functions in system libraries
// (for example, libgcc.a), presumably due to CGO code. Return
// value is a list of loader.Sym's corresponding to the undefined
// cross-refs. The "limit" param controls the maximum number of
// results returned; if "limit" is -1, then all undefs are returned.
private static slice<Sym> UndefinedRelocTargets(this ptr<Loader> _addr_l, nint limit) {
    ref Loader l = ref _addr_l.val;

    Sym result = new slice<Sym>(new Sym[] {  });
    for (var si = Sym(1); si < Sym(len(l.objSyms)); si++) {
        var relocs = l.Relocs(si);
        for (nint ri = 0; ri < relocs.Count(); ri++) {
            var r = relocs.At(ri);
            var rs = r.Sym();
            if (rs != 0 && l.SymType(rs) == sym.SXREF && l.RawSymName(rs) != ".got") {
                result = append(result, rs);
                if (limit != -1 && len(result) >= limit) {
                    break;
                }
            }
        }
    }
    return result;
}

// AssignTextSymbolOrder populates the Textp slices within each
// library and compilation unit, insuring that packages are laid down
// in dependency order (internal first, then everything else). Return value
// is a slice of all text syms.
private static slice<Sym> AssignTextSymbolOrder(this ptr<Loader> _addr_l, slice<ptr<sym.Library>> libs, slice<bool> intlibs, slice<Sym> extsyms) => func((_, panic, _) => {
    ref Loader l = ref _addr_l.val;

    // Library Textp lists should be empty at this point.
    {
        var lib__prev1 = lib;

        foreach (var (_, __lib) in libs) {
            lib = __lib;
            if (len(lib.Textp) != 0) {
                panic("expected empty Textp slice for library");
            }
            if (len(lib.DupTextSyms) != 0) {
                panic("expected empty DupTextSyms slice for library");
            }
        }
        lib = lib__prev1;
    }

    var assignedToUnit = MakeBitmap(l.NSym() + 1); 

    // Start off textp with reachable external syms.
    Sym textp = new slice<Sym>(new Sym[] {  });
    {
        var sym__prev1 = sym;

        foreach (var (_, __sym) in extsyms) {
            sym = __sym;
            if (!l.attrReachable.Has(sym)) {
                continue;
            }
            textp = append(textp, sym);
        }
        sym = sym__prev1;
    }

    foreach (var (_, o) in l.objs[(int)goObjStart..]) {
        var r = o.r;
        var lib = r.unit.Lib;
        {
            var i__prev2 = i;

            for (var i = uint32(0);
            var n = uint32(r.NAlldef()); i < n; i++) {
                var gi = l.toGlobal(r, i);
                if (!l.attrReachable.Has(gi)) {
                    continue;
                }
                var osym = r.Sym(i);
                var st = sym.AbiSymKindToSymKind[objabi.SymKind(osym.Type())];
                if (st != sym.STEXT) {
                    continue;
                }
                var dupok = osym.Dupok();
                {
                    var (r2, i2) = l.toLocal(gi);

                    if (r2 != r || i2 != i) { 
                        // A dupok text symbol is resolved to another package.
                        // We still need to record its presence in the current
                        // package, as the trampoline pass expects packages
                        // are laid out in dependency order.
                        lib.DupTextSyms = append(lib.DupTextSyms, sym.LoaderSym(gi));
                        continue; // symbol in different object
                    }

                }

                if (dupok) {
                    lib.DupTextSyms = append(lib.DupTextSyms, sym.LoaderSym(gi));
                    continue;
                }

                lib.Textp = append(lib.Textp, sym.LoaderSym(gi));

            }


            i = i__prev2;
        }

    }    foreach (var (_, doInternal) in new array<bool>(new bool[] { true, false })) {
        {
            var lib__prev2 = lib;

            foreach (var (__idx, __lib) in libs) {
                idx = __idx;
                lib = __lib;
                if (intlibs[idx] != doInternal) {
                    continue;
                }
                array<slice<sym.LoaderSym>> lists = new array<slice<sym.LoaderSym>>(new slice<sym.LoaderSym>[] { lib.Textp, lib.DupTextSyms });
                {
                    var i__prev3 = i;

                    foreach (var (__i, __list) in lists) {
                        i = __i;
                        list = __list;
                        foreach (var (_, s) in list) {
                            var sym = Sym(s);
                            if (!assignedToUnit.Has(sym)) {
                                textp = append(textp, sym);
                                var unit = l.SymUnit(sym);
                                if (unit != null) {
                                    unit.Textp = append(unit.Textp, s);
                                    assignedToUnit.Set(sym);
                                } 
                                // Dupok symbols may be defined in multiple packages; the
                                // associated package for a dupok sym is chosen sort of
                                // arbitrarily (the first containing package that the linker
                                // loads). Canonicalizes its Pkg to the package with which
                                // it will be laid down in text.
                                if (i == 1 && l.SymPkg(sym) != lib.Pkg) {
                                    l.SetSymPkg(sym, lib.Pkg);
                                }

                            }

                        }

                    }

                    i = i__prev3;
                }

                lib.Textp = null;
                lib.DupTextSyms = null;

            }

            lib = lib__prev2;
        }
    }    return textp;

});

// ErrorReporter is a helper class for reporting errors.
public partial struct ErrorReporter {
    public ptr<Loader> ldr;
    public Action AfterErrorAction;
}

// Errorf method logs an error message.
//
// After each error, the error actions function will be invoked; this
// will either terminate the link immediately (if -h option given)
// or it will keep a count and exit if more than 20 errors have been printed.
//
// Logging an error means that on exit cmd/link will delete any
// output file and return a non-zero error code.
//
private static void Errorf(this ptr<ErrorReporter> _addr_reporter, Sym s, @string format, params object[] args) {
    args = args.Clone();
    ref ErrorReporter reporter = ref _addr_reporter.val;

    if (s != 0 && reporter.ldr.SymName(s) != "") {
        format = reporter.ldr.SymName(s) + ": " + format;
    }
    else
 {
        format = fmt.Sprintf("sym %d: %s", s, format);
    }
    format += "\n";
    fmt.Fprintf(os.Stderr, format, args);
    reporter.AfterErrorAction();

}

// GetErrorReporter returns the loader's associated error reporter.
private static ptr<ErrorReporter> GetErrorReporter(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    return _addr_l.errorReporter!;
}

// Errorf method logs an error message. See ErrorReporter.Errorf for details.
private static void Errorf(this ptr<Loader> _addr_l, Sym s, @string format, params object[] args) {
    args = args.Clone();
    ref Loader l = ref _addr_l.val;

    l.errorReporter.Errorf(s, format, args);
}

// Symbol statistics.
private static @string Stat(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    var s = fmt.Sprintf("%d symbols, %d reachable\n", l.NSym(), l.NReachableSym());
    s += fmt.Sprintf("\t%d package symbols, %d hashed symbols, %d non-package symbols, %d external symbols\n", l.npkgsyms, l.nhashedsyms, int(l.extStart) - l.npkgsyms - l.nhashedsyms, l.NSym() - int(l.extStart));
    return s;
}

// For debugging.
private static void Dump(this ptr<Loader> _addr_l) {
    ref Loader l = ref _addr_l.val;

    fmt.Println("objs");
    foreach (var (_, obj) in l.objs[(int)goObjStart..]) {
        if (obj.r != null) {
            fmt.Println(obj.i, obj.r.unit.Lib);
        }
    }    fmt.Println("extStart:", l.extStart);
    fmt.Println("Nsyms:", len(l.objSyms));
    fmt.Println("syms");
    {
        var i__prev1 = i;

        for (var i = Sym(1); i < Sym(len(l.objSyms)); i++) {
            @string pi = "";
            if (l.IsExternal(i)) {
                pi = fmt.Sprintf("<ext %d>", l.extIndex(i));
            }
            @string sect = "";
            if (l.SymSect(i) != null) {
                sect = l.SymSect(i).Name;
            }
            fmt.Printf("%v %v %v %v %x %v\n", i, l.SymName(i), l.SymType(i), pi, l.SymValue(i), sect);
        }

        i = i__prev1;
    }
    fmt.Println("symsByName");
    {
        var name__prev1 = name;
        var i__prev1 = i;

        foreach (var (__name, __i) in l.symsByName[0]) {
            name = __name;
            i = __i;
            fmt.Println(i, name, 0);
        }
        name = name__prev1;
        i = i__prev1;
    }

    {
        var name__prev1 = name;
        var i__prev1 = i;

        foreach (var (__name, __i) in l.symsByName[1]) {
            name = __name;
            i = __i;
            fmt.Println(i, name, 1);
        }
        name = name__prev1;
        i = i__prev1;
    }

    fmt.Println("payloads:");
    {
        var i__prev1 = i;

        foreach (var (__i) in l.payloads) {
            i = __i;
            var pp = l.payloads[i];
            fmt.Println(i, pp.name, pp.ver, pp.kind);
        }
        i = i__prev1;
    }
}

} // end loader_package
